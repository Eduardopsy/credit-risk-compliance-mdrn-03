# SPEC-04 Implementation Plan

**Specification:** SPEC-04 — Integration  
**Version:** 1.0.0  
**Date:** 2026-08-26  
**Status:** 📋 Planning Phase

---

## Executive Summary

SPEC-04 defines the **asynchronous processing layer** using MassTransit/RabbitMQ, consisting of:

1. **Credit Analysis Worker** - Processes `CreditProposalCreatedEvent`, queries bureau, runs scoring engine, publishes results
2. **Compliance Worker** - Processes `TransactionReceivedCommand`, applies AML/CFT rules, creates alerts, publishes events  
3. **Operations Hub** - SignalR hub for real-time dashboard updates via RabbitMQ event consumers
4. **Outbox Pattern** - Guarantees message delivery with transactional consistency
5. **Bureau HTTP Client** - Resilient HTTP client with Polly retry/circuit breaker and Redis caching (24h TTL)
6. **PEP Screening Service** - Cached external API with Redis (24h TTL)

---

## Current State Analysis

### ✅ Already Implemented (from SPEC-02 & SPEC-03)

- **Domain & Application Layers**
  - `CreditRisk.CreditAnalysis.Domain` with scoring engine ✅
  - `CreditRisk.Compliance.Domain` with AML rules ✅
  - `CreditRisk.IAM.Domain` with auth ✅
  - Infrastructure layer with EF Core ✅

- **APIs (Partial)**
  - `CreditRisk.CreditAnalysis.Api` with endpoints ✅
  - `CreditRisk.Compliance.Api` partially working ✅
  - `CreditRisk.IAM.Api` ✅

- **Test Suite**
  - 29 unit tests passing ✅
  - Test runner infrastructure (run-tests.sh, Makefile) ✅

- **Shared Infrastructure**
  - `CreditRisk.Shared.Contracts` (message contracts needed)
  - `CreditRisk.Shared.Kernel` (base types, outbox infrastructure)
  - `CreditRisk.Shared.Observability` (OpenTelemetry)

### ❌ NOT Implemented Yet

- **Worker Projects**
  - `CreditRisk.CreditAnalysis.Worker` - MISSING
  - `CreditRisk.Compliance.Worker` - MISSING

- **Operations Module**
  - `CreditRisk.Operations.Server` - MISSING
  - SignalR hub and consumers - MISSING

- **MassTransit Integration**
  - Consumer implementations - MISSING
  - RabbitMQ configuration - MISSING
  - Outbox pattern implementation - MISSING

- **External Service Clients**
  - Bureau HTTP client - MISSING
  - PEP screening service - MISSING

- **Integration Tests**
  - Worker consumer tests - MISSING
  - Outbox processor tests - MISSING
  - End-to-end messaging tests - MISSING

---

## Implementation Phases

### Phase 1: Foundation & Infrastructure (2-3 hours)

#### 1.1 Create Shared Message Contracts
**Files to create:**
- `src/shared/CreditRisk.Shared.Contracts/CreditAnalysis/Events/CreditProposalCreatedEvent.cs`
- `src/shared/CreditRisk.Shared.Contracts/CreditAnalysis/Events/CreditProposalEvaluatedEvent.cs`
- `src/shared/CreditRisk.Shared.Contracts/Compliance/Commands/TransactionReceivedCommand.cs`
- `src/shared/CreditRisk.Shared.Contracts/Compliance/Events/AmlAlertCreatedEvent.cs`
- `src/shared/CreditRisk.Shared.Contracts/Compliance/Events/TransactionFlaggedEvent.cs`

**Contract Structure:**
```csharp
public record CreditProposalCreatedEvent
{
    public Guid ProposalId { get; init; }
    public Guid CustomerId { get; init; }
    public string CustomerDocument { get; init; }
    public string CustomerDocumentType { get; init; }
    public decimal RequestedLimit { get; init; }
    public string ProposalType { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public string CreatedBy { get; init; }
    public Guid CorrelationId { get; init; }
}

// Similar pattern for other events/commands
```

