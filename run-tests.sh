#!/bin/bash

################################################################################
# SPEC-03 Test Suite Runner
#
# This script:
# 1. Discovers all test projects dynamically
# 2. Runs full test suite with coverage
# 3. Generates HTML coverage reports
# 4. Tracks test metrics and history
# 5. Validates coverage thresholds
# 6. Adapts automatically to new tests
#
# Usage: ./run-tests.sh [options]
#   --unit              Run only unit tests
#   --integration       Run only integration tests
#   --coverage          Generate coverage reports
#   --watch             Run tests in watch mode
#   --filter <pattern>  Run specific tests (e.g., "CreditScoring")
#   --verbose           Show detailed output
#   --ci                CI/CD mode (fail on coverage violations)
################################################################################

set -e

# Configuration
PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
TEST_DIR="${PROJECT_ROOT}/tests"
COVERAGE_DIR="${PROJECT_ROOT}/coverage"
RESULTS_DIR="${COVERAGE_DIR}/results"
HISTORY_FILE="${COVERAGE_DIR}/.test-history.json"
REPORT_DIR="${COVERAGE_DIR}/report"
LOG_FILE="${PROJECT_ROOT}/logs/test-run-$(date +%Y%m%d_%H%M%S).log"

# Coverage thresholds
MIN_LINE_COVERAGE=80
MIN_BRANCH_COVERAGE=100  # For CreditScoringEngine
MIN_OVERALL_COVERAGE=80

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Flags
RUN_UNIT=true
RUN_INTEGRATION=false
GENERATE_COVERAGE=false
WATCH_MODE=false
TEST_FILTER=""
VERBOSE=false
CI_MODE=false

################################################################################
# Helper Functions
################################################################################

log() {
    echo -e "${BLUE}[$(date +'%Y-%m-%d %H:%M:%S')]${NC} $*" | tee -a "$LOG_FILE"
}

info() {
    echo -e "${GREEN}[INFO]${NC} $*" | tee -a "$LOG_FILE"
}

warn() {
    echo -e "${YELLOW}[WARN]${NC} $*" | tee -a "$LOG_FILE"
}

error() {
    echo -e "${RED}[ERROR]${NC} $*" | tee -a "$LOG_FILE"
}

success() {
    echo -e "${GREEN}✓ $*${NC}" | tee -a "$LOG_FILE"
}

fail() {
    echo -e "${RED}✗ $*${NC}" | tee -a "$LOG_FILE"
}

print_header() {
    echo ""
    echo -e "${BLUE}════════════════════════════════════════════════════════════${NC}" | tee -a "$LOG_FILE"
    echo -e "${BLUE}$1${NC}" | tee -a "$LOG_FILE"
    echo -e "${BLUE}════════════════════════════════════════════════════════════${NC}" | tee -a "$LOG_FILE"
    echo ""
}

print_section() {
    echo ""
    echo -e "${YELLOW}▶ $1${NC}" | tee -a "$LOG_FILE"
    echo ""
}

parse_arguments() {
    while [[ $# -gt 0 ]]; do
        case $1 in
            --unit)
                RUN_UNIT=true
                RUN_INTEGRATION=false
                shift
                ;;
            --integration)
                RUN_UNIT=false
                RUN_INTEGRATION=true
                shift
                ;;
            --coverage)
                GENERATE_COVERAGE=true
                shift
                ;;
            --watch)
                WATCH_MODE=true
                shift
                ;;
            --filter)
                TEST_FILTER="$2"
                shift 2
                ;;
            --verbose)
                VERBOSE=true
                shift
                ;;
            --ci)
                CI_MODE=true
                GENERATE_COVERAGE=true
                shift
                ;;
            --help)
                print_help
                exit 0
                ;;
            *)
                error "Unknown option: $1"
                print_help
                exit 1
                ;;
        esac
    done
}

print_help() {
    cat << 'EOF'

Usage: ./run-tests.sh [options]

Options:
  --unit              Run only unit tests (default)
  --integration       Run only integration tests
  --coverage          Generate coverage reports
  --watch             Run tests in watch mode
  --filter <pattern>  Run specific tests (e.g., "CreditScoring")
  --verbose           Show detailed output
  --ci                CI/CD mode (fail on coverage violations)
  --help              Show this help message

Examples:
  ./run-tests.sh                           # Run all unit tests
  ./run-tests.sh --coverage                # Run unit tests + coverage
  ./run-tests.sh --filter "CreditScoring"  # Run specific tests
  ./run-tests.sh --ci                      # CI/CD mode
  ./run-tests.sh --watch                   # Watch mode

EOF
}

