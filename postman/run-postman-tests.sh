#!/bin/bash

###############################################################################
# Postman Collection Runner Script
#
# This script helps run the Postman collection using Newman (CLI tool)
# Supports multiple output formats and test execution modes
#
# Usage:
#   ./run-postman-tests.sh [options]
#
# Options:
#   -h, --help              Show this help message
#   -i, --install          Install Newman if not present
#   -r, --run              Run collection tests
#   -o, --output FORMAT    Output format: html, json, cli (default: cli)
#   -e, --environment FILE Use custom environment file
#   -c, --collection FILE  Use custom collection file
#   -v, --verbose          Show verbose output
###############################################################################

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Script directory
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

# Default values
COLLECTION_FILE="$SCRIPT_DIR/Credit-Risk-Compliance-Collection.postman_collection.json"
ENVIRONMENT_FILE="$SCRIPT_DIR/Credit-Risk-Compliance-Environment.postman_environment.json"
OUTPUT_FORMAT="cli"
VERBOSE=false
INSTALL_NEWMAN=false
RUN_TESTS=false

# Functions
print_header() {
    echo -e "${BLUE}========================================${NC}"
    echo -e "${BLUE}$1${NC}"
    echo -e "${BLUE}========================================${NC}"
}

print_success() {
    echo -e "${GREEN}✅ $1${NC}"
}

print_error() {
    echo -e "${RED}❌ $1${NC}"
}

print_warning() {
    echo -e "${YELLOW}⚠️  $1${NC}"
}

print_info() {
    echo -e "${BLUE}ℹ️  $1${NC}"
}

show_help() {
    cat << EOF
Postman Collection Runner

Usage: $0 [options]

Options:
    -h, --help              Show this help message
    -i, --install          Install Newman if not present
    -r, --run              Run collection tests
    -o, --output FORMAT    Output format: html, json, cli (default: cli)
    -e, --environment FILE Use custom environment file
    -c, --collection FILE  Use custom collection file
    -v, --verbose          Show verbose output

Examples:
    # Check Newman installation
    $0 --help

    # Install Newman
    $0 --install

    # Run tests with CLI output
    $0 --run

    # Run tests with HTML report
    $0 --run --output html

    # Run with custom collection
    $0 --run -c my-collection.json

EOF
}

check_newman() {
    if ! command -v newman &> /dev/null; then
        print_error "Newman is not installed"
        echo ""
        print_info "Install Newman with:"
        echo "  npm install -g newman"
        echo ""
        print_info "Or run this script with --install flag:"
        echo "  $0 --install"
        return 1
    fi
    print_success "Newman is installed"
    newman --version
    return 0
}

install_newman() {
    print_header "Installing Newman"

    if ! command -v npm &> /dev/null; then
        print_error "npm is not installed"
        echo "Install Node.js from https://nodejs.org/"
        return 1
    fi

    print_info "Installing Newman globally..."
    npm install -g newman

    if check_newman; then
        print_success "Newman installed successfully"
        return 0
    else
        print_error "Failed to install Newman"
        return 1
    fi
}

validate_files() {
    print_header "Validating Files"

    if [[ ! -f "$COLLECTION_FILE" ]]; then
        print_error "Collection file not found: $COLLECTION_FILE"
        return 1
    fi
    print_success "Collection file found"

    if [[ ! -f "$ENVIRONMENT_FILE" ]]; then
        print_error "Environment file not found: $ENVIRONMENT_FILE"
        return 1
    fi
    print_success "Environment file found"

    # Validate JSON
    if python3 -m json.tool "$COLLECTION_FILE" > /dev/null 2>&1; then
        print_success "Collection JSON is valid"
    else
        print_error "Collection JSON is invalid"
        return 1
    fi

    if python3 -m json.tool "$ENVIRONMENT_FILE" > /dev/null 2>&1; then
        print_success "Environment JSON is valid"
    else
        print_error "Environment JSON is invalid"
        return 1
    fi

    return 0
}

