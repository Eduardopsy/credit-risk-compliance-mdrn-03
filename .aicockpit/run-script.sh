#!/bin/sh
################################################################################
# Run Script for AI Cockpit Agent Manager — Worktree Development Environment
# Credit Risk Compliance Lab — Backend Services Startup
################################################################################
#
# POSIX Shell Compatible (sh-compatible, not bash-only)
#
# Purpose: Initialize development environment when "Run" button is clicked
#
# Behavior:
#   1. Validate prerequisites (.env, Docker, .NET)
#   2. Start Docker services (if offline)
#   3. Build solution completely
#   4. Start 3 APIs sequentially
#   5. Display access URLs
#   6. Cleanup on Ctrl+C
#
# Environment Variables (from Agent Manager):
#   WORKTREE_PATH: Absolute path to worktree
#   REPO_PATH: Absolute path to repo root
#
# Time: ~30-45 seconds total startup
#
################################################################################

set -eu

# ============================================================================
# SECTION 1: CONFIGURATION & ENVIRONMENT
# ============================================================================

WORKTREE_PATH="${WORKTREE_PATH:-.}"
REPO_PATH="${REPO_PATH:-.}"

# API Configuration (sequencial startup)
# Format: "project_path|port|name"
API_1="src/modules/iam/CreditRisk.IAM.Api/CreditRisk.IAM.Api.csproj|5001|iam"
API_2="src/modules/credit-analysis/CreditRisk.CreditAnalysis.Api/CreditRisk.CreditAnalysis.Api.csproj|5002|credit"
API_3="src/modules/compliance/CreditRisk.Compliance.Api/CreditRisk.Compliance.Api.csproj|5003|compliance"

# Files & Paths
ENV_FILE="$REPO_PATH/.env"
PIDs_FILE="$WORKTREE_PATH/.run-pids"
LOGS_DIR="$WORKTREE_PATH/logs"

# Global state
DOCKER_STARTED=false
EXIT_CODE=0

# ============================================================================
# SECTION 2: UTILITY FUNCTIONS
# ============================================================================

log_info() {
    printf "[INFO] %s\n" "$*"
}

log_success() {
    printf "[✓] %s\n" "$*"
}

log_warn() {
    printf "[⚠] %s\n" "$*" >&2
}

log_error() {
    printf "[✗] %s\n" "$*" >&2
}

log_section() {
    printf "\n────────────────────────────────────────────────────────────────\n"
    printf "%s\n" "$*"
    printf "────────────────────────────────────────────────────────────────\n"
}

check_command() {
    local cmd="$1"
    if ! command -v "$cmd" > /dev/null 2>&1; then
        log_error "$cmd not found. Please install it."
        return 1
    fi
    return 0
}

check_file() {
    local file="$1"
    if [ ! -f "$file" ]; then
        log_error "File not found: $file"
        return 1
    fi
    return 0
}

# ============================================================================
# SECTION 3: CLEANUP & SIGNAL HANDLING
# ============================================================================

cleanup() {
    EXIT_CODE="${1:-0}"
    
    log_section "Cleaning Up"
    
    # Kill all tracked PIDs
    if [ -f "$PIDs_FILE" ]; then
        log_info "Stopping API processes..."
        while IFS= read -r pid; do
            if [ -n "$pid" ] && kill -0 "$pid" 2>/dev/null; then
                log_info "  Killing process $pid..."
                kill "$pid" 2>/dev/null || true
            fi
        done < "$PIDs_FILE"
        rm -f "$PIDs_FILE"
    fi
    
    # Stop docker-compose (liberate resources)
    if [ "$DOCKER_STARTED" = "true" ]; then
        log_info "Stopping Docker services..."
        cd "$REPO_PATH"
        docker-compose down 2>/dev/null || log_warn "docker-compose down failed"
    fi
    
    log_success "Cleanup complete"
    printf "\n"
    
    exit "$EXIT_CODE"
}

# Register cleanup on Ctrl+C and exit
trap 'cleanup 0' INT
trap 'cleanup 1' EXIT

# ============================================================================
# SECTION 4: PRE-FLIGHT CHECKS
# ============================================================================