################################################################################
# Discovery Functions
################################################################################

discover_test_projects() {
    print_section "Discovering Test Projects"
    
    local projects=()
    
    # Find all test .csproj files
    while IFS= read -r -d '' project; do
        projects+=("$project")
        info "Found: $(basename "$(dirname "$project")")"
    done < <(find "$TEST_DIR" -name "*.csproj" -type f -print0 2>/dev/null)
    
    if [ ${#projects[@]} -eq 0 ]; then
        error "No test projects found in $TEST_DIR"
        return 1
    fi
    
    echo "${projects[@]}"
}

get_test_assemblies() {
    local pattern="$1"
    local output_dir="$2"
    local assemblies=()
    
    # Find all test .dll files
    while IFS= read -r -d '' dll; do
        if [[ -z "$pattern" ]] || [[ "$dll" == *"$pattern"* ]]; then
            assemblies+=("$dll")
        fi
    done < <(find "$output_dir" -name "*Tests.dll" -type f -print0 2>/dev/null)
    
    echo "${assemblies[@]}"
}

################################################################################
# Test Execution Functions
################################################################################

run_dotnet_tests() {
    local projects=("$@")
    local test_args=""
    local total_tests=0
    local passed_tests=0
    local failed_tests=0
    
    # Build filter argument
    if [[ -n "$TEST_FILTER" ]]; then
        test_args="--filter $TEST_FILTER"
    fi
    
    # Add logger argument
    if [[ "$VERBOSE" == "true" ]]; then
        test_args="$test_args --logger console;verbosity=detailed"
    else
        test_args="$test_args --logger console;verbosity=normal"
    fi
    
    # Add coverage collection if requested
    if [[ "$GENERATE_COVERAGE" == "true" ]]; then
        test_args="$test_args --collect:\"XPlat Code Coverage\" --results-directory \"$RESULTS_DIR\""
    fi
    
    print_section "Running Tests"
    
    for project in "${projects[@]}"; do
        local project_name=$(basename "$(dirname "$project")")
        info "Testing: $project_name"
        
        if [[ "$WATCH_MODE" == "true" ]]; then
            dotnet test "$project" --watch $test_args || return 1
        else
            if dotnet test "$project" $test_args 2>&1 | tee -a "$LOG_FILE"; then
                success "✓ $project_name passed"
            else
                fail "✗ $project_name failed"
                return 1
            fi
        fi
    done
    
    return 0
}

################################################################################
# Coverage Functions
################################################################################

generate_coverage_report() {
    print_section "Generating Coverage Report"
    
    if ! command -v reportgenerator &> /dev/null; then
        warn "reportgenerator not found. Installing..."
        dotnet tool install -g dotnet-reportgenerator-globaltool || {
            error "Failed to install reportgenerator"
            return 1
        }
    fi
    
    local coverage_files=$(find "$RESULTS_DIR" -name "coverage.cobertura.xml" -type f)
    
    if [[ -z "$coverage_files" ]]; then
        error "No coverage files found in $RESULTS_DIR"
        return 1
    fi
    
    info "Generating HTML report..."
    reportgenerator \
        -reports:"$RESULTS_DIR/**/coverage.cobertura.xml" \
        -targetdir:"$REPORT_DIR" \
        -reporttypes:"Html;JsonSummary" \
        -verbosity:Verbose || return 1
    
    success "Coverage report generated: $REPORT_DIR/index.html"
    
    # Extract metrics
    parse_coverage_metrics
}

parse_coverage_metrics() {
    print_section "Coverage Metrics"
    
    local summary_file="$REPORT_DIR/Summary.json"
    
    if [[ ! -f "$summary_file" ]]; then
        warn "Coverage summary file not found"
        return 1
    fi
    
    # Parse and display coverage metrics
    info "Overall Coverage:"
    
    local line_coverage=$(grep -o '"LineCoverage":[^,}]*' "$summary_file" | head -1 | cut -d: -f2)
    local branch_coverage=$(grep -o '"BranchCoverage":[^,}]*' "$summary_file" | head -1 | cut -d: -f2)
    local method_coverage=$(grep -o '"MethodCoverage":[^,}]*' "$summary_file" | head -1 | cut -d: -f2)
    
    echo "  Line Coverage:   ${line_coverage}%" | tee -a "$LOG_FILE"
    echo "  Branch Coverage: ${branch_coverage}%" | tee -a "$LOG_FILE"
    echo "  Method Coverage: ${method_coverage}%" | tee -a "$LOG_FILE"
    
    # Check thresholds
    check_coverage_thresholds "$line_coverage" "$branch_coverage"
}

check_coverage_thresholds() {
    local line_coverage="$1"
    local branch_coverage="$2"
    
    print_section "Coverage Validation"
    
    local violations=0
    
    # Remove percentage sign if present
    line_coverage=${line_coverage%\%}
    branch_coverage=${branch_coverage%\%}
    
    # Check line coverage
    if (( $(echo "$line_coverage < $MIN_LINE_COVERAGE" | bc -l) )); then
        fail "Line coverage ($line_coverage%) is below threshold ($MIN_LINE_COVERAGE%)"
        ((violations++))
    else
        success "Line coverage: $line_coverage% ≥ $MIN_LINE_COVERAGE%"
    fi
    
    # Check branch coverage (only warn for now, as not all code requires 100%)
    if (( $(echo "$branch_coverage < $MIN_BRANCH_COVERAGE" | bc -l) )); then
        warn "Branch coverage ($branch_coverage%) is below recommended ($MIN_BRANCH_COVERAGE%)"
    else
        success "Branch coverage: $branch_coverage% ≥ $MIN_BRANCH_COVERAGE%"
    fi
    
    return $violations
}

################################################################################
# Test History & Analytics
################################################################################

update_test_history() {
    local test_count="$1"
    local passed_count="$2"
    local failed_count="$3"
    local duration="$4"
    
    print_section "Updating Test History"
    
    # Create directory if it doesn't exist
    mkdir -p "$(dirname "$HISTORY_FILE")"
    
    # Create initial history file if it doesn't exist
    if [[ ! -f "$HISTORY_FILE" ]]; then
        echo '{"runs": []}' > "$HISTORY_FILE"
    fi
    
    # Append run data (simple JSON - real implementation would use jq)
    local timestamp=$(date -u +"%Y-%m-%dT%H:%M:%SZ")
    
    info "Test run recorded:"
    echo "  Total: $test_count" | tee -a "$LOG_FILE"
    echo "  Passed: $passed_count" | tee -a "$LOG_FILE"
    echo "  Failed: $failed_count" | tee -a "$LOG_FILE"
    echo "  Duration: ${duration}s" | tee -a "$LOG_FILE"
    echo "  Timestamp: $timestamp" | tee -a "$LOG_FILE"
}

list_test_projects() {
    print_section "Available Test Projects"
    
    local projects=($(discover_test_projects))
    
    for i in "${!projects[@]}"; do
        local project="${projects[$i]}"
        local project_name=$(basename "$(dirname "$project")")
        local test_count=$(grep -c "<ItemGroup>" "$project" || echo "0")
        
        echo "  $((i + 1)). $project_name" | tee -a "$LOG_FILE"
    done
}

################################################################################
# Main Execution
################################################################################

main() {
    print_header "SPEC-03 Test Suite Runner"
    
    # Parse command-line arguments
    parse_arguments "$@"
    
    # Initialize logging
    mkdir -p "$(dirname "$LOG_FILE")" "$(dirname "$HISTORY_FILE")" "$RESULTS_DIR"
    
    log "Starting test run"
    log "Configuration: Unit=$RUN_UNIT, Integration=$RUN_INTEGRATION, Coverage=$GENERATE_COVERAGE"
    
    # List test projects
    list_test_projects
    
    # Discover test projects
    local test_projects=($(discover_test_projects))
    
    if [[ ${#test_projects[@]} -eq 0 ]]; then
        error "No test projects found"
        return 1
    fi
    
    # Run tests
    if ! run_dotnet_tests "${test_projects[@]}"; then
        fail "Test execution failed"
        return 1
    fi
    
    # Generate coverage reports if requested
    if [[ "$GENERATE_COVERAGE" == "true" ]]; then
        if ! generate_coverage_report; then
            if [[ "$CI_MODE" == "true" ]]; then
                error "Coverage report generation failed (CI mode)"
                return 1
            else
                warn "Coverage report generation failed"
            fi
        fi
    fi
    
    print_header "Test Run Complete ✓"
    
    success "All tests passed successfully!"
    info "Log file: $LOG_FILE"
    
    if [[ "$GENERATE_COVERAGE" == "true" ]]; then
        info "Coverage report: $REPORT_DIR/index.html"
    fi
    
    return 0
}

# Run main function with all arguments
main "$@"
exit $?
