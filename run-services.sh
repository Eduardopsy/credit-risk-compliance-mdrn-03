#!/usr/bin/env bash
set -e

echo "Starting infrastructure services (PostgreSQL, Redis, RabbitMQ, Keycloak)..."
docker-compose down -v
docker-compose up -d

echo "Waiting for PostgreSQL to be ready..."
until docker exec crcl-postgres pg_isready -U crcl -d creditrisk; do
  sleep 2
done

echo "Infrastructure services started successfully!"

echo "Building back-end applications..."
dotnet build

echo "Starting IAM API in background..."
dotnet run --project src/modules/iam/CreditRisk.IAM.Api/CreditRisk.IAM.Api.csproj --urls "http://localhost:5001" &
IAM_PID=$!

echo "Starting Credit Analysis API in background..."
dotnet run --project src/modules/credit-analysis/CreditRisk.CreditAnalysis.Api/CreditRisk.CreditAnalysis.Api.csproj --urls "http://localhost:5002" &
CREDIT_PID=$!

echo "Starting Compliance API in background..."
dotnet run --project src/modules/compliance/CreditRisk.Compliance.Api/CreditRisk.Compliance.Api.csproj --urls "http://localhost:5003" &
COMPLIANCE_PID=$!

echo "All services and applications started successfully!"
echo "IAM API: http://localhost:5001"
echo "Credit Analysis API: http://localhost:5002"
echo "Compliance API: http://localhost:5003"

# Save PIDs to stop later
echo "$IAM_PID" > .iam.pid
echo "$CREDIT_PID" > .credit.pid
echo "$COMPLIANCE_PID" > .compliance.pid
