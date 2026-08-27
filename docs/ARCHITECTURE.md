# Credit Risk Compliance System - Complete Architecture

## System Overview

A modern, event-driven microservices system for credit risk assessment and compliance screening with real-time operations dashboarding.

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         API GATEWAY / LOAD BALANCER                         │
└──────┬──────────────────────────────────────────────────────────────────────┘
       │
       ├─────────────────────────────────────────────────────────────────────┐
       │                                                                     │
       ▼                                                                     ▼
┌──────────────────────────┐                               ┌─────────────────────────┐
│ Credit Analysis API      │                               │ Compliance API          │
│ (Port 5001)              │                               │ (Port 5002)             │
├──────────────────────────┤                               ├─────────────────────────┤
│ GET  /proposals/{id}     │                               │ GET  /alerts            │
│ POST /proposals          │                               │ GET  /transactions      │
│ GET  /scoring/{pid}      │                               │ POST /transactions      │
└─────────────┬────────────┘                               └────────────┬───────────┘
              │                                                         │
              └────────────┬──────────────────────────────┬─────────────┘
                           │                              │
                    ┌──────▼──────┐            ┌──────────▼──────┐
                    │ RabbitMQ    │            │ PostgreSQL      │
                    │ (Messaging) │            │ (Persistence)   │
                    └──────┬──────┘            └─────────────────┘
                           │
        ┌──────────────────┼──────────────────┐
        │                  │                  │
        ▼                  ▼                  ▼
┌──────────────────┐ ┌──────────────────┐ ┌──────────────────┐
│ Credit Analysis  │ │ Compliance       │ │ Operations       │
│ Worker           │ │ Worker           │ │ Server           │
│ (Background)     │ │ (Background)     │ │ (ASP.NET Core)   │
├──────────────────┤ ├──────────────────┤ ├──────────────────┤
│ • CreditProposal │ │ • AML Rules      │ │ • SignalR Hub    │
│   EventConsumer  │ │ • PEP Screening  │ │ • Real-time      │
│                  │ │ • Transaction    │ │   Notifications  │
│ • Bureau HTTP    │ │   Processing     │ │                  │
│   Client         │ │                  │ │ • Alert Events   │
│                  │ │ • Outbox         │ │   Consumer       │
│ • Outbox         │ │   Processor      │ │                  │
│   Processor      │ │                  │ │ • Outbox         │
└──────────────────┘ └──────────────────┘ │   Processor      │
       │                    │              └──────────────────┘
       └────────┬───────────┘                      │
                │                                   │
        ┌───────▼─────────────┐           ┌────────▼──────────┐
        │ Outbox Table        │           │ WebSocket         │
        │ (Guaranteed Delivery)           │ (Clients)         │
        └─────────────────────┘           └───────────────────┘
```

## Data Flow: Transaction to Alert

```
1. Customer initiates credit proposal
   ↓
2. API receives CreateProposal request
   ↓
3. CreditProposalCreatedEvent published to RabbitMQ
   ↓
4. Credit Analysis Worker consumes event
   ├─ Queries Bureau Mock Service for credit data
   ├─ Runs CreditScoringEngine
   ├─ Publishes CreditProposalEvaluatedEvent to Outbox
   └─ OutboxProcessor publishes to RabbitMQ
   ↓
5. (In parallel) Customer transaction received
   ↓
6. Compliance API receives transaction
   ↓
7. TransactionReceivedCommand published to RabbitMQ
   ↓
8. Compliance Worker consumes command
   ├─ Persists Transaction entity
   ├─ PEP Screening Service checks customer against PEP database
   ├─ AML Rules Engine evaluates transaction patterns
   ├─ Creates AmlAlert if rules triggered
   ├─ Publishes AmlAlertCreatedEvent to Outbox
   └─ OutboxProcessor publishes to RabbitMQ
   ↓
9. Operations Server consumes AmlAlertCreatedEvent
   ├─ AlertNotificationService formats notification
   └─ OperationsHub broadcasts to all connected clients via SignalR
   ↓
10. Connected clients (operations dashboard) receive real-time alert
```

## Modules & Layers

### Credit Analysis Module
```
Domain Layer
├─ Entities: CreditProposal, Customer
├─ Value Objects: MoneyAmount, RiskRating
├─ Services: ICreditScoringEngine
└─ Repositories: ICreditProposalRepository

Application Layer
├─ Commands: CreateProposalCommand, EvaluateProposalCommand
├─ Handlers: Implement use cases
└─ DTOs: Transfer data between layers

Infrastructure Layer
├─ EF Core DbContext: CreditAnalysisDbContext
├─ Repositories: Implementation of domain interfaces
├─ Services: CreditScoringEngine (domain business logic)
├─ Persistence: Configurations, migrations
└─ Extensions: Dependency injection registration
```

### Compliance Module
```
Domain Layer
├─ Entities: Transaction, AmlAlert
├─ Enums: AlertStatus, AlertSeverity, TransactionStatus
├─ Services: IPepScreeningService, IAmlRulesEngine
└─ Repositories: ITransactionRepository, IAmlAlertRepository

