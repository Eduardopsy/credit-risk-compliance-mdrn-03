# Starting the Credit Risk Compliance System

## ✅ Infrastructure Status
Infrastructure services are running:
- ✅ PostgreSQL (5432)
- ✅ Redis (6379)
- ✅ RabbitMQ (5672, management: 15672)
- ✅ Keycloak (8080)

## 🚀 Starting Applications

### Option 1: Run All Services (Recommended)

```bash
# Terminal 1: Bureau Mock Service (Port 8081)
dotnet run -p src/external/CreditRisk.BureauMock.Service

# Terminal 2: Credit Analysis API (Port 5001)
dotnet run -p src/modules/credit-analysis/CreditRisk.CreditAnalysis.Api

# Terminal 3: Compliance API (Port 5002)
dotnet run -p src/modules/compliance/CreditRisk.Compliance.Api

# Terminal 4: IAM API (Port 5000)
dotnet run -p src/modules/iam/CreditRisk.IAM.Api

# Terminal 5: Operations Server (Port 5003, SignalR)
dotnet run -p src/servers/CreditRisk.Operations.Server

# Terminal 6: Credit Analysis Worker (Background)
dotnet run -p src/workers/CreditRisk.CreditAnalysis.Worker

# Terminal 7: Compliance Worker (Background)
dotnet run -p src/workers/CreditRisk.Compliance.Worker
```

### Option 2: Run Only APIs (for testing)

```bash
# Terminal 1: Bureau Mock Service
dotnet run -p src/external/CreditRisk.BureauMock.Service

# Terminal 2: Credit Analysis API
dotnet run -p src/modules/credit-analysis/CreditRisk.CreditAnalysis.Api

# Terminal 3: Compliance API
dotnet run -p src/modules/compliance/CreditRisk.Compliance.Api

# Terminal 4: Operations Server
dotnet run -p src/servers/CreditRisk.Operations.Server
```

## 🔌 Service Endpoints

| Service | Port | URL | Purpose |
|---------|------|-----|---------|
| **Bureau Mock** | 8081 | http://localhost:8081 | Credit bureau simulation |
| **IAM API** | 5000 | http://localhost:5000 | Identity & access management |
| **Credit Analysis API** | 5001 | http://localhost:5001 | Credit proposals & scoring |
| **Compliance API** | 5002 | http://localhost:5002 | Transactions & alerts |
| **Operations Server** | 5003 | http://localhost:5003 | Real-time dashboards (SignalR) |
| **Keycloak** | 8080 | http://localhost:8080 | OAuth2/OIDC provider |
| **RabbitMQ** | 15672 | http://localhost:15672 | Message broker management |
| **PostgreSQL** | 5432 | localhost:5432 | Database (guest/guest) |
| **Redis** | 6379 | localhost:6379 | Cache |

## 🧪 Running Tests

### Unit Tests Only
```bash
dotnet test --filter "FullyQualifiedName!~Integration" -c Debug
# Result: 29/29 passing
```

### Integration Tests (requires Testcontainers)
```bash
dotnet test tests/integration -c Debug --test-timeout 60000
# Result: 7 tests (+ fixtures)
```

### All Tests
```bash
dotnet test
```

## 📊 Checking System Health

### Health Endpoints
```bash
# Credit Analysis API
curl http://localhost:5001/health

# Compliance API
curl http://localhost:5002/health

# Operations Server
curl http://localhost:5003/health
```

### RabbitMQ Management
Visit: http://localhost:15672
- Username: guest
- Password: guest

### Database Access
```bash
psql -h localhost -U postgres -d creditrisk
# Password: postgres
```

### Redis CLI
```bash
redis-cli -h localhost
```

## 📝 Sample API Calls

### Create Credit Proposal
```bash
curl -X POST http://localhost:5001/api/credit-analysis/proposals \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": "550e8400-e29b-41d4-a716-446655440000",
    "customerDocument": "12345678901",
    "customerDocumentType": "CPF",
    "requestedLimit": 50000.00,
    "proposalType": "Individual"
  }'
```

### Ingest Transaction
```bash
curl -X POST http://localhost:5002/api/compliance/transactions \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": "550e8400-e29b-41d4-a716-446655440000",
    "customerDocument": "12345678901",
    "amount": 5000.00,
    "transactionType": "Transfer",
    "channel": "Online",
    "transactionDate": "2026-08-26T16:07:39Z"
  }'
```

### List Alerts
```bash
curl http://localhost:5002/api/compliance/alerts?status=Open
```

## 🔌 WebSocket (SignalR) Connection

### JavaScript Example
```typescript
import * as signalR from "@microsoft/signalr";

const connection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5003/hub/operations")
    .withAutomaticReconnect()
    .build();

connection.on("ReceiveAmlAlert", (alert) => {
    console.log(`Alert: ${alert.alertType} - ${alert.severity}`);
});

connection.on("ReceiveUrgentAlert", (alert) => {
    console.warn(`URGENT: ${alert.alertType}`);
});

await connection.start();
```

## 🛑 Stopping Services

### Stop Infrastructure
```bash
docker-compose down
```

### Kill All Running Services
```bash
# Press Ctrl+C in each terminal, or:
pkill -f "dotnet run"
```

## 📖 Documentation

See the following guides for more information:

- **README.md** - Project overview & features
- **API_REFERENCE.md** - Complete API documentation
- **ARCHITECTURE.md** - System design & topology
- **DEPLOYMENT.md** - Deployment strategies
- **TESTING_GUIDE.md** - Testing best practices

## ✨ Quick Verification

After starting all services, verify they're running:

```bash
# Check all health endpoints
for port in 5000 5001 5002 5003; do
  echo "=== Port $port ==="
  curl -s http://localhost:$port/health | jq . || echo "Not responding"
done

# Check RabbitMQ
curl -s -u guest:guest http://localhost:15672/api/queues | jq . | head -20

# Check PostgreSQL
psql -h localhost -U postgres -d creditrisk -c "SELECT COUNT(*) FROM information_schema.tables;"
```

---

**Happy Testing! 🚀**
