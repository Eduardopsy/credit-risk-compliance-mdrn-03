#!/usr/bin/env bash
set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}════════════════════════════════════════════════════════════════${NC}"
echo -e "${BLUE}  Credit Risk Compliance System - Service Startup Script${NC}"
echo -e "${BLUE}════════════════════════════════════════════════════════════════${NC}"
echo ""

# Function to handle cleanup on exit
cleanup() {
    echo -e "\n${YELLOW}Stopping services...${NC}"
    # Kill all background processes
    jobs -p | xargs -r kill 2>/dev/null || true
    echo -e "${GREEN}Services stopped.${NC}"
}

trap cleanup EXIT

echo -e "${BLUE}Step 1: Starting infrastructure services...${NC}"
echo -e "  ${YELLOW}• Stopping any existing containers...${NC}"
docker-compose down -v 2>/dev/null || true

echo -e "  ${YELLOW}• Starting PostgreSQL, Redis, RabbitMQ, Keycloak...${NC}"
docker-compose up -d

echo -e "${BLUE}Step 2: Waiting for services to be ready...${NC}"
echo -e "  ${YELLOW}• Checking PostgreSQL...${NC}"
until docker exec crcl-postgres pg_isready -U postgres -d creditrisk >/dev/null 2>&1; do
  echo -n "."
  sleep 2
done
echo -e "  ${GREEN}✓ PostgreSQL ready${NC}"

echo -e "  ${YELLOW}• Checking RabbitMQ...${NC}"
until docker exec crcl-rabbitmq rabbitmq-diagnostics ping >/dev/null 2>&1; do
  echo -n "."
  sleep 2
done
echo -e "  ${GREEN}✓ RabbitMQ ready${NC}"

echo -e "  ${YELLOW}• Checking Redis...${NC}"
until docker exec crcl-redis redis-cli ping >/dev/null 2>&1; do
  echo -n "."
  sleep 2
done
echo -e "  ${GREEN}✓ Redis ready${NC}"

echo ""
echo -e "${GREEN}✅ Infrastructure services started successfully!${NC}"
echo ""

echo -e "${BLUE}Step 3: Building applications...${NC}"
dotnet build -c Debug >/dev/null 2>&1
echo -e "${GREEN}✅ Build completed successfully${NC}"
echo ""

echo -e "${BLUE}════════════════════════════════════════════════════════════════${NC}"
echo -e "${BLUE}  Ready to start applications${NC}"
echo -e "${BLUE}════════════════════════════════════════════════════════════════${NC}"
echo ""

echo -e "${YELLOW}Run the following commands in separate terminals:${NC}"
echo ""
echo -e "${GREEN}Terminal 1 - Bureau Mock Service (Port 8081):${NC}"
echo -e "  ${BLUE}dotnet run -p src/external/CreditRisk.BureauMock.Service${NC}"
echo ""
echo -e "${GREEN}Terminal 2 - IAM API (Port 5000):${NC}"
echo -e "  ${BLUE}dotnet run -p src/modules/iam/CreditRisk.IAM.Api${NC}"
echo ""
echo -e "${GREEN}Terminal 3 - Credit Analysis API (Port 5001):${NC}"
echo -e "  ${BLUE}dotnet run -p src/modules/credit-analysis/CreditRisk.CreditAnalysis.Api${NC}"
echo ""
echo -e "${GREEN}Terminal 4 - Compliance API (Port 5002):${NC}"
echo -e "  ${BLUE}dotnet run -p src/modules/compliance/CreditRisk.Compliance.Api${NC}"
echo ""
echo -e "${GREEN}Terminal 5 - Operations Server (Port 5003, SignalR):${NC}"
echo -e "  ${BLUE}dotnet run -p src/servers/CreditRisk.Operations.Server${NC}"
echo ""
echo -e "${GREEN}Terminal 6 - Credit Analysis Worker (Background):${NC}"
echo -e "  ${BLUE}dotnet run -p src/workers/CreditRisk.CreditAnalysis.Worker${NC}"
echo ""
echo -e "${GREEN}Terminal 7 - Compliance Worker (Background):${NC}"
echo -e "  ${BLUE}dotnet run -p src/workers/CreditRisk.Compliance.Worker${NC}"
echo ""

echo -e "${BLUE}════════════════════════════════════════════════════════════════${NC}"
echo -e "${BLUE}  Service Endpoints${NC}"
echo -e "${BLUE}════════════════════════════════════════════════════════════════${NC}"
echo ""
echo -e "  ${GREEN}Bureau Mock Service${NC}        : http://localhost:8081"
echo -e "  ${GREEN}IAM API${NC}                    : http://localhost:5000"
echo -e "  ${GREEN}Credit Analysis API${NC}        : http://localhost:5001"
echo -e "  ${GREEN}Compliance API${NC}             : http://localhost:5002"
echo -e "  ${GREEN}Operations Server${NC}          : http://localhost:5003"
echo -e "  ${GREEN}Keycloak${NC}                   : http://localhost:8080"
echo -e "  ${GREEN}RabbitMQ Management${NC}        : http://localhost:15672 (guest/guest)"
echo -e "  ${GREEN}PostgreSQL${NC}                 : localhost:5432 (postgres/postgres)"
echo -e "  ${GREEN}Redis${NC}                      : localhost:6379"
echo ""

echo -e "${BLUE}════════════════════════════════════════════════════════════════${NC}"
echo -e "${BLUE}  Running Tests${NC}"
echo -e "${BLUE}════════════════════════════════════════════════════════════════${NC}"
echo ""
echo -e "  ${YELLOW}Unit Tests Only:${NC}"
echo -e "    ${BLUE}dotnet test --filter \"FullyQualifiedName!~Integration\" -c Debug${NC}"
echo ""
echo -e "  ${YELLOW}Integration Tests:${NC}"
echo -e "    ${BLUE}dotnet test tests/integration -c Debug --test-timeout 60000${NC}"
echo ""
echo -e "  ${YELLOW}All Tests:${NC}"
echo -e "    ${BLUE}dotnet test${NC}"
echo ""

echo -e "${BLUE}════════════════════════════════════════════════════════════════${NC}"
echo -e "${GREEN}✅ All infrastructure ready! Use commands above to start apps.${NC}"
echo -e "${BLUE}════════════════════════════════════════════════════════════════${NC}"
echo ""
echo -e "${YELLOW}Press Ctrl+C to stop this script and cleanup resources.${NC}"
echo ""

# Keep script running until interrupted
wait
