# Deployment Guide - Credit Risk Compliance System

## Table of Contents

1. [Local Development](#local-development)
2. [Docker Deployment](#docker-deployment)
3. [Kubernetes Deployment](#kubernetes-deployment)
4. [Environment Configuration](#environment-configuration)
5. [Database Migrations](#database-migrations)
6. [Health Checks](#health-checks)
7. [Monitoring & Logging](#monitoring--logging)
8. [Troubleshooting](#troubleshooting)

## Local Development

### Prerequisites

- .NET 8.0.129 SDK
- PostgreSQL 16+
- RabbitMQ 3.13+
- Redis 7+
- Docker (optional, for containers)

### Setup Infrastructure

```bash
# Using Docker Compose (recommended)
docker-compose -f infrastructure/docker-compose.dev.yml up -d

# Or manually:
# PostgreSQL on 5432
# RabbitMQ on 5672 (AMQP), 15672 (Management)
# Redis on 6379
```

### Build & Run

```bash
# Build all projects
dotnet build

# Run unit tests
dotnet test --filter "FullyQualifiedName!~Integration" -c Debug

# Run API projects
dotnet run -p src/modules/credit-analysis/CreditRisk.CreditAnalysis.Api
dotnet run -p src/modules/compliance/CreditRisk.Compliance.Api
dotnet run -p src/servers/CreditRisk.Operations.Server

# Run workers
dotnet run -p src/workers/CreditRisk.CreditAnalysis.Worker
dotnet run -p src/workers/CreditRisk.Compliance.Worker
```

## Docker Deployment

### Build Images

```bash
# Credit Analysis API
docker build -f src/modules/credit-analysis/CreditRisk.CreditAnalysis.Api/Dockerfile \
  -t creditrisk-credit-analysis:latest \
  .

# Compliance API
docker build -f src/modules/compliance/CreditRisk.Compliance.Api/Dockerfile \
  -t creditrisk-compliance:latest \
  .

# Credit Analysis Worker
docker build -f src/workers/CreditRisk.CreditAnalysis.Worker/Dockerfile \
  -t creditrisk-credit-analysis-worker:latest \
  .

# Compliance Worker
docker build -f src/workers/CreditRisk.Compliance.Worker/Dockerfile \
  -t creditrisk-compliance-worker:latest \
  .

# Operations Server
docker build -f src/servers/CreditRisk.Operations.Server/Dockerfile \
  -t creditrisk-operations:latest \
  .
```

### Docker Compose Stack

```bash
# Deploy full stack
docker-compose -f infrastructure/docker-compose.prod.yml up -d

# View logs
docker-compose -f infrastructure/docker-compose.prod.yml logs -f

# Stop stack
docker-compose -f infrastructure/docker-compose.prod.yml down
```

### Environment Variables

```bash
# Database
POSTGRES_CONNECTION_STRING=Server=postgres;Port=5432;Database=creditrisk_prod;User ID=postgres;Password=...;

# RabbitMQ
RABBITMQ_HOST=rabbitmq
RABBITMQ_PORT=5672
RABBITMQ_USERNAME=guest
RABBITMQ_PASSWORD=guest

# Redis
REDIS_CONNECTION_STRING=redis:6379

# API
API_PORT=5001
COMPLIANCE_API_PORT=5002
OPERATIONS_SERVER_PORT=5003

# Logging
LOG_LEVEL=Information
```

## Kubernetes Deployment

### Prerequisite: Build & Push Images

```bash
# Build with version tag
docker build -t myregistry.azurecr.io/creditrisk-credit-analysis:v1.0.0 .
docker build -t myregistry.azurecr.io/creditrisk-compliance:v1.0.0 .
docker build -t myregistry.azurecr.io/creditrisk-credit-analysis-worker:v1.0.0 .
docker build -t myregistry.azurecr.io/creditrisk-compliance-worker:v1.0.0 .
docker build -t myregistry.azurecr.io/creditrisk-operations:v1.0.0 .

# Push to registry
docker push myregistry.azurecr.io/creditrisk-*:v1.0.0
```

### Create Kubernetes Manifests

```yaml
# Credit Analysis API Deployment
apiVersion: apps/v1
kind: Deployment
metadata:
  name: credit-analysis-api
  namespace: creditrisk
spec:
  replicas: 3
  selector:
    matchLabels:
      app: credit-analysis-api
  template:
    metadata:
      labels:
        app: credit-analysis-api
    spec:
      containers:
      - name: api
        image: myregistry.azurecr.io/creditrisk-credit-analysis:v1.0.0
        ports:
        - containerPort: 5001
        env:
        - name: POSTGRES_CONNECTION_STRING
          valueFrom:
            secretKeyRef:
              name: db-credentials
              key: connection-string
        - name: RABBITMQ_HOST
          value: rabbitmq
        - name: REDIS_CONNECTION_STRING
          value: redis:6379
        livenessProbe:
          httpGet:
            path: /health
            port: 5001
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /health/ready
            port: 5001
          initialDelaySeconds: 10
          periodSeconds: 5
        resources:
          requests:
            memory: "256Mi"
            cpu: "250m"
          limits:
            memory: "512Mi"
            cpu: "500m"

---
# Credit Analysis API Service
apiVersion: v1
kind: Service
metadata:
  name: credit-analysis-api
  namespace: creditrisk
spec:
  type: LoadBalancer
  selector:
    app: credit-analysis-api
  ports:
  - protocol: TCP
    port: 80
    targetPort: 5001

---
# Credit Analysis Worker Deployment
apiVersion: apps/v1
kind: Deployment
metadata:
  name: credit-analysis-worker
  namespace: creditrisk
spec:
  replicas: 2
  selector:
    matchLabels:
      app: credit-analysis-worker
  template:
    metadata:
      labels:
        app: credit-analysis-worker
    spec:
      containers:
      - name: worker
        image: myregistry.azurecr.io/creditrisk-credit-analysis-worker:v1.0.0
        env:
        - name: POSTGRES_CONNECTION_STRING
          valueFrom:
            secretKeyRef:
              name: db-credentials
              key: connection-string
        - name: RABBITMQ_HOST
          value: rabbitmq
        resources:
          requests:
            memory: "256Mi"
            cpu: "250m"
          limits:
            memory: "512Mi"
            cpu: "500m"

---
# PostgreSQL StatefulSet
apiVersion: apps/v1
kind: StatefulSet
metadata:
  name: postgres
  namespace: creditrisk
spec:
  serviceName: postgres
  replicas: 1
  selector:
    matchLabels:
      app: postgres
  template:
    metadata:
      labels:
        app: postgres
    spec:
      containers:
      - name: postgres
        image: postgres:16-alpine
        ports:
        - containerPort: 5432
        env:
        - name: POSTGRES_DB
          value: creditrisk
        - name: POSTGRES_PASSWORD
          valueFrom:
            secretKeyRef:
              name: db-credentials
              key: password
        volumeMounts:
        - name: data
          mountPath: /var/lib/postgresql/data
  volumeClaimTemplates:
  - metadata:
      name: data
    spec:
      accessModes: ["ReadWriteOnce"]
      resources:
        requests:
          storage: 10Gi
```

### Deploy to Kubernetes

```bash
# Create namespace
kubectl create namespace creditrisk

# Create secrets
kubectl create secret generic db-credentials \
  --from-literal=connection-string='...' \
  --from-literal=password='...' \
  -n creditrisk

# Deploy
kubectl apply -f kubernetes/

# Verify deployments
kubectl get deployments -n creditrisk
kubectl get pods -n creditrisk

# View logs
kubectl logs -f deployment/credit-analysis-api -n creditrisk
```

## Environment Configuration

### Configuration Files

```
.env                          # Local development (git ignored)
appsettings.json             # Default settings (committed)
appsettings.Production.json  # Production overrides (git ignored)
appsettings.{Environment}.json
```

### Required Settings

```json
{
  "ConnectionStrings": {
    "CreditAnalysisDb": "Server=localhost;Port=5432;Database=creditrisk_ca;User Id=postgres;Password=...",
    "ComplianceDb": "Server=localhost;Port=5432;Database=creditrisk_comp;User Id=postgres;Password=..."
  },
  "RabbitMQ": {
    "Host": "localhost",
    "Port": 5672,
    "Username": "guest",
    "Password": "guest",
    "Vhost": "/"
  },
  "Redis": {
    "ConnectionString": "localhost:6379"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  },
  "ApiKey": "your-api-key-here",
  "JwtSecret": "your-jwt-secret-here"
}
```

## Database Migrations

### Running Migrations

```bash
# Create new migration
dotnet ef migrations add AddAlertTable \
  -p src/modules/compliance/CreditRisk.Compliance.Infrastructure \
  -s src/apis/CreditRisk.Compliance.Api

# Apply migrations to database
dotnet ef database update \
  -p src/modules/compliance/CreditRisk.Compliance.Infrastructure \
  -s src/apis/CreditRisk.Compliance.Api

# Rollback to previous migration
dotnet ef database update PreviousMigrationName \
  -p src/modules/compliance/CreditRisk.Compliance.Infrastructure
```

### Migration Best Practices

1. Always create migrations in development environment
2. Test migrations with data backups
3. Plan maintenance windows for production migrations
4. Use zero-downtime migration strategies (e.g., feature flags)

## Health Checks

### API Health Endpoints

```bash
# Basic health check
curl http://localhost:5001/health

# Detailed health check
curl http://localhost:5001/health/detailed

# Readiness check (dependencies ready)
curl http://localhost:5001/health/ready

# Liveness check (service running)
curl http://localhost:5001/health/live
```

### Health Check Response

```json
{
  "status": "Healthy",
  "checks": {
    "database": "Healthy",
    "rabbitmq": "Healthy",
    "redis": "Healthy"
  },
  "timestamp": "2026-08-26T15:53:00Z"
}
```

## Monitoring & Logging

### Structured Logging

All services emit JSON-structured logs to stdout:

```json
{
  "Timestamp": "2026-08-26T15:53:00.123Z",
  "Level": "Information",
  "MessageTemplate": "Alert created: {AlertType} for customer {CustomerId}",
  "AlertType": "PepMatch",
  "CustomerId": "550e8400-e29b-41d4-a716-446655440000",
  "SourceContext": "CreditRisk.Compliance.Domain.Services.AmlRulesEngine",
  "TraceId": "4bf92f3577b34da6a3ce929d0e0e4736",
  "SpanId": "0af7651dee81cd7e"
}
```

### Log Aggregation

```bash
# Docker Logs
docker logs --follow credit-analysis-api

# Kubernetes Logs
kubectl logs -f deployment/credit-analysis-api -n creditrisk

# Stream to ELK
kubectl logs -f deployment/credit-analysis-api -n creditrisk | \
  curl -X POST -H "Content-Type: application/json" \
  --data-binary @- http://elasticsearch:9200/_bulk
```

### Metrics

- Alert creation rate (alerts/minute)
- Outbox processing latency (ms)
- Consumer lag (messages behind)
- API response times (ms)
- Database connection pool usage (%)

## Troubleshooting

### Consumers Not Processing Messages

```bash
# Check RabbitMQ queues
curl http://localhost:15672/api/queues | jq '.'

# Check consumer state
kubectl logs deployment/credit-analysis-worker -n creditrisk | grep "consumer"

# Reset queue
docker exec rabbitmq rabbitmqctl purge_queue credit.proposal.created
```

### Database Connection Errors

```bash
# Test PostgreSQL connectivity
psql -h localhost -U postgres -d creditrisk -c "SELECT 1"

# Check connection pool
kubectl exec -it pod/credit-analysis-api -n creditrisk -- \
  curl http://localhost:5001/health/db
```

### High Memory Usage

```bash
# Check memory limits
kubectl describe pod credit-analysis-api -n creditrisk

# Increase memory limit
kubectl patch deployment credit-analysis-api -n creditrisk \
  -p '{"spec":{"template":{"spec":{"containers":[{"name":"api","resources":{"limits":{"memory":"1Gi"}}}]}}'
```

### SignalR Connection Issues

```bash
# Check WebSocket connection
curl -i -N -H "Connection: Upgrade" -H "Upgrade: websocket" \
  http://localhost:5003/hub/operations

# View active connections
kubectl exec -it pod/credit-analysis-api -n creditrisk -- \
  curl http://localhost:5003/metrics | grep signalr
```

## Performance Tuning

### Database

```sql
-- Add indexes for common queries
CREATE INDEX idx_alerts_customer_created 
  ON aml_alerts(customer_id, created_at DESC);

CREATE INDEX idx_transactions_customer_date 
  ON transactions(customer_id, transaction_date DESC);
```

### RabbitMQ

```bash
# Increase prefetch count for better throughput
docker exec rabbitmq rabbitmqctl set_global_parameter \
  consumer_max_queues 2048
```

### Redis

```bash
# Monitor Redis usage
redis-cli INFO memory
redis-cli --stat
```

## Disaster Recovery

### Database Backup

```bash
# Manual backup
pg_dump creditrisk > backup_$(date +%Y%m%d_%H%M%S).sql

# Restore from backup
psql creditrisk < backup_20260826_153000.sql

# Kubernetes cronjob backup
kubectl apply -f kubernetes/backup-cronjob.yaml
```

### Message Recovery

Outbox messages are persisted in database. If RabbitMQ fails:

1. Messages remain in Outbox table
2. OutboxProcessor retries after RabbitMQ recovery
3. No message loss with Outbox Pattern

### State Recovery

All service state is reconstructed from:
1. Database (entities, transactions, alerts)
2. Message history (Outbox, events)
3. Event stream (for event sourcing)

No in-memory state is critical.

## Rollback Strategy

```bash
# Identify current version
kubectl set image deployment/credit-analysis-api \
  credit-analysis-api=myregistry.azurecr.io/creditrisk-credit-analysis:v0.9.0 \
  -n creditrisk

# Monitor rollout
kubectl rollout status deployment/credit-analysis-api -n creditrisk

# View rollout history
kubectl rollout history deployment/credit-analysis-api -n creditrisk

# Undo last deployment
kubectl rollout undo deployment/credit-analysis-api -n creditrisk
```
