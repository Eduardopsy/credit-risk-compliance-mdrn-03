#!/usr/bin/env bash

# Quick Start Script for Credit Risk Compliance System
# This script provides easy commands for common tasks

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

# Print menu
show_menu() {
    echo -e "\n${BLUE}╔════════════════════════════════════════════════════════════════╗${NC}"
    echo -e "${BLUE}║  Credit Risk Compliance System - Quick Start Menu              ║${NC}"
    echo -e "${BLUE}╚════════════════════════════════════════════════════════════════╝${NC}"
    echo ""
    echo -e "${GREEN}1)${NC} Start infrastructure (Docker containers)"
    echo -e "${GREEN}2)${NC} Show service endpoints"
    echo -e "${GREEN}3)${NC} Run unit tests (29 tests)"
    echo -e "${GREEN}4)${NC} Run integration tests (7 tests)"
    echo -e "${GREEN}5)${NC} Build solution"
    echo -e "${GREEN}6)${NC} View documentation index"
    echo -e "${GREEN}7)${NC} Check service health"
    echo -e "${GREEN}8)${NC} Stop infrastructure"
    echo -e "${GREEN}9)${NC} Full cleanup"
    echo -e "${GREEN}0)${NC} Exit"
    echo ""
    echo -n "Select option: "
}

# Function to start infrastructure
start_infrastructure() {
    echo -e "\n${BLUE}Starting infrastructure services...${NC}"
    bash run-services.sh
}

# Function to show endpoints
show_endpoints() {
    echo -e "\n${BLUE}════════════════════════════════════════════════════════════════${NC}"
    echo -e "${BLUE}Service Endpoints${NC}"
    echo -e "${BLUE}════════════════════════════════════════════════════════════════${NC}"
    echo ""
    echo -e "${GREEN}Bureau Mock Service${NC}        : http://localhost:8081"
    echo -e "${GREEN}IAM API${NC}                    : http://localhost:5000"
    echo -e "${GREEN}Credit Analysis API${NC}        : http://localhost:5001"
    echo -e "${GREEN}Compliance API${NC}             : http://localhost:5002"
    echo -e "${GREEN}Operations Server (SignalR)${NC} : http://localhost:5003"
    echo -e "${GREEN}Keycloak${NC}                   : http://localhost:8080"
    echo -e "${GREEN}RabbitMQ Management${NC}        : http://localhost:15672"
    echo -e "${GREEN}PostgreSQL${NC}                 : localhost:5432"
    echo -e "${GREEN}Redis${NC}                      : localhost:6379"
    echo ""
    echo -e "${YELLOW}Credentials:${NC}"
    echo -e "  RabbitMQ: guest / guest"
    echo -e "  PostgreSQL: postgres / postgres"
    echo -e "  Keycloak: admin / admin"
    echo ""
}

# Function to run unit tests
run_unit_tests() {
    echo -e "\n${BLUE}Running 29 unit tests...${NC}"
    dotnet test --filter "FullyQualifiedName!~Integration" -c Debug
    echo -e "\n${GREEN}✅ Unit tests completed${NC}"
}

# Function to run integration tests
run_integration_tests() {
    echo -e "\n${BLUE}Running 7 integration tests...${NC}"
    dotnet test tests/integration -c Debug --test-timeout 60000
    echo -e "\n${GREEN}✅ Integration tests completed${NC}"
}

# Function to build solution
build_solution() {
    echo -e "\n${BLUE}Building solution...${NC}"
    dotnet build -c Debug
    echo -e "\n${GREEN}✅ Build completed${NC}"
}

# Function to show documentation
show_documentation() {
    echo -e "\n${BLUE}════════════════════════════════════════════════════════════════${NC}"
    echo -e "${BLUE}Documentation Files${NC}"
    echo -e "${BLUE}════════════════════════════════════════════════════════════════${NC}"
    echo ""
    echo -e "${GREEN}📖 README.md${NC}"
    echo -e "  Project overview, quick start, features"
    echo ""
    echo -e "${GREEN}📡 API_REFERENCE.md${NC}"
    echo -e "  Complete REST/WebSocket API documentation"
    echo ""
    echo -e "${GREEN}🏗️ ARCHITECTURE.md${NC}"
    echo -e "  System design, data flows, topology diagrams"
    echo ""
    echo -e "${GREEN}🐳 DEPLOYMENT.md${NC}"
    echo -e "  Deployment strategies (Docker, Kubernetes)"
    echo ""
    echo -e "${GREEN}🧪 TESTING_GUIDE.md${NC}"
    echo -e "  Testing best practices and strategies"
    echo ""
    echo -e "${GREEN}⚡ AOT_COMPILATION.md${NC}"
    echo -e "  Native compilation and optimization"
    echo ""
    echo -e "${GREEN}📋 START_SERVICES.md${NC}"
    echo -e "  Detailed service startup guide"
    echo ""
    echo -e "${YELLOW}Open with:${NC} cat docs/<filename>"
    echo ""
}

# Function to check health
check_health() {
    echo -e "\n${BLUE}Checking service health...${NC}"
    echo ""
    
    for port in 5000 5001 5002 5003; do
        service_name=$([ $port -eq 5000 ] && echo "IAM" || [ $port -eq 5001 ] && echo "Credit Analysis" || [ $port -eq 5002 ] && echo "Compliance" || echo "Operations")
        if curl -s http://localhost:$port/health >/dev/null 2>&1; then
            echo -e "${GREEN}✓${NC} Port $port ($service_name) - ${GREEN}OK${NC}"
        else
            echo -e "${RED}✗${NC} Port $port ($service_name) - ${RED}Not responding${NC}"
        fi
    done
    
    echo ""
    echo -e "${YELLOW}Docker containers:${NC}"
    docker-compose ps 2>/dev/null || echo "Docker Compose not running"
    echo ""
}

# Function to stop infrastructure
stop_infrastructure() {
    echo -e "\n${YELLOW}Stopping infrastructure services...${NC}"
    docker-compose down
    echo -e "${GREEN}✅ Infrastructure stopped${NC}"
}

# Function to cleanup
cleanup() {
    echo -e "\n${YELLOW}Performing full cleanup...${NC}"
    echo -e "  Stopping infrastructure..."
    docker-compose down -v 2>/dev/null || true
    echo -e "  Cleaning build artifacts..."
    find . -type d -name "bin" -o -name "obj" | xargs rm -rf
    echo -e "${GREEN}✅ Cleanup completed${NC}"
}

# Main loop
while true; do
    show_menu
    read -r option
    
    case $option in
        1) start_infrastructure ;;
        2) show_endpoints ;;
        3) run_unit_tests ;;
        4) run_integration_tests ;;
        5) build_solution ;;
        6) show_documentation ;;
        7) check_health ;;
        8) stop_infrastructure ;;
        9) cleanup ;;
        0) echo -e "\n${GREEN}Goodbye!${NC}"; exit 0 ;;
        *) echo -e "${RED}Invalid option${NC}" ;;
    esac
done