**Acceptance Criteria:**
- [ ] All contracts are `public sealed record`
- [ ] All properties are `init` only (immutable)
- [ ] Include `CorrelationId` for distributed tracing
- [ ] Include `DateTimeOffset` for timestamps
- [ ] No inheritance (avoid IL trimming issues with AOT)

#### 1.2 Implement Outbox Infrastructure in Shared.Kernel
**Files to create/update:**
- `src/shared/CreditRisk.Shared.Kernel/Outbox/OutboxMessage.cs` - Domain aggregate for outbox
- `src/shared/CreditRisk.Shared.Kernel/Outbox/IOutboxRepository.cs` - Interface

**Acceptance Criteria:**
- [ ] OutboxMessage entity has: `Id`, `MessageType`, `Payload`, `ScheduledAt`, `ProcessedAt`, `RetryCount`, `Error`
- [ ] Factory method `Create(messageType, payload)` validates inputs
- [ ] Methods: `MarkProcessed()`, `MarkFailed(error)`, `MarkRetry()`
- [ ] Entity implements `IEntity` base class

#### 1.3 Create Worker Projects in Solution
**Projects to create:**
- `src/modules/credit-analysis/CreditRisk.CreditAnalysis.Worker/CreditRisk.CreditAnalysis.Worker.csproj`
- `src/modules/compliance/CreditRisk.Compliance.Worker/CreditRisk.Compliance.Worker.csproj`
- `src/modules/operations/CreditRisk.Operations.Server/CreditRisk.Operations.Server.csproj`

**Project Configuration:**
```xml
<Project Sdk="Microsoft.NET.Sdk.Worker">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <AssemblyName>CreditRisk.CreditAnalysis.Worker</AssemblyName>
    <PublishAot>true</PublishAot>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="MassTransit" />
    <PackageReference Include="MassTransit.RabbitMQ" />
    <!-- ... -->
  </ItemGroup>
</Project>
```

**Acceptance Criteria:**
- [ ] Projects created and added to solution
- [ ] `PublishAot=true` set for AOT compilation
- [ ] All dependencies (MassTransit, Polly, Redis, OpenTelemetry) added
- [ ] Solution builds successfully

---

### Phase 2: MassTransit Configuration (2-3 hours)

#### 2.1 Configure MassTransit in All Services
**Files to create:**
- `src/modules/credit-analysis/CreditRisk.CreditAnalysis.Worker/Program.cs`
- `src/modules/compliance/CreditRisk.Compliance.Worker/Program.cs`
- `src/modules/operations/CreditRisk.Operations.Server/Program.cs`

**Configuration Pattern:**
```csharp
// Global retry: 3 retries, exponential backoff
cfg.UseMessageRetry(r => r.Exponential(
    retryLimit: 3,
    minInterval: TimeSpan.FromSeconds(1),
    maxInterval: TimeSpan.FromSeconds(30),
    intervalDelta: TimeSpan.FromSeconds(5)));

// Global circuit breaker
cfg.UseCircuitBreaker(cb =>
{
    cb.TrackingPeriod = TimeSpan.FromMinutes(1);
    cb.TripThreshold = 15;          // 15% failure rate
    cb.ActiveThreshold = 10;        // Min 10 messages
    cb.ResetInterval = TimeSpan.FromMinutes(5);
});

// OpenTelemetry propagation
cfg.PropagateActivityContext = true;

// Explicit consumer registration (AOT-compatible)
x.AddConsumer<CreditProposalCreatedEventConsumer>();

// Auto-configure endpoints
cfg.ConfigureEndpoints(context);
```

**Acceptance Criteria:**
- [ ] All services register MassTransit with RabbitMQ
- [ ] Consumers registered explicitly (NOT via assembly scanning)
- [ ] Global retry and circuit breaker configured
- [ ] OpenTelemetry activity propagation enabled
- [ ] Configuration reads from environment variables (RabbitMQ__Host, etc.)

