#!/bin/bash

#############################################################################
#  START ALL SERVICES - Credit Risk Compliance System
#
#  This script starts both infrastructure (Docker) and application services.
#  Usage: ./start-all-services.sh [option]
#
#  Options:
#    (no args)     Start infrastructure + services (auto-detect mode)
#    full          Start infrastructure + services (complete setup)
#    infra-only    Start only Docker infrastructure
#    services-only Start only application services (assumes infra running)
#    tmux          Start services with tmux (interactive windows)
#    background    Start services in background (logs to /tmp)
#    help          Show this help
#
#############################################################################

set -e

BLUE='\033[0;34m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
CYAN='\033[0;36m'
NC='\033[0m'

PROJECT_DIR="$(cd "$(dirname "$0")" && pwd)"
cd "$PROJECT_DIR"

# Default mode
MODE="${1:-full}"

# Helper functions
print_header() {
    echo -e "${BLUE}════════════════════════════════════════════════════════════════${NC}"
    echo -e "${BLUE}  $1${NC}"
    echo -e "${BLUE}════════════════════════════════════════════════════════════════${NC}"
}

print_success() {
    echo -e "${GREEN}✅ $1${NC}"
}

print_info() {
    echo -e "${YELLOW}→${NC} $1"
}

print_section() {
    echo -e "${YELLOW}$1${NC}"
}

print_error() {
    echo -e "${RED}❌ $1${NC}"
}

show_help() {
    cat << 'EOF'

╔════════════════════════════════════════════════════════════════════════════╗
║          START ALL SERVICES - Credit Risk Compliance System               ║
╚════════════════════════════════════════════════════════════════════════════╝

USAGE:
  ./start-all-services.sh [option]

OPTIONS:
  (no args)     Start infrastructure + services (auto-detect best mode)
  full          Start infrastructure + services (complete setup)
  infra-only    Start only Docker infrastructure
  services-only Start only application services (assumes infra running)
  tmux          Start services with tmux (interactive windows)
  background    Start services in background (logs to /tmp)
  help          Show this help

EXAMPLES:
  ./start-all-services.sh
  ./start-all-services.sh full
  ./start-all-services.sh infra-only
  ./start-all-services.sh services-only
  ./start-all-services.sh background

SERVICES STARTED:
  1. Bureau Mock Service           (port 8081)
  2. IAM API                       (port 5000)
  3. Credit Analysis API           (port 5001)
  4. Compliance API                (port 5002)
  5. Operations Server (SignalR)   (port 5003)
  6. Credit Analysis Worker
  7. Compliance Worker

INFRASTRUCTURE (Docker):
  ✓ PostgreSQL    (5432)
  ✓ RabbitMQ      (5672, 15672)
  ✓ Redis         (6379)
  ✓ Keycloak      (8080)

AFTER STARTING:
  Test services:
    curl http://localhost:5001/health
    curl http://localhost:5002/health
    curl http://localhost:5003/health

  View logs (background mode):
    tail -f /tmp/bureau.log
    tail -f /tmp/credit.log
    tail -f /tmp/compliance.log
    tail -f /tmp/operations.log
    etc.

  Stop all services:
    killall dotnet

  Stop infrastructure:
    docker-compose down

EOF
}

#############################################################################
# INFRASTRUCTURE
#############################################################################
start_infrastructure() {
    print_header "Starting Docker Infrastructure"
    
    # Check if docker-compose is available
    if ! command -v docker-compose &> /dev/null && ! command -v docker &> /dev/null; then
        print_error "docker-compose or docker not found. Please install Docker."
        return 1
    fi
    
    print_info "Starting containers (PostgreSQL, RabbitMQ, Redis, Keycloak)..."
    echo ""
    
    # Determine which docker compose command to use
    if command -v docker-compose &> /dev/null; then
        COMPOSE_CMD="docker-compose"
    else
        COMPOSE_CMD="docker compose"
    fi
    
    # Start containers in detached mode
    $COMPOSE_CMD up -d
    
    sleep 5
    
    # Check if containers are running
    print_info "Verifying containers are ready..."
    echo ""
    
    # PostgreSQL
    if timeout 30 bash -c "until pg_isready -h localhost -p 5432 2>/dev/null; do sleep 1; done" 2>/dev/null; then
        print_success "PostgreSQL is ready (5432)"
    else
        print_info "PostgreSQL starting... (may take a few seconds)"
    fi
    
    # RabbitMQ
    if timeout 30 bash -c "until curl -s http://localhost:15672/api/aliveness-test >/dev/null 2>&1; do sleep 1; done" 2>/dev/null; then
        print_success "RabbitMQ is ready (5672, 15672)"
    else
        print_info "RabbitMQ starting... (may take a few seconds)"
    fi
    
    # Redis
    if timeout 30 bash -c "until redis-cli -p 6379 ping >/dev/null 2>&1; do sleep 1; done" 2>/dev/null; then
        print_success "Redis is ready (6379)"
    else
        print_info "Redis starting... (may take a few seconds)"
    fi
    
    print_success "Docker infrastructure started!"
    echo ""
}

