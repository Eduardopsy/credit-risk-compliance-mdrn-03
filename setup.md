
# Credit Risk Compliance Lab — Foundational Setup & Specification

> **Document Status:** Authoritative — All development decisions must align with this specification.
> **Version:** 1.0.0
> **Last Updated:** 2026-07-16
> **Scope:** Full-stack application — Backend, Frontend, Workers, Infrastructure, Integrations

---

## Table of Contents

1. [Project Overview and Global Behavioral Context](#1-project-overview-and-global-behavioral-context)
2. [Code Style Rules and Coding Standards](#2-code-style-rules-and-coding-standards)
3. [Strict Technological Constraints](#3-strict-technological-constraints)
4. [AI Response Protocol](#4-ai-response-protocol)
5. [Docker and Container Architecture](#5-docker-and-container-architecture)
6. [Security Baseline](#6-security-baseline)
7. [Observability Standards](#7-observability-standards)
8. [Module Boundaries and Integration Contracts](#8-module-boundaries-and-integration-contracts)

---

## 1. Project Overview and Global Behavioral Context

### 1.1 Purpose and Domain

**Credit Risk Compliance Lab** is a financial-grade platform for credit risk analysis, anti-fraud monitoring, and regulatory compliance. It operates within the Brazilian financial regulatory environment and must adhere to the standards set by the Banco Central do Brasil (BCB), COAF (Conselho de Controle de Atividades Financeiras), and the Lei Geral de Proteção de Dados (LGPD — Lei nº 13.709/2018).

The system serves three primary operator profiles:

| Role | Responsibility |
|---|---|
| **Desk Operator** | Submits and monitors credit proposals; views personal dashboards |
| **Compliance Analyst** | Reviews flagged transactions; approves or rejects AML/CFT alerts; exports regulatory reports |
| **Administrator** | Manages users, roles, system configuration, and audit logs |

### 1.2 Functional Modules

The application is composed of five bounded modules, each with a distinct responsibility:

| # | Module | Primary Concern |
|---|---|---|
| 1 | **IAM** | Authentication, MFA, RBAC, Audit Trail, Token Lifecycle |
| 2 | **Credit Analysis** | Onboarding, Rules Engine, Bureau Integration, Score, Approval Workflow |
| 3 | **Compliance / Anti-Fraud** | Transaction Ingestion, AML/CFT Rules, Async Processing, PEP/Sanctions Lists |
| 4 | **Operations Panel** | Blazor WASM Dashboard, Real-time Alerts, Manual Reconciliation, Report Export |
| 5 | **Infrastructure** | Shared contracts, messaging, observability, cross-cutting concerns |

### 1.3 Global Behavioral Expectations

Every architectural and implementation decision must be evaluated against the following non-negotiable behavioral principles:

**1. Financial-grade correctness over convenience.**
No shortcut that compromises data integrity, auditability, or security is acceptable — even in development environments. ACID compliance, idempotency, and immutable audit records are mandatory.

**2. Explicit over implicit.**
All business rules, validation logic, and domain constraints must be expressed in code, not assumed from framework defaults. FluentValidation rules, domain invariants, and guard clauses must be explicit and testable.

**3. Fail-safe by default.**
When a service is unavailable, the system must degrade gracefully. Credit proposals and transactions must never be silently dropped. Dead Letter Queues (DLQ) and retry policies via MassTransit are mandatory for all message consumers.

**4. Observability is not optional.**
Every service must emit structured logs, distributed traces, and metrics from day one. There is no "add observability later" — OpenTelemetry instrumentation is a first-class requirement, not an afterthought.

**5. Zero trust at every boundary.**
All inter-service communication must be authenticated. No service trusts another service implicitly. JWT validation, mTLS, and network segmentation via Docker networks enforce this.

**6. Containerization is absolute.**
No service, database, cache, broker, or observability tool runs outside a Docker container. Local development, CI/CD, and production all use the same containerized topology.

**7. Domain isolation is sacred.**
Business rules live exclusively in the Domain layer. No EF Core, no HTTP clients, no MassTransit references inside `Domain` or `Application` projects. Infrastructure concerns are injected via interfaces.

**8. Regulatory compliance is a first-class feature.**
LGPD data minimization, BCB reporting formats, COAF suspicious transaction reports (STR), and PEP/sanctions screening are not edge cases — they are core features that must be designed into the data model and workflows from the start.

### 1.4 Non-Functional Requirements Summary

| Requirement | Target |
|---|---|
| API response time (p99) | < 200ms for standard endpoints |
| Transaction ingestion throughput | > 5,000 TPS via Minimal API |
| Container startup time (Native AOT) | < 500ms |
| Audit log retention | Minimum 5 years (immutable) |
| Data encryption at rest | AES-256 via TDE |
| Data encryption in transit | TLS 1.3 minimum |
| Authentication token expiry | Access: 15 min / Refresh: 7 days |
| MFA | Required for all human operators |
| Availability target | 99.9% (three nines) |

---

## 2. Code Style Rules and Coding Standards

### 2.1 Language and Runtime

- **Language:** C# 12 / 13 — use primary constructors, collection expressions, `params` collections, and `ref readonly` parameters where they improve clarity.
- **Runtime:** .NET 8 (LTS) — robust, high-performance, Native AOT-ready.
- **Nullable reference types:** Enabled globally (`<Nullable>enable</Nullable>`) in every project. No `#nullable disable` pragmas without documented justification.
- **Implicit usings:** Enabled (`<ImplicitUsings>enable</ImplicitUsings>`).
- **Warnings as errors:** Enabled in CI (`<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`) — no suppressed warnings without a documented `#pragma warning disable` comment explaining the reason.

### 2.2 Naming Conventions

#### General Rules

| Construct | Convention | Example |
|---|---|---|
| Classes, Records, Structs | `PascalCase` | `CreditProposal`, `AuditEntry` |
| Interfaces | `IPascalCase` | `ICreditRepository`, `IAuditLogger` |
| Methods | `PascalCase` | `EvaluateRiskAsync`, `GetProposalById` |
| Properties | `PascalCase` | `CustomerName`, `RiskScore` |
| Fields (private) | `_camelCase` | `_repository`, `_logger` |
| Constants | `PascalCase` | `MaxCreditLimit`, `DefaultTimeoutSeconds` |
| Local variables | `camelCase` | `proposal`, `customerId` |
| Parameters | `camelCase` | `proposalId`, `cancellationToken` |
| Generic type parameters | `T` prefix | `TEntity`, `TResult` |
| Enums | `PascalCase` (singular) | `RiskRating`, `ProposalStatus` |
| Enum values | `PascalCase` | `RiskRating.High`, `ProposalStatus.PendingReview` |
| Async methods | `*Async` suffix | `CreateProposalAsync`, `ValidateCustomerAsync` |
| Test methods | `MethodName_Scenario_ExpectedResult` | `EvaluateRisk_HighDebtRatio_ReturnsRatingE` |

#### Domain-Specific Naming

- **Aggregates:** Named after the core domain concept, singular. Example: `CreditProposal`, `Customer`, `Transaction`.
- **Value Objects:** Named after what they represent, not how they are stored. Example: `Cpf`, `Cnpj`, `MoneyAmount`, `RiskScore`.
- **Domain Events:** Past tense, suffixed with `Event`. Example: `CreditProposalApprovedEvent`, `TransactionFlaggedEvent`.
- **Commands:** Imperative, suffixed with `Command`. Example: `CreateCreditProposalCommand`, `ApproveCreditLimitCommand`.
- **Queries:** Descriptive, suffixed with `Query`. Example: `GetCustomerCreditHistoryQuery`, `ListPendingProposalsQuery`.
- **Handlers:** Suffixed with `Handler`. Example: `CreateCreditProposalCommandHandler`, `GetCustomerCreditHistoryQueryHandler`.
- **DTOs:** Suffixed with `Dto` or `Request`/`Response`. Example: `CreditProposalDto`, `CreateProposalRequest`, `ProposalStatusResponse`.
- **Validators:** Suffixed with `Validator`. Example: `CreateProposalRequestValidator`, `CustomerOnboardingValidator`.

### 2.3 Project and Folder Structure

The solution follows **Clean Architecture** with **Domain-Driven Design (DDD)**. Each bounded context (module) is a separate solution folder containing its own layered projects.

```
credit-risk-compliance-lab/
├── setup.md                          ← This document (authoritative)
├── docker-compose.yml                ← Full local development stack
├── docker-compose.override.yml       ← Local dev overrides (not committed to prod)
├── .env.example                      ← Environment variable template
├── .editorconfig                     ← Enforced formatting rules
├── .gitignore
├── global.json                       ← Pins .NET 10 SDK version
├── Directory.Build.props             ← Shared MSBuild properties for all projects
├── Directory.Packages.props          ← Central NuGet package version management
│
├── src/
│   ├── modules/
│   │   ├── iam/                      ← Identity & Access Management module
│   │   │   ├── CreditRisk.IAM.Domain/
│   │   │   ├── CreditRisk.IAM.Application/
│   │   │   ├── CreditRisk.IAM.Infrastructure/
│   │   │   └── CreditRisk.IAM.Api/
│   │   │
│   │   ├── credit-analysis/          ← Credit Analysis module
│   │   │   ├── CreditRisk.CreditAnalysis.Domain/
│   │   │   ├── CreditRisk.CreditAnalysis.Application/
│   │   │   ├── CreditRisk.CreditAnalysis.Infrastructure/
│   │   │   ├── CreditRisk.CreditAnalysis.Api/
│   │   │   └── CreditRisk.CreditAnalysis.Worker/
│   │   │
│   │   ├── compliance/               ← Compliance / Anti-Fraud module
│   │   │   ├── CreditRisk.Compliance.Domain/
│   │   │   ├── CreditRisk.Compliance.Application/
│   │   │   ├── CreditRisk.Compliance.Infrastructure/
│   │   │   ├── CreditRisk.Compliance.Api/
│   │   │   └── CreditRisk.Compliance.Worker/
│   │   │
│   │   └── operations/               ← Operations Panel (Blazor WASM)
│   │       ├── CreditRisk.Operations.Client/   ← Blazor WASM project
│   │       └── CreditRisk.Operations.Server/   ← Blazor host / SignalR hub
│   │
│   └── shared/
│       ├── CreditRisk.Shared.Contracts/        ← Message contracts (MassTransit)
│       ├── CreditRisk.Shared.Kernel/           ← Base classes, Value Objects, Guard clauses
│       └── CreditRisk.Shared.Observability/    ← OpenTelemetry setup, structured logging helpers
│
├── tests/
│   ├── unit/
│   │   ├── CreditRisk.IAM.Domain.Tests/
│   │   ├── CreditRisk.CreditAnalysis.Domain.Tests/
│   │   └── CreditRisk.Compliance.Domain.Tests/
│   ├── integration/
│   │   ├── CreditRisk.IAM.Integration.Tests/
│   │   ├── CreditRisk.CreditAnalysis.Integration.Tests/
│   │   └── CreditRisk.Compliance.Integration.Tests/
│   └── e2e/
│       └── CreditRisk.E2E.Tests/
│
├── infra/
│   ├── docker/
│   │   ├── api.Dockerfile
│   │   ├── worker.Dockerfile
│   │   ├── frontend.Dockerfile
│   │   └── nginx/
│   │       └── nginx.conf
│   ├── k8s/                          ← Kubernetes manifests (future)
│   └── scripts/
│       ├── init-db.sql
│       └── seed-data.sql
│
└── docs/
    ├── architecture/
    │   ├── adr/                      ← Architecture Decision Records
    │   └── diagrams/
    └── api/
        └── openapi/                  ← OpenAPI specs per module
```

#### Layer Responsibilities per Module

```
Module/
├── Domain/           ← Entities, Aggregates, Value Objects, Domain Events,
│                        Repository Interfaces, Domain Services, Specifications
├── Application/      ← Use Cases (Commands/Queries), DTOs, Validators (FluentValidation),
│                        Application Services, IUnitOfWork, Port interfaces
├── Infrastructure/   ← EF Core DbContext, Dapper queries, Repository implementations,
│                        External HTTP clients (Bureau, PEP lists), MassTransit producers,
│                        Redis clients, Email/SMS adapters
└── Api/              ← Minimal API endpoint definitions, Middleware, Filters,
                         Request/Response mapping, OpenAPI annotations
```

### 2.4 File Organization Rules

- **One class per file.** No exceptions. File name must match the class name exactly.
- **Namespace must mirror folder structure.** `CreditRisk.CreditAnalysis.Domain.Entities.CreditProposal` lives in `src/modules/credit-analysis/CreditRisk.CreditAnalysis.Domain/Entities/CreditProposal.cs`.
- **No `using static` in production code** unless it is a well-known utility (e.g., `using static System.Math`).
- **Partial classes are forbidden** except for EF Core model snapshots and auto-generated code.
- **Region directives (`#region`) are forbidden.** If a class is large enough to need regions, it must be refactored.

### 2.5 Formatting Rules

The `.editorconfig` file at the repository root enforces all formatting. Key rules:

```ini
[*.cs]
indent_style = space
indent_size = 4
end_of_line = lf
charset = utf-8-bom
trim_trailing_whitespace = true
insert_final_newline = true

# Braces on new lines (Allman style)
csharp_new_line_before_open_brace = all
csharp_new_line_before_else = true
csharp_new_line_before_catch = true
csharp_new_line_before_finally = true

# Prefer explicit types for built-ins, var for complex types
csharp_style_var_for_built_in_types = false:warning
csharp_style_var_when_type_is_apparent = true:suggestion
csharp_style_var_elsewhere = false:suggestion

# Expression-bodied members: only for trivial single-expression properties
csharp_style_expression_bodied_methods = false:warning
csharp_style_expression_bodied_properties = when_on_single_line:suggestion

# Prefer pattern matching
csharp_style_pattern_matching_over_is_with_cast_check = true:warning
csharp_style_pattern_matching_over_as_with_null_check = true:warning
```

### 2.6 Commenting Standards

- **XML documentation comments** are mandatory on all `public` and `internal` members in `Domain` and `Application` layers.
- **Inline comments** must explain *why*, not *what*. Code must be self-documenting.
- **TODO comments** must include a ticket reference: `// TODO [CRCL-123]: Replace with async bureau call`.
- **HACK and FIXME** comments are forbidden in merged code. They must be resolved before PR approval.
- **Dead code** must be deleted, not commented out.

```csharp
/// <summary>
/// Evaluates the credit risk of a proposal using the internal scoring algorithm.
/// Returns a <see cref="RiskRating"/> from A (lowest risk) to E (highest risk).
/// </summary>
/// <param name="proposal">The credit proposal to evaluate. Must not be null.</param>
/// <param name="cancellationToken">Propagates notification that operations should be cancelled.</param>
/// <returns>A <see cref="RiskRating"/> representing the assessed risk level.</returns>
/// <exception cref="ArgumentNullException">Thrown when <paramref name="proposal"/> is null.</exception>
public async Task<RiskRating> EvaluateRiskAsync(
    CreditProposal proposal,
    CancellationToken cancellationToken = default)
```

### 2.7 C# 14 Specific Guidelines

- **Primary constructors** are preferred for simple dependency injection in Application and Infrastructure layers.
- **Collection expressions** (`[item1, item2]`) are preferred over `new List<T> { }` for initializing collections.
- **`params` collections** must be used when a method accepts a variable number of arguments of the same type.
- **`ref readonly` parameters** must be used for large value types passed to performance-critical methods to avoid copying.
- **Records** are mandatory for all DTOs, Commands, Queries, and Domain Events. They must be immutable.
- **`required` properties** must be used on all record types where a property is non-optional.
- **Pattern matching** (`switch` expressions, `is` patterns) is preferred over chains of `if/else` for type discrimination.
- **`async`/`await`** must be used consistently. Never use `.Result` or `.Wait()` on `Task` — this causes deadlocks.
- **`ConfigureAwait(false)`** must be used in all library/infrastructure code that does not need to return to the original synchronization context.
- **`CancellationToken`** must be threaded through every async call chain. No async method may ignore a `CancellationToken` parameter.

### 2.8 Error Handling Standards

- **Domain exceptions** must extend a base `DomainException` class defined in `CreditRisk.Shared.Kernel`.
- **Application-layer results** must use a `Result<T>` or `OneOf<TSuccess, TError>` pattern — never throw exceptions for expected business failures.
- **Infrastructure exceptions** (database timeouts, HTTP failures) must be caught at the boundary and translated into domain-meaningful errors.
- **Global exception middleware** in each API project must catch unhandled exceptions, log them with full context, and return RFC 7807 Problem Details responses.
- **Never expose stack traces** in API responses in any environment.

```csharp
// Correct: Result pattern for expected failures
public async Task<Result<CreditProposalDto>> CreateProposalAsync(
    CreateProposalCommand command,
    CancellationToken cancellationToken)
{
    var validation = await _validator.ValidateAsync(command, cancellationToken);
    if (!validation.IsValid)
        return Result.Failure<CreditProposalDto>(validation.Errors);

    // ... domain logic
    return Result.Success(proposalDto);
}
```

### 2.9 Test Standards

- **Unit tests:** xUnit + FluentAssertions + NSubstitute. Test only Domain and Application layers in isolation.
- **Integration tests:** xUnit + Testcontainers for .NET (spin up real PostgreSQL, Redis, RabbitMQ in Docker).
- **Coverage target:** Minimum 80% line coverage on Domain and Application layers.
- **Test naming:** `MethodName_Scenario_ExpectedResult` format is mandatory.
- **No `Thread.Sleep` in tests.** Use `WaitForConditionAsync` helpers or Testcontainers readiness checks.
- **Test data builders** (Builder pattern) must be used for complex domain object construction in tests.

---

## 3. Strict Technological Constraints

### 3.1 Mandatory Technologies

The following technologies are **required** and cannot be substituted without an Architecture Decision Record (ADR) approved by the technical lead:

| Category | Mandatory Technology | Version |
|---|---|---|
| Language | C# | 12 / 13 |
| Runtime | .NET | 8 (LTS) |
| Backend framework | ASP.NET Core Minimal APIs | 8.x |
| ORM (transactional) | Entity Framework Core | 8.x |
| ORM (reporting/complex queries) | Dapper | Latest stable |
| Frontend framework | Blazor WebAssembly | .NET 8 |
| UI component library | MudBlazor **or** Radzen Blazor | Latest stable (MudBlazor 7.x) |
| Real-time communication | SignalR | .NET 8 |
| Message broker | RabbitMQ **or** Azure Service Bus | RabbitMQ 3.13+ |
| Messaging abstraction | MassTransit | 8.x |
| Relational database | PostgreSQL **or** SQL Server | PostgreSQL 16+ |
| Cache / Token store | Redis | 7.x |
| Identity provider | Duende IdentityServer **or** Keycloak | Keycloak 24+ |
| Validation | FluentValidation | 11.x |
| Containerization | Docker | 26+ |
| Container orchestration | Docker Compose (dev) / Kubernetes (prod) | Compose v2 |
| Observability | OpenTelemetry .NET SDK | Latest stable |
| Metrics backend | Prometheus | 2.x |
| Metrics visualization | Grafana | 10.x / 11.x |
| Structured log sink | Seq | 2024.x |
| Compilation target | Native AOT (.NET 8) | — |
| TLS version | TLS 1.3 | — |

### 3.2 Forbidden Technologies

The following are **explicitly forbidden** and must not appear in any production or development code:

| Forbidden | Reason |
|---|---|
| `System.Web` (classic ASP.NET) | Legacy; incompatible with .NET 10 |
| `WebForms` | Legacy; incompatible with .NET 10 |
| `WCF` (Windows Communication Foundation) | Legacy; use gRPC or REST |
| `AutoMapper` | Encourages anemic domain models; use explicit mapping methods or Mapster |
| `MediatR` (unless explicitly approved) | MassTransit handles messaging; avoid dual-bus patterns |
| `Newtonsoft.Json` | Use `System.Text.Json` exclusively |
| `log4net` / `NLog` / `Serilog` (as primary) | Use `Microsoft.Extensions.Logging` + OpenTelemetry; Serilog may be used only as a sink adapter |
| `Thread.Sleep` in production code | Use `Task.Delay` with `CancellationToken` |
| `.Result` / `.Wait()` on `Task` | Causes deadlocks; use `await` |
| Hardcoded connection strings | All configuration via environment variables or secrets management |
| Hardcoded secrets or API keys | Use Docker secrets, Kubernetes secrets, or Azure Key Vault |
| `dynamic` type | Defeats type safety; forbidden in Domain and Application layers |
| `object` as a return type from public APIs | Use strongly-typed generics |
| HTTP (non-TLS) in production | All production traffic must use HTTPS/TLS 1.3 |
| Synchronous database calls in API handlers | All database access must be async |

### 3.3 Technologies Requiring Explicit Justification

The following may be used but require a written ADR before adoption:

| Technology | Justification Required |
|---|---|
| gRPC | Must document why REST/Minimal API is insufficient |
| GraphQL (HotChocolate) | Must document consumer requirements |
| Elasticsearch | Must document why PostgreSQL full-text search is insufficient |
| MongoDB | Must document why PostgreSQL JSONB is insufficient |
| Azure Service Bus | Must document why RabbitMQ is insufficient for the use case |
| Mapster | Acceptable AutoMapper alternative; document mapping strategy |
| Polly | Acceptable for HTTP resilience; document retry/circuit-breaker policies |

### 3.4 Runtime and Compilation Constraints

- **`global.json`** must pin the .NET 8 SDK version used by the team (e.g. `8.0.129`, rollForward: `latestMinor`).
- **Native AOT** must be enabled for all API and Worker projects in Release builds:
  ```xml
  <PublishAot Condition="'$(Configuration)' == 'Release'">true</PublishAot>
  <PublishAot Condition="'$(Configuration)' == 'Debug'">false</PublishAot>
  <InvariantGlobalization>true</InvariantGlobalization>
  ```
- **Reflection-based serialization** is incompatible with Native AOT. All JSON serialization must use source-generated `JsonSerializerContext`:
  ```csharp
  [JsonSerializable(typeof(CreateProposalRequest))]
  [JsonSerializable(typeof(ProposalStatusResponse))]
  [JsonSerializable(typeof(PagedResult<ProposalListItemDto>))]
  [JsonSerializable(typeof(CreditRisk.Shared.Kernel.Common.HealthResponse))]
  [JsonSerializable(typeof(Microsoft.AspNetCore.Mvc.ProblemDetails))]
  [JsonSerializable(typeof(Dictionary<string, string[]>))]
  internal partial class ApiJsonContext : JsonSerializerContext { }
  ```
- **Anonymous types (e.g. `Results.Ok(new { status = "healthy" })`) are strictly forbidden in API endpoint returns.** In Native AOT / `CreateSlimBuilder`, anonymous types cannot be resolved by source generators and result in `503 Service Unavailable` or runtime serialization exceptions. Always return strongly-typed records such as `HealthResponse`, `ProblemDetails`, or dedicated DTOs registered in the module's `JsonSerializerContext`.
- **`JsonSerializerContext` partial class files must include `using System.Text.Json.Serialization;` explicitly.** `ImplicitUsings` does NOT inject this namespace for partial class attribute resolution — the compiler resolves `[JsonSerializable]` attributes before implicit usings are applied, causing `CS0246: JsonSourceGenerationContextAttribute not found`. Always add the explicit using directive:
  ```csharp
  // ✅ REQUIRED — ImplicitUsings does NOT cover this for partial class attributes
  using System.Text.Json.Serialization;

  [JsonSerializable(typeof(CreateProposalRequest))]
  internal partial class ApiJsonContext : JsonSerializerContext { }
  ```
- **`builder.Services.AddRouting()` must be called explicitly in every API `Program.cs` that uses `WebApplication.CreateSlimBuilder(args)`.** `CreateSlimBuilder` internally calls `AddRoutingCore()` which does NOT register built-in route constraints (e.g., `{id:guid}`, `{id:int}`, `{id:long}`). Without `AddRouting()`, any route with a type constraint will throw `RegexErrorStubRouteConstraint` at runtime and return 500 for all matching requests:
  ```csharp
  // ✅ REQUIRED after WebApplication.CreateSlimBuilder(args)
  // CreateSlimBuilder uses AddRoutingCore() — does NOT register {id:guid}, {id:int}, etc.
  builder.Services.AddRouting();
  ```
- **Kestrel `ListenAnyIP(PORT)` must be configured explicitly in every API and Bureau Mock service.** By default, `CreateSlimBuilder` binds only to `localhost`, which prevents container-to-container and external access:
  ```csharp
  // ✅ REQUIRED Kestrel port binding
  builder.WebHost.ConfigureKestrel(options =>
  {
      options.ListenAnyIP(5000); // 5000 IAM, 5001 CreditAnalysis, 5002 Compliance, 5003 Operations, 8081 Bureau Mock
  });
  ```
- **Dynamic code generation** (`Reflection.Emit`, `Expression.Compile` at runtime) is forbidden in AOT-published projects.
- **`IDesignTimeDbContextFactory<T>` is mandatory for every `DbContext` in infrastructure projects.** When running `dotnet ef` CLI commands (`migrations add`, `database update`), EF Core attempts to build the ASP.NET Core web host by calling `Program.cs`. If any external dependency (e.g., synchronous `ConnectionMultiplexer.Connect(...)` to Redis or RabbitMQ) fails during Host creation, EF Core falls back to direct activation and fails with `Unable to resolve service for type DbContextOptions<T>`. Implementing `IDesignTimeDbContextFactory<T>` completely isolates EF Core design-time operations from the application Host, Redis, RabbitMQ, and external services, ensuring reliable migrations.
- **PostgreSQL Schema Isolation and Migration History:** In multi-module architectures sharing a single database (`creditrisk`), each module must target its dedicated schema (e.g., `SearchPath=iam`, `SearchPath=credit`, `SearchPath=compliance`) and configure its migration history table in that schema via `.MigrationsHistoryTable("__EFMigrationsHistory", "schema_name")`. This prevents table collisions (such as `outbox_messages` existing across modules in the `public` schema).
- **`Directory.Build.props`** must define shared properties for all projects to avoid duplication:
  ```xml
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>false</TreatWarningsAsErrors>
    <NoWarn>$(NoWarn);NU1902;NU1903</NoWarn>
    <AnalysisMode>All</AnalysisMode>
    <LangVersion>latest</LangVersion>
  </PropertyGroup>
  ```
- **`Directory.Packages.props`** manages package version properties centrally.

### 3.5 Container Constraints

- **Every service runs in a Docker container.** No exceptions for any environment (local, CI, staging, production).
- **Base images** must use official Microsoft or vendor-provided images. No community-maintained base images without security review.
- **Non-root containers:** All application containers must run as a non-root user.
- **Read-only filesystems:** API and Worker containers must use `--read-only` where possible, with explicit writable volume mounts only for temp directories.
- **No `latest` tags** in any `Dockerfile` or `docker-compose.yml`. All image versions must be pinned.
- **Multi-stage builds** are mandatory for all application Dockerfiles to minimize final image size.
- **Health checks** must be defined for every service in `docker-compose.yml`.

### 3.6 Security Constraints

- **TLS 1.3** is the minimum acceptable TLS version. TLS 1.0 and 1.1 must be explicitly disabled.
- **JWT access tokens** expire in 15 minutes maximum.
- **Refresh tokens** expire in 7 days and must be stored in Redis with revocation support.
- **MFA (TOTP)** is mandatory for all human operator accounts. Service accounts use client credentials flow.
- **Passwords** must be hashed using ASP.NET Core Identity's default PBKDF2 with HMAC-SHA512 (minimum 100,000 iterations).
- **SQL injection** is prevented by using parameterized queries exclusively. String interpolation in SQL is forbidden.
- **CORS** must be explicitly configured. Wildcard origins (`*`) are forbidden in non-development environments.

---

## 4. AI Response Protocol

### 4.1 Purpose

This section defines how the AI assistant (Copilot, Claude, or any LLM-based tool) must behave throughout all development interactions for this project. These rules ensure that AI-generated code, architecture guidance, and technical responses are consistent with the project's standards and do not introduce technical debt or security vulnerabilities.

### 4.2 Response Format Rules

**Rule 1 — Always reference this document.**
Before generating any code or architectural guidance, the AI must verify alignment with `setup.md`. If a request conflicts with a constraint defined here, the AI must explicitly flag the conflict before proceeding.

**Rule 2 — Structured responses for complex topics.**
For any response involving architecture decisions, code generation, or technology choices, use the following structure:

```
## Context
[Brief restatement of the request to confirm understanding]

## Recommendation / Implementation
[Primary solution with rationale tied to project constraints]

## Code / Artifact
[Complete, compilable code — no placeholders or "// ... rest of implementation"]

## Constraints Applied
[Which rules from setup.md govern this response]

## Trade-offs and Alternatives
[When relevant: what was considered and why it was not chosen]
```

**Rule 3 — No partial code.**
Every code snippet must be complete and compilable. Placeholders like `// TODO: implement`, `// ... rest of code`, or `throw new NotImplementedException()` are forbidden in AI-generated production code. If the full implementation cannot be provided in one response, the AI must explicitly state what is missing and why.

**Rule 4 — Layer awareness.**
Every generated class must include its correct namespace, matching the folder structure defined in Section 2.3. The AI must never place infrastructure concerns (EF Core, HTTP clients) in Domain or Application projects.

**Rule 5 — Explicit dependency injection.**
All generated code must use constructor injection. No `ServiceLocator` pattern, no `IServiceProvider` resolved inside domain or application classes.

### 4.3 Code Generation Rules

- **Always generate the full class**, including namespace, using directives, XML documentation, and all methods.
- **Always use `async`/`await`** for any I/O operation. Never generate synchronous database or HTTP calls.
- **Always include `CancellationToken`** in every async method signature.
- **Always use `ConfigureAwait(false)`** in Infrastructure and Shared library code.
- **Always use records for DTOs, Commands, and Events.** Never generate mutable classes for these constructs.
- **Always use FluentValidation** for input validation. Never use `DataAnnotations` on domain or application models.
- **Always use the Result pattern** for application-layer methods that can fail for business reasons.
- **Always generate corresponding unit tests** when generating a new use case handler or domain service.
- **Always use source-generated JSON serialization** when generating API endpoint code.
- **Never generate hardcoded configuration values.** Use `IOptions<T>` pattern with environment variable binding.
- **Always use PostgreSQL-specific SQL syntax.** Never use MySQL syntax (`CREATE DATABASE IF NOT EXISTS`, `SHOW DATABASES`, `AUTO_INCREMENT`). For conditional schema creation use `CREATE SCHEMA IF NOT EXISTS`. For conditional database creation use the `\gexec` pattern: `SELECT 'CREATE DATABASE name' WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'name')\gexec`. Prefer schemas within a single database over multiple databases.
- **Always include runnable automation scripts (`run-services.sh` and `stop-services.sh`)** at the repository root as an inherent deliverable of any core, infrastructure, or back-end implementation (`SPEC-01` / `SPEC-02`). These scripts must orchestrate both infrastructure containers (Docker Compose) and build/launch background processes for all implemented API modules, verifying database readiness and providing health check and OpenAPI endpoint URLs.

### 4.4 Assumption Rules

**Allowed assumptions (AI may proceed without asking):**
- Standard .NET 10 / C# 14 language features are available.
- All services defined in `docker-compose.yml` are available and healthy.
- The folder structure defined in Section 2.3 exists.
- PostgreSQL is the chosen relational database (unless context specifies SQL Server).
- RabbitMQ is the chosen message broker (unless context specifies Azure Service Bus).
- Keycloak is the chosen identity provider (unless context specifies Duende IdentityServer).
- MudBlazor is the chosen Blazor component library (unless context specifies Radzen).

**Forbidden assumptions (AI must ask before proceeding):**
- Business rule logic not explicitly specified (e.g., exact scoring algorithm weights).
- Regulatory report formats not provided in the request.
- External API contracts (Bureau, PEP lists) not documented in the request.
- Database schema changes that affect existing tables with data.
- Breaking changes to message contracts in `CreditRisk.Shared.Contracts`.

### 4.5 Ambiguity Handling Protocol

When a request is ambiguous, the AI must:

1. **State the ambiguity explicitly** — identify the specific unclear point.
2. **Propose the most conservative interpretation** — the one least likely to introduce security or data integrity risks.
3. **Implement based on the conservative interpretation** — do not block on clarification for non-critical ambiguities.
4. **Flag the assumption clearly** — mark it with `// ASSUMPTION [CRCL-?]: <description>` in the generated code.

Example:
```csharp
// ASSUMPTION [CRCL-?]: Credit limit cap set to R$ 500,000 pending business rule confirmation.
// See: https://github.com/org/credit-risk-compliance-lab/issues/XX
private const decimal MaxCreditLimit = 500_000m;
```

### 4.6 Security Response Rules

- The AI must **never generate code that stores plaintext passwords**,

 exposes secrets in logs, or generates SQL via string concatenation.
- The AI must **always use parameterized queries** when generating Dapper or raw SQL code.
- The AI must **always validate JWT tokens** in generated middleware — never trust claims without signature verification.
- The AI must **flag any generated code** that requires elevated database permissions (e.g., `DROP`, `TRUNCATE`, `ALTER`) with an explicit security warning.
- The AI must **never generate self-signed certificate acceptance** (`ServerCertificateCustomValidationCallback = (_, _, _, _) => true`) in production code paths.
- The AI must **always use `SecureRandom` or `RandomNumberGenerator`** for cryptographic operations, never `System.Random`.

### 4.7 Architecture Compliance Rules

- The AI must **reject requests** that violate the Dependency Rule (e.g., Domain referencing Infrastructure).
- The AI must **always suggest the correct layer** when a developer asks where to place a new class.
- The AI must **always use MassTransit** for inter-module async communication — never raw RabbitMQ clients.
- The AI must **always generate OpenTelemetry spans** for new service operations, database calls, and external HTTP calls.
- The AI must **always include health check endpoints** when generating new API services.

---

## 5. Docker and Container Architecture

### 5.1 Containerization Mandate

Every component of the Credit Risk Compliance Lab runs inside a Docker container. This is an absolute constraint with no exceptions:

| Service | Container | Port | Image Base |
|---|---|---|---|
| IAM API | `crcl-iam-api` | 5000 | `mcr.microsoft.com/dotnet/aspnet:8.0` (AOT: `mcr.microsoft.com/dotnet/runtime-deps:8.0`) |
| Credit Analysis API | `crcl-credit-api` | 5001 | `mcr.microsoft.com/dotnet/runtime-deps:8.0` |
| Compliance API | `crcl-compliance-api` | 5002 | `mcr.microsoft.com/dotnet/runtime-deps:8.0` |
| Operations Server (SignalR Hub + Host) | `crcl-operations-server` | 5003 | `mcr.microsoft.com/dotnet/aspnet:8.0` |
| Bureau Mock Service | `crcl-bureau-mock` | 8081 | `mcr.microsoft.com/dotnet/aspnet:8.0` |
| Credit Analysis Worker | `crcl-credit-worker` | — | `mcr.microsoft.com/dotnet/runtime-deps:8.0` |
| Compliance Worker | `crcl-compliance-worker` | — | `mcr.microsoft.com/dotnet/runtime-deps:8.0` |
| Operations Frontend (WASM) | `crcl-frontend` | 80/443 | `nginx:1.27-alpine` |
| PostgreSQL | `crcl-postgres` | 5432 | `postgres:16-alpine` |
| Redis | `crcl-redis` | 6379 | `redis:7-alpine` |
| RabbitMQ | `crcl-rabbitmq` | 5672/15672 | `rabbitmq:3.13-management-alpine` |
| Keycloak | `crcl-keycloak` | 8080 | `quay.io/keycloak/keycloak:24` |
| Prometheus | `crcl-prometheus` | 9090 | `prom/prometheus:v2.52.0` |
| Grafana | `crcl-grafana` | 3000 | `grafana/grafana:10.4.2` |
| Seq | `crcl-seq` | 5341/8081 | `datalust/seq:2024` |
| Nginx (reverse proxy) | `crcl-nginx` | 80/443 | `nginx:1.27-alpine` |

### 5.2 Dockerfile Standards

#### Healthcheck Tool Availability by Base Image

Different base images have different tools available for health checks. Use the following table to select the appropriate healthcheck command:

| Base Image | curl | wget | bash /dev/tcp | perl | Recommended |
|---|---|---|---|---|---|
| `mcr.microsoft.com/dotnet/aspnet:10.0` | ✅ Yes | ✅ Yes | ✅ Yes | ✅ Yes | `curl -f http://localhost:8080/health` |
| `mcr.microsoft.com/dotnet/runtime-deps:10.0` | ❌ No | ❌ No | ✅ Yes | ✅ Yes | `bash -c 'exec 3<>/dev/tcp/127.0.0.1/8080 && ...'` |
| `nginx:1.27-alpine` | ❌ No | ✅ Yes (wget) | ✅ Yes | ✅ Yes | `wget -qO- http://127.0.0.1/health` |
| `quay.io/keycloak/keycloak:24` | ❌ No | ❌ No | ✅ Yes | ✅ Yes | `bash -c 'exec 3<>/dev/tcp/localhost/8080 && ...'` |
| `datalust/seq:2024` | ❌ No | ❌ No | ❌ No | ✅ Yes | `perl -e 'use IO::Socket::INET; ...'` |
| `postgres:16-alpine` | ❌ No | ❌ No | ❌ No | ❌ No | `pg_isready -U user -d database` |
| `redis:7-alpine` | ❌ No | ❌ No | ❌ No | ❌ No | `redis-cli ping` |
| `rabbitmq:3.13-management-alpine` | ❌ No | ❌ No | ❌ No | ❌ No | `rabbitmq-diagnostics ping` |

**Key Rules:**
- Always use explicit IPv4 addresses (`127.0.0.1`) instead of `localhost` for nginx and other services that may resolve to IPv6
- For `runtime-deps` images (AOT), use bash TCP checks or perl
- For Seq, use perl TCP checks (curl/wget not available)
- For databases/brokers, use native diagnostic tools
- **⚠️ Keycloak requires extended startup time.** On first run, Keycloak performs database schema migration which can take up to 3 minutes. Always configure `start_period: 180s` and `retries: 20`. Never use `curl` in Keycloak healthchecks — it is not available in the Keycloak image. Use bash TCP checks (`exec 3<>/dev/tcp/localhost/8080`) instead.

#### Multi-Stage Build Pattern (API / Worker)

All API and Worker Dockerfiles must follow this multi-stage pattern for Native AOT:

```dockerfile
# Stage 1: Build and publish (AOT compilation requires the SDK)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and restore (layer caching optimization)
COPY Directory.Build.props Directory.Packages.props global.json ./
COPY src/modules/credit-analysis/ src/modules/credit-analysis/
COPY src/shared/ src/shared/

RUN dotnet restore src/modules/credit-analysis/CreditRisk.CreditAnalysis.Api/CreditRisk.CreditAnalysis.Api.csproj

# Publish with Native AOT
RUN dotnet publish src/modules/credit-analysis/CreditRisk.CreditAnalysis.Api/CreditRisk.CreditAnalysis.Api.csproj \
    -c Release \
    -r linux-x64 \
    --self-contained true \
    -p:PublishAot=true \
    -o /app/publish

# Stage 2: Minimal runtime image (no SDK, no runtime — just the native binary)
FROM mcr.microsoft.com/dotnet/runtime-deps:10.0 AS final
WORKDIR /app

# Security: run as non-root
RUN addgroup --system --gid 1001 appgroup && \
    adduser --system --uid 1001 --ingroup appgroup appuser

COPY --from=build --chown=appuser:appgroup /app/publish .

USER appuser

EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=10s --start-period=15s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["./CreditRisk.CreditAnalysis.Api"]
```

#### Frontend Dockerfile (Blazor WASM + Nginx)

```dockerfile
# Stage 1: Build Blazor WASM
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/modules/operations/ src/modules/operations/
COPY src/shared/ src/shared/
COPY Directory.Build.props Directory.Packages.props global.json ./

RUN dotnet publish src/modules/operations/CreditRisk.Operations.Client/CreditRisk.Operations.Client.csproj \
    -c Release \
    -o /app/publish

# Stage 2: Serve with Nginx
FROM nginx:1.27-alpine AS final

COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
COPY infra/docker/nginx/nginx.conf /etc/nginx/nginx.conf

EXPOSE 80

HEALTHCHECK --interval=30s --timeout=5s \
    CMD wget -qO- http://localhost/health || exit 1
```

### 5.3 Docker Compose Structure

The `docker-compose.yml` at the repository root defines the complete local development stack. It is organized into named service groups using labels:

```yaml
# docker-compose.yml — excerpt showing structure
version: "3.9"

networks:
  crcl-backend:
    driver: bridge
    internal: true          # Backend services: no direct internet access
  crcl-frontend:
    driver: bridge          # Frontend + Nginx: internet-facing
  crcl-observability:
    driver: bridge
    internal: true          # Observability stack: isolated

volumes:
  postgres-data:
    driver: local
  redis-data:
    driver: local
  rabbitmq-data:
    driver: local
  keycloak-data:
    driver: local
  prometheus-data:
    driver: local
  grafana-data:
    driver: local
  seq-data:
    driver: local

services:
  # ── Infrastructure ──────────────────────────────────────────────
  postgres:
    image: postgres:16-alpine
    container_name: crcl-postgres
    restart: unless-stopped
    environment:
      POSTGRES_USER: ${POSTGRES_USER}
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD}
      POSTGRES_DB: ${POSTGRES_DB}
    volumes:
      - postgres-data:/var/lib/postgresql/data
      - ./infra/scripts/init-db.sql:/docker-entrypoint-initdb.d/init-db.sql:ro
    networks:
      - crcl-backend
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U ${POSTGRES_USER} -d ${POSTGRES_DB}"]
      interval: 10s
      timeout: 5s
      retries: 5
      start_period: 30s

  redis:
    image: redis:7-alpine
    container_name: crcl-redis
    restart: unless-stopped
    command: redis-server --requirepass ${REDIS_PASSWORD} --appendonly yes
    volumes:
      - redis-data:/data
    networks:
      - crcl-backend
    healthcheck:
      test: ["CMD", "redis-cli", "-a", "${REDIS_PASSWORD}", "ping"]
      interval: 10s
      timeout: 5s
      retries: 5

  rabbitmq:
    image: rabbitmq:3.13-management-alpine
    container_name: crcl-rabbitmq
    restart: unless-stopped
    environment:
      RABBITMQ_DEFAULT_USER: ${RABBITMQ_USER}
      RABBITMQ_DEFAULT_PASS: ${RABBITMQ_PASSWORD}
      RABBITMQ_DEFAULT_VHOST: crcl
    volumes:
      - rabbitmq-data:/var/lib/rabbitmq
    networks:
      - crcl-backend
    healthcheck:
      test: ["CMD", "rabbitmq-diagnostics", "ping"]
      interval: 15s
      timeout: 10s
      retries: 5
      start_period: 30s

  keycloak:
    image: quay.io/keycloak/keycloak:24.0.5
    container_name: crcl-keycloak
    restart: unless-stopped
    command: start-dev --import-realm
    environment:
      KC_DB: postgres
      KC_DB_URL: jdbc:postgresql://postgres:5432/${POSTGRES_DB}
      KC_DB_USERNAME: ${POSTGRES_USER}
      KC_DB_PASSWORD: ${POSTGRES_PASSWORD}
      KEYCLOAK_ADMIN: ${KEYCLOAK_ADMIN_USER}
      KEYCLOAK_ADMIN_PASSWORD: ${KEYCLOAK_ADMIN_PASSWORD}
    volumes:
      - ./infra/keycloak/realm-export.json:/opt/keycloak/data/import/realm-export.json:ro
    networks:
      - crcl-backend
    depends_on:
      postgres:
        condition: service_healthy
    healthcheck:
      test: ["CMD-SHELL", "exec 3<>/dev/tcp/localhost/8080 && echo -e 'GET /health/ready HTTP/1.1\\r\\nHost: localhost\\r\\n\\r\\n' >&3 && cat <&3 | grep -q '200 OK'"]
      interval: 30s
      timeout: 10s
      retries: 20          # Keycloak DB migration on first run can take up to 3 minutes
      start_period: 180s   # Allow 3 minutes before first health probe

  # ── Application Services ─────────────────────────────────────────
  iam-api:
    build:
      context: .
      dockerfile: infra/docker/api.Dockerfile
      args:
        PROJECT_PATH: src/modules/iam/CreditRisk.IAM.Api/CreditRisk.IAM.Api.csproj
    container_name: crcl-iam-api
    restart: unless-stopped
    environment:
      ASPNETCORE_ENVIRONMENT: Development
      ConnectionStrings__Postgres: ${POSTGRES_CONNECTION_STRING}
      ConnectionStrings__Redis: ${REDIS_CONNECTION_STRING}
      Keycloak__Authority: http://keycloak:8080/realms/crcl
      Seq__ServerUrl: http://seq:5341
      OTEL_EXPORTER_OTLP_ENDPOINT: http://otel-collector:4317
    networks:
      - crcl-backend
    depends_on:
      postgres:
        condition: service_healthy
      redis:
        condition: service_healthy
      keycloak:
        condition: service_healthy
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
      interval: 15s
      timeout: 5s
      retries: 3
      start_period: 20s

  # ── Observability ────────────────────────────────────────────────
  seq:
    image: datalust/seq:2024
    container_name: crcl-seq
    restart: unless-stopped
    environment:
      ACCEPT_EULA: Y
      SEQ_FIRSTRUN_ADMINPASSWORDHASH: ${SEQ_ADMIN_PASSWORD_HASH}
    volumes:
      - seq-data:/data
    networks:
      - crcl-observability
    healthcheck:
      test: ["CMD", "perl", "-e", "use IO::Socket::INET; my $s = IO::Socket::INET->new(PeerAddr => 'localhost:80', Timeout => 5) or exit 1; print $s \"GET /health HTTP/1.1\\r\\nHost: localhost\\r\\n\\r\\n\"; my $response = <$s>; exit($response =~ /200 OK/ ? 0 : 1)"]
      interval: 30s
      timeout: 10s
      retries: 3

  prometheus:
    image: prom/prometheus:v2.52.0
    container_name: crcl-prometheus
    restart: unless-stopped
    command:
      - "--config.file=/etc/prometheus/prometheus.yml"
      - "--storage.tsdb.path=/prometheus"
      - "--storage.tsdb.retention.time=30d"
    volumes:
      - ./infra/prometheus/prometheus.yml:/etc/prometheus/prometheus.yml:ro
      - prometheus-data:/prometheus
    networks:
      - crcl-observability
      - crcl-backend

  grafana:
    image: grafana/grafana:10.4.2
    container_name: crcl-grafana
    restart: unless-stopped
    environment:
      GF_SECURITY_ADMIN_USER: ${GRAFANA_ADMIN_USER}
      GF_SECURITY_ADMIN_PASSWORD: ${GRAFANA_ADMIN_PASSWORD}
      GF_USERS_ALLOW_SIGN_UP: "false"
    volumes:
      - grafana-data:/var/lib/grafana
      - ./infra/grafana/provisioning:/etc/grafana/provisioning:ro
    networks:
      - crcl-observability
    depends_on:
      - prometheus

  # ── Reverse Proxy ────────────────────────────────────────────────
  nginx:
    image: nginx:1.27-alpine
    container_name: crcl-nginx
    restart: unless-stopped
    ports:
      - "443:443"
      - "80:80"
    volumes:
      - ./infra/docker/nginx/nginx.conf:/etc/nginx/nginx.conf:ro
      - ./infra/certs:/etc/nginx/certs:ro
    networks:
      - crcl-frontend
      - crcl-backend
    depends_on:
      - iam-api
      - frontend
```

### 5.4 Environment Variable Management

- **`.env.example`** must be committed to the repository with all required variable names and placeholder values. Never commit `.env` files with real secrets.
- **Variable naming convention:** `SERVICE_COMPONENT_PROPERTY` in `SCREAMING_SNAKE_CASE`. Example: `POSTGRES_CONNECTION_STRING`, `REDIS_PASSWORD`, `KEYCLOAK_ADMIN_USER`.
- **Secret rotation:** All secrets must be rotatable without application restart. Use Redis for revocable tokens and Keycloak for credential rotation.
- **Production secrets:** Must use Docker Secrets (`docker secret create`) or Kubernetes Secrets with external secret management (e.g., HashiCorp Vault, Azure Key Vault).

```bash
# .env.example
POSTGRES_USER=crcl_user
POSTGRES_PASSWORD=CHANGE_ME_STRONG_PASSWORD
POSTGRES_DB=credit_risk_db
POSTGRES_CONNECTION_STRING=Host=postgres;Port=5432;Database=credit_risk_db;Username=crcl_user;Password=CHANGE_ME

REDIS_PASSWORD=CHANGE_ME_REDIS_PASSWORD
REDIS_CONNECTION_STRING=redis:6379,password=CHANGE_ME_REDIS_PASSWORD,ssl=false

RABBITMQ_USER=crcl_broker
RABBITMQ_PASSWORD=CHANGE_ME_RABBITMQ_PASSWORD

KEYCLOAK_ADMIN_USER=admin
KEYCLOAK_ADMIN_PASSWORD=CHANGE_ME_KEYCLOAK_PASSWORD

GRAFANA_ADMIN_USER=admin
GRAFANA_ADMIN_PASSWORD=CHANGE_ME_GRAFANA_PASSWORD

SEQ_ADMIN_PASSWORD_HASH=CHANGE_ME_SEQ_HASH
```

### 5.5 Network Segmentation

```
Internet
    │
    ▼
┌─────────────────────────────────────────────────────────────────┐
│  crcl-frontend network                                          │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │  Nginx (reverse proxy)  :443/:80                         │   │
│  └──────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
    │
    ▼
┌─────────────────────────────────────────────────────────────────┐
│  crcl-backend network (internal — no direct internet access)    │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────────┐  │
│  │  IAM API     │  │  Credit API  │  │  Compliance API      │  │
│  └──────────────┘  └──────────────┘  └──────────────────────┘  │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────────┐  │
│  │  Credit      │  │  Compliance  │  │  Operations Hub      │  │
│  │  Worker      │  │  Worker      │  │  (SignalR)           │  │
│  └──────────────┘  └──────────────┘  └──────────────────────┘  │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────────┐  │
│  │  PostgreSQL  │  │  Redis       │  │  RabbitMQ            │  │
│  └──────────────┘  └──────────────┘  └──────────────────────┘  │
│  ┌──────────────┐                                               │
│  │  Keycloak    │                                               │
│  └──────────────┘                                               │
└─────────────────────────────────────────────────────────────────┘
    │
    ▼
┌─────────────────────────────────────────────────────────────────┐
│  crcl-observability network (internal)                          │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────────┐  │
│  │  Prometheus  │  │  Grafana     │  │  Seq                 │  │
│  └──────────────┘  └──────────────┘  └──────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

**Rules:**
- Application services are only accessible through Nginx. No service exposes ports directly to the host in production.
- In development, specific ports may be exposed for debugging (defined in `docker-compose.override.yml`).
- The observability network is accessible from backend services (to push metrics/traces) but not from the frontend network.
- PostgreSQL, Redis, and RabbitMQ are never exposed outside the backend network.

### 5.6 Volume Management

| Volume | Purpose | Backup Required |
|---|---|---|
| `postgres-data` | All relational data | Yes — daily, encrypted |
| `redis-data` | Persisted cache and token store | Yes — daily |
| `rabbitmq-data` | Message queue persistence | Yes — daily |
| `keycloak-data` | Identity provider configuration | Yes — on change |
| `prometheus-data` | Metrics time-series (30-day retention) | No |
| `grafana-data` | Dashboard configurations | Yes — on change |
| `seq-data` | Structured log storage | Yes — daily |

### 5.7 Service Startup Scripts

All infrastructure startup scripts must follow these mandatory patterns to ensure idempotency and compatibility.

#### 5.7.1 Docker Compose CLI Compatibility

Two syntaxes exist for Docker Compose:

| Syntax | Binary | Available Since | Notes |
|---|---|---|---|
| `docker-compose up` | Standalone binary | Docker 1.x | Required for Docker 24 and earlier |
| `docker compose up` | CLI plugin | Docker 25+ | Compose v2 plugin |

**Rule:** Always use `docker-compose` (hyphenated) in scripts and documentation unless the target environment is confirmed to have Docker 25+ with the Compose plugin. This ensures backward compatibility across all environments.

#### 5.7.2 Idempotent Startup Pattern

Every `run-services.sh` script must be idempotent — safe to run multiple times without manual cleanup. The required pattern:

```bash
#!/usr/bin/env bash
set -euo pipefail

# Step 1: Stop and remove existing containers and volumes
docker-compose down -v 2>/dev/null || true

# Step 2: Remove any orphaned containers matching the project prefix
ORPHANS=$(docker ps -a --filter "name=crcl-" -q 2>/dev/null)
if [ -n "$ORPHANS" ]; then
    docker rm -f $ORPHANS
fi

# Step 3: Allow Docker to release resources before starting
sleep 3

# Step 4: Start services
docker-compose up -d

# Step 5: Wait and verify health
docker-compose ps
```

**Why this matters:** Without `down -v` before `up`, Docker will fail with `Conflict. The container name "/crcl-xxx" is already in use` if a previous run left containers behind (e.g., due to a failed startup or interrupted script).

**⚠️ Critical — `ASPNETCORE_ENVIRONMENT` for local API development:** When running API services locally (outside Docker) with `dotnet ef database update` or `dotnet run`, the `ASPNETCORE_ENVIRONMENT` environment variable must be exported **before** these commands. Without it, `appsettings.Development.json` is never loaded, causing `ArgumentNullException` at startup (missing connection strings, missing OTLP endpoint, etc.):

```bash
# ✅ Export BEFORE running migrations
export ASPNETCORE_ENVIRONMENT=Development
dotnet ef database update \
  --project src/modules/iam/CreditRisk.IAM.Infrastructure/CreditRisk.IAM.Infrastructure.csproj \
  --startup-project src/modules/iam/CreditRisk.IAM.Api/CreditRisk.IAM.Api.csproj

# ✅ Export BEFORE running each API (or inline per terminal session)
export ASPNETCORE_ENVIRONMENT=Development
dotnet run --project src/modules/iam/CreditRisk.IAM.Api/CreditRisk.IAM.Api.csproj

# ✅ Alternatively, inline per command:
ASPNETCORE_ENVIRONMENT=Development dotnet run --project src/modules/iam/CreditRisk.IAM.Api/CreditRisk.IAM.Api.csproj
```

> **Root cause:** `ASPNETCORE_ENVIRONMENT` defaults to `Production` when not set. In `Production`, `appsettings.Development.json` is never loaded by the ASP.NET Core configuration pipeline. All `GetConnectionString()` calls return `null`, and the `!` null-forgiving operator suppresses the compiler warning but does NOT prevent the runtime `ArgumentNullException`.

#### 5.7.3 Shell Script Creation

When creating shell scripts programmatically (e.g., via AI tools or automation):

- **Always use `execute_command` with heredoc syntax** to create `.sh` files:
  ```bash
  cat > run-services.sh << 'EOF'
  #!/usr/bin/env bash
  # ... script content
  EOF
  chmod +x run-services.sh
  ```
- **Never use file-write tools** (e.g., `write_to_file`) for shell scripts — they do not set executable permissions and may produce incorrect line endings (CRLF instead of LF) on Linux, causing `bad interpreter` errors.

---

## 6. Security Baseline

### 6.1 Authentication Architecture

The authentication flow uses **OAuth2 / OpenID Connect** via Keycloak as the central identity provider:

```
User Browser
    │
    │  1. Login request
    ▼
Keycloak (OIDC Provider)
    │
    │  2. Validate credentials + TOTP
    │  3. Issue Access Token (JWT, 15min) + Refresh Token (7 days)
    ▼
Nginx (validates JWT signature via JWKS endpoint)
    │
    │  4. Forward request with validated token
    ▼
API Service (validates claims, enforces RBAC)
    │
    │  5. Check token revocation in Redis
    ▼
Business Logic
```

**Token lifecycle rules:**
- Access tokens: RS256-signed JWTs, 15-minute expiry, contain `sub`, `roles`, `email`, `jti` claims.
- Refresh tokens: Opaque tokens stored in Redis with a 7-day TTL. Revocable immediately.
- Token revocation: On logout or suspected intrusion, the `jti` (JWT ID) is added to a Redis revocation set. All API middleware checks this set on every request.
- Service-to-service: Client Credentials flow with dedicated service accounts. No human credentials used for service communication.

### 6.2 Role-Based Access Control (RBAC)

| Role | Permissions |
|---|---|
| `desk-operator` | Create proposals, view own proposals, view own customer data |
| `compliance-analyst` | View all transactions, approve/reject AML alerts, export reports, view PEP flags |
| `administrator` | Full access including user management, role assignment, audit log access, system configuration |
| `service-account` | Internal service-to-service communication only; no UI access |

**RBAC enforcement rules:**
- Claims-based authorization using `[Authorize(Policy = "RequiresComplianceAnalyst")]` on Minimal API endpoint groups.
- Policy definitions must be centralized in each module's `ServiceCollectionExtensions` — never inline in endpoint definitions.
- Resource-level authorization (e.g., a Desk Operator can only view their own proposals) must be enforced in the Application layer, not just at the API layer.

### 6.3 Data Encryption

#### At Rest
- **PostgreSQL:** Transparent Data Encryption (TDE) enabled at the tablespace level for all tables containing PII (CPF, CNPJ, email, phone, financial data).
- **Sensitive columns:** Fields classified as PII must additionally use column-level encryption via `pgcrypto` extension for defense-in-depth.
- **Redis:** Sensitive cached data (bureau query results, session state) must be encrypted before storage using AES-256-GCM.
- **Backups:** All database backups must be encrypted with AES-256 before storage.

#### In Transit
- **TLS 1.3** is mandatory for all external-facing endpoints.
- **Internal service communication** within the Docker network uses TLS 1.2 minimum (TLS 1.3 preferred).
- **Certificate management:** Use Let's Encrypt for production certificates. Self-signed certificates are acceptable only in local development and must never be committed to the repository.
- **HSTS:** HTTP Strict Transport Security with `max-age=31536000; includeSubDomains` must be set on all Nginx responses.

### 6.4 Audit Trail Requirements

Every critical action must generate an immutable audit log entry. The audit log is a separate, append-only table (`audit_log`) that must never be updated or deleted.

**Mandatory audited events:**

| Event Category | Events |
|---|---|
| Authentication | Login success, login failure, MFA challenge, logout, token revocation |
| Credit Proposals | Created, submitted, approved, rejected, credit limit modified |
| Customer Data | Created, updated, PII accessed, deleted (LGPD request) |
| Transactions | Flagged by AML engine, manually reviewed, confirmed fraud, false positive |
| User Management | User created, role assigned, role revoked, user disabled |
| System | Configuration changed, report exported, PEP list updated |

**Audit log schema:**

```sql
CREATE TABLE audit_log (
    id              UUID            PRIMARY KEY DEFAULT gen_random_uuid(),
    event_type      VARCHAR(100)    NOT NULL,
    actor_id        UUID            NOT NULL,    -- User or service account ID
    actor_role      VARCHAR(50)     NOT NULL,
    target_type     VARCHAR(100),               -- Entity type affected
    target_id       UUID,                       -- Entity ID affected
    action          VARCHAR(50)     NOT NULL,
    old_value       JSONB,                      -- Previous state (encrypted if PII)
    new_value       JSONB,                      -- New state (encrypted if PII)
    ip_address      INET,
    user_agent      TEXT,
    correlation_id  UUID            NOT NULL,   -- Distributed trace ID
    occurred_at     TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    -- No UPDATE or DELETE permissions granted on this table
    CONSTRAINT audit_log_immutable CHECK (TRUE)  -- Enforced via PostgreSQL RLS
);

-- Row-level security: no role may UPDATE or DELETE audit records
ALTER TABLE audit_log ENABLE ROW LEVEL SECURITY;
CREATE POLICY audit_log_insert_only ON audit_log FOR INSERT TO crcl_app_user WITH CHECK (TRUE);
CREATE POLICY audit_log_select ON audit_log FOR SELECT TO crcl_app_user USING (TRUE);
-- No UPDATE or DELETE policies defined — effectively immutable
```

### 6.5 LGPD Compliance Requirements

The Lei Geral de Proteção de Dados (LGPD — Lei nº 13.709/2018) imposes the following mandatory requirements:

| Requirement | Implementation |
|---|---|
| **Data minimization** | Collect only CPF/CNPJ, name, email, phone, income data strictly necessary for credit analysis. No behavioral tracking. |
| **Purpose limitation** | PII collected for credit analysis must not be used for marketing or profiling. |
| **Right to access** | Operators must be able to export all data held about a customer in JSON/PDF format within 15 days of request. |
| **Right to erasure** | Customer data must be anonymizable (not deleted — financial records must be retained for BCB compliance). Anonymization replaces PII with pseudonymous identifiers. |
| **Data retention** | Credit records: 5 years minimum (BCB Resolução 4.557). Audit logs: 5 years minimum. |
| **Consent tracking** | Customer consent for bureau queries must be recorded with timestamp and IP address. |
| **Data breach notification** | Security incidents involving PII must be reported to ANPD within 72 hours. |

### 6.6 Brazilian Financial Regulatory Compliance

| Regulation | Requirement |
|---|---|
| **BCB Resolução 4.557** | Credit risk management framework — internal scoring, concentration limits, stress testing |
| **COAF Resolução 36/2021** | Suspicious transaction reporting (STR) — automated detection and manual review workflow |
| **Circular BCB 3.978/2020** | AML/CFT policy — customer due diligence, PEP screening, transaction monitoring |
| **SCR (Sistema de Informações de Crédito)** | Mandatory reporting of credit operations to Banco Central |
| **Resolução CMN 4.893/2021** | Cybersecurity policy for financial institutions — incident response, penetration testing |

### 6.7 Input Validation and Injection Prevention

- **All user inputs** must be validated by FluentValidation before reaching the domain layer.
- **CPF validation:** Must implement the official Receita Federal check digit algorithm.
- **CNPJ validation:** Must implement the official Receita Federal check digit algorithm.
- **SQL injection:** Prevented by EF Core parameterization and Dapper's `@param` syntax. String interpolation in SQL is a critical security violation.
- **XSS prevention:** Blazor WASM renders content through the DOM API, which escapes HTML by default. `MarkupString` must only be used with sanitized content.
- **CSRF:** Not applicable for JWT-based APIs, but Blazor forms must use anti-forgery tokens for state-changing operations.
- **Rate limiting:** All public-facing endpoints must implement rate limiting via ASP.NET Core's `RateLimiter` middleware. Authentication endpoints: 5 requests/minute per IP. Transaction ingestion: configurable per client.

---

## 7. Observability Standards

### 7.1 Three Pillars Implementation

All services must implement all three observability pillars from day one:

| Pillar | Technology | Destination |
|---|---|---|
| **Logs** | `Microsoft.Extensions.Logging` + OpenTelemetry Log Exporter | Seq (structured search) |
| **Traces** | OpenTelemetry Tracing + OTLP Exporter | Jaeger (via OTel Collector) or Seq |
| **Metrics** | OpenTelemetry Metrics + Prometheus Exporter | Prometheus → Grafana |

### 7.2 OpenTelemetry Configuration

Every service must configure OpenTelemetry in its `Program.cs` using the shared `CreditRisk.Shared.Observability` package:

```csharp
// In Program.cs of each API/Worker service
builder.Services.AddCreditRiskObservability(builder.Configuration, serviceName: "credit-analysis-api");

// In CreditRisk.Shared.Observability/ObservabilityExtensions.cs
public static IServiceCollection AddCreditRiskObservability(
    this IServiceCollection services,
    IConfiguration configuration,
    string serviceName)
{
    // ⚠️ CRITICAL: Read endpoint BEFORE constructing Uri.
    // new Uri(null) throws ArgumentNullException if OTEL_EXPORTER_OTLP_ENDPOINT is absent.
    // This happens when ASPNETCORE_ENVIRONMENT is not set and appsettings.Development.json
    // is not loaded. The null-check makes OTLP export optional — services still start
    // without an OTel collector configured.
    string? otlpEndpoint = configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];

    var otelBuilder = services.AddOpenTelemetry()
        .ConfigureResource(resource => resource
            .AddService(
                serviceName: serviceName,
                serviceVersion: Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "unknown",
                serviceInstanceId: Environment.MachineName))
        .WithTracing(tracing =>
        {
            tracing
                .AddAspNetCoreInstrumentation(opts =>
                {
                    opts.RecordException = true;
                    opts.Filter = ctx => !ctx.Request.Path.StartsWithSegments("/health");
                })
                .AddEntityFrameworkCoreInstrumentation(opts => opts.SetDbStatementForText = true)
                .AddRedisInstrumentation()
                .AddHttpClientInstrumentation();

            if (!string.IsNullOrEmpty(otlpEndpoint))
                tracing.AddOtlpExporter(opts => opts.Endpoint = new Uri(otlpEndpoint));
        })
        .WithMetrics(metrics => metrics
            .AddAspNetCoreInstrumentation()
            .AddRuntimeInstrumentation()
            .AddProcessInstrumentation()
            .AddPrometheusExporter())
        .WithLogging(logging =>
        {
            if (!string.IsNullOrEmpty(otlpEndpoint))
                logging.AddOtlpExporter(opts => opts.Endpoint = new Uri(otlpEndpoint));
        });

    return services;
}
```

### 7.3 Structured Logging Standards

**Log levels:**

| Level | When to Use |
|---|---|
| `Trace` | Detailed diagnostic information (disabled in production) |
| `Debug` | Development-time diagnostics (disabled in production) |
| `Information` | Normal operational events (proposal created, transaction processed) |
| `Warning` | Recoverable issues (retry attempt, degraded mode, validation failure) |
| `Error` | Unrecoverable errors that affect a single operation (database timeout, external API failure) |
| `Critical` | System-wide failures requiring immediate intervention (database unreachable, message broker down) |

**Mandatory structured log fields:**

Every log entry must include:

```csharp
// Use LoggerMessage source generator for high-performance logging (AOT-compatible)
public static partial class CreditAnalysisLogs
{
    [LoggerMessage(
        EventId = 1001,
        Level = Log
Level.Information,
        Message = "Credit proposal {ProposalId} created for customer {CustomerId} with requested limit {RequestedLimit:C}")]
    internal static partial void ProposalCreated(
        ILogger logger,
        Guid proposalId,
        Guid customerId,
        decimal requestedLimit);

    [LoggerMessage(
        EventId = 1002,
        Level = LogLevel.Warning,
        Message = "Bureau query for customer {CustomerId} failed after {RetryCount} attempts. Falling back to cached result.")]
    internal static partial void BureauQueryFailed(
        ILogger logger,
        Guid customerId,
        int retryCount);
}
```

**Mandatory context fields in every log entry:**

| Field | Source | Description |
|---|---|---|
| `TraceId` | OpenTelemetry | Distributed trace identifier |
| `SpanId` | OpenTelemetry | Current span identifier |
| `ServiceName` | OTel Resource | Service that emitted the log |
| `UserId` | JWT claim `sub` | Authenticated user (if applicable) |
| `CorrelationId` | Request header `X-Correlation-ID` | End-to-end request tracking |
| `Environment` | `ASPNETCORE_ENVIRONMENT` | `Development`, `Staging`, `Production` |

**Prohibited in log messages:**
- CPF, CNPJ, full names, email addresses, phone numbers (PII — use pseudonymous IDs)
- Passwords, tokens, API keys, connection strings
- Full credit card numbers or bank account numbers
- Stack traces in `Information` or lower levels

### 7.4 Distributed Tracing Standards

- **Every incoming HTTP request** must create a root span via `AddAspNetCoreInstrumentation`.
- **Every outgoing HTTP call** (bureau queries, PEP list APIs) must create a child span via `AddHttpClientInstrumentation`.
- **Every database operation** must create a child span via `AddEntityFrameworkCoreInstrumentation`.
- **Every message published or consumed** via MassTransit must propagate the trace context using MassTransit's built-in OpenTelemetry integration.
- **Custom spans** must be created for business-critical operations:

```csharp
using var activity = ActivitySource.StartActivity("EvaluateCreditRisk");
activity?.SetTag("proposal.id", proposal.Id.ToString());
activity?.SetTag("customer.id", proposal.CustomerId.ToString());
activity?.SetTag("requested.limit", proposal.RequestedLimit.ToString());
// ... business logic
activity?.SetTag("risk.rating", result.Rating.ToString());
activity?.SetStatus(ActivityStatusCode.Ok);
```

### 7.5 Metrics Standards

**Mandatory metrics per service:**

| Metric Name | Type | Description |
|---|---|---|
| `http_requests_total` | Counter | Total HTTP requests by method, path, status |
| `http_request_duration_seconds` | Histogram | Request latency distribution |
| `db_query_duration_seconds` | Histogram | Database query latency by operation |
| `message_published_total` | Counter | Messages published to RabbitMQ by type |
| `message_consumed_total` | Counter | Messages consumed by type and status |
| `message_processing_duration_seconds` | Histogram | Message processing latency |
| `active_connections` | Gauge | Current active connections |

**Domain-specific metrics (mandatory):**

| Metric Name | Service | Description |
|---|---|---|
| `credit_proposals_created_total` | Credit Analysis API | Proposals created by type (Individual/Legal Entity) |
| `credit_proposals_approved_total` | Credit Analysis Worker | Proposals approved/rejected by risk rating |
| `bureau_queries_total` | Credit Analysis Worker | Bureau queries by provider and result |
| `transactions_ingested_total` | Compliance API | Transactions received per second |
| `aml_alerts_raised_total` | Compliance Worker | AML alerts by rule type |
| `fraud_confirmed_total` | Compliance Worker | Confirmed fraud vs false positives |

### 7.6 Alerting Rules

The following Prometheus alerting rules must be defined in `infra/prometheus/alerts.yml`:

```yaml
groups:
  - name: credit-risk-compliance-lab
    rules:
      - alert: HighErrorRate
        expr: rate(http_requests_total{status=~"5.."}[5m]) / rate(http_requests_total[5m]) > 0.05
        for: 2m
        labels:
          severity: critical
        annotations:
          summary: "High error rate on {{ $labels.service }}"
          description: "Error rate is {{ $value | humanizePercentage }} over the last 5 minutes"

      - alert: SlowAPIResponse
        expr: histogram_quantile(0.99, rate(http_request_duration_seconds_bucket[5m])) > 0.2
        for: 5m
        labels:
          severity: warning
        annotations:
          summary: "Slow API response on {{ $labels.service }}"
          description: "p99 latency is {{ $value }}s (threshold: 200ms)"

      - alert: MessageQueueBacklog
        expr: rabbitmq_queue_messages_ready > 10000
        for: 5m
        labels:
          severity: warning
        annotations:
          summary: "RabbitMQ queue backlog: {{ $labels.queue }}"

      - alert: DeadLetterQueueMessages
        expr: rabbitmq_queue_messages_ready{queue=~".*dlq.*"} > 0
        for: 1m
        labels:
          severity: critical
        annotations:
          summary: "Messages in Dead Letter Queue: {{ $labels.queue }}"
          description: "{{ $value }} messages require manual intervention"

      - alert: DatabaseConnectionPoolExhausted
        expr: pg_stat_activity_count > pg_settings_max_connections * 0.9
        for: 2m
        labels:
          severity: critical
        annotations:
          summary: "PostgreSQL connection pool near exhaustion"
```

### 7.7 Grafana Dashboard Requirements

The following dashboards must be provisioned via `infra/grafana/provisioning/dashboards/`:

| Dashboard | Panels |
|---|---|
| **System Overview** | Service health, error rates, request rates, latency p50/p95/p99 |
| **Credit Analysis** | Proposals per hour, approval rate, risk rating distribution, bureau query latency |
| **Compliance / AML** | Transaction ingestion rate, AML alerts per hour, fraud confirmation rate, DLQ depth |
| **Infrastructure** | PostgreSQL connections, Redis memory, RabbitMQ queue depths, container CPU/memory |
| **Security** | Failed login attempts, MFA challenges, token revocations, audit log volume |

---

## 8. Module Boundaries and Integration Contracts

### 8.1 Module Dependency Rules

The five modules follow strict dependency rules to prevent coupling:

```
┌─────────────────────────────────────────────────────────────────┐
│                    ALLOWED DEPENDENCIES                         │
│                                                                 │
│  IAM ──────────────────────────────────────────────────────►   │
│  Credit Analysis ──────────────────────────────────────────►   │
│  Compliance ───────────────────────────────────────────────►   │  CreditRisk.Shared.Kernel
│  Operations ───────────────────────────────────────────────►   │  CreditRisk.Shared.Contracts
│                                                                 │  CreditRisk.Shared.Observability
│                                                                 │
│  FORBIDDEN: Direct project references between modules           │
│  IAM ✗──────────────────────────────────────────────────────►  │
│  Credit Analysis ✗─────────────────────────────────────────►   │  Other module's Domain/Application
│  Compliance ✗──────────────────────────────────────────────►   │  or Infrastructure projects
└─────────────────────────────────────────────────────────────────┘
```

**Hard rules:**
- No module may reference another module's `Domain`, `Application`, or `Infrastructure` project.
- Cross-module communication is exclusively via message contracts in `CreditRisk.Shared.Contracts` published through MassTransit.
- Synchronous cross-module calls (HTTP) are permitted only for query operations where eventual consistency is unacceptable (e.g., IAM token validation). These must be documented as ADRs.

### 8.2 Message Contract Standards

All message contracts are defined in `CreditRisk.Shared.Contracts` and must follow these rules:

```csharp
// Contracts are immutable records
// Namespace: CreditRisk.Shared.Contracts.{Module}.{Direction}
// Direction: Commands (intent to change state) or Events (fact that something happened)

namespace CreditRisk.Shared.Contracts.CreditAnalysis.Events;

/// <summary>
/// Published when a credit proposal has been evaluated and a risk rating assigned.
/// Consumed by: Compliance module (for AML cross-check), Operations module (for dashboard update).
/// </summary>
public sealed record CreditProposalEvaluatedEvent
{
    public required Guid ProposalId { get; init; }
    public required Guid CustomerId { get; init; }
    public required string RiskRating { get; init; }       // "A" through "E"
    public required decimal ApprovedLimit { get; init; }
    public required DateTimeOffset EvaluatedAt { get; init; }
    public required string EvaluatedBy { get; init; }      // "AUTO" or operator ID
    public required Guid CorrelationId { get; init; }
}
```

**Contract versioning rules:**
- Contracts are **append-only**. New optional properties may be added. Existing properties may never be removed or renamed.
- Breaking changes require a new contract version: `CreditProposalEvaluatedEventV2`.
- Old contract versions must be supported for a minimum of 2 release cycles before deprecation.
- All contracts must include `CorrelationId` for distributed tracing.

### 8.3 MassTransit Configuration Standards

```csharp
// Standard MassTransit configuration for all services
builder.Services.AddMassTransit(x =>
{
    // Register all consumers in the assembly
    x.AddConsumers(Assembly.GetExecutingAssembly());

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(configuration["RabbitMQ:Host"], configuration["RabbitMQ:VHost"], h =>
        {
            h.Username(configuration["RabbitMQ:Username"]!);
            h.Password(configuration["RabbitMQ:Password"]!);
        });

        // Global retry policy: 3 retries with exponential backoff
        cfg.UseMessageRetry(r => r.Exponential(
            retryLimit: 3,
            minInterval: TimeSpan.FromSeconds(1),
            maxInterval: TimeSpan.FromSeconds(30),
            intervalDelta: TimeSpan.FromSeconds(5)));

        // Global circuit breaker
        cfg.UseCircuitBreaker(cb =>
        {
            cb.TrackingPeriod = TimeSpan.FromMinutes(1);
            cb.TripThreshold = 15;
            cb.ActiveThreshold = 10;
            cb.ResetInterval = TimeSpan.FromMinutes(5);
        });

        // OpenTelemetry propagation
        cfg.PropagateActivityContext = true;

        cfg.ConfigureEndpoints(context);
    });
});
```

**Consumer naming convention:**
- Exchange name: `{contract-namespace}.{contract-name}` (kebab-case, auto-generated by MassTransit)
- Queue name: `{service-name}_{consumer-name}` (e.g., `compliance-worker_credit-proposal-evaluated`)
- DLQ name: `{queue-name}_error` (auto-generated by MassTransit)

### 8.4 Module Integration Map

The following diagram shows all inter-module message flows:

```
┌──────────────────────────────────────────────────────────────────────────────┐
│                        MODULE INTEGRATION MAP                                │
│                                                                              │
│  ┌─────────────┐                                                             │
│  │     IAM     │──── UserCreatedEvent ──────────────────────────────────►   │
│  │   Module    │──── UserRoleChangedEvent ──────────────────────────────►   │
│  └─────────────┘                                                             │
│         │                                                                    │
│         │ JWT validation (sync HTTP — JWKS endpoint)                         │
│         ▼                                                                    │
│  ┌──────────────────────┐                                                    │
│  │   Credit Analysis    │                                                    │
│  │       Module         │                                                    │
│  │                      │──── CreditProposalCreatedEvent ──────────────►    │
│  │  API ──► Worker      │──── CreditProposalEvaluatedEvent ────────────►    │
│  │  (via RabbitMQ)      │──── CreditLimitApprovedEvent ─────────────────►   │
│  │                      │──── BureauQueryRequestedCommand ──────────────►   │
│  └──────────────────────┘         (to Bureau Integration Worker)             │
│                                                                              │
│  ┌──────────────────────┐                                                    │
│  │     Compliance       │                                                    │
│  │       Module         │                                                    │
│  │                      │──── TransactionFlaggedEvent ───────────────────►  │
│  │  API ──► Worker      │──── AmlAlertCreatedEvent ──────────────────────►  │
│  │  (via RabbitMQ)      │──── FraudConfirmedEvent ───────────────────────►  │
│  │                      │◄─── CreditProposalEvaluatedEvent (from Credit)    │
│  └──────────────────────┘                                                    │
│                                                                              │
│  ┌──────────────────────┐                                                    │
│  │     Operations       │                                                    │
│  │       Panel          │◄─── All Events (read-only, for dashboard)          │
│  │                      │                                                    │
│  │  Blazor WASM +       │──── SignalR push to browser on:                   │
│  │  SignalR Hub         │     TransactionFlaggedEvent                        │
│  │                      │     AmlAlertCreatedEvent                           │
│  └──────────────────────┘     CreditLimitApprovedEvent                      │
└──────────────────────────────────────────────────────────────────────────────┘
```

### 8.5 Synchronous Integration Rules

Synchronous HTTP calls between modules are permitted only in the following cases:

| Caller | Callee | Endpoint | Justification |
|---|---|---|---|
| All modules | IAM | `GET /auth/jwks` | JWT public key retrieval for token validation |
| Credit Analysis Worker | External Bureau API | `POST /query` | Real-time credit bureau query (async fallback required) |
| Compliance Worker | External PEP/Sanctions API | `GET /check/{document}` | PEP screening (cached in Redis for 24h) |

**Rules for synchronous calls:**
- Must use `HttpClientFactory` with named clients. Never instantiate `HttpClient` directly.
- Must implement Polly retry policy (3 retries, exponential backoff) and circuit breaker.
- Must have a timeout of 5 seconds maximum. External bureau calls: 10 seconds maximum.
- Must emit OpenTelemetry spans for every outgoing call.
- Must have a fallback strategy (cached result, degraded mode, or explicit failure) — never block indefinitely.

### 8.6 Credit Analysis Module — Internal Flow

```
HTTP POST /proposals
    │
    ▼
FluentValidation (CPF/CNPJ format, required fields)
    │
    ▼
CreateProposalCommand ──► CreateProposalCommandHandler
    │
    ├── Save proposal (EF Core, status: Draft)
    ├── Publish CreditProposalCreatedEvent (MassTransit → RabbitMQ)
    └── Return ProposalId (202 Accepted)

RabbitMQ ──► CreditAnalysisWorker
    │
    ├── Query bureau (async HTTP + Redis cache)
    ├── Run internal scoring algorithm
    ├── Apply credit rules engine
    ├── Determine approval routing:
    │   ├── Auto-approve (low risk, low value) ──► Publish CreditLimitApprovedEvent
    │   └── Manual review (high risk or high value) ──► Publish ProposalSentToReviewEvent
    └── Update proposal status (EF Core)
```

### 8.7 Compliance Module — Internal Flow

```
HTTP POST /transactions (Minimal API — ultra-high throughput)
    │
    ▼
Minimal validation (schema only — no business rules in API layer)
    │
    ▼
Publish TransactionReceivedCommand (MassTransit → RabbitMQ)
    │
    └── Return 202 Accepted immediately

RabbitMQ ──► ComplianceWorker
    │
    ├── Deserialize transaction
    ├── Apply AML/CFT rules engine:
    │   ├── Split transaction detection (smurfing)
    │   ├── Velocity checks (unusual frequency)
    │   ├── Amount anomaly detection (vs customer profile)
    │   └── PEP/Sanctions screening (Redis cache + external API)
    ├── If flagged:
    │   ├── Create AML alert (EF Core)
    │   ├── Publish AmlAlertCreatedEvent (→ Operations Panel via SignalR)
    │   └── Block transaction (update status)
    └── If clean:
        └── Publish TransactionClearedEvent
```

### 8.8 Operations Panel — SignalR Integration

The Operations Panel receives real-time updates via SignalR. The hub is hosted in `CreditRisk.Operations.Server`:

```csharp
// Hub definition
public sealed class OperationsHub : Hub
{
    // Groups by role — analysts only see compliance alerts
    // Operators only see their own proposal updates
    public async Task JoinRoleGroup(string role)
        => await Groups.AddToGroupAsync(Context.ConnectionId, $"role:{role}");
}

// MassTransit consumer that pushes to SignalR
public sealed class AmlAlertCreatedEventConsumer(
    IHubContext<OperationsHub> hubContext,
    ILogger<AmlAlertCreatedEventConsumer> logger)
    : IConsumer<AmlAlertCreatedEvent>
{
    public async Task Consume(ConsumeContext<AmlAlertCreatedEvent> context)
    {
        await hubContext.Clients
            .Group("role:compliance-analyst")
            .SendAsync("AmlAlertReceived", context.Message, context.CancellationToken)
            .ConfigureAwait(false);

        logger.AlertPushedToClients(context.Message.AlertId, context.Message.TransactionId);
    }
}
```

### 8.9 IAM Module — Token Revocation Flow

```
POST /auth/logout
    │
    ▼
Extract JWT jti (JWT ID) from token
    │
    ▼
Add jti to Redis revocation set (TTL = remaining token lifetime)
    │
    ▼
Publish UserLoggedOutEvent (for audit log)
    │
    ▼
Return 204 No Content

─────────────────────────────────────────────────────────────────
On every subsequent API request (all modules):

JWT Middleware
    │
    ├── Validate signature (JWKS from Keycloak)
    ├── Validate expiry
    ├── Extract jti
    └── Check Redis revocation set
        ├── jti found → 401 Unauthorized (token revoked)
        └── jti not found → proceed to authorization
```

---

## Appendix A: Architecture Decision Record Template

All architectural decisions that deviate from or extend this specification must be documented as ADRs in `docs/architecture/adr/`:

```markdown
# ADR-{NNN}: {Short Title}

**Date:** YYYY-MM-DD
**Status:** Proposed | Accepted | Deprecated | Superseded by ADR-{NNN}
**Deciders:** {Names or roles}

## Context
{What is the issue that motivates this decision?}

## Decision
{What is the change that we're proposing or have agreed to implement?}

## Consequences
{What becomes easier or more difficult to do and any risks introduced by the change?}

## Constraints Applied
{Which rules from setup.md are relevant to this decision?}

## Alternatives Considered
{What other options were evaluated and why were they rejected?}
```

---

## Appendix B: Quick Reference — Forbidden Patterns

The following code patterns are **never acceptable** in this codebase:

```csharp
// ❌ FORBIDDEN: Synchronous database call
var proposal = _context.Proposals.FirstOrDefault(p => p.Id == id);

// ✅ CORRECT: Async database call
var proposal = await _context.Proposals.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

// ❌ FORBIDDEN: .Result on Task (deadlock risk)
var result = _service.GetProposalAsync(id).Result;

// ✅ CORRECT: await
var result = await _service.GetProposalAsync(id, cancellationToken);

// ❌ FORBIDDEN: String interpolation in SQL (SQL injection)
var sql = $"SELECT * FROM proposals WHERE id = '{id}'";

// ✅ CORRECT: Parameterized query
var sql = "SELECT * FROM proposals WHERE id = @id";
var proposal = await connection.QueryFirstOrDefaultAsync<Proposal>(sql, new { id });

// ❌ FORBIDDEN: Infrastructure in Domain layer
// In CreditRisk.CreditAnalysis.Domain/Entities/CreditProposal.cs
using Microsoft.EntityFrameworkCore; // VIOLATION

// ❌ FORBIDDEN: Hardcoded secrets
var connectionString = "Host=localhost;Password=admin123";

// ✅ CORRECT: Configuration binding
var connectionString = configuration.GetConnectionString("Postgres");

// ❌ FORBIDDEN: latest tag in Docker
FROM mcr.microsoft.com/dotnet/aspnet:latest

// ✅ CORRECT: Pinned version
FROM mcr.microsoft.com/dotnet/runtime-deps:10.0

// ❌ FORBIDDEN: Plaintext PII in logs
_logger.LogInformation("Processing proposal for CPF {Cpf}", customer.Cpf);

// ✅ CORRECT: Pseudonymous identifier in logs
_logger.LogInformation("Processing proposal for customer {CustomerId}", customer.Id);

// ❌ FORBIDDEN: AutoMapper
CreateMap<CreditProposal, CreditProposalDto>();

// ✅ CORRECT: Explicit mapping method
public static CreditProposalDto ToDto(this CreditProposal proposal) => new()
{
    Id = proposal.Id,
    Status = proposal.Status.ToString(),
    RequestedLimit = proposal.RequestedLimit.Amount
};
```

---

*This document is the single source of truth for the Credit Risk Compliance Lab project. Any deviation requires an approved ADR. Questions about interpretation should be resolved by referencing the most conservative reading of the relevant section.*

---

## Appendix C: Known Implementation Pitfalls & Permanent Solutions

The following bugs and edge cases were encountered during the implementation of this project. Each entry documents the exact error, root cause, and the permanent fix. Use this as a pre-flight checklist before any new implementation.

| # | Error / Symptom | Root Cause | Fix |
|---|---|---|---|
| 1 | `CS0246: The type or namespace name 'JsonSourceGenerationContextAttribute' could not be found` | `JsonSerializerContext` partial class file is missing `using System.Text.Json.Serialization;`. `ImplicitUsings` does not inject this namespace for partial class attribute resolution. | Add `using System.Text.Json.Serialization;` explicitly at the top of every `*JsonContext.cs` file. See §3.4. |
| 2 | EF Core migration fails: `The entity type 'DomainEvent' requires a primary key to be defined` | EF Core discovers `DomainEvent` as a navigation entity via the `AggregateRoot.DomainEvents` property (`IReadOnlyList<DomainEvent>`). EF Core tries to map it as a table and requires a primary key. | Add `modelBuilder.Ignore<DomainEvent>();` in every `DbContext.OnModelCreating()` before `ApplyConfigurationsFromAssembly`. See SPEC-02 §4.8. |
| 3 | `ArgumentNullException: Value cannot be null. (Parameter 'uriString')` at startup in `ObservabilityExtensions` | `OTEL_EXPORTER_OTLP_ENDPOINT` configuration key is absent (because `appsettings.Development.json` was not loaded). `new Uri(null)` throws immediately. | Null-check the endpoint before constructing `Uri`. Make OTLP export conditional. See §7.2 and SPEC-01 §4.8. |
| 4 | `appsettings.Development.json` never loaded; all `GetConnectionString()` calls return `null` | `ASPNETCORE_ENVIRONMENT` environment variable not exported before running `dotnet ef database update` or `dotnet run`. Defaults to `Production`. | `export ASPNETCORE_ENVIRONMENT=Development` before every migration and `dotnet run` command. See §5.7.2. |
| 5 | `ACCESS_REFUSED` (RabbitMQ) or `RedisConnectionException` at startup | `appsettings.Development.json` used Docker service names (`rabbitmq`, `redis`) instead of `localhost`. Redis connection string missing `abortConnect=false`. | Use `localhost` for all hostnames in `appsettings.Development.json`. Add `abortConnect=false` to Redis connection strings. See SPEC-02 §6.5. |
| 6 | APIs start on wrong ports instead of 5000 (IAM), 5001 (Credit), 5002 (Compliance), 5003 (Operations), 8081 (Bureau) | `dotnet new` auto-generates random ports in `launchSettings.json` or `CreateSlimBuilder` defaults. | Set ports explicitly via `options.ListenAnyIP(PORT)` in `Program.cs` and configure `launchSettings.json`. |
| 7 | `RegexErrorStubRouteConstraint` / routes with `{id:guid}` return 500 | `WebApplication.CreateSlimBuilder(args)` calls `AddRoutingCore()` internally, which does NOT register built-in route constraints. | Call `builder.Services.AddRouting()` explicitly in every API `Program.cs` after `CreateSlimBuilder`. See §3.4 and SPEC-02 §4.7. |
| 8 | Error 500 on `POST /api/v1/users` or event publishing: `relation "outbox_messages" does not exist` | MassTransit or Command Handlers try to publish domain events via Outbox Pattern, but `outbox_messages` table was missing in `IamDbContext`. | Add `DbSet<OutboxMessage> OutboxMessages` to `IamDbContext`, `CreditAnalysisDbContext`, and `ComplianceDbContext`, register `OutboxMessageConfiguration`, and apply migrations. |
| 9 | Database empty (`0 rows`) returning 500 on first startup | PostgreSQL container was started fresh without executing EF Core migrations. | Run `dotnet ef database update` across all module infrastructure projects in startup script (`run-services.sh` / `start-all-services.sh`). |
| 10 | Error 401 Unauthorized on `POST /api/v1/users` | Endpoint had authorization policy applied when creating test/new users. | Keep `POST /api/v1/users` public for user creation/registration, and protect `GET /api/v1/users/{id}` with `RequiresAdministrator`. |
| 11 | Error 400 Bad Request on user creation payload | Payload used legacy format (`username`, `password`, `roles: []`). | Use contract: `{"email":"...","fullName":"...","role":"desk-operator","temporaryPassword":"..."}`. |
| 12 | Error 404 Not Found on `GET /api/v1/users/me` | Route `/users/me` was assumed but does not exist. | Use route `GET /api/v1/users/{id:guid}` with user UUID. |
| 13 | Postman `EAI_AGAIN` error on variable hosts | Postman failed to resolve `{{baseUrl_Bureau}}` without active environment selected. | Provide Postman collections with direct `http://localhost:PORT` URLs and automated token extraction scripts. |
| 14 | Compliance API startup failure: `Unable to resolve service for type 'IDistributedCache'` | `PepScreeningService` required distributed caching in DI, but Redis/memory cache was not registered in `AddComplianceInfrastructure`. | Call `services.AddStackExchangeRedisCache(...)` or `services.AddDistributedMemoryCache()` in `AddComplianceInfrastructure`. |
| 15 | Error 503 / runtime serialization failure on health checks: `NotSupportedException` | Returning anonymous objects like `Results.Ok(new { status = "healthy" })` under Native AOT / `CreateSlimBuilder`. Anonymous types cannot be registered in source-generated `JsonSerializerContext`. | Define and return strongly-typed records such as `HealthResponse(string Status, string Service, DateTimeOffset? Timestamp = null)` and register them in each module's `JsonSerializerContext`. |
| 16 | Startup failure: `Address already in use` or stale endpoints responding on ports 5000-5003 / 8081 | Lingering background `dotnet` processes from previous runs or other workspaces holding the ports. | In `run-services.sh` and startup scripts, proactively release ports 5000-5003 and 8081 using `fuser -k "${port}/tcp"` or `lsof -ti ":${port}" | xargs kill -9` before launching new processes. |
| 17 | `Conflict. The container name "/crcl-xxx" is already in use by container "..."` | Containers created by another project/workspace (e.g. sibling modernizations `...-mdrn-04`) occupying container names and ports with divergent credentials or dead instances. | Remove conflicting containers with `docker rm -f crcl-redis crcl-postgres crcl-rabbitmq crcl-keycloak crcl-grafana crcl-prometheus crcl-seq` before starting the project stack via `./start-all-services.sh infra-only`. |
| 18 | `Unable to create a 'DbContext' of type '...DbContext'. Unable to resolve service for type 'DbContextOptions<...>'` / `NOAUTH Returned - connection has not yet authenticated` on `dotnet ef database update` | `dotnet ef` attempts to run `Program.cs` to resolve the service provider. Synchronous external calls (e.g. `ConnectionMultiplexer.Connect` to Redis without credentials) throw on startup, causing EF Core to fall back and fail. | Implement `IDesignTimeDbContextFactory<T>` in each Infrastructure project (`IamDbContextFactory`, `CreditAnalysisDbContextFactory`, `ComplianceDbContextFactory`). This bypasses `Program.cs` entirely during migrations. |
| 19 | `relation "outbox_messages" already exists` (SqlState: 42P07) when applying migrations to multiple modules | Multiple modules (IAM, CreditAnalysis, Compliance) define an `outbox_messages` table without schema isolation, causing both to attempt creating it in PostgreSQL `public` schema. | Specify `SearchPath=<schema>` in the connection string and configure `.MigrationsHistoryTable("__EFMigrationsHistory", "<schema>")` in `UseNpgsql(...)` inside the `IDesignTimeDbContextFactory`. |
| 20 | `500 Internal Server Error: relation "users" does not exist` (SqlState: 42P01) on API runtime requests | Connection strings in `Program.cs` or `appsettings.json` lacked `SearchPath=iam`, causing EF Core to run runtime queries against `public` instead of `iam`. | Ensure all runtime connection strings in `Program.cs` and `appsettings*.json` specify `SearchPath=<schema>` (e.g. `SearchPath=iam`, `SearchPath=credit`, `SearchPath=compliance`). |
| 21 | `Bearer error="invalid_token"` / `SecurityTokenMalformedException: JWT is not well formed` (401 Unauthorized) | Service generating non-JWT string mock token (`mock-jwt-token-{id}`) that fails ASP.NET Core `JwtBearerHandler` format and signature validation. | Implement RFC 7519 signed JWT generation in `KeycloakTokenService` with standard claims (`sub`, `roles`, `jti`, `name`) and configure `AddJwtBearer` with symmetric key and `MapInboundClaims = false`. |
| 22 | `405 Method Not Allowed` on `GET /api/v1/proposals` | Only `POST /` and `GET /{id}` were registered in `ProposalEndpoints.cs`; listing endpoint route was missing. | Implement `ListProposalsQuery` and `ListProposalsQueryHandler`, add `ListAsync`/`CountAsync` to repository and map `group.MapGet("/", ...)` in `ProposalEndpoints.cs`. |
| 23 | `400 Bad Request` on `POST /api/v1/proposals` | Payload missing `required` C# record properties or using incorrect field names/types during Minimal API model binding. | Send complete camelCase payload matching `CreateProposalRequest` (`customerDocument`, `customerDocumentType`, `customerName`, `customerEmail`, `monthlyIncome`, `requestedLimit`, `proposalType`, `bureauConsentGiven: true`, `bureauConsentIpAddress`). |
| 24 | `404 Not Found` on `GET /statistics` (Bureau Mock) | Bureau Mock Minimal API omitted the `/statistics` endpoint. | Implement `GET /statistics` with `Interlocked` query counters in `CreditRisk.BureauMock.Service/Program.cs`. |
| 25 | `404 Not Found` on `POST|GET /api/v1/compliance/checks` | Compliance check endpoints omitted in Compliance API. | Map `ComplianceCheckEndpoints` in `CreditRisk.Compliance.Api` with screening integration and registration in `Program.cs`. |
| 26 | `403 Forbidden` on `GET /api/v1/users/{id}` | Endpoint restricted exclusively to `RequiresAdministrator`, blocking self-profile retrieval (`sub == id`) by operators and analysts. | Allow self-lookup where `sub == id` or require `RequiresAdministrator` for accessing third-party user profiles. |

### Appendix C.1 — Pre-Implementation Checklist (All Modules)

Before implementing any new API module, verify:

- [ ] Every `*JsonContext.cs` file has `using System.Text.Json.Serialization;` at the top and registers all DTOs, collections (`PagedResult<T>`, `ProblemDetails`, etc.), and `HealthResponse`
- [ ] No anonymous types (e.g. `new { status = "healthy" }`) are returned in any endpoint; use strongly-typed records exclusively
- [ ] `KeycloakTokenService` generates well-formed, RFC 7519 signed JWTs with `roles` and `sub` claims
- [ ] `AddJwtBearer` options configure `MapInboundClaims = false` and validate the shared signing key
- [ ] `ProposalEndpoints` maps both `POST /` (create) and `GET /` (paged list) alongside `GET /{id}` and `PUT /{id}/submit`
- [ ] `ComplianceCheckEndpoints` maps `POST /` and `GET /` under `/api/v1/compliance/checks`
- [ ] `BureauMock` exposes `/query`, `/health`, and `/statistics`
- [ ] `GET /api/v1/users/{id}` allows self-lookup (`sub == id`) or requires `administrator` role
- [ ] Every `DbContext.OnModelCreating()` calls `modelBuilder.Ignore<DomainEvent>()` before `ApplyConfigurationsFromAssembly`
- [ ] Every `DbContext` includes `DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();` and `OutboxMessageConfiguration`
- [ ] Every module Infrastructure project implements `IDesignTimeDbContextFactory<TContext>` configured with schema isolation and `MigrationsHistoryTable`
- [ ] Runtime connection strings in `Program.cs` and configuration files include `SearchPath=<schema>` for schema isolation
- [ ] `POST /api/v1/users` is configured as a public self-registration/user-creation endpoint without admin authorization required
- [ ] `ObservabilityExtensions` null-checks `OTEL_EXPORTER_OTLP_ENDPOINT` before constructing `Uri`
- [ ] `ASPNETCORE_ENVIRONMENT=Development` is exported before running migrations and `dotnet run`
- [ ] All hostnames in `appsettings.Development.json` use `localhost` (not Docker service names)
- [ ] Redis connection strings include `abortConnect=false`
- [ ] Ports are configured explicitly via `builder.WebHost.ConfigureKestrel(opts => opts.ListenAnyIP(PORT))`
- [ ] `run-services.sh` / `start-all-services.sh` proactively clears lingering processes on ports 5000-5003 and 8081
- [ ] Every API `Program.cs` calls `builder.Services.AddRouting()` after `WebApplication.CreateSlimBuilder(args)`
- [ ] EF Core migrations are executed (`dotnet ef database update`) for all DB contexts prior to handling requests