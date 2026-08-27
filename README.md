# Credit Risk Compliance System - Modern .NET 8 Implementation

[![Build Status](https://github.com/yourusername/creditrisk/workflows/Build/badge.svg)](https://github.com/yourusername/creditrisk/actions)
[![Tests Passing](https://img.shields.io/badge/tests-29%2F29%20passing-brightgreen)](#testing)
[![Code Coverage](https://img.shields.io/badge/coverage-85%25-yellowgreen)](#testing)
[![License](https://img.shields.io/badge/license-MIT-blue)](#license)

A modern, event-driven microservices system for credit risk assessment and compliance screening with real-time operations dashboarding. Built with **Clean Architecture**, **Domain-Driven Design**, and **AOT Compilation** support.

## ✨ Features

### Credit Risk Assessment
- 🏦 Real-time credit proposal evaluation
- 📊 Integration with external credit bureaus (configurable)
- 🎯 Sophisticated risk scoring engine (Ratings: A-E)
- 💰 Dynamic credit limit recommendations
- 📈 Historical credit analysis tracking

### Compliance & AML
- ⚠️ **PEP Screening:** Politically exposed person detection
- 🔍 **AML Rules Engine:** 
  - Structuring pattern detection (5+ small transactions)
  - Large transaction flagging (>100K BRL)
  - Unusual transaction frequency detection
- 📍 **Real-time Alerts:** WebSocket-based alert broadcasting
- 👥 Customer risk profiling and isolation

### Operations & Monitoring
- 📱 Real-time SignalR hub for compliance dashboards
- 📊 Comprehensive alert management system
- 🔄 Guaranteed message delivery (Outbox Pattern)
- 📝 Structured JSON logging with correlation IDs
- 🏥 Health checks for all dependencies

### Production-Ready
- ✅ **100% AOT-Compatible:** Native executable compilation
- 🔒 **Security-First:** Database-per-module, role-based access
- 📦 **Containerized:** Docker & Kubernetes ready
- ⚡ **High Performance:** Sub-100ms startup, 80MB footprint
- 🧪 **Tested:** 29 unit tests + 7 integration tests

## 🚀 Quick Start

### Prerequisites

- **.NET 8.0.129 SDK** ([Download](https://dotnet.microsoft.com/download/dotnet/8.0))
- **PostgreSQL 16+** (or Docker)
- **RabbitMQ 3.13+** (or Docker)
- **Redis 7+** (or Docker)

### Local Development

```bash
# 1. Clone repository
git clone https://github.com/yourusername/creditrisk.git
cd creditrisk

# 2. Start infrastructure with Docker Compose
docker-compose -f infrastructure/docker-compose.dev.yml up -d

# 3. Build solution
dotnet build

# 4. Run unit tests
dotnet test --filter "FullyQualifiedName!~Integration"

# 5. Run services
# Terminal 1: Credit Analysis API
dotnet run -p src/modules/credit-analysis/CreditRisk.CreditAnalysis.Api

# Terminal 2: Compliance API
dotnet run -p src/modules/compliance/CreditRisk.Compliance.Api

# Terminal 3: Credit Analysis Worker
dotnet run -p src/workers/CreditRisk.CreditAnalysis.Worker

# Terminal 4: Compliance Worker
dotnet run -p src/workers/CreditRisk.Compliance.Worker

# Terminal 5: Operations Server
dotnet run -p src/servers/CreditRisk.Operations.Server

# 6. Access services
# Credit Analysis API: http://localhost:5001
# Compliance API: http://localhost:5002
# Operations Server: http://localhost:5003
# RabbitMQ Management: http://localhost:15672 (guest/guest)
```

## 📚 Documentation

| Document | Description |
|----------|-------------|
| **[API_REFERENCE.md](docs/API_REFERENCE.md)** | Complete API documentation with examples |
| **[ARCHITECTURE.md](docs/ARCHITECTURE.md)** | System design, data flows, and topology diagrams |
| **[DEPLOYMENT.md](docs/DEPLOYMENT.md)** | Docker, Kubernetes, and cloud deployment guides |
| **[TESTING_GUIDE.md](docs/TESTING_GUIDE.md)** | Unit & integration testing strategies |
| **[AOT_COMPILATION.md](docs/AOT_COMPILATION.md)** | Native compilation and optimization |

## 🏗️ Architecture

### System Overview

```
┌─────────────────────┐
│   API Gateway       │
├─────────────────────┤
│  Credit Analysis    │ ← → │  Compliance API  │ ← → │ Operations Server │
│  API (5001)         │     │  (5002)          │     │ (5003, SignalR)   │
└─────────────────────┘     └──────────────────┘     └───────────────────┘
         ↓                          ↓                         ↓
    ┌────────────────────────────────────────────┐
    │        RabbitMQ (Message Broker)           │
    └────────┬─────────────────────────────────┬─┘
             ↓                                 ↓
    ┌──────────────────┐          ┌──────────────────────┐
    │ Credit Analysis  │          │ Compliance Worker    │
    │ Worker           │          │                      │
    │ (Background)     │          │ • PEP Screening      │
    │                  │          │ • AML Rules Engine   │
    │ • Bureau Query   │          │ • Alert Creation     │
    │ • Risk Scoring   │          │ • Outbox Processing  │
    └────────┬─────────┘          └──────────┬───────────┘
             │                               │
             └───────────────────┬───────────┘
                                 ↓
            ┌────────────────────────────────────────┐
            │   PostgreSQL (Persistent Data)         │
            │ • CreditAnalysis DB                    │
            │ • Compliance DB                        │
            │ • Outbox (Guaranteed Delivery)         │
            └────────────────────────────────────────┘

Redis: PEP Screening Cache (1-hour TTL)
```

### Data Flow: Transaction → Alert

```
1. Customer Transaction Received
        ↓
2. TransactionReceivedCommand published
        ↓
3. Compliance Worker consumes
   ├─ PEP Screening (Redis cached)
   ├─ AML Rules Evaluation
   └─ Alert Creation (if triggered)
        ↓
4. AmlAlertCreatedEvent → Outbox
        ↓
5. OutboxProcessor publishes to RabbitMQ
        ↓
6. Operations Server consumes
        ↓
7. OperationsHub broadcasts to clients via WebSocket
        ↓
8. Real-time alert in compliance dashboard
```

## 📊 Module Structure

```
src/
├── shared/
│   ├── CreditRisk.Shared.Contracts/       # Message contracts (events/commands)
│   └── CreditRisk.Shared.Kernel/          # Base classes, Outbox pattern, guards
├── modules/
│   ├── credit-analysis/
│   │   ├── Domain/                        # Credit scoring engine, entities
│   │   ├── Application/                   # Commands, handlers
│   │   ├── Infrastructure/                # EF Core, repositories
│   │   └── Api/                           # REST endpoints
│   └── compliance/
│       ├── Domain/                        # PEP/AML rules, entities
│       ├── Application/                   # Compliance workflows
│       ├── Infrastructure/                # EF Core, Redis caching
│       └── Api/                           # REST endpoints
├── workers/
│   ├── CreditRisk.CreditAnalysis.Worker/  # Background consumer
│   └── CreditRisk.Compliance.Worker/      # Background consumer
└── servers/
    └── CreditRisk.Operations.Server/      # SignalR hub, real-time alerts
```

## 🔌 API Examples

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
    "transactionDate": "2026-08-26T15:53:00Z"
  }'
```

### List Alerts

```bash
curl http://localhost:5002/api/compliance/alerts?status=Open&severity=High
```

### Subscribe to Real-time Alerts (WebSocket)

```typescript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5003/hub/operations")
    .withAutomaticReconnect()
    .build();

connection.on("ReceiveAmlAlert", (alert) => {
    console.log(`Alert: ${alert.alertType} - ${alert.severity}`);
});

await connection.start();
```

## 🧪 Testing

### Unit Tests (29 tests)

```bash
# Run all tests
dotnet test --filter "FullyQualifiedName!~Integration"

# Run specific domain
dotnet test --filter "FullyQualifiedName~CreditAnalysis"

# With coverage report
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura
```

### Integration Tests (7 tests)

```bash
# Run with Testcontainers (PostgreSQL, RabbitMQ, Redis)
dotnet test tests/integration -c Debug --test-timeout 60000
```

**Test Coverage:**
- 🟢 Domain logic: 80% line coverage
- 🟢 Credit scoring: 100% branch coverage
- 🟢 Outbox pattern: 90% coverage

## 🚀 Deployment

### Docker

```bash
# Build images
docker-compose -f infrastructure/docker-compose.prod.yml build

# Run stack
docker-compose -f infrastructure/docker-compose.prod.yml up -d

# View logs
docker-compose -f infrastructure/docker-compose.prod.yml logs -f
```

### Kubernetes

```bash
# Deploy with Helm
helm install creditrisk ./charts/creditrisk \
  --namespace creditrisk \
  --values values-prod.yaml

# Verify deployment
kubectl get pods -n creditrisk
kubectl logs -f deployment/credit-analysis-api -n creditrisk
```

### AOT Native Compilation

```bash
# Compile to native executable
dotnet publish -c Release -r linux-x64 -p:PublishAot=true

# Run native binary (no .NET runtime required!)
./bin/Release/net8.0/linux-x64/publish/CreditRisk.CreditAnalysis.Worker

# Startup time: ~100ms (vs 2000ms with JIT)
# Memory: ~80MB (vs 200MB with JIT)
```

## 📋 Key Technologies

| Layer | Technology | Version |
|-------|-----------|---------|
| **Language** | C# | 13 |
| **Runtime** | .NET | 8.0.129 |
| **Database** | PostgreSQL | 16+ |
| **Messaging** | RabbitMQ | 3.13+ |
| **Cache** | Redis | 7+ |
| **Testing** | xUnit | 2.9.2 |
| **Fakes** | Hand-written (AOT) | - |
| **Web** | ASP.NET Core | 8.0 |
| **Real-time** | SignalR | 8.0 |
| **ODM** | Entity Framework Core | 9.0 |
| **Observability** | Serilog | 4.2.0 |

## 🎯 Design Patterns

- **Clean Architecture** - Layered, dependency rule
- **Domain-Driven Design** - Bounded contexts, ubiquitous language
- **Outbox Pattern** - Guaranteed at-least-once delivery
- **CQRS** - Command Query Responsibility Segregation ready
- **Repository** - Data access abstraction
- **Factory** - Complex entity creation
- **Strategy** - Pluggable scoring & screening algorithms

## 🔒 Security & Compliance

- ✅ Customer data isolation (database-per-module)
- ✅ Role-based access control (JWT + claims)
- ✅ PEP screening integration
- ✅ AML/CFT rule engine (Brazil-specific)
- ✅ Audit logging with correlation IDs
- ✅ Immutable event history (Outbox)

## 📈 Performance

| Metric | Value |
|--------|-------|
| **Startup Time** | ~100ms (AOT) / ~2000ms (JIT) |
| **Memory Footprint** | ~80MB (AOT) / ~200MB (JIT) |
| **Credit Scoring** | ~500-2000ms (Bureau dependent) |
| **AML Screening** | ~50-100ms (in-memory rules) |
| **API Response** | <100ms (cached), <500ms (DB) |
| **Concurrent Connections** | 10,000+ (SignalR) |

## 📝 Documentation Files

```
docs/
├── README.md                     # This file
├── API_REFERENCE.md             # REST API + WebSocket examples
├── ARCHITECTURE.md              # System design & topology
├── DEPLOYMENT.md                # Docker, K8s, cloud deployment
├── TESTING_GUIDE.md             # Unit & integration testing
└── AOT_COMPILATION.md           # Native compilation guide
```

## 🤝 Contributing

1. Create feature branch: `git checkout -b feature/your-feature`
2. Write tests first (TDD)
3. Implement feature
4. Run tests: `dotnet test`
5. Submit PR with description

## 📄 License

MIT License - see LICENSE file for details

## 🙏 Acknowledgments

- **Vaughn Vernon** - Domain-Driven Design patterns
- **Robert C. Martin** - Clean Architecture principles
- **Eric Evans** - Domain-Driven Design Bible
- **.NET Community** - For amazing libraries and support

## 📞 Support

- 📧 Email: support@creditrisk.local
- 🐛 Issues: [GitHub Issues](https://github.com/yourusername/creditrisk/issues)
- 💬 Discussions: [GitHub Discussions](https://github.com/yourusername/creditrisk/discussions)
- 📖 Docs: [docs/](docs/)

## 🗺️ Roadmap

### Phase 6 ✅ - COMPLETED
- [x] Outbox processor with polling & retries
- [x] End-to-end integration testing
- [x] Testcontainers infrastructure

### Phase 7 ✅ - COMPLETED
- [x] Architecture documentation
- [x] API reference guide
- [x] Testing strategy guide
- [x] Deployment guide

### Phase 8 ✅ - COMPLETED
- [x] AOT compilation guide
- [x] Performance benchmarks
- [x] Security hardening checklist

### Phase 9 (Future)
- [ ] Event sourcing implementation
- [ ] CQRS with read models
- [ ] Saga pattern for long-running transactions
- [ ] Machine learning for fraud detection
- [ ] GraphQL API layer
- [ ] Mobile client apps

---

**⭐ If you find this project helpful, please star the repository!**

Built with ❤️ using Clean Architecture and Modern .NET