check_services() {
    print_header "Checking Service Health"

    local services=(
        "Bureau Mock:8081"
        "IAM:5000"
        "Credit Analysis:5001"
        "Compliance:5002"
        "Operations:5003"
    )

    local healthy=0
    local total=${#services[@]}

    for service in "${services[@]}"; do
        IFS=':' read -r name port <<< "$service"
        if timeout 2 curl -s http://localhost:$port/health > /dev/null 2>&1; then
            print_success "$name ($port) is healthy"
            ((healthy++))
        else
            print_warning "$name ($port) is not responding"
        fi
    done

    echo ""
    print_info "$healthy/$total services are responding"

    if [[ $healthy -lt $total ]]; then
        print_warning "Some services are not responding. Start them with: ./start-all-services.sh"
    fi
}

run_collection() {
    print_header "Running Postman Collection"

    if [[ "$OUTPUT_FORMAT" == "html" ]]; then
        local report_file="postman-report-$(date +%Y%m%d_%H%M%S).html"
        print_info "Running tests with HTML report output..."
        print_info "Report will be saved to: $report_file"

        if [[ "$VERBOSE" == "true" ]]; then
            newman run "$COLLECTION_FILE" \
                -e "$ENVIRONMENT_FILE" \
                -r html --reporter-html-export "$report_file" \
                -v
        else
            newman run "$COLLECTION_FILE" \
                -e "$ENVIRONMENT_FILE" \
                -r html --reporter-html-export "$report_file"
        fi

        local result=$?

        if [[ $result -eq 0 ]]; then
            print_success "Tests completed successfully"
            print_info "Report saved to: $(pwd)/$report_file"
        else
            print_error "Tests failed"
        fi

        return $result

    elif [[ "$OUTPUT_FORMAT" == "json" ]]; then
        local report_file="postman-report-$(date +%Y%m%d_%H%M%S).json"
        print_info "Running tests with JSON report output..."
        print_info "Report will be saved to: $report_file"

        if [[ "$VERBOSE" == "true" ]]; then
            newman run "$COLLECTION_FILE" \
                -e "$ENVIRONMENT_FILE" \
                -r json --reporter-json-export "$report_file" \
                -v
        else
            newman run "$COLLECTION_FILE" \
                -e "$ENVIRONMENT_FILE" \
                -r json --reporter-json-export "$report_file"
        fi

        local result=$?

        if [[ $result -eq 0 ]]; then
            print_success "Tests completed successfully"
            print_info "Report saved to: $(pwd)/$report_file"
        else
            print_error "Tests failed"
        fi

        return $result

    else  # CLI output (default)
        print_info "Running tests with CLI output..."

        if [[ "$VERBOSE" == "true" ]]; then
            newman run "$COLLECTION_FILE" \
                -e "$ENVIRONMENT_FILE" \
                -v
        else
            newman run "$COLLECTION_FILE" \
                -e "$ENVIRONMENT_FILE"
        fi

        local result=$?

        if [[ $result -eq 0 ]]; then
            print_success "All tests passed"
        else
            print_error "Some tests failed"
        fi

        return $result
    fi
}

main() {
    if [[ $# -eq 0 ]]; then
        show_help
        return 0
    fi

    # Parse arguments
    while [[ $# -gt 0 ]]; do
        case $1 in
            -h|--help)
                show_help
                return 0
                ;;
            -i|--install)
                install_newman
                return $?
                ;;
            -r|--run)
                RUN_TESTS=true
                shift
                ;;
            -o|--output)
                OUTPUT_FORMAT="$2"
                shift 2
                ;;
            -e|--environment)
                ENVIRONMENT_FILE="$2"
                shift 2
                ;;
            -c|--collection)
                COLLECTION_FILE="$2"
                shift 2
                ;;
            -v|--verbose)
                VERBOSE=true
                shift
                ;;
            *)
                print_error "Unknown option: $1"
                show_help
                return 1
                ;;
        esac
    done

    # Main logic
    if ! check_newman; then
        print_error "Newman is required to run tests"
        print_info "Install with: npm install -g newman"
        return 1
    fi

    if ! validate_files; then
        return 1
    fi

    if [[ "$RUN_TESTS" == "true" ]]; then
        check_services
        echo ""
        run_collection
        return $?
    else
        print_header "Postman Collection Information"
        print_info "Collection: $COLLECTION_FILE"
        print_info "Environment: $ENVIRONMENT_FILE"
        echo ""
        print_info "To run tests, use: $0 --run [--output format]"
    fi

    return 0
}

# Run main function
main "$@"