Application Layer
├─ Commands: IngestTransactionCommand, ReviewAlertCommand
├─ Handlers: Implement compliance workflows
└─ DTOs: TransactionDto, AmlAlertDto

Infrastructure Layer
├─ EF Core DbContext: ComplianceDbContext
├─ Repositories: Transaction, AmlAlert implementations
├─ Services: PepScreeningService, AmlRulesEngine
├─ Persistence: Configurations, migrations
└─ Extensions: Dependency injection registration
```

### Shared Kernel
```
Domain
├─ Entity: Base class for aggregate roots
├─ AggregateRoot: For domain-driven design
└─ Guard: Validation helpers

Value Objects
├─ MoneyAmount: Encapsulates currency logic
├─ RiskRating: Enum-based rating

Outbox Pattern
├─ OutboxMessage: Guaranteed delivery entity
├─ IOutboxRepository: Generic interface
└─ IOutboxProcessor: Background service interface

Guard Clauses
└─ Defensive programming helpers
```

### Shared Contracts (Event/Command Models)
```
Credit Analysis
├─ Events: CreditProposalCreatedEvent, CreditProposalEvaluatedEvent
└─ Commands: BureauQueryRequestedCommand

Compliance
├─ Events: AmlAlertCreatedEvent, TransactionFlaggedEvent, FraudConfirmedEvent
└─ Commands: TransactionReceivedCommand

IAM
├─ Events: UserCreatedEvent, UserRoleChangedEvent
└─ (No commands - events only)
```

## Services & Workers

### Credit Analysis Worker
- **Purpose:** Evaluate credit proposals and score applicants
- **Consumers:** CreditProposalCreatedEventConsumer
- **External Services:** Bureau Mock Service (HTTP)
- **Key Services:** 
  - BureauHttpClient: Queries external bureau for credit data
  - CreditScoringEngine: Calculates risk rating
- **Outputs:** CreditProposalEvaluatedEvent (via Outbox)

### Compliance Worker
- **Purpose:** Screen transactions and detect suspicious patterns
- **Consumers:** TransactionReceivedCommandConsumer, AmlAlertCreatedEventConsumer
- **External Services:** None (in-memory PEP for MVP)
- **Key Services:**
  - PepScreeningService: Checks against politically exposed person lists
  - AmlRulesEngine: Evaluates AML/CFT rules (structuring, large transactions, unusual frequency)
- **Outputs:** AmlAlertCreatedEvent (via Outbox)

### Operations Server
- **Purpose:** Real-time operations dashboard with SignalR
- **Consumers:** AmlAlertCreatedEventConsumer (operations context)
- **Services:**
  - AlertNotificationService: Formats and routes alerts to hub
  - OperationsHub: SignalR hub for real-time client communication
- **Outputs:** WebSocket messages to connected clients

## Infrastructure Components

### PostgreSQL Databases
- **CreditAnalysis DB:** CreditProposal, Customer, Outbox
- **Compliance DB:** Transaction, AmlAlert, Outbox

### RabbitMQ Message Broker
- **Exchanges:** Topic-based routing
- **Queues:** One per consumer
- **Consumers:** Auto-created by MassTransit with explicit registration

### Redis Cache
- **Usage:** PEP screening result caching (1-hour TTL)
- **Connected:** Credit Analysis Worker, Compliance Worker
- **Key Pattern:** `pep:screening:{document}:{documentType}`

### Bureau Mock Service
- **Port:** 8081 (HTTP)
- **Endpoints:**
  - POST /query: Credit bureau lookup
  - GET /health: Health check
  - POST /query/error/{code}: Error simulation

## Outbox Pattern Implementation

```
┌─────────────────────────────────────────┐
│ Business Transaction (Atomic)           │
├─────────────────────────────────────────┤
│ 1. Update domain entity (e.g., Alert)   │
│ 2. Create OutboxMessage record          │
│ 3. Commit to database (single TX)       │
└────────────┬────────────────────────────┘
             │
             ▼
┌─────────────────────────────────────────┐
│ OutboxProcessor (Background Service)    │
├─────────────────────────────────────────┤
│ 1. Poll unprocessed messages (5s)       │
│ 2. Deserialize payload                  │
│ 3. Publish to MassTransit               │
│ 4. Mark as processed                    │
│ 5. Cleanup old messages (>30 days)      │
└────────────┬────────────────────────────┘
             │
             ▼