#### 2.2 Create Port Interfaces in Infrastructure Layers
**Files to create:**
- `src/modules/credit-analysis/CreditRisk.CreditAnalysis.Infrastructure/Outbox/IOutboxRepository.cs`
- `src/modules/credit-analysis/CreditRisk.CreditAnalysis.Infrastructure/Outbox/OutboxRepository.cs`
- `src/modules/credit-analysis/CreditRisk.CreditAnalysis.Infrastructure/Outbox/OutboxProcessor.cs`
- `src/modules/credit-analysis/CreditRisk.CreditAnalysis.Worker/Consumers/CreditProposalCreatedEventConsumer.cs`
- `src/modules/credit-analysis/CreditRisk.CreditAnalysis.Worker/Services/BureauHttpClient.cs`

**Acceptance Criteria:**
- [ ] OutboxRepository implements IOutboxRepository from Shared.Kernel
- [ ] OutboxProcessor is a BackgroundService that polls every 5 seconds
- [ ] Processes up to 100 messages per batch
- [ ] Marks messages processed/failed appropriately

---

### Phase 3: Credit Analysis Worker (3-4 hours)

#### 3.1 Implement CreditProposalCreatedEventConsumer
**File:** `src/modules/credit-analysis/CreditRisk.CreditAnalysis.Worker/Consumers/CreditProposalCreatedEventConsumer.cs`

**Responsibilities:**
1. Consume `CreditProposalCreatedEvent` from RabbitMQ
2. Fetch proposal and customer from database
3. Query bureau API (with Polly resilience)
4. Run scoring engine
5. Apply evaluation to proposal aggregate
6. Publish `CreditProposalEvaluatedEvent`

**Key Details:**
- Idempotency: Check if already processed (by ProposalId)
- Bureau unavailability: Return degraded result (score=0) → manual review
- Bureau caching: 24h TTL in Redis
- OpenTelemetry tracing: Set tags for proposal/customer IDs
- Queue name: `credit-worker_credit-proposal-created-event`
- DLQ name: `credit-worker_credit-proposal-created-event_error`

**Acceptance Criteria:**
- [ ] Implements `IConsumer<CreditProposalCreatedEvent>`
- [ ] Loads proposal and customer (handles not-found gracefully)
- [ ] Calls bureau query service with resilience
- [ ] Runs scoring engine on bureau result
- [ ] Updates proposal with evaluation result
- [ ] Persists to database via UnitOfWork
- [ ] Publishes `CreditProposalEvaluatedEvent` on success
- [ ] Idempotent (can process same message twice safely)
- [ ] Sets OpenTelemetry activity tags

#### 3.2 Implement BureauHttpClient Service
**File:** `src/modules/credit-analysis/CreditRisk.CreditAnalysis.Worker/Services/BureauHttpClient.cs`

**Responsibilities:**
1. Make HTTP POST to bureau API
2. Cache result in Redis (24h TTL)
3. Implement Polly resilience (retry, circuit breaker)
4. Fallback to degraded result on failure

**Details:**
- Named HttpClient: `"bureau-api"`
- Timeout: 10 seconds
- Retry: 3 attempts, exponential backoff
- Circuit breaker: 50% failure rate, 5-minute reset
- Cache key: `bureau:result:{document}`
- Degraded result: `Score=0, TotalMonthlyDebt=0, IsDegraded=true`

**Acceptance Criteria:**
- [ ] Implements `IBureauQueryService` from Application layer
- [ ] Uses `IHttpClientFactory` to get named `"bureau-api"` client
- [ ] Checks Redis cache first before HTTP call
- [ ] Caches successful results for 24 hours
- [ ] Returns degraded result on HTTP failure (never throws)
- [ ] Logs cache hits/misses and failures

#### 3.3 Add Outbox Pattern to CreditAnalysis Infrastructure
**Files to create/update:**
- `src/modules/credit-analysis/CreditRisk.CreditAnalysis.Infrastructure/Outbox/OutboxRepository.cs`
- `src/modules/credit-analysis/CreditRisk.CreditAnalysis.Infrastructure/Outbox/OutboxProcessor.cs`
- `src/modules/credit-analysis/CreditRisk.CreditAnalysis.Infrastructure/CreditAnalysisDbContext.cs` - Add `DbSet<OutboxMessage>`