#############################################################################
# MODE: TMUX (Interactive)
#############################################################################
start_services_tmux() {
    print_header "Starting Services with TMUX (Interactive Mode)"
    
    SESSION="creditrisk"
    
    # Kill existing session
    tmux kill-session -t $SESSION 2>/dev/null || true
    sleep 1
    
    # Create new session with first window
    tmux new-session -d -s $SESSION -x 250 -y 50
    
    print_info "Creating windows and starting services..."
    echo ""
    
    # Window 0: Bureau Mock Service
    print_info "Bureau Mock Service (port 8081)"
    tmux send-keys -t $SESSION "cd $PROJECT_DIR && dotnet run -p src/external/CreditRisk.BureauMock.Service" C-m
    tmux rename-window -t $SESSION:0 "bureau"
    sleep 3
    
    # Window 1: IAM API
    print_info "IAM API (port 5000)"
    tmux new-window -t $SESSION -n "iam"
    tmux send-keys -t $SESSION:iam "cd $PROJECT_DIR && dotnet run -p src/modules/iam/CreditRisk.IAM.Api" C-m
    sleep 3
    
    # Window 2: Credit Analysis API
    print_info "Credit Analysis API (port 5001)"
    tmux new-window -t $SESSION -n "credit"
    tmux send-keys -t $SESSION:credit "cd $PROJECT_DIR && dotnet run -p src/modules/credit-analysis/CreditRisk.CreditAnalysis.Api" C-m
    sleep 3
    
    # Window 3: Compliance API
    print_info "Compliance API (port 5002)"
    tmux new-window -t $SESSION -n "compliance"
    tmux send-keys -t $SESSION:compliance "cd $PROJECT_DIR && dotnet run -p src/modules/compliance/CreditRisk.Compliance.Api" C-m
    sleep 3
    
    # Window 4: Operations Server
    print_info "Operations Server (port 5003)"
    tmux new-window -t $SESSION -n "operations"
    tmux send-keys -t $SESSION:operations "cd $PROJECT_DIR && dotnet run -p src/servers/CreditRisk.Operations.Server" C-m
    sleep 3
    
    # Window 5: Credit Analysis Worker
    print_info "Credit Analysis Worker"
    tmux new-window -t $SESSION -n "ca-worker"
    tmux send-keys -t $SESSION:ca-worker "cd $PROJECT_DIR && dotnet run -p src/workers/CreditRisk.CreditAnalysis.Worker" C-m
    sleep 2
    
    # Window 6: Compliance Worker
    print_info "Compliance Worker"
    tmux new-window -t $SESSION -n "comp-worker"
    tmux send-keys -t $SESSION:comp-worker "cd $PROJECT_DIR && dotnet run -p src/workers/CreditRisk.Compliance.Worker" C-m
    
    echo ""
    print_header "TMUX Session Ready!"
    echo ""
    print_success "Attach to session: tmux attach -t $SESSION"
    echo ""
    print_section "Windows available:"
    echo "  Ctrl+B then 0: Bureau Mock Service"
    echo "  Ctrl+B then 1: IAM API"
    echo "  Ctrl+B then 2: Credit Analysis API"
    echo "  Ctrl+B then 3: Compliance API"
    echo "  Ctrl+B then 4: Operations Server"
    echo "  Ctrl+B then 5: Credit Analysis Worker"
    echo "  Ctrl+B then 6: Compliance Worker"
    echo ""
    print_section "Other tmux commands:"
    echo "  Ctrl+B then l: Last window"
    echo "  Ctrl+B then n: Next window"
    echo "  Ctrl+B then p: Previous window"
    echo "  Ctrl+B then ?: Help"
    echo ""
    print_section "Test services (after they start):"
    echo "  curl http://localhost:5001/health"
    echo "  curl http://localhost:5002/health"
    echo "  curl http://localhost:5003/health"
    echo ""
    print_section "Stop all:"
    echo "  killall dotnet"
    echo ""
}