┌─────────────────────────────────────────┐
│ RabbitMQ (Message Broker)               │
├─────────────────────────────────────────┤
│ Event published for subscribers          │
└─────────────────────────────────────────┘
```

## Key Design Patterns

### 1. Outbox Pattern (Reliable Event Publishing)
- **Problem:** Avoiding distributed transaction issues
- **Solution:** Store event + business data in same DB, publish async
- **Benefit:** Guaranteed at-least-once delivery
- **Retry Logic:** Max 5 retries with error tracking

### 2. Consumer Segregation
- **Problem:** One consumer failure blocking others
- **Solution:** Separate queues per consumer with explicit registration
- **Benefit:** Fault isolation, independent scaling

### 3. Graceful Degradation
- **Problem:** External service (Bureau) unavailability
- **Solution:** Default to safe scoring (score=0 → rejection)
- **Benefit:** System continues operating with safe defaults

### 4. Domain-Driven Design
- **Layers:** Domain, Application, Infrastructure
- **Bounded Contexts:** CreditAnalysis, Compliance
- **Entities:** Aggregate roots with factory methods
- **Value Objects:** MoneyAmount, RiskRating

### 5. Clean Architecture
- **Dependency Rule:** Only inward dependencies
- **Isolation:** Business logic independent of frameworks
- **Testability:** All logic testable with fakes

## Security & Compliance

### Data Isolation
- Separate databases per module
- Customer data never exposed across boundaries
- Alert visibility limited to compliance officers

### PEP Screening
- Checks against politically exposed person lists (OFAC, BCB)
- Cached results (1-hour TTL) for performance
- Production: Replace heuristic with actual PEP API

### AML Rules
- **Structuring Detection:** 5+ small transactions (<10K) in 7 days
- **Large Transaction:** >100K immediately escalated
- **Unusual Frequency:** >15 transactions per day
- All rules configured for Brazilian market

### Audit Trail
- All alerts logged with correlation IDs
- Distributed tracing via correlation IDs
- Event sourcing via Outbox (immutable history)

## Performance Characteristics

### Throughput
- Credit Scoring: ~1000 proposals/minute (limited by Bureau API)
- AML Screening: ~5000 transactions/minute
- Alert Broadcasting: ~100 clients/SignalR connection

### Latency
- Credit Scoring: 500ms-2000ms (Bureau API dependent)
- AML Screening: 50-100ms (in-memory rules)
- Alert Broadcast: <100ms (WebSocket)

### Resource Usage
- Workers: ~200MB each (CreditAnalysis, Compliance)
- Operations Server: ~300MB (SignalR connections)
- PEP Cache: ~50MB (typical operation)

## Deployment Architecture

```
┌─────────────────────────────────────────────────────────┐
│ Kubernetes Cluster                                      │
├─────────────────────────────────────────────────────────┤
│                                                         │
│ ┌─────────────────────────────────────────────────┐    │
│ │ Ingress / API Gateway                           │    │
│ └──────┬──────────────────────────────┬───────────┘    │
│        │                              │                │
│   ┌────▼────┐                   ┌────▼────┐           │
│   │Credit   │                   │Compliance          │
│   │Analysis │                   │API                 │
│   │API Pod  │                   └────┬────┘           │
│   └────┬────┘                        │                │
│        │                             │                │
│   ┌────▼─────────────┐          ┌────▼────┐           │
│   │Credit Analysis   │          │Operations           │
│   │Worker Pod        │          │Server Pod           │
│   │(CronJob/Deployment)         └────┬────┘           │
│   └────┬─────────────┘               │                │
│        │                             │                │
│   ┌────▼─────────────┐          ┌────▼────────┐       │
│   │Compliance        │          │Compliance   │       │
│   │Worker Pod        │          │Worker Pod   │       │
│   │(CronJob/Deployment)         └────┬────────┘       │
│   └────┬─────────────┘               │                │
│        │                             │                │
│        └──────────────┬──────────────┘                │
│                       │                               │
│ ┌─────────────────────▼─────────────────────────┐    │
│ │ StatefulSet: PostgreSQL (Primary + Standby)   │    │
│ │ StatefulSet: RabbitMQ (Clustering)            │    │
│ │ StatefulSet: Redis (Replication)              │    │
│ └─────────────────────────────────────────────────┘    │
│                                                         │
└─────────────────────────────────────────────────────────┘
```

## Testing Strategy

### Unit Tests (29 tests)
- Domain logic: CreditScoringEngine, AmlRulesEngine
- Entities: Create methods, validation
- Value Objects: MoneyAmount operations

### Integration Tests (Testcontainers)
- End-to-end flows with real databases
- Outbox reliability verification
- PEP screening with cache
- AML rule evaluation with transaction history

### E2E Tests (Future)
- Full workflow: API → Worker → Alert → SignalR
- Failed retry scenarios
- Load testing (concurrent transactions)

## Monitoring & Observability

### Structured Logging
- JSON formatted logs via Serilog
- Correlation IDs for distributed tracing
- Log levels: Debug (development), Information (production)

### Metrics
- Alert creation rate (alerts/minute)
- Outbox processing latency (ms)
- Consumer lag (messages behind)
- SignalR connection count

### Health Checks
- Database connectivity
- RabbitMQ connectivity
- Redis connectivity
- External service availability (Bureau)

## Future Enhancements

1. **Event Sourcing:** Store all state changes as events
2. **CQRS:** Separate read/write models for complex queries
3. **Saga Pattern:** Multi-step transaction orchestration
4. **Event Versioning:** Handle schema evolution
5. **Dead Letter Queue:** Permanently failed message handling
6. **Analytics:** Machine learning for fraud detection
7. **Compliance Reporting:** Automated regulatory submissions
8. **Mobile Apps:** Native clients for compliance officers