**Acceptance Criteria:**
- [ ] OutboxMessage added to DbContext
- [ ] OutboxRepository persists/queries messages
- [ ] OutboxProcessor is a hosted service
- [ ] Processes batches every 5 seconds
- [ ] Publishes messages via IPublishEndpoint
- [ ] Marks as processed after successful publish
- [ ] Handles failures with retry logic

---

### Phase 4: Compliance Worker (3-4 hours)

#### 4.1 Implement TransactionReceivedCommandConsumer
**File:** `src/modules/compliance/CreditRisk.Compliance.Worker/Consumers/TransactionReceivedCommandConsumer.cs`

**Responsibilities:**
1. Consume `TransactionReceivedCommand` from RabbitMQ
2. Check idempotency (transaction not already processed)
3. Perform PEP screening
4. Create transaction entity
5. Apply AML rules engine
6. Create alerts for flagged transactions
7. Publish events (AmlAlertCreatedEvent, TransactionFlaggedEvent)

**Key Details:**
- Queue name: `compliance-worker_transaction-received-command`
- DLQ name: `compliance-worker_transaction-received-command_error`
- Idempotency: Skip if transaction already in database
- PEP screening: Redis-cached, 24h TTL
- Multiple alerts: Each AML flag → separate alert + event
- Critical severity: When PEP match detected

**Acceptance Criteria:**
- [ ] Implements `IConsumer<TransactionReceivedCommand>`
- [ ] Checks if transaction already processed (idempotent)
- [ ] Calls PEP screening service
- [ ] Creates transaction entity via domain factory
- [ ] Evaluates all AML rules
- [ ] Creates alerts for each flag
- [ ] Publishes `AmlAlertCreatedEvent` for each alert
- [ ] Publishes `TransactionFlaggedEvent` if any flags
- [ ] Persists transaction and alerts to database

#### 4.2 Implement PepScreeningService
**File:** `src/modules/compliance/CreditRisk.Compliance.Worker/Services/PepScreeningService.cs`

**Responsibilities:**
1. Query external PEP/Sanctions API
2. Cache results in Redis (24h TTL)
3. Handle API unavailability (fallback to false = no PEP match)

**Details:**
- Implementation of `IPepScreeningService` from Application
- Redis cache key: `pep:document:{document}`
- TTL: 24 hours (configurable via AmlRules__PepScreening__CacheTtlHours)
- API endpoint: `{PepScreening__BaseUrl}/check`
- API key: `PepScreening__ApiKey`

**Acceptance Criteria:**
- [ ] Implements `IPepScreeningService`
- [ ] Checks Redis cache first
- [ ] Makes HTTP call to external API
- [ ] Caches result for 24 hours
- [ ] Returns false if API unavailable (never throws)
- [ ] Logs all API calls and cache hits

---

### Phase 5: Operations Hub (2-3 hours)

#### 5.1 Create Operations.Server Project
**Project:** `src/modules/operations/CreditRisk.Operations.Server/CreditRisk.Operations.Server.csproj`

**Includes:**
- SignalR hub implementation
- MassTransit consumers
- Real-time event consumers

#### 5.2 Implement OperationsHub (SignalR)
**File:** `src/modules/operations/CreditRisk.Operations.Server/Hubs/OperationsHub.cs`

**Responsibilities:**
1. Handle SignalR client connections
2. Add clients to role-based groups
3. Require JWT authentication

**Details:**
- Requires `[Authorize]` attribute
- `JoinRoleGroup(string role)` method
- Maps roles: `desk-operator` → `role:desk-operator`, etc.
- On disconnect: Remove from all groups

**Acceptance Criteria:**
- [ ] Extends `Hub` class
- [ ] Requires authentication
- [ ] Has `JoinRoleGroup(role)` method
- [ ] Adds connection to role group
- [ ] Removes from groups on disconnect

#### 5.3 Implement Event Consumers
**Files to create:**
- `src/modules/operations/CreditRisk.Operations.Server/Consumers/AmlAlertCreatedEventConsumer.cs`
- `src/modules/operations/CreditRisk.Operations.Server/Consumers/CreditLimitApprovedEventConsumer.cs`
- `src/modules/operations/CreditRisk.Operations.Server/Consumers/TransactionFlaggedEventConsumer.cs`