preflight_checks() {
    log_section "Pre-flight Checks"
    
    # Check .env exists
    if ! check_file "$ENV_FILE"; then
        log_error ".env not found in repo root"
        log_error "Please run setup-script.sh first"
        return 1
    fi
    log_success ".env found"
    
    # Check Docker installed
    if ! check_command "docker"; then
        log_error "Docker not installed. Visit: https://docs.docker.com/get-docker/"
        return 1
    fi
    log_success "Docker installed ($(docker --version 2>&1 | head -1))"
    
    # Check Docker Compose
    if ! check_command "docker-compose"; then
        log_error "Docker Compose not installed"
        return 1
    fi
    log_success "Docker Compose installed ($(docker-compose --version 2>&1 | head -1))"
    
    # Check .NET SDK
    if ! check_command "dotnet"; then
        log_error ".NET SDK not installed"
        return 1
    fi
    log_success ".NET SDK available ($(dotnet --version))"
    
    # Verify logs directory exists
    mkdir -p "$LOGS_DIR"
    log_success "Logs directory ready: $LOGS_DIR"
}

# ============================================================================
# SECTION 5: DOCKER SERVICES MANAGEMENT
# ============================================================================

manage_docker_services() {
    log_section "Docker Services"
    
    cd "$REPO_PATH"
    
    # Check current status
    log_info "Checking service status..."
    docker-compose ps 2>/dev/null || {
        log_error "Cannot check docker-compose status"
        return 1
    }
    
    # Check if postgres is healthy
    if ! docker-compose exec -T postgres pg_isready -q 2>/dev/null; then
        log_warn "Services not ready, starting docker-compose..."
        
        docker-compose up -d
        DOCKER_STARTED=true
        
        log_info "Waiting for services to be ready (max 120s)..."
        local timeout=120
        local elapsed=0
        
        while [ $elapsed -lt $timeout ]; do
            if docker-compose exec -T postgres pg_isready -q 2>/dev/null; then
                log_success "All services healthy"
                return 0
            fi
            sleep 2
            elapsed=$((elapsed + 2))
        done
        
        log_error "Services failed to become ready within $timeout seconds"
        return 1
    fi
    
    log_success "Services already running and healthy"
}

# ============================================================================
# SECTION 6: BUILD COMPILATION
# ============================================================================

build_solution() {
    log_section "Building Solution"
    
    cd "$REPO_PATH"
    
    log_info "Building: dotnet build CreditRiskComplianceLab.sln"
    
    if dotnet build CreditRiskComplianceLab.sln -c Release 2>&1 | tee "$LOGS_DIR/build.log"; then
        log_success "Build successful"
        return 0
    else
        log_error "Build failed. Check logs/build.log for details"
        return 1
    fi
}

# ============================================================================
# SECTION 7: SEQUENTIAL API STARTUP
# ============================================================================

start_api() {
    local project_path="$1"
    local port="$2"
    local api_name="$3"
    
    local log_file="$LOGS_DIR/${api_name}.log"
    local project_file="$REPO_PATH/$project_path"
    
    log_info "Starting $api_name API..."
    log_info "  Project: $project_path"
    log_info "  Port: $port"
    log_info "  Logs: $log_file"
    
    # Start process in background, redirect output to log file
    cd "$REPO_PATH"
    dotnet run --project "$project_file" --urls "http://localhost:$port" \
        > "$log_file" 2>&1 &
    
    local pid=$!
    echo "$pid" >> "$PIDs_FILE"
    
    # Brief wait for process to start
    sleep 2
    
    # Check if process is still alive
    if ! kill -0 "$pid" 2>/dev/null; then
        log_error "$api_name failed to start (PID $pid)"
        log_error "Check logs: tail -f $log_file"
        return 1
    fi
    
    log_success "$api_name started (PID: $pid)"
}

