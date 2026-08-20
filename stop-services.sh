#!/usr/bin/env bash
set -e

echo "Stopping application processes..."
if [ -f .iam.pid ]; then
  kill $(cat .iam.pid) 2>/dev/null || true
  rm .iam.pid
fi

if [ -f .credit.pid ]; then
  kill $(cat .credit.pid) 2>/dev/null || true
  rm .credit.pid
fi

if [ -f .compliance.pid ]; then
  kill $(cat .compliance.pid) 2>/dev/null || true
  rm .compliance.pid
fi

echo "Stopping infrastructure services..."
docker-compose down -v

echo "All services and applications stopped successfully."