**Key Details:**
- `AmlAlertCreatedEventConsumer` → Push to `role:compliance-analyst` and `role:administrator` groups
- Queue names: `operations-hub_{event-name}`
- Use `IHubContext<OperationsHub>` to push to groups
- Fire-and-forget: Don't fail if no clients connected
- Never throw on SignalR push failure

**Acceptance Criteria:**
- [ ] Each consumer implements `IConsumer<TEvent>`
- [ ] Uses `IHubContext<OperationsHub>` to send messages
- [ ] Pushes to role-based groups
- [ ] Handles SignalR failures gracefully
- [ ] Doesn't affect MassTransit consumer status

---

### Phase 6: Configuration & Environment (1-2 hours)

#### 6.1 Add Environment Variables
**File:** `.env` and documentation

**Variables:**
```
# Worker-specific
RABBITMQ__HOST=rabbitmq
RABBITMQ__VHOST=crcl
RABBITMQ__USERNAME=crcl_broker
RABBITMQ__PASSWORD=<secret>

# Bureau
BUREAU__BASEURL=http://bureau-mock:8080
BUREAU__APIKEY=<secret>

# PEP Screening
PEPSCREENING__BASEURL=http://pep-mock:8080
PEPSCREENING__APIKEY=<secret>
PEPSCREENING__CACHETTLHOURS=24

# AML Rules
AMLRULES__SMURFINGWINDOWHOURS=24
AMLRULES__SMURFINGTRESHOLDAMOUNT=10000
AMLRULES__SMURFINGCOMBINEDTHRESHOLD=30000

# SignalR (Operations Server)
SIGNALR__BACKPLANEREDIS=<redis-connection>
```

**Acceptance Criteria:**
- [ ] All environment variables documented
- [ ] Defaults provided for development
- [ ] Secrets not hardcoded
- [ ] Connection strings read from configuration

#### 6.2 Update Worker Program.cs Files
**Each worker needs:**
1. Observability configuration
2. MassTransit registration
3. Infrastructure registration
4. HTTP clients registration
5. Background service registration (for Outbox)

**Acceptance Criteria:**
- [ ] All services registered in dependency injection
- [ ] Observability enabled
- [ ] MassTransit configured with retry/circuit breaker
- [ ] Named HTTP clients configured with resilience
- [ ] Database migrations run on startup (or manual step)

---

### Phase 7: Integration Tests (3-4 hours)

#### 7.1 Create Integration Test Projects
**Projects to create:**
- `tests/integration/CreditRisk.CreditAnalysis.Integration.Tests/`
- `tests/integration/CreditRisk.Compliance.Integration.Tests/`

#### 7.2 Write Consumer Tests
**Test scenarios per SPEC-04 (Section 8):**

**CreditAnalysisWorkerTests:**
- [ ] Valid proposal → published CreditProposalEvaluatedEvent
- [ ] Bureau unavailable → published with degraded score
- [ ] Bureau timeout → circuit breaker → degraded result
- [ ] Duplicate message (idempotency) → single result
- [ ] Missing proposal → log error, don't crash

**ComplianceWorkerTests:**
- [ ] Valid transaction → creates alerts for triggered rules
- [ ] PEP match → creates Critical alert
- [ ] Duplicate transaction → skipped (idempotent)
- [ ] All 5 AML rules evaluated in order
- [ ] No flags → no alerts, no events

**OutboxProcessorTests:**
- [ ] Pending messages → published and marked processed
- [ ] Max retries exceeded → skipped
- [ ] Deserialization error → marked failed, not retried

#### 7.3 Use Testcontainers
**For each test:**
- PostgreSQL container (for database)
- RabbitMQ container (for MassTransit testing)
- Redis container (for caching)
- Use `MassTransit.Testing` `ITestHarness`

**Acceptance Criteria:**
- [ ] All integration tests pass
- [ ] Use real containers (Testcontainers)
- [ ] Assert on published/consumed messages
- [ ] Assert on database state changes
- [ ] Clean up resources in DisposeAsync()

---

### Phase 8: Verification & Documentation (2-3 hours)