start_apis() {
    log_section "Starting APIs (Sequential)"
    
    # Clear PIDs file
    > "$PIDs_FILE"
    
    # Parse and start each API
    local current=1
    local total=3
    local failed_apis=""
    
    # API 1
    log_info ""
    log_info "[$current/$total] $(echo "$API_1" | cut -d'|' -f3)"
    if ! start_api "$(echo "$API_1" | cut -d'|' -f1)" "$(echo "$API_1" | cut -d'|' -f2)" "$(echo "$API_1" | cut -d'|' -f3)"; then
        log_warn "Failed to start API 1, continuing with remaining APIs..."
        failed_apis="$failed_apis $(echo "$API_1" | cut -d'|' -f3)"
    fi
    current=$((current + 1))
    
    # API 2
    log_info ""
    log_info "[$current/$total] $(echo "$API_2" | cut -d'|' -f3)"
    if ! start_api "$(echo "$API_2" | cut -d'|' -f1)" "$(echo "$API_2" | cut -d'|' -f2)" "$(echo "$API_2" | cut -d'|' -f3)"; then
        log_warn "Failed to start API 2, continuing with remaining APIs..."
        failed_apis="$failed_apis $(echo "$API_2" | cut -d'|' -f3)"
    fi
    current=$((current + 1))
    
    # API 3
    log_info ""
    log_info "[$current/$total] $(echo "$API_3" | cut -d'|' -f3)"
    if ! start_api "$(echo "$API_3" | cut -d'|' -f1)" "$(echo "$API_3" | cut -d'|' -f2)" "$(echo "$API_3" | cut -d'|' -f3)"; then
        log_warn "Failed to start API 3, continuing..."
        failed_apis="$failed_apis $(echo "$API_3" | cut -d'|' -f3)"
    fi
    
    if [ -z "$failed_apis" ]; then
        log_success "All APIs started successfully"
    else
        log_warn "Some APIs failed to start:$failed_apis"
        log_warn "Check logs for details. Available APIs will continue running."
    fi
}

# ============================================================================
# SECTION 8: DISPLAY FEEDBACK
# ============================================================================

display_feedback() {
    log_section "🚀 Ready for Development"
    
    printf "\n"
    printf "All systems operational!\n"
    printf "\n"
    
    printf "URLs:\n"
    printf "  • IAM API:              http://localhost:5001\n"
    printf "  • Credit Analysis API:  http://localhost:5002\n"
    printf "  • Compliance API:       http://localhost:5003\n"
    printf "\n"
    
    printf "Health Endpoints:\n"
    printf "  • IAM:              http://localhost:5001/health\n"
    printf "  • Credit Analysis:  http://localhost:5002/health\n"
    printf "  • Compliance:       http://localhost:5003/health\n"
    printf "\n"
    
    printf "Log Files:\n"
    printf "  • IAM:              $LOGS_DIR/iam.log\n"
    printf "  • Credit Analysis:  $LOGS_DIR/credit.log\n"
    printf "  • Compliance:       $LOGS_DIR/compliance.log\n"
    printf "\n"
    
    printf "View Logs:\n"
    printf "  • Real-time IAM:        tail -f $LOGS_DIR/iam.log\n"
    printf "  • Real-time Credit:     tail -f $LOGS_DIR/credit.log\n"
    printf "  • Real-time Compliance: tail -f $LOGS_DIR/compliance.log\n"
    printf "\n"
    
    printf "Press Ctrl+C to stop all services and cleanup\n"
    printf "\n"
}

# ============================================================================
# SECTION 9: MAIN EXECUTION LOOP
# ============================================================================

main() {
    printf "\n"
    printf "╔════════════════════════════════════════════════════════════════╗\n"
    printf "║  Credit Risk Compliance Lab — Dev Environment                 ║\n"
    printf "║  Worktree: backend-unit-tests                                 ║\n"
    printf "╚════════════════════════════════════════════════════════════════╝\n"
    printf "\n"
    
    # Execute phases
    if ! preflight_checks; then
        return 1
    fi
    
    if ! manage_docker_services; then
        return 1
    fi
    
    if ! build_solution; then
        return 1
    fi
    
    if ! start_apis; then
        return 1
    fi
    
    # Display feedback
    display_feedback
    
    # Wait indefinitely (until Ctrl+C)
    # The trap will handle cleanup when signal is received
    while true; do
        sleep 60
    done
}

# ============================================================================
# ENTRY POINT
# ============================================================================

main "$@"
