#!/usr/bin/env bash
set -e

echo "Stopping application processes..."
killall dotnet 2>/dev/null || true

if [ -f .iam.pid ]; then
  kill $(cat .iam.pid) 2>/dev/null || true
  rm -f .iam.pid
fi

if [ -f .credit.pid ]; then
  kill $(cat .credit.pid) 2>/dev/null || true
  rm -f .credit.pid
fi

if [ -f .compliance.pid ]; then
  kill $(cat .compliance.pid) 2>/dev/null || true
  rm -f .compliance.pid
fi

echo "Stopping infrastructure services..."
docker rm -f crcl-keycloak crcl-postgres crcl-rabbitmq crcl-redis 2>/dev/null || true
docker-compose down -v --remove-orphans 2>/dev/null || true

echo "All services and applications stopped successfully."