#### 8.1 AOT Compilation Check
```bash
dotnet publish src/modules/credit-analysis/CreditRisk.CreditAnalysis.Worker/ \
  -c Release -r linux-x64 --self-contained -p:PublishAot=true
# Must complete with 0 ILC warnings
```

**Acceptance Criteria:**
- [ ] Credit Analysis Worker publishes with `PublishAot=true` successfully
- [ ] Compliance Worker publishes with `PublishAot=true` successfully
- [ ] 0 ILC warnings
- [ ] 0 IL3050/IL2026 warnings

#### 8.2 Local Execution Verification
```bash
# 1. Start infrastructure
docker compose up -d postgres redis rabbitmq

# 2. Apply migrations
dotnet ef database update (for both workers)

# 3. Run workers
dotnet run --project src/modules/credit-analysis/CreditRisk.CreditAnalysis.Worker/
dotnet run --project src/modules/compliance/CreditRisk.Compliance.Worker/
dotnet run --project src/modules/operations/CreditRisk.Operations.Server/

# 4. Verify queues in RabbitMQ UI
open http://localhost:15672

# 5. Run integration tests
dotnet test tests/integration/
```

**Acceptance Criteria:**
- [ ] All workers start without errors
- [ ] Queues created in RabbitMQ (no errors)
- [ ] Integration tests pass
- [ ] Can manually publish events and see them consumed

#### 8.3 Add to Solution File
**Update:** `CreditRiskComplianceLab.sln`

Add project entries:
```xml
<Project>... = "CreditRisk.CreditAnalysis.Worker"</Project>
<Project>... = "CreditRisk.Compliance.Worker"</Project>
<Project>... = "CreditRisk.Operations.Server"</Project>
<Project>... = "CreditRisk.CreditAnalysis.Integration.Tests"</Project>
<Project>... = "CreditRisk.Compliance.Integration.Tests"</Project>
```

**Acceptance Criteria:**
- [ ] All new projects added to solution
- [ ] Solution builds without errors
- [ ] Solution can be opened in Visual Studio

#### 8.4 Update Documentation
**Files to create/update:**
- `docs/INTEGRATION_GUIDE.md` - How to run workers locally
- `docs/MESSAGING_TOPOLOGY.md` - RabbitMQ topology diagram
- Update `setup.md` with worker startup instructions

**Acceptance Criteria:**
- [ ] All integration architecture documented
- [ ] Queue/exchange names documented
- [ ] Consumer responsibilities documented
- [ ] Error handling documented
- [ ] Example usage provided

---

## Dependency Map

```
CreditRisk.CreditAnalysis.Worker
├── CreditRisk.CreditAnalysis.Domain
├── CreditRisk.CreditAnalysis.Application
├── CreditRisk.CreditAnalysis.Infrastructure
├── CreditRisk.Shared.Contracts (events)
├── CreditRisk.Shared.Kernel (outbox)
├── CreditRisk.Shared.Observability
├── MassTransit 8.3.6
├── MassTransit.RabbitMQ 8.3.6
├── Microsoft.Extensions.Http.Resilience 9.3.0
├── StackExchange.Redis 2.8.16
└── OpenTelemetry (full stack)

CreditRisk.Compliance.Worker
├── CreditRisk.Compliance.Domain
├── CreditRisk.Compliance.Application
├── CreditRisk.Compliance.Infrastructure
├── CreditRisk.Shared.Contracts (commands/events)
├── CreditRisk.Shared.Kernel (outbox)
├── CreditRisk.Shared.Observability
├── MassTransit 8.3.6
├── MassTransit.RabbitMQ 8.3.6
├── StackExchange.Redis 2.8.16
└── OpenTelemetry (full stack)

CreditRisk.Operations.Server
├── CreditRisk.Shared.Contracts (events)
├── CreditRisk.Shared.Observability
├── MassTransit 8.3.6
├── MassTransit.RabbitMQ 8.3.6
├── Microsoft.AspNetCore.SignalR
├── StackExchange.Redis 2.8.16
└── OpenTelemetry (full stack)
```

---

## Timeline Estimate