#############################################################################
# MODE: BACKGROUND (Logs to /tmp)
#############################################################################
start_services_background() {
    print_header "Starting Services in BACKGROUND Mode"
    
    # Kill any existing dotnet processes
    killall dotnet 2>/dev/null || true
    sleep 1
    
    print_info "Starting services (background processes)..."
    echo ""
    
    # Bureau Mock Service
    print_info "Bureau Mock Service (port 8081)"
    dotnet run -p src/external/CreditRisk.BureauMock.Service > /tmp/bureau.log 2>&1 &
    sleep 2
    
    # IAM API
    print_info "IAM API (port 5000)"
    dotnet run -p src/modules/iam/CreditRisk.IAM.Api > /tmp/iam.log 2>&1 &
    sleep 2
    
    # Credit Analysis API
    print_info "Credit Analysis API (port 5001)"
    dotnet run -p src/modules/credit-analysis/CreditRisk.CreditAnalysis.Api > /tmp/credit.log 2>&1 &
    sleep 2
    
    # Compliance API
    print_info "Compliance API (port 5002)"
    dotnet run -p src/modules/compliance/CreditRisk.Compliance.Api > /tmp/compliance.log 2>&1 &
    sleep 2
    
    # Operations Server
    print_info "Operations Server (port 5003)"
    dotnet run -p src/servers/CreditRisk.Operations.Server > /tmp/operations.log 2>&1 &
    sleep 2
    
    # Credit Analysis Worker
    print_info "Credit Analysis Worker"
    dotnet run -p src/workers/CreditRisk.CreditAnalysis.Worker > /tmp/ca-worker.log 2>&1 &
    sleep 1
    
    # Compliance Worker
    print_info "Compliance Worker"
    dotnet run -p src/workers/CreditRisk.Compliance.Worker > /tmp/comp-worker.log 2>&1 &
    
    echo ""
    print_header "Background Services Started!"
    echo ""
    print_section "Service Endpoints:"
    echo "  Bureau Mock        : http://localhost:8081/health"
    echo "  IAM API            : http://localhost:5000/health"
    echo "  Credit Analysis    : http://localhost:5001/health"
    echo "  Compliance API     : http://localhost:5002/health"
    echo "  Operations Server  : http://localhost:5003"
    echo ""
    print_section "View logs:"
    echo "  tail -f /tmp/bureau.log"
    echo "  tail -f /tmp/iam.log"
    echo "  tail -f /tmp/credit.log"
    echo "  tail -f /tmp/compliance.log"
    echo "  tail -f /tmp/operations.log"
    echo "  tail -f /tmp/ca-worker.log"
    echo "  tail -f /tmp/comp-worker.log"
    echo ""
    print_section "Test services:"
    echo "  curl http://localhost:5001/health"
    echo "  curl http://localhost:5002/health"
    echo "  curl http://localhost:5003/health"
    echo ""
    print_section "Stop all:"
    echo "  killall dotnet"
    echo ""
}

#############################################################################
# MAIN
#############################################################################

case "$MODE" in
    full|'')
        # Start infrastructure first
        start_infrastructure
        echo ""
        sleep 10
        
        # Then start services (auto-detect best mode)
        if command -v tmux &> /dev/null; then
            start_services_tmux
        else
            start_services_background
        fi
        ;;
    infra-only)
        start_infrastructure
        ;;
    services-only)
        if command -v tmux &> /dev/null; then
            start_services_tmux
        else
            start_services_background
        fi
        ;;
    tmux)
        start_services_tmux
        ;;
    background)
        start_services_background
        ;;
    help|--help|-h)
        show_help
        ;;
    *)
        print_error "Invalid option: $MODE"
        echo ""
        show_help
        exit 1
        ;;
esac