| Phase | Task | Hours | Cumulative |
|-------|------|-------|-----------|
| 1 | Foundation & Infrastructure | 2.5 | 2.5h |
| 2 | MassTransit Configuration | 2.5 | 5h |
| 3 | Credit Analysis Worker | 3.5 | 8.5h |
| 4 | Compliance Worker | 3.5 | 12h |
| 5 | Operations Hub | 2.5 | 14.5h |
| 6 | Configuration & Environment | 1.5 | 16h |
| 7 | Integration Tests | 3.5 | 19.5h |
| 8 | Verification & Documentation | 2.5 | 22h |
| **Total** | — | **22 hours** | — |

**Realistic estimate with debugging/iterations: 25-30 hours**

---

## Key Technical Decisions

### 1. Explicit Consumer Registration (NOT Assembly Scanning)
**Why:** Assembly scanning uses reflection which is incompatible with Native AOT trimming.
```csharp
// ✅ AOT-compatible
x.AddConsumer<CreditProposalCreatedEventConsumer>();

// ❌ NOT AOT-compatible
x.AddConsumers(typeof(Program).Assembly);
```

### 2. OutboxPattern Over Direct Publishing
**Why:** Guarantees at-least-once delivery. Without outbox, a crash between SaveChanges and Publish leaves inconsistency.

### 3. MassTransit Over Raw RabbitMQ
**Why:** Provides retry, circuit breaker, DLQ, OpenTelemetry propagation out-of-the-box.

### 4. Polly Resilience for Bureau HTTP
**Why:** Automatic retry, circuit breaker, and degraded mode fallback without blocking pipeline.

### 5. Redis Caching for Bureau/PEP (24h TTL)
**Why:** Reduces external API calls, improves performance, reduces costs.

### 6. Fire-and-Forget SignalR Push
**Why:** If no clients connected, silently drop (don't fail MassTransit consumer).

---

## Risk Mitigation

| Risk | Mitigation |
|------|-----------|
| RabbitMQ topology not created | Verify with RabbitMQ UI after startup; check logs |
| Bureau API unavailable | Graceful degradation (score=0 → manual review) |
| PEP API unavailable | Cache + fallback to false (no PEP match) |
| Duplicate message processing | Idempotency checks in consumers |
| Outbox processor stuck | Monitor via Prometheus; alert on stuck messages |
| AOT compilation failures | Test early; avoid reflection-based patterns |
| SignalR disconnections | Fire-and-forget approach; no retry on push |

---

## Implementation Checklist

### Phase 1: Foundation ✅
- [ ] Create message contracts (Events, Commands) in `CreditRisk.Shared.Contracts`
- [ ] Implement OutboxMessage entity in `CreditRisk.Shared.Kernel`
- [ ] Create 3 worker projects (.csproj files)
- [ ] Add projects to solution file
- [ ] Create Bureau Mock Service (ASP.NET Core Minimal API)
- [ ] Verify all projects compile

### Phase 2: MassTransit ✅
- [ ] Configure MassTransit in Credit Analysis Worker
- [ ] Configure MassTransit in Compliance Worker  
- [ ] Configure MassTransit in Operations Server
- [ ] Configure named HttpClient for bureau API
- [ ] Verify all services boot without errors
- [ ] Test RabbitMQ connection

### Phase 3: Credit Analysis Worker ✅
- [ ] Implement CreditProposalCreatedEventConsumer
- [ ] Implement BureauHttpClient with Polly
- [ ] Implement OutboxRepository in infrastructure
- [ ] Implement OutboxProcessor as BackgroundService
- [ ] Add Outbox to DbContext
- [ ] Write 5+ integration tests
- [ ] All tests passing

### Phase 4: Compliance Worker ✅
- [ ] Implement TransactionReceivedCommandConsumer
- [ ] Implement PepScreeningService
- [ ] Add PEP screening to consumer flow
- [ ] Create alerts for flagged transactions
- [ ] Write 5+ integration tests
- [ ] All tests passing

### Phase 5: Operations Hub ✅
- [ ] Create Operations.Server project
- [ ] Implement OperationsHub (SignalR)
- [ ] Implement AmlAlertCreatedEventConsumer
- [ ] Implement CreditLimitApprovedEventConsumer
- [ ] Implement TransactionFlaggedEventConsumer
- [ ] Write 3+ integration tests
- [ ] SignalR connections working

### Phase 6: Configuration ✅
- [ ] Update .env with all variables
- [ ] Configure each worker's Program.cs
- [ ] Add environment variable documentation
- [ ] Create appsettings.json per environment
- [ ] Verify configuration loads correctly

### Phase 7: Integration Tests ✅
- [ ] Create test base class with Testcontainers
- [ ] Write CreditAnalysis worker tests (5+)
- [ ] Write Compliance worker tests (5+)
- [ ] Write Outbox processor tests (3+)
- [ ] Write Operations Hub tests (3+)
- [ ] All 25+ tests passing
- [ ] Testcontainers cleanup working

### Phase 8: Verification ✅
- [ ] Add projects to solution
- [ ] Solution builds without errors
- [ ] Publish with `PublishAot=true` (0 warnings)
- [ ] Run local end-to-end flow
- [ ] Verify RabbitMQ topology in UI
- [ ] Write integration guide (Markdown)
- [ ] Write messaging topology docs
- [ ] Add XML documentation
- [ ] Create example workflows

---

---

## Decisions Made ✅

| Decision | Choice | Rationale |
|----------|--------|-----------|
| **Order of Implementation** | Start immediately with Phase 1 | Clear priority for sprint execution |
| **Bureau Mock Service** | Create full service (HTTP server) | Better dev experience, more realistic testing |
| **Bureau Mock Port** | 8081 | Avoid conflicts with other services |
| **Bureau Mock Tech** | ASP.NET Core Minimal API | Consistency with rest of system |
| **Bureau Mock Endpoints** | POST /query, GET /health | Standard patterns |
| **Testcontainers** | Official packages (PostgreSql, RabbitMq, Redis) | Best support and stability |
| **Testcontainers Versions** | Pinned versions (not latest) | Reproducible builds, stability |
| **Integration Tests Location** | `tests/integration/` (root) | Clear separation from unit tests |
| **Consumer Implementation Order** | Credit Analysis first | Simpler, fewer dependencies |
| **Documentation Format** | Both (Markdown + XML comments) | Best of both worlds |

---

## Acceptance Criteria Summary

### Must Have (MVP)
- [ ] All workers create and connect to RabbitMQ
- [ ] All consumers implemented and tested
- [ ] Outbox pattern working end-to-end
- [ ] Bureau HTTP client with resilience
- [ ] PEP screening service
- [ ] Operations Hub with SignalR
- [ ] 20+ integration tests passing
- [ ] AOT compilation successful

### Should Have
- [ ] Comprehensive error handling
- [ ] Detailed logging/tracing
- [ ] Performance optimizations
- [ ] Documentation & examples

### Could Have
- [ ] Prometheus metrics
- [ ] Health checks
- [ ] Admin endpoints
- [ ] UI dashboard

---

## Next Steps for Implementation

### Immediate (Upon plan_exit)
1. **Start Phase 1:** Create shared contracts and worker projects
2. **Create Bureau Mock Service:** ASP.NET Core API on port 8081
3. **Setup Testcontainers:** Use pinned versions in Directory.Packages.props

### Weekly Checkpoints
- **Week 1 (Batch 1):** All workers compile and connect to infrastructure
- **Week 2 (Batch 2):** Credit Analysis Worker fully functional with tests
- **Week 3 (Batch 3):** Compliance Worker fully functional with tests
- **Week 4 (Batch 4):** Operations Hub working with SignalR
- **Week 5 (Batch 5):** Full verification, AOT compilation, documentation

### Quality Gates
- ✅ All workers compile with `PublishAot=true` (0 ILC warnings)
- ✅ 25+ integration tests passing
- ✅ RabbitMQ topology correct (verified in UI)
- ✅ Local end-to-end flow working
- ✅ Documentation complete (Markdown + XML)

---

*Plan created: 2026-08-26*  
*Decisions finalized: 2026-08-26*  
*Status: ✅ READY FOR IMPLEMENTATION*
