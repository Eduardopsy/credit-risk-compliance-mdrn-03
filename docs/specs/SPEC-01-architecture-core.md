
# SPEC-01 — Architecture & Core
## Credit Risk Compliance Lab — Technical Specification

> **Status:** Authoritative | **Version:** 1.0.0 | **Source:** [`setup.md`](../../setup.md)

---

## Table of Contents

1. [Overview and Responsibilities](#1-overview-and-responsibilities)
2. [Complete Technology Stack](#2-complete-technology-stack)
3. [Directory Structure and Naming Conventions](#3-directory-structure-and-naming-conventions)
4. [Contracts and Interfaces](#4-contracts-and-interfaces)
5. [Business Rules and Invariants](#5-business-rules-and-invariants)
6. [Design Decisions and Architectural Patterns](#6-design-decisions-and-architectural-patterns)
7. [Configuration and Environment Variables](#7-configuration-and-environment-variables)
8. [Detailed Test Scenarios](#8-detailed-test-scenarios)
9. [Acceptance Criteria and Definition of Done](#9-acceptance-criteria-and-definition-of-done)
10. [Local Execution Instructions](#10-local-execution-instructions)

---

## 1. Overview and Responsibilities

### 1.1 Scope

This frente owns the foundational layer of the entire Credit Risk Compliance Lab solution:

- **Solution scaffold**: `.sln`, all `.csproj` files, `Directory.Build.props`, `Directory.Packages.props`, `global.json`, `.editorconfig`.
- **`CreditRisk.Shared.Kernel`**: Base domain abstractions — `Entity`, `AggregateRoot`, `ValueObject`, `DomainEvent`, `Result<T>`, `Guard`, value objects (`Cpf`, `Cnpj`, `MoneyAmount`), `DomainException`.
- **`CreditRisk.Shared.Contracts`**: All MassTransit message contracts shared across module boundaries.
- **`CreditRisk.Shared.Observability`**: OpenTelemetry setup, `LoggerMessage` source-generated helpers.
- **Native AOT compliance**: `JsonSerializerContext` per module, no runtime reflection, source generator configurations.
- **FluentValidation pipeline**: Base `ValidationBehavior<TCommand, TResult>` used by all Application layers.

### 1.2 Boundaries

**Owns:** `src/shared/`, `Directory.Build.props`, `Directory.Packages.props`, `global.json`, `.editorconfig`, `CreditRiskComplianceLab.sln`.

**Does NOT own:** Module-specific domain logic (SPEC-02/04/05), infrastructure implementations (SPEC-02), Docker/CI (SPEC-07), test projects (SPEC-03/06).

**All other frentes depend on this frente's output. No circular dependencies permitted.**

---

## 2. Complete Technology Stack

| Component | Technology | Exact Version |
|---|---|---|
| Language | C# | 12 / 13 |
| Runtime | .NET | 8.0.0 |
| SDK | .NET SDK | 8.0.129+ |
| Validation | FluentValidation | 11.11.0 |
| Messaging | MassTransit | 8.3.6 |
| Messaging RabbitMQ | MassTransit.RabbitMQ | 8.3.6 |
| ORM | Microsoft.EntityFrameworkCore | 8.0.11 |
| PostgreSQL EF driver | Npgsql.EntityFrameworkCore.PostgreSQL | 8.0.11 |
| Micro-ORM | Dapper | 2.1.35 |
| PostgreSQL ADO.NET | Npgsql | 8.0.6 |
| Redis client | StackExchange.Redis | 2.8.16 |
| JWT auth | Microsoft.AspNetCore.Authentication.JwtBearer | 8.0.11 |
| OpenAPI | Microsoft.AspNetCore.OpenApi | 8.0.11 |
| OpenAPI UI | Swagger UI (`Swashbuckle.AspNetCore`) | 6.9.0 |
| Resilience | Polly | 8.4.2 |
| HTTP resilience | Microsoft.Extensions.Http.Resilience | 8.10.0 |
| OTel SDK | OpenTelemetry | 1.9.0 |
| OTel hosting | OpenTelemetry.Extensions.Hosting | 1.9.0 |
| OTel ASP.NET Core | OpenTelemetry.Instrumentation.AspNetCore | 1.9.0 |
| OTel HTTP | OpenTelemetry.Instrumentation.Http | 1.9.0 |
| OTel EF Core | OpenTelemetry.Instrumentation.EntityFrameworkCore | 1.0.0-beta.12 |
| OTel Runtime | OpenTelemetry.Instrumentation.Runtime | 1.9.0 |
| OTel Process | OpenTelemetry.Instrumentation.Process | 0.5.0-beta.6 |
| OTel OTLP exporter | OpenTelemetry.Exporter.OpenTelemetryProtocol | 1.9.0 |
| OTel Prometheus | OpenTelemetry.Exporter.Prometheus.AspNetCore | 1.9.0-rc.1 |
| Frontend | Microsoft.AspNetCore.Components.WebAssembly | 8.0.11 |
| Frontend auth | Microsoft.AspNetCore.Components.WebAssembly.Authentication | 8.0.11 |
| SignalR client | Microsoft.AspNetCore.SignalR.Client | 8.0.11 |
| UI components | MudBlazor | 7.15.0 |
| Unit test framework | xunit | 2.9.2 |
| Test runner | xunit.runner.visualstudio | 2.8.2 |
| Assertions | FluentAssertions | 7.0.0 |
| Test SDK | Microsoft.NET.Test.Sdk | 17.12.0 |
| Coverage | coverlet.collector | 6.0.2 |
| Integration containers | Testcontainers | 3.10.0 |
| PostgreSQL container | Testcontainers.PostgreSql | 3.10.0 |
| RabbitMQ container | Testcontainers.RabbitMq | 3.10.0 |
| Redis container | Testcontainers.Redis | 3.10.0 |
| Blazor component tests | bunit | 1.35.6 |

---

## 3. Directory Structure and Naming Conventions

### 3.1 Complete Solution Tree

```
credit-risk-compliance-lab/
├── CreditRiskComplianceLab.sln
├── setup.md
├── docker-compose.yml
├── docker-compose.override.yml
├── run-services.sh              ← Start all infrastructure services (idempotent — see §3.3)
├── stop-services.sh             ← Stop all infrastructure services
├── .env.example
├── .editorconfig
├── .gitignore
├── global.json
├── Directory.Build.props
├── Directory.Packages.props
├── src/
│   ├── modules/
│   │   ├── iam/
│   │   │   ├── CreditRisk.IAM.Domain/
│   │   │   ├── CreditRisk.IAM.Application/
│   │   │   ├── CreditRisk.IAM.Infrastructure/
│   │   │   └── CreditRisk.IAM.Api/
│   │   ├── credit-analysis/
│   │   │   ├── CreditRisk.CreditAnalysis.Domain/
│   │   │   ├── CreditRisk.CreditAnalysis.Application/
│   │   │   ├── CreditRisk.CreditAnalysis.Infrastructure/
│   │   │   ├── CreditRisk.CreditAnalysis.Api/
│   │   │   └── CreditRisk.CreditAnalysis.Worker/
│   │   ├── compliance/
│   │   │   ├── CreditRisk.Compliance.Domain/
│   │   │   ├── CreditRisk.Compliance.Application/
│   │   │   ├── CreditRisk.Compliance.Infrastructure/
│   │   │   ├── CreditRisk.Compliance.Api/
│   │   │   └── CreditRisk.Compliance.Worker/
│   │   └── operations/
│   │       └── CreditRisk.Operations.Client/
│   ├── servers/
│   │   └── CreditRisk.Operations.Server/
│   ├── workers/
│   │   ├── CreditRisk.CreditAnalysis.Worker/
│   │   └── CreditRisk.Compliance.Worker/
│   ├── external/
│   │   └── CreditRisk.BureauMock.Service/
│   └── shared/
│       ├── CreditRisk.Shared.Kernel/
│       │   ├── CreditRisk.Shared.Kernel.csproj
│       │   ├── Domain/
│       │   │   ├── Entity.cs
│       │   │   ├── AggregateRoot.cs
│       │   │   ├── ValueObject.cs
│       │   │   └── DomainEvent.cs
│       │   ├── Result/
│       │   │   ├── Result.cs
│       │   │   └── Error.cs
│       │   ├── Guard/
│       │   │   └── Guard.cs
│       │   ├── ValueObjects/
│       │   │   ├── Cpf.cs
│       │   │   ├── Cnpj.cs
│       │   │   └── MoneyAmount.cs
│       │   └── Exceptions/
│       │       └── DomainException.cs
│       ├── CreditRisk.Shared.Contracts/
│       │   ├── CreditRisk.Shared.Contracts.csproj
│       │   ├── CreditAnalysis/
│       │   │   ├── Commands/BureauQueryRequestedCommand.cs
│       │   │   └── Events/
│       │   │       ├── CreditProposalCreatedEvent.cs
│       │   │       ├── CreditProposalEvaluatedEvent.cs
│       │   │       └── CreditLimitApprovedEvent.cs
│       │   ├── Compliance/
│       │   │   ├── Commands/TransactionReceivedCommand.cs
│       │   │   └── Events/
│       │   │       ├── TransactionFlaggedEvent.cs
│       │   │       ├── AmlAlertCreatedEvent.cs
│       │   │       └── FraudConfirmedEvent.cs
│       │   └── IAM/
│       │       └── Events/
│       │           ├── UserCreatedEvent.cs
│       │           └── UserRoleChangedEvent.cs
│       └── CreditRisk.Shared.Observability/
│           ├── CreditRisk.Shared.Observability.csproj
│           └── ObservabilityExtensions.cs
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
├── infra/
│   ├── docker/
│   │   ├── api.Dockerfile
│   │   ├── worker.Dockerfile
│   │   ├── frontend.Dockerfile
│   │   └── nginx/nginx.conf
│   ├── keycloak/realm-export.json
│   ├── prometheus/
│   │   ├── prometheus.yml
│   │   └── alerts.yml
│   ├── grafana/provisioning/dashboards/
│   └── scripts/
│       ├── init-db.sql
│       └── seed-data.sql
└── docs/specs/
    ├── SPEC-01-architecture-core.md
    ├── SPEC-02-backend.md
    ├── SPEC-03-backend-unit-tests.md
    ├── SPEC-04-integration.md
    ├── SPEC-05-frontend.md
    ├── SPEC-06-frontend-unit-tests.md
    └── SPEC-07-devops-infrastructure.md
```

### 3.2 Naming Conventions

| Construct | Convention | Example |
|---|---|---|
| Value Object | `PascalCase`, named after concept | `Cpf`, `MoneyAmount` |
| Domain Event (internal) | Past tense + `Event` | `CreditProposalCreatedEvent` |
| Message Contract (shared) | Same as domain event, in `Shared.Contracts` | `CreditProposalEvaluatedEvent` |
| Guard method | Imperative verb | `Guard.AgainstNull`, `Guard.AgainstNegative` |
| Result factory | `Result.Success`, `Result.Failure` | — |
| Error code constant | `PascalCase` | `ErrorCodes.InvalidCpf` |

### 3.3 Infrastructure Scripts

#### `run-services.sh` and `stop-services.sh` (Mandatory Deliverable)

These scripts **must** be created and maintained at the repository root as part of any core/infrastructure or backend implementation (`SPEC-01` / `SPEC-02`). They follow the idempotency pattern defined in [`setup.md` §5.7](../../setup.md). The startup script must start all required infrastructure containers (PostgreSQL, Redis, RabbitMQ, Keycloak) and subsequently build and launch all implemented back-end modules in the background, outputting their access URLs and saving PIDs for clean termination via `stop-services.sh`.

#### `infra/scripts/init-db.sql`

This file initializes the PostgreSQL database on first container start. It is mounted as `/docker-entrypoint-initdb.d/init-db.sql` in the PostgreSQL container.

> **⚠️ Critical — PostgreSQL syntax only:** The following MySQL syntax is **invalid** in PostgreSQL and will cause the container to fail silently on startup, which in turn causes Keycloak to fail (it depends on a healthy PostgreSQL):
>
> ```sql
> -- ❌ FORBIDDEN — MySQL syntax, invalid in PostgreSQL:
> CREATE DATABASE IF NOT EXISTS creditrisk;
> CREATE DATABASE IF NOT EXISTS keycloak;
> ```
>
> The correct approach is to use **schemas within a single database** (not separate databases). Keycloak uses a dedicated schema, not a separate database:
>
> ```sql
> -- ✅ CORRECT — PostgreSQL syntax using schemas within a single database:
> CREATE SCHEMA IF NOT EXISTS iam;
> CREATE SCHEMA IF NOT EXISTS credit;
> CREATE SCHEMA IF NOT EXISTS compliance;
> CREATE SCHEMA IF NOT EXISTS worker;
> CREATE SCHEMA IF NOT EXISTS keycloak;
>
> GRANT ALL ON SCHEMA iam        TO crcl;
> GRANT ALL ON SCHEMA credit     TO crcl;
> GRANT ALL ON SCHEMA compliance TO crcl;
> GRANT ALL ON SCHEMA worker     TO crcl;
> GRANT ALL ON SCHEMA keycloak   TO crcl;
> ```
>
> If a separate database is truly required, use the PostgreSQL `\gexec` pattern:
>
> ```sql
> -- ✅ CORRECT — PostgreSQL conditional database creation:
> SELECT 'CREATE DATABASE keycloak'
> WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'keycloak')\gexec
>
> GRANT ALL PRIVILEGES ON DATABASE keycloak TO crcl;
> ```

The complete `init-db.sql` content (with schemas, extensions, audit log, and RLS policies) is specified in [SPEC-07 §10.1](SPEC-07-devops-infrastructure.md).

---

## 4. Contracts and Interfaces

### 4.1 `global.json`

```json
{
  "sdk": {
    "version": "8.0.129",
    "rollForward": "latestMinor"
  }
}
```

### 4.2 `Directory.Build.props`

```xml
<Project>
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>false</TreatWarningsAsErrors>
    <NoWarn>$(NoWarn);NU1902;NU1903</NoWarn>
    <AnalysisMode>All</AnalysisMode>
    <LangVersion>latest</LangVersion>
    <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
  </PropertyGroup>
  <PropertyGroup Condition="$(MSBuildProjectName.EndsWith('.Api')) Or $(MSBuildProjectName.EndsWith('.Worker'))">
    <PublishAot Condition="'$(Configuration)' == 'Release'">true</PublishAot>
    <PublishAot Condition="'$(Configuration)' == 'Debug'">false</PublishAot>
    <InvariantGlobalization>true</InvariantGlobalization>
    <StripSymbols>true</StripSymbols>
    <OptimizationPreference>Speed</OptimizationPreference>
  </PropertyGroup>
  <PropertyGroup Condition="$(MSBuildProjectName.EndsWith('.Tests'))">
    <PublishAot>false</PublishAot>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>
</Project>
```

### 4.3 `Directory.Packages.props`

```xml
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>
  <ItemGroup Label="Core">
    <PackageVersion Include="FluentValidation" Version="11.11.0" />
    <PackageVersion Include="Microsoft.Extensions.Logging.Abstractions" Version="10.0.0" />
    <PackageVersion Include="Microsoft.Extensions.Options" Version="10.0.0" />
    <PackageVersion Include="Microsoft.Extensions.Http" Version="10.0.0" />
    <PackageVersion Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="10.0.0" />
  </ItemGroup>
  <ItemGroup Label="Messaging">
    <PackageVersion Include="MassTransit" Version="8.3.6" />
    <PackageVersion Include="MassTransit.RabbitMQ" Version="8.3.6" />
  </ItemGroup>
  <ItemGroup Label="Persistence">
    <PackageVersion Include="Microsoft.EntityFrameworkCore" Version="10.0.0" />
    <PackageVersion Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.0" />
    <PackageVersion Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="10.0.0" />
    <PackageVersion Include="Dapper" Version="2.1.35" />
    <PackageVersion Include="Npgsql" Version="9.0.2" />
    <PackageVersion Include="StackExchange.Redis" Version="2.8.16" />
  </ItemGroup>
  <ItemGroup Label="Observability">
    <PackageVersion Include="OpenTelemetry" Version="1.9.0" />
    <PackageVersion Include="OpenTelemetry.Extensions.Hosting" Version="1.9.0" />
    <PackageVersion Include="OpenTelemetry.Instrumentation.AspNetCore" Version="1.9.0" />
    <PackageVersion Include="OpenTelemetry.Instrumentation.Http" Version="1.9.0" />
    <PackageVersion Include="OpenTelemetry.Instrumentation.EntityFrameworkCore" Version="1.0.0-beta.12" />
    <PackageVersion Include="OpenTelemetry.Instrumentation.Runtime" Version="1.9.0" />
    <PackageVersion Include="OpenTelemetry.Instrumentation.Process" Version="0.5.0-beta.6" />
    <PackageVersion Include="OpenTelemetry.Exporter.OpenTelemetryProtocol" Version="1.9.0" />
    <PackageVersion Include="OpenTelemetry.Exporter.Prometheus.AspNetCore" Version="1.9.0-rc.1" />
  </ItemGroup>
  <ItemGroup Label="API">
    <PackageVersion Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="10.0.0" />
    <PackageVersion Include="Microsoft.AspNetCore.OpenApi" Version="10.0.0" />
    <PackageVersion Include="Swashbuckle.AspNetCore" Version="6.9.0" />
  </ItemGroup>
  <ItemGroup Label="Resilience">
    <PackageVersion Include="Polly" Version="8.4.2" />
    <PackageVersion Include="Microsoft.Extensions.Http.Resilience" Version="9.3.0" />
  </ItemGroup>
  <ItemGroup Label="Frontend">
    <PackageVersion Include="MudBlazor" Version="7.15.0" />
    <PackageVersion Include="Microsoft.AspNetCore.Components.WebAssembly" Version="10.0.0" />
    <PackageVersion Include="Microsoft.AspNetCore.Components.WebAssembly.Authentication" Version="10.0.0" />
    <PackageVersion Include="Microsoft.AspNetCore.SignalR.Client" Version="10.0.0" />
  </ItemGroup>
  <ItemGroup Label="Testing">
    <PackageVersion Include="xunit" Version="2.9.2" />
    <PackageVersion Include="xunit.runner.visualstudio" Version="2.8.2" />
    <PackageVersion Include="FluentAssertions" Version="7.0.0" />
    <PackageVersion Include="Microsoft.NET.Test.Sdk" Version="17.12.0" />
    <PackageVersion Include="coverlet.collector" Version="6.0.2" />
    <PackageVersion Include="Testcontainers" Version="3.10.0" />
    <PackageVersion Include="Testcontainers.PostgreSql" Version="3.10.0" />
    <PackageVersion Include="Testcontainers.RabbitMq" Version="3.10.0" />
    <PackageVersion Include="Testcontainers.Redis" Version="3.10.0" />
    <PackageVersion Include="bunit" Version="1.35.6" />
  </ItemGroup>
</Project>
```

### 4.4 `.editorconfig`

```ini
root = true

[*]
end_of_line = lf
insert_final_newline = true
trim_trailing_whitespace = true
charset = utf-8

[*.cs]
indent_style = space
indent_size = 4
charset = utf-8-bom
csharp_new_line_before_open_brace = all
csharp_new_line_before_else = true
csharp_new_line_before_catch = true
csharp_new_line_before_finally = true
csharp_style_var_for_built_in_types = false:warning
csharp_style_var_when_type_is_apparent = true:suggestion
csharp_style_var_elsewhere = false:suggestion
csharp_style_expression_bodied_methods = false:warning
csharp_style_expression_bodied_properties = when_on_single_line:suggestion
csharp_style_pattern_matching_over_is_with_cast_check = true:warning
csharp_style_pattern_matching_over_as_with_null_check = true:warning
dotnet_sort_system_directives_first = true

[*.{json,yml,yaml}]
indent_style = space
indent_size = 2

[*.md]
trim_trailing_whitespace = false
```

### 4.5 Core Domain Abstractions

#### `Entity.cs`

```csharp
// File: src/shared/CreditRisk.Shared.Kernel/Domain/Entity.cs
namespace CreditRisk.Shared.Kernel.Domain;

/// <summary>Base class for all domain entities. Identity equality semantics.</summary>
public abstract class Entity
{
    public Guid Id { get; protected init; }
    public DateTimeOffset CreatedAt { get; protected init; }
    public DateTimeOffset UpdatedAt { get; protected set; }

    protected Entity()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    protected Entity(Guid id, DateTimeOffset createdAt, DateTimeOffset updatedAt)
    {
        Guard.AgainstEmpty(id, nameof(id));
        Id = id;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Entity other) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetType() != other.GetType()) return false;
        return Id == other.Id;
    }

    public override int GetHashCode() => Id.GetHashCode();
    public static bool operator ==(Entity? left, Entity? right) => left?.Equals(right) ?? right is null;
    public static bool operator !=(Entity? left, Entity? right) => !(left == right);
}
```

#### `AggregateRoot.cs`

```csharp
// File: src/shared/CreditRisk.Shared.Kernel/Domain/AggregateRoot.cs
namespace CreditRisk.Shared.Kernel.Domain;

/// <summary>Base class for aggregate roots. Extends Entity with domain event collection.</summary>
public abstract class AggregateRoot : Entity
{
    private readonly List<DomainEvent> _domainEvents = [];

    public IReadOnlyList<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected AggregateRoot() : base() { }

    protected AggregateRoot(Guid id, DateTimeOffset createdAt, DateTimeOffset updatedAt)
        : base(id, createdAt, updatedAt) { }

    protected void RaiseDomainEvent(DomainEvent domainEvent)
    {
        Guard.AgainstNull(domainEvent, nameof(domainEvent));
        _domainEvents.Add(domainEvent);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ClearDomainEvents() => _domainEvents.Clear();
}
```

> **⚠️ EF Core Warning — `DomainEvents` navigation property:** EF Core's model discovery scans all `IReadOnlyList<T>` properties on entities and attempts to map them as navigation collections. Because `AggregateRoot` exposes `IReadOnlyList<DomainEvent> DomainEvents`, EF Core will try to create a `DomainEvent` table and require a primary key on `DomainEvent`, causing the migration to fail with:
> ```
> The entity type 'DomainEvent' requires a primary key to be defined.
> ```
> **Fix:** Every `DbContext` that maps any aggregate must call `modelBuilder.Ignore<DomainEvent>()` in `OnModelCreating()` before `ApplyConfigurationsFromAssembly`. See §4.8 in SPEC-02 for the canonical `OnModelCreating` template.

#### `ValueObject.cs`

```csharp
// File: src/shared/CreditRisk.Shared.Kernel/Domain/ValueObject.cs
namespace CreditRisk.Shared.Kernel.Domain;

/// <summary>Base class for value objects. Compared by component values, not identity.</summary>
public abstract class ValueObject
{
    protected abstract IEnumerable<object?> GetEqualityComponents();

    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType()) return false;
        return GetEqualityComponents().SequenceEqual(((ValueObject)obj).GetEqualityComponents());
    }

    public override int GetHashCode()
        => GetEqualityComponents().Select(x => x?.GetHashCode() ?? 0).Aggregate((x, y) => x ^ y);

    public static bool operator ==(ValueObject? left, ValueObject? right) => left?.Equals(right) ?? right is null;
    public static bool operator !=(ValueObject? left, ValueObject? right) => !(left == right);
}
```

#### `DomainEvent.cs`

```csharp
// File: src/shared/CreditRisk.Shared.Kernel/Domain/DomainEvent.cs
namespace CreditRisk.Shared.Kernel.Domain;

/// <summary>Base record for all domain events. Raised by aggregates, dispatched after UoW commits.</summary>
public abstract record DomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
}
```

#### `Error.cs` and `Result.cs`

```csharp
// File: src/shared/CreditRisk.Shared.Kernel/Result/Error.cs
namespace CreditRisk.Shared.Kernel.Result;

/// <summary>Represents a structured error with a code and human-readable description.</summary>
public sealed record Error
{
    public required string Code { get; init; }
    public required string Description { get; init; }
    public int HttpStatusCode { get; init; } = 400;

    public static Error Validation(string code, string description)
        => new() { Code = code, Description = description, HttpStatusCode = 422 };

    public static Error NotFound(string code, string description)
        => new() { Code = code, Description = description, HttpStatusCode = 404 };

    public static Error Unauthorized(string code, string description)
        => new() { Code = code, Description = description, HttpStatusCode = 401 };

    public static Error Conflict(string code, string description)
        => new() { Code = code, Description = description, HttpStatusCode = 409 };

    public static Error BusinessRule(string code, string description)
        => new() { Code = code, Description = description, HttpStatusCode = 422 };
}
```

```csharp
// File: src/shared/CreditRisk.Shared.Kernel/Result/Result.cs
namespace CreditRisk.Shared.Kernel.Result;

/// <summary>
/// Represents the outcome of an operation that can succeed or fail.
/// Use for application-layer methods that can fail for business reasons.
/// Never throw exceptions for expected business failures.
/// </summary>
public sealed class Result<TValue>
{
    private readonly TValue? _value;
    private readonly Error? _error;

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access Value on a failed Result.");

    public Error Error => IsFailure
        ? _error!
        : throw new InvalidOperationException("Cannot access Error on a successful Result.");

    private Result(TValue value) { IsSuccess = true; _value = value; }
    private Result(Error error) { IsSuccess = false; _error = error; }

    public static Result<TValue> Success(TValue value) => new(value);
    public static Result<TValue> Failure(Error error) => new(error);

    public static implicit operator Result<TValue>(TValue value) => Success(value);
    public static implicit operator Result<TValue>(Error error) => Failure(error);
}

/// <summary>Non-generic result for operations that return no value.</summary>
public sealed class Result
{
    private readonly Error? _error;

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    public Error Error => IsFailure
        ? _error!
        : throw new InvalidOperationException("Cannot access Error on a successful Result.");

    private Result() { IsSuccess = true; }
    private Result(Error error) { IsSuccess = false; _error = error; }

    public static Result Success() => new();
    public static Result Failure(Error error) => new(error);

    public static implicit operator Result(Error error) => Failure(error);
}
```

```csharp
// File: src/shared/CreditRisk.Shared.Kernel/Result/PagedResult.cs
namespace CreditRisk.Shared.Kernel.Result;

/// <summary>
/// Represents a paginated result set.
/// This is the CANONICAL definition — do NOT redefine PagedResult&lt;T&gt; in any
/// Application query file. Always import from CreditRisk.Shared.Kernel.Result.
/// </summary>
/// <typeparam name="T">The type of items in the page.</typeparam>
public sealed record PagedResult<T>
{
    public required IReadOnlyList<T> Items { get; init; }
    public required int TotalCount { get; init; }
    public required int Page { get; init; }
    public required int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
```

#### `Guard.cs`

```csharp
// File: src/shared/CreditRisk.Shared.Kernel/Guard/Guard.cs
namespace CreditRisk.Shared.Kernel.Guard;

/// <summary>
/// Provides guard clause methods for validating preconditions.
/// Throws <see cref="ArgumentException"/> variants for invalid inputs.
/// </summary>
public static class Guard
{
    /// <summary>Throws <see cref="ArgumentNullException"/> if value is null.</summary>
    public static T AgainstNull<T>(T? value, string paramName) where T : class
    {
        ArgumentNullException.ThrowIfNull(value, paramName);
        return value;
    }

    /// <summary>Throws <see cref="ArgumentException"/> if string is null or whitespace.</summary>
    public static string AgainstNullOrWhiteSpace(string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"'{paramName}' must not be null or whitespace.", paramName);
        return value;
    }

    /// <summary>Throws <see cref="ArgumentException"/> if Guid is empty.</summary>
    public static Guid AgainstEmpty(Guid value, string paramName)
    {
        if (value == Guid.Empty)
            throw new ArgumentException($"'{paramName}' must not be an empty Guid.", paramName);
        return value;
    }

    /// <summary>Throws <see cref="ArgumentOutOfRangeException"/> if value is negative.</summary>
    public static decimal AgainstNegative(decimal value, string paramName)
    {

        if (value < 0)
            throw new ArgumentOutOfRangeException(paramName, value, $"'{paramName}' must not be negative.");
        return value;
    }

    /// <summary>Throws <see cref="ArgumentOutOfRangeException"/> if value is not positive.</summary>
    public static decimal AgainstZeroOrNegative(decimal value, string paramName)
    {
        if (value <= 0)
            throw new ArgumentOutOfRangeException(paramName, value, $"'{paramName}' must be greater than zero.");
        return value;
    }

    /// <summary>Throws <see cref="ArgumentOutOfRangeException"/> if value exceeds maximum.</summary>
    public static decimal AgainstExceedingMaximum(decimal value, decimal maximum, string paramName)
    {
        if (value > maximum)
            throw new ArgumentOutOfRangeException(paramName, value, $"'{paramName}' must not exceed {maximum}.");
        return value;
    }
}
```

#### `DomainException.cs`

```csharp
// File: src/shared/CreditRisk.Shared.Kernel/Exceptions/DomainException.cs
namespace CreditRisk.Shared.Kernel.Exceptions;

/// <summary>
/// Base exception for all domain-level violations.
/// Thrown only for programming errors (invariant violations), not expected business failures.
/// Expected business failures must use Result pattern instead.
/// </summary>
public class DomainException : Exception
{
    public string ErrorCode { get; }

    public DomainException(string errorCode, string message) : base(message)
    {
        ErrorCode = errorCode;
    }

    public DomainException(string errorCode, string message, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}
```

#### Outbox Pattern Core Abstractions

```csharp
// File: src/shared/CreditRisk.Shared.Kernel/Outbox/OutboxMessage.cs
using CreditRisk.Shared.Kernel.Domain;
using CreditRisk.Shared.Kernel.Guard;

namespace CreditRisk.Shared.Kernel.Outbox;

/// <summary>
/// Represents a message stored in the outbox table for guaranteed delivery.
/// Messages are written atomically with the business transaction and processed by OutboxProcessor.
/// </summary>
public sealed class OutboxMessage : Entity
{
    public string MessageType { get; private init; } = string.Empty;
    public string Payload { get; private init; } = string.Empty;
    public DateTimeOffset ScheduledAt { get; private init; }
    public DateTimeOffset? ProcessedAt { get; private set; }
    public int RetryCount { get; private set; }
    public string? Error { get; private set; }

    private OutboxMessage() : base() { }

    public static OutboxMessage Create(string messageType, string payload)
    {
        Guard.AgainstNullOrWhiteSpace(messageType, nameof(messageType));
        Guard.AgainstNullOrWhiteSpace(payload, nameof(payload));

        return new OutboxMessage
        {
            MessageType = messageType,
            Payload = payload,
            ScheduledAt = DateTimeOffset.UtcNow
        };
    }

    public void MarkProcessed() => ProcessedAt = DateTimeOffset.UtcNow;

    public void MarkFailed(string error)
    {
        Error = error;
        RetryCount++;
    }
}
```

```csharp
// File: src/shared/CreditRisk.Shared.Kernel/Outbox/IOutboxRepository.cs
namespace CreditRisk.Shared.Kernel.Outbox;

/// <summary>
/// Repository interface for persisting and retrieving outbox messages.
/// </summary>
public interface IOutboxRepository
{
    Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OutboxMessage>> GetUnprocessedAsync(int maxRetries = 5, int limit = 100, CancellationToken cancellationToken = default);
    Task UpdateAsync(OutboxMessage message, CancellationToken cancellationToken = default);
    Task<int> DeleteProcessedAsync(DateTimeOffset olderThan, CancellationToken cancellationToken = default);
}
```

```csharp
// File: src/shared/CreditRisk.Shared.Kernel/Outbox/IOutboxProcessor.cs
namespace CreditRisk.Shared.Kernel.Outbox;

/// <summary>
/// Marker interface for outbox processor services.
/// </summary>
public interface IOutboxProcessor
{
    Task StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
}
```

#### Common & Health Check Abstractions (Native AOT)

```csharp
// File: src/shared/CreditRisk.Shared.Kernel/Common/HealthResponse.cs
namespace CreditRisk.Shared.Kernel.Common;

/// <summary>
/// Strongly-typed health check response record for Native AOT serialization compatibility.
/// Anonymous types (e.g. Results.Ok(new { status = "healthy" })) throw runtime serialization
/// exceptions in Native AOT / CreateSlimBuilder.
/// </summary>
public sealed record HealthResponse(
    string Status,
    string Service,
    DateTimeOffset? Timestamp = null);
```

### 4.6 Value Objects

#### `Cpf.cs`

```csharp
// File: src/shared/CreditRisk.Shared.Kernel/ValueObjects/Cpf.cs
using CreditRisk.Shared.Kernel.Domain;
using CreditRisk.Shared.Kernel.Exceptions;

namespace CreditRisk.Shared.Kernel.ValueObjects;

/// <summary>
/// Represents a Brazilian CPF (Cadastro de Pessoas Físicas) document number.
/// Validates using the official Receita Federal check digit algorithm.
/// Stored as 11 digits without formatting (no dots or dashes).
/// </summary>
public sealed class Cpf : ValueObject
{
    /// <summary>Gets the CPF value as 11 unformatted digits.</summary>
    public string Value { get; }

    private Cpf(string value) => Value = value;

    /// <summary>
    /// Creates a new <see cref="Cpf"/> instance after validating the check digits.
    /// </summary>
    /// <param name="value">CPF string — may include formatting (dots and dashes) or be 11 raw digits.</param>
    /// <returns>A valid <see cref="Cpf"/> instance.</returns>
    /// <exception cref="DomainException">Thrown when the CPF fails check digit validation.</exception>
    public static Cpf Create(string value)
    {
        Guard.AgainstNullOrWhiteSpace(value, nameof(value));
        string digits = new string(value.Where(char.IsDigit).ToArray());

        if (digits.Length != 11)
            throw new DomainException("CPF.InvalidLength", $"CPF must have exactly 11 digits. Got: {digits.Length}.");

        if (digits.Distinct().Count() == 1)
            throw new DomainException("CPF.AllSameDigits", "CPF with all identical digits is invalid.");

        if (!ValidateCheckDigits(digits))
            throw new DomainException("CPF.InvalidCheckDigit", "CPF check digits are invalid.");

        return new Cpf(digits);
    }

    /// <summary>Returns the CPF formatted as XXX.XXX.XXX-XX.</summary>
    public string ToFormattedString() => $"{Value[..3]}.{Value[3..6]}.{Value[6..9]}-{Value[9..11]}";

    /// <inheritdoc/>
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    /// <inheritdoc/>
    public override string ToString() => Value;

    private static bool ValidateCheckDigits(string digits)
    {
        // First check digit
        int sum = 0;
        for (int i = 0; i < 9; i++)
            sum += (digits[i] - '0') * (10 - i);

        int remainder = sum % 11;
        int firstCheckDigit = remainder < 2 ? 0 : 11 - remainder;

        if ((digits[9] - '0') != firstCheckDigit)
            return false;

        // Second check digit
        sum = 0;
        for (int i = 0; i < 10; i++)
            sum += (digits[i] - '0') * (11 - i);

        remainder = sum % 11;
        int secondCheckDigit = remainder < 2 ? 0 : 11 - remainder;

        return (digits[10] - '0') == secondCheckDigit;
    }
}
```

#### `Cnpj.cs`

```csharp
// File: src/shared/CreditRisk.Shared.Kernel/ValueObjects/Cnpj.cs
using CreditRisk.Shared.Kernel.Domain;
using CreditRisk.Shared.Kernel.Exceptions;

namespace CreditRisk.Shared.Kernel.ValueObjects;

/// <summary>
/// Represents a Brazilian CNPJ (Cadastro Nacional da Pessoa Jurídica) document number.
/// Validates using the official Receita Federal check digit algorithm.
/// Stored as 14 digits without formatting.
/// </summary>
public sealed class Cnpj : ValueObject
{
    /// <summary>Gets the CNPJ value as 14 unformatted digits.</summary>
    public string Value { get; }

    private Cnpj(string value) => Value = value;

    /// <summary>Creates a new <see cref="Cnpj"/> after validating check digits.</summary>
    public static Cnpj Create(string value)
    {
        Guard.AgainstNullOrWhiteSpace(value, nameof(value));
        string digits = new string(value.Where(char.IsDigit).ToArray());

        if (digits.Length != 14)
            throw new DomainException("CNPJ.InvalidLength", $"CNPJ must have exactly 14 digits. Got: {digits.Length}.");

        if (digits.Distinct().Count() == 1)
            throw new DomainException("CNPJ.AllSameDigits", "CNPJ with all identical digits is invalid.");

        if (!ValidateCheckDigits(digits))
            throw new DomainException("CNPJ.InvalidCheckDigit", "CNPJ check digits are invalid.");

        return new Cnpj(digits);
    }

    /// <summary>Returns the CNPJ formatted as XX.XXX.XXX/XXXX-XX.</summary>
    public string ToFormattedString()
        => $"{Value[..2]}.{Value[2..5]}.{Value[5..8]}/{Value[8..12]}-{Value[12..14]}";

    /// <inheritdoc/>
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    /// <inheritdoc/>
    public override string ToString() => Value;

    private static bool ValidateCheckDigits(string digits)
    {
        int[] weights1 = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
        int[] weights2 = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];

        int sum = 0;
        for (int i = 0; i < 12; i++)
            sum += (digits[i] - '0') * weights1[i];

        int remainder = sum % 11;
        int firstCheck = remainder < 2 ? 0 : 11 - remainder;

        if ((digits[12] - '0') != firstCheck) return false;

        sum = 0;
        for (int i = 0; i < 13; i++)
            sum += (digits[i] - '0') * weights2[i];

        remainder = sum % 11;
        int secondCheck = remainder < 2 ? 0 : 11 - remainder;

        return (digits[13] - '0') == secondCheck;
    }
}
```

#### `MoneyAmount.cs`

```csharp
// File: src/shared/CreditRisk.Shared.Kernel/ValueObjects/MoneyAmount.cs
using CreditRisk.Shared.Kernel.Domain;
using CreditRisk.Shared.Kernel.Exceptions;

namespace CreditRisk.Shared.Kernel.ValueObjects;

/// <summary>
/// Represents a monetary amount in Brazilian Reais (BRL).
/// Always non-negative. Precision: 2 decimal places.
/// </summary>
public sealed class MoneyAmount : ValueObject
{
    /// <summary>Gets the monetary amount value.</summary>
    public decimal Amount { get; }

    /// <summary>Gets the currency code. Always "BRL" for this system.</summary>
    public string Currency { get; } = "BRL";

    private MoneyAmount(decimal amount) => Amount = Math.Round(amount, 2, MidpointRounding.AwayFromZero);

    /// <summary>Creates a new <see cref="MoneyAmount"/>. Amount must be non-negative.</summary>
    public static MoneyAmount Create(decimal amount)
    {
        if (amount < 0)
            throw new DomainException("MoneyAmount.Negative", "Monetary amount must be non-negative.");
        return new MoneyAmount(amount);
    }

    /// <summary>Creates a zero monetary amount.</summary>
    public static MoneyAmount Zero() => new(0m);

    /// <summary>Adds two monetary amounts.</summary>
    public MoneyAmount Add(MoneyAmount other)
    {
        Guard.AgainstNull(other, nameof(other));
        return new MoneyAmount(Amount + other.Amount);
    }

    /// <summary>Subtracts a monetary amount. Result must be non-negative.</summary>
    public MoneyAmount Subtract(MoneyAmount other)
    {
        Guard.AgainstNull(other, nameof(other));
        if (Amount < other.Amount)
            throw new DomainException("MoneyAmount.NegativeResult", "Subtraction would result in a negative amount.");
        return new MoneyAmount(Amount - other.Amount);
    }

    /// <inheritdoc/>
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    /// <inheritdoc/>
    public override string ToString() => $"{Amount:F2} {Currency}";
}
```

### 4.7 Message Contracts (`CreditRisk.Shared.Contracts`)

All contracts are immutable records. Namespace pattern: `CreditRisk.Shared.Contracts.{Module}.{Direction}`.

```csharp
// File: src/shared/CreditRisk.Shared.Contracts/CreditAnalysis/Events/CreditProposalCreatedEvent.cs
namespace CreditRisk.Shared.Contracts.CreditAnalysis.Events;

/// <summary>
/// Published when a credit proposal is created and saved.
/// Consumed by: CreditAnalysis.Worker (to start evaluation pipeline).
/// </summary>
public sealed record CreditProposalCreatedEvent
{
    public required Guid ProposalId { get; init; }
    public required Guid CustomerId { get; init; }
    public required string CustomerDocument { get; init; }   // CPF or CNPJ (digits only)
    public required string CustomerDocumentType { get; init; } // "CPF" or "CNPJ"
    public required decimal RequestedLimit { get; init; }
    public required string ProposalType { get; init; }       // "Individual" or "LegalEntity"
    public required DateTimeOffset CreatedAt { get; init; }
    public required string CreatedBy { get; init; }          // Operator user ID
    public required Guid CorrelationId { get; init; }
}
```

```csharp
// File: src/shared/CreditRisk.Shared.Contracts/CreditAnalysis/Events/CreditProposalEvaluatedEvent.cs
namespace CreditRisk.Shared.Contracts.CreditAnalysis.Events;

/// <summary>
/// Published when a credit proposal has been evaluated and a risk rating assigned.
/// Consumed by: Compliance module (AML cross-check), Operations module (dashboard update).
/// </summary>
public sealed record CreditProposalEvaluatedEvent
{
    public required Guid ProposalId { get; init; }
    public required Guid CustomerId { get; init; }
    public required string RiskRating { get; init; }         // "A" | "B" | "C" | "D" | "E"
    public required decimal ApprovedLimit { get; init; }
    public required DateTimeOffset EvaluatedAt { get; init; }
    public required string EvaluatedBy { get; init; }        // "AUTO" or operator ID
    public required bool RequiresManualReview { get; init; }
    public required Guid CorrelationId { get; init; }
}
```

```csharp
// File: src/shared/CreditRisk.Shared.Contracts/CreditAnalysis/Events/CreditLimitApprovedEvent.cs
namespace CreditRisk.Shared.Contracts.CreditAnalysis.Events;

/// <summary>Published when a credit limit is auto-approved (Rating A or B, limit below threshold).</summary>
public sealed record CreditLimitApprovedEvent
{
    public required Guid ProposalId { get; init; }
    public required Guid CustomerId { get; init; }
    public required decimal ApprovedLimit { get; init; }
    public required string RiskRating { get; init; }
    public required DateTimeOffset ApprovedAt { get; init; }
    public required Guid CorrelationId { get; init; }
}
```

```csharp
// File: src/shared/CreditRisk.Shared.Contracts/CreditAnalysis/Commands/BureauQueryRequestedCommand.cs
namespace CreditRisk.Shared.Contracts.CreditAnalysis.Commands;

/// <summary>Command to trigger a credit bureau query for a customer.</summary>
public sealed record BureauQueryRequestedCommand
{
    public required Guid ProposalId { get; init; }
    public required Guid CustomerId { get; init; }
    public required string CustomerDocument { get; init; }
    public required string CustomerDocumentType { get; init; }
    public required Guid CorrelationId { get; init; }
}
```

```csharp
// File: src/shared/CreditRisk.Shared.Contracts/Compliance/Commands/TransactionReceivedCommand.cs
namespace CreditRisk.Shared.Contracts.Compliance.Commands;

/// <summary>Command published by Compliance API for async AML processing.</summary>
public sealed record TransactionReceivedCommand
{
    public required Guid TransactionId { get; init; }
    public required Guid CustomerId { get; init; }
    public required string CustomerDocument { get; init; }
    public required decimal Amount { get; init; }
    public required string TransactionType { get; init; }    // "Credit" | "Debit" | "Transfer"
    public required string Channel { get; init; }            // "ATM" | "Online" | "Branch" | "Mobile"
    public required DateTimeOffset TransactionDate { get; init; }
    public required string OriginAccountId { get; init; }
    public required string DestinationAccountId { get; init; }
    public required Guid CorrelationId { get; init; }
}
```

```csharp
// File: src/shared/CreditRisk.Shared.Contracts/Compliance/Events/AmlAlertCreatedEvent.cs
namespace CreditRisk.Shared.Contracts.Compliance.Events;

/// <summary>
/// Published when an AML alert is created for a flagged transaction.
/// Consumed by: Operations module (SignalR push to compliance-analyst group).
/// </summary>
public sealed record AmlAlertCreatedEvent
{
    public required Guid AlertId { get; init; }
    public required Guid TransactionId { get; init; }
    public required Guid CustomerId { get; init; }
    public required string AlertType { get; init; }          // "Smurfing" | "VelocityAnomaly" | "AmountAnomaly" | "PepMatch"
    public required string Severity { get; init; }           // "Low" | "Medium" | "High" | "Critical"
    public required decimal TransactionAmount { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required Guid CorrelationId { get; init; }
}
```

```csharp
// File: src/shared/CreditRisk.Shared.Contracts/Compliance/Events/TransactionFlaggedEvent.cs
namespace CreditRisk.Shared.Contracts.Compliance.Events;

/// <summary>Published when a transaction is flagged by the AML rules engine.</summary>
public sealed record TransactionFlaggedEvent
{
    public required Guid TransactionId { get; init; }
    public required Guid CustomerId { get; init; }
    public required string FlagReason { get; init; }
    public required string[] TriggeredRules { get; init; }
    public required DateTimeOffset FlaggedAt { get; init; }
    public required Guid CorrelationId { get; init; }
}
```

```csharp
// File: src/shared/CreditRisk.Shared.Contracts/Compliance/Events/FraudConfirmedEvent.cs
namespace CreditRisk.Shared.Contracts.Compliance.Events;

/// <summary>Published when a compliance analyst confirms a transaction as fraud.</summary>
public sealed record FraudConfirmedEvent
{
    public required Guid AlertId { get; init; }
    public required Guid TransactionId { get; init; }
    public required Guid CustomerId { get; init; }
    public required string ConfirmedBy { get; init; }        // Analyst user ID
    public required string FraudType { get; init; }
    public required DateTimeOffset ConfirmedAt { get; init; }
    public required Guid CorrelationId { get; init; }
}
```

```csharp
// File: src/shared/CreditRisk.Shared.Contracts/IAM/Events/UserCreatedEvent.cs
namespace CreditRisk.Shared.Contracts.IAM.Events;

/// <summary>Published when a new user account is created.</summary>
public sealed record UserCreatedEvent
{
    public required Guid UserId { get; init; }
    public required string Email { get; init; }
    public required string Role { get; init; }               // "desk-operator" | "compliance-analyst" | "administrator"
    public required DateTimeOffset CreatedAt { get; init; }
    public required string CreatedBy { get; init; }
    public required Guid CorrelationId { get; init; }
}
```

```csharp
// File: src/shared/CreditRisk.Shared.Contracts/IAM/Events/UserRoleChangedEvent.cs
namespace CreditRisk.Shared.Contracts.IAM.Events;

/// <summary>Published when a user's role is changed by an administrator.</summary>
public sealed record UserRoleChangedEvent
{
    public required Guid UserId { get; init; }
    public required string PreviousRole { get; init; }
    public required string NewRole { get; init; }
    public required string ChangedBy { get; init; }
    public required DateTimeOffset ChangedAt { get; init; }
    public required Guid CorrelationId { get; init; }
}
```

### 4.8 `ObservabilityExtensions.cs`

```csharp
// File: src/shared/CreditRisk.Shared.Observability/ObservabilityExtensions.cs
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Logs;

namespace CreditRisk.Shared.Observability;

/// <summary>
/// Extension methods to register OpenTelemetry for all Credit Risk services.
/// Call AddCreditRiskObservability in each service's Program.cs.
/// </summary>
public static class ObservabilityExtensions
{
    /// <summary>
    /// Registers OpenTelemetry tracing, metrics, and logging for a Credit Risk service.
    /// OTLP export is conditional: if OTEL_EXPORTER_OTLP_ENDPOINT is absent (e.g., when
    /// ASPNETCORE_ENVIRONMENT is not set and appsettings.Development.json is not loaded),
    /// the service starts normally without OTLP — instead of throwing ArgumentNullException.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="serviceName">The service name used in OTel resource attributes.</param>
    public static IServiceCollection AddCreditRiskObservability(
        this IServiceCollection services,
        IConfiguration configuration,
        string serviceName)
    {
        // ⚠️ CRITICAL: Do NOT use new Uri(configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]!)
        // If the key is absent (e.g., appsettings.Development.json not loaded because
        // ASPNETCORE_ENVIRONMENT was not exported), new Uri(null) throws ArgumentNullException
        // and the service fails to start entirely. Always null-check first.
        string? otlpEndpoint = configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];

        string serviceVersion = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "1.0.0";

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(
                    serviceName: serviceName,
                    serviceVersion: serviceVersion,
                    serviceInstanceId: Environment.MachineName))
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation(opts =>
                    {
                        opts.RecordException = true;
                        opts.Filter = ctx => !ctx.Request.Path.StartsWithSegments("/health")
                                         && !ctx.Request.Path.StartsWithSegments("/metrics");
                    })
                    .AddEntityFrameworkCoreInstrumentation(opts => opts.SetDbStatementForText = true)
                    .AddRedisInstrumentation()
                    .AddHttpClientInstrumentation(opts => opts.RecordException = true);

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
}
```

### 4.9 `ValidationBehavior.cs` (FluentValidation Pipeline)

```csharp
// File: src/shared/CreditRisk.Shared.Kernel/Validation/ValidationBehavior.cs
using CreditRisk.Shared.Kernel.Result;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace CreditRisk.Shared.Kernel.Validation;

/// <summary>
/// Pipeline behavior that runs FluentValidation before executing a command handler.
/// Returns a validation failure Result without invoking the handler if validation fails.
/// </summary>
/// <typeparam name="TCommand">The command type being validated.</typeparam>
/// <typeparam name="TResult">The result type returned by the handler.</typeparam>
public sealed class ValidationBehavior<TCommand, TResult>(
    IEnumerable<IValidator<TCommand>> validators,
    ILogger<ValidationBehavior<TCommand, TResult>> logger)
    where TCommand : notnull
    where TResult : class
{
    /// <summary>
    /// Validates the command and invokes the next handler if validation passes.
    /// </summary>
    public async Task<Result<TResult>> HandleAsync(
        TCommand command,
        Func<TCommand, CancellationToken, Task<Result<TResult>>> next,
        CancellationToken cancellationToken = default)
    {
        IValidator<TCommand>[] validatorArray = validators.ToArray();

        if (validatorArray.Length == 0)
            return await next(command, cancellationToken).ConfigureAwait(false);

        var context = new ValidationContext<TCommand>(command);
        var validationResults = await Task.WhenAll(
            validatorArray.Select(v => v.ValidateAsync(context, cancellationToken)))
            .ConfigureAwait(false);

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count > 0)
        {
            string commandName = typeof(TCommand).Name;
            logger.LogWarning(
                "Validation failed for {CommandName} with {FailureCount} errors: {Errors}",
                commandName,
                failures.Count,
                string.Join("; ", failures.Select(f => $"{f.PropertyName}: {f.ErrorMessage}")));

            string errorDescription = string.Join("; ", failures.Select(f => f.ErrorMessage));
            return Result<TResult>.Failure(Error.Validation(
                code: $"{commandName}.ValidationFailed",
                description: errorDescription));
        }

        return await next(command, cancellationToken).ConfigureAwait(false);
    }
}
```

### 4.10 CQRS Handler Interfaces

Every command and query handler must implement one of these interfaces. They are defined in `CreditRisk.Shared.Kernel.CQRS` and must be imported with `using CreditRisk.Shared.Kernel.CQRS;` in all endpoint and service registration files.

```csharp
// File: src/shared/CreditRisk.Shared.Kernel/CQRS/ICommandHandler.cs
namespace CreditRisk.Shared.Kernel.CQRS;

/// <summary>
/// Marker interface for command handlers that return a typed result.
/// Register with: services.AddScoped&lt;ICommandHandler&lt;TCommand, TResult&gt;, THandler&gt;()
/// </summary>
public interface ICommandHandler<TCommand, TResult>
    where TCommand : notnull
{
    Task<Result<TResult>> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}
```

```csharp
// File: src/shared/CreditRisk.Shared.Kernel/CQRS/IQueryHandler.cs
namespace CreditRisk.Shared.Kernel.CQRS;

/// <summary>
/// Marker interface for query handlers that return a typed result.
/// Register with: services.AddScoped&lt;IQueryHandler&lt;TQuery, TResult&gt;, THandler&gt;()
/// </summary>
public interface IQueryHandler<TQuery, TResult>
    where TQuery : notnull
{
    Task<Result<TResult>> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}
```

**⚠️ Critical — No `IPipelineBehavior<,>` in this codebase:**
`IPipelineBehavior<TRequest, TResponse>` is a **MediatR** interface. This project does NOT use MediatR. Never register `IPipelineBehavior<,>` in `ServiceCollectionExtensions`. Use `ValidationBehavior<TCommand, TResult>` directly (see §4.9) or call validators explicitly in handlers.

### 4.11 Native AOT — `JsonSerializerContext` Pattern

Each API project must define its own `JsonSerializerContext`. The correct pattern uses `[JsonSerializable(typeof(T))]` once per type, with `[JsonSourceGenerationOptions(...)]` on the context class. Example for IAM API:

```csharp
// File: src/modules/iam/CreditRisk.IAM.Api/Serialization/IamApiJsonContext.cs
using System.Text.Json.Serialization;
using CreditRisk.IAM.Application.DTOs;

namespace CreditRisk.IAM.Api.Serialization;

/// <summary>
/// Source-generated JSON serializer context for the IAM API.
/// Required for Native AOT — replaces reflection-based serialization.
/// Register all request/response types used in this API's endpoints.
/// </summary>
[JsonSerializable(typeof(LoginRequest))]
[JsonSerializable(typeof(LoginResponse))]
[JsonSerializable(typeof(CreateUserRequest))]
[JsonSerializable(typeof(UserDto))]
[JsonSerializable(typeof(Microsoft.AspNetCore.Mvc.ProblemDetails))]
[JsonSerializable(typeof(Dictionary<string, string[]>))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    WriteIndented = false)]
internal sealed partial class IamApiJsonContext : JsonSerializerContext
{
}
```

**Registration in `Program.cs`:**

```csharp
// In each API's Program.cs
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, IamApiJsonContext.Default);
});
```

---

## 5. Business Rules and Invariants

### 5.1 CPF Validation Rules

1. Must contain exactly 11 numeric digits (formatting characters stripped before validation).
2. Must not consist of all identical digits (e.g., `111.111.111-11` is invalid).
3. First check digit: `sum = Σ(digit[i] × (10 - i))` for i=0..8; `remainder = sum % 11`; `check = remainder < 2 ? 0 : 11 - remainder`.
4. Second check digit: `sum = Σ(digit[i] × (11 - i))` for i=0..9; same formula.
5. Both check digits must match digits[9] and digits[10] respectively.

### 5.2 CNPJ Validation Rules

1. Must contain exactly 14 numeric digits.
2. Must not consist of all identical digits.
3. First check digit uses weights `[5,4,3,2,9,8,7,6,5,4,3,2]` applied to digits[0..11].
4. Second check digit uses weights `[6,5,4,3,2,9,8,7,6,5,4,3,2]` applied to digits[0..12].
5. Both check digits must match digits[12] and digits[13] respectively.

### 5.3 MoneyAmount Rules

1. Amount must be non-negative (≥ 0).
2. Amount is always rounded to 2 decimal places using `MidpointRounding.AwayFromZero`.
3. Currency is always `"BRL"` — no multi-currency support in this system.
4. Subtraction that would produce a negative result throws `DomainException`.

### 5.4 Entity Identity Rules

1. `Entity.Id` is set once at construction and never changes.
2. Two entities of the same type with the same `Id` are considered equal regardless of other properties.
3. `AggregateRoot.UpdatedAt` is updated automatically whenever `RaiseDomainEvent` is called.
4. Domain events are cleared only by the infrastructure layer after successful dispatch.

### 5.5 Result Pattern Rules

1. `Result<T>.Value` must never be accessed without first checking `IsSuccess`.
2. `Result<T>.Error` must never be accessed without first checking `IsFailure`.
3. Application layer handlers must return `Result<T>` for all operations that can fail for business reasons.
4. Infrastructure exceptions (database timeouts, network failures) are caught at the infrastructure boundary and translated to `Result.Failure` with appropriate `Error` codes.
5. `DomainException` is reserved for programming errors (invariant violations), not expected business failures.

### 5.6 Message Contract Rules

1. All contracts are immutable records — no setters, no mutable collections.
2. All contracts include `CorrelationId` for distributed tracing.
3. Contracts are append-only — new optional properties may be added, existing properties may never be removed or renamed.
4. Breaking changes require a new contract version: `CreditProposalEvaluatedEventV2`.
5. Old contract versions must be supported for a minimum of 2 release cycles.

### 5.7 Native AOT Invariants

1. No `Type.GetType()` calls at runtime in AOT-published projects.
2. No `Activator.CreateInstance`, `Reflection.Emit`, or `Expression.Compile` at runtime.
3. All JSON serialization must use source-generated `JsonSerializerContext` — no `JsonSerializer.Serialize(obj)` without explicit type info.
4. `FluentValidation` 11.x is AOT-compatible when validators are registered explicitly (not via assembly scanning with reflection).
5. `MassTransit` 8.x is AOT-compatible when consumers are registered explicitly.
6. Every `JsonSerializerContext` partial class file **must** include `using System.Text.Json.Serialization;` explicitly. `ImplicitUsings` does not resolve `[JsonSerializable]` attribute types before compilation — omitting this using causes `CS0246: JsonSourceGenerationContextAttribute not found`.
7. Every API `Program.cs` using `WebApplication.CreateSlimBuilder(args)` **must** call `builder.Services.AddRouting()` explicitly. `CreateSlimBuilder` uses `AddRoutingCore()` internally, which does not register built-in route constraints (`{id:guid}`, `{id:int}`, etc.). Without `AddRouting()`, constrained routes throw `RegexErrorStubRouteConstraint` at runtime.

---

## 6. Design Decisions and Architectural Patterns

### 6.1 Clean Architecture with DDD

**Decision:** The solution follows Clean Architecture with Domain-Driven Design. Each bounded context (module) has its own layered projects: Domain → Application → Infrastructure → API.

**Dependency Rule:** Dependencies flow inward only. Domain has no external dependencies. Application depends only on Domain. Infrastructure depends on Application and Domain. API depends on Infrastructure.

**Rationale:** Enforces domain isolation, enables independent testing of business logic, and prevents infrastructure concerns from leaking into domain models.

### 6.2 Result Pattern over Exceptions

**Decision:** Application-layer methods that can fail for expected business reasons return `Result<T>` instead of throwing exceptions.

**Rationale:** Exceptions are expensive (stack unwinding), non-obvious in method signatures, and create implicit control flow. `Result<T>` makes failure paths explicit and forces callers to handle them.

**Canonical usage:**

```csharp
// Application handler — returns Result, never throws for business failures
public async Task<Result<CreditProposalDto>> HandleAsync(
    CreateProposalCommand command,
    CancellationToken cancellationToken)
{
    var validationResult = await _validator.ValidateAsync(command, cancellationToken)
        .ConfigureAwait(false);

    if (!validationResult.IsValid)
        return Result<CreditProposalDto>.Failure(
            Error.Validation("Proposal.ValidationFailed",
                string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage))));

    var customer = await _customerRepository.GetByIdAsync(command.CustomerId, cancellationToken)
        .ConfigureAwait(false);

    if (customer is null)
        return Result<CreditProposalDto>.Failure(
            Error.NotFound("Customer.NotFound", $"Customer {command.CustomerId} not found."));

    var proposal = CreditProposal.Create(customer, MoneyAmount.Create(command.RequestedLimit));
    await _proposalRepository.AddAsync(proposal, cancellationToken).ConfigureAwait(false);
    await _unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);

    return Result<CreditProposalDto>.Success(proposal.ToDto());
}
```

**API layer mapping:**

```csharp
// Minimal API endpoint maps Result to HTTP response
app.MapPost("/proposals", async (
    CreateProposalRequest request,
    ICreateProposalHandler handler,
    CancellationToken ct) =>
{
    var command = new CreateProposalCommand(
        CustomerId: request.CustomerId,
        RequestedLimit: request.RequestedLimit);

    var result = await handler.HandleAsync(command, ct);

    return result.IsSuccess
        ? Results.Accepted($"/proposals/{result.Value.Id}", result.Value)
        : result.Error.HttpStatusCode switch
        {
            404 => Results.NotFound(result.Error.ToProblemDetails()),
            422 => Results.UnprocessableEntity(result.Error.ToProblemDetails()),
            409 => Results.Conflict(result.Error.ToProblemDetails()),
            _ => Results.BadRequest(result.Error.ToProblemDetails())
        };
});
```

### 6.3 Aggregate Root and Domain Events

**Decision:** Aggregates raise domain events internally. Events are dispatched by the infrastructure layer after the unit of work commits.

**Canonical pattern:**

```csharp
// Domain aggregate raises event
public sealed class CreditProposal : AggregateRoot
{
    public ProposalStatus Status { get; private set; }
    public MoneyAmount RequestedLimit { get; private init; }
    public Guid CustomerId { get; private init; }

    private CreditProposal() : base() { }

    public static CreditProposal Create(Guid customerId, MoneyAmount requestedLimit)
    {
        Guard.AgainstEmpty(customerId, nameof(customerId));
        Guard.AgainstNull(requestedLimit, nameof(requestedLimit));

        var proposal = new CreditProposal
        {
            CustomerId = customerId,
            RequestedLimit = requestedLimit,
            Status = ProposalStatus.Draft
        };

        proposal.RaiseDomainEvent(new CreditProposalCreatedDomainEvent
        {
            ProposalId = proposal.Id,
            CustomerId = customerId,
            RequestedLimit = requestedLimit.Amount,
            CorrelationId = Guid.NewGuid()
        });

        return proposal;
    }
}
```

### 6.4 Native AOT Compliance Strategy

**Decision:** All API and Worker projects compile with `PublishAot=true`. Shared libraries are marked `IsAotCompatible=true`.

**Forbidden patterns in AOT projects:**

| Pattern | Reason | Alternative |
|---|---|---|
| `JsonSerializer.Serialize(obj)` without type info | Uses reflection | `JsonSerializer.Serialize(obj, MyContext.Default.MyType)` |
| `Assembly.GetTypes()` at runtime | Reflection scan | Explicit registration |
| `Activator.CreateInstance(type)` | Dynamic instantiation | Factory pattern |
| `Expression.Compile()` at runtime | JIT compilation | Pre-compiled delegates |
| MassTransit `AddConsumers(Assembly)` | Assembly scan | `AddConsumer<TConsumer>()` per type |
| FluentValidation `AddValidatorsFromAssembly` | Assembly scan | `AddValidator<TValidator>()` per type |

### 6.5 Shared Observability Pattern

All services use the same `AddCreditRiskObservability` extension. Custom business spans follow this pattern:

```csharp
private static readonly ActivitySource ActivitySource = new("CreditRisk.CreditAnalysis");

public async Task<Result<RiskRating>> EvaluateAsync(CreditProposal proposal, CancellationToken ct)
{
    using var activity = ActivitySource.StartActivity("EvaluateCreditRisk");
    activity?.SetTag("proposal.id", proposal.Id.ToString());
    activity?.SetTag("customer.id", proposal.CustomerId.ToString());
    activity?.SetTag("requested.limit", proposal.RequestedLimit.Amount.ToString());

    // ... business logic

    activity?.SetTag("risk.rating", result.ToString());
    activity?.SetStatus(ActivityStatusCode.Ok);
    return result;
}
```

---

## 7. Configuration and Environment Variables

### 7.1 Variables Consumed by Shared Libraries

Shared libraries do not consume environment variables directly. They accept configuration via `IOptions<T>` injected by the host application.

### 7.2 Variables Required by All Services

Every service (API and Worker) must have these variables configured:

| Variable | Type | Default (Dev) | Description |
|---|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | `string` | `Development` | Controls log level, Swagger UI, CORS |
| `OTEL_EXPORTER_OTLP_ENDPOINT` | `string` | `http://otel-collector:4317` | OpenTelemetry collector OTLP gRPC endpoint |
| `OTEL_SERVICE_NAME` | `string` | (set per service) | Overrides service name in OTel resource |
| `ConnectionStrings__Postgres` | `string` | (see `.env.example`) | PostgreSQL connection string |
| `ConnectionStrings__Redis` | `string` | (see `.env.example`) | Redis connection string with password |
| `RabbitMQ__Host` | `string` | `rabbitmq` | RabbitMQ hostname |
| `RabbitMQ__VHost` | `string` | `crcl` | RabbitMQ virtual host |
| `RabbitMQ__Username` | `string` | `crcl_broker` | RabbitMQ username |
| `RabbitMQ__Password` | `string` | (secret) | RabbitMQ password |
| `Keycloak__Authority` | `string` | `http://keycloak:8080/realms/crcl` | Keycloak realm URL for JWT validation |
| `Keycloak__Audience` | `string` | `crcl-api` | Expected JWT audience claim |

### 7.3 `.env.example`

```bash
# PostgreSQL
POSTGRES_USER=crcl
POSTGRES_PASSWORD=CHANGE_ME_STRONG_PASSWORD
POSTGRES_DB=creditrisk
POSTGRES_CONNECTION_STRING=Host=postgres;Port=5432;Database=creditrisk;Username=crcl;Password=CHANGE_ME

# Redis
REDIS_PASSWORD=CHANGE_ME_REDIS_PASSWORD
REDIS_CONNECTION_STRING=redis:6379,password=CHANGE_ME_REDIS_PASSWORD,ssl=false

# RabbitMQ
RABBITMQ_USER=crcl_broker
RABBITMQ_PASSWORD=CHANGE_ME_RABBITMQ_PASSWORD

# Keycloak
KEYCLOAK_ADMIN_USER=admin
KEYCLOAK_ADMIN_PASSWORD=CHANGE_ME_KEYCLOAK_PASSWORD

# Grafana
GRAFANA_ADMIN_USER=admin
GRAFANA_ADMIN_PASSWORD=CHANGE_ME_GRAFANA_PASSWORD

# Seq
SEQ_ADMIN_PASSWORD_HASH=CHANGE_ME_SEQ_HASH

# OpenTelemetry
OTEL_EXPORTER_OTLP_ENDPOINT=http://otel-collector:4317
```

---

## 8. Detailed Test Scenarios

### 8.1 Test Project Configuration

```xml
<!-- File: tests/unit/CreditRisk.CreditAnalysis.Domain.Tests/CreditRisk.CreditAnalysis.Domain.Tests.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <AssemblyName>CreditRisk.CreditAnalysis.Domain.Tests</AssemblyName>
    <RootNamespace>CreditRisk.CreditAnalysis.Domain.Tests</RootNamespace>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="xunit" />
    <PackageReference Include="xunit.runner.visualstudio" />
    <PackageReference Include="FluentAssertions" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" />
    <PackageReference Include="coverlet.collector">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="../../../src/shared/CreditRisk.Shared.Kernel/CreditRisk.Shared.Kernel.csproj" />
    <ProjectReference Include="../../../src/modules/credit-analysis/CreditRisk.CreditAnalysis.Domain/CreditRisk.CreditAnalysis.Domain.csproj" />
  </ItemGroup>
</Project>
```

### 8.2 `Cpf` Value Object Tests

```csharp
// File: tests/unit/CreditRisk.IAM.Domain.Tests/ValueObjects/CpfTests.cs
using CreditRisk.Shared.Kernel.Exceptions;
using CreditRisk.Shared.Kernel.ValueObjects;
using FluentAssertions;

namespace CreditRisk.IAM.Domain.Tests.ValueObjects;

public sealed class CpfTests
{
    [Theory]
    [InlineData("529.982.247-25")]   // formatted
    [InlineData("52998224725")]      // unformatted
    [InlineData("529 982 247 25")]   // spaces
    public void Create_ValidCpf_ReturnsInstance(string input)
    {
        // Arrange & Act
        var cpf = Cpf.Create(input);

        // Assert
        cpf.Should().NotBeNull();
        cpf.Value.Should().Be("52998224725");
    }

    [Theory]
    [InlineData("111.111.111-11")]   // all same digits
    [InlineData("000.000.000-00")]   // all zeros
    [InlineData("123.456.789-00")]   // wrong check digits
    [InlineData("1234")]             // too short
    [InlineData("")]                 // empty
    public void Create_InvalidCpf_ThrowsDomainException(string input)
    {
        // Arrange & Act
        Action act = () => Cpf.Create(input);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_NullCpf_ThrowsArgumentException()
    {
        // Arrange & Act
        Action act = () => Cpf.Create(null!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Equals_SameCpfValue_ReturnsTrue()
    {
        // Arrange
        var cpf1 = Cpf.Create("529.982.247-25");
        var cpf2 = Cpf.Create("52998224725");

        // Act & Assert
        cpf1.Should().Be(cpf2);
        (cpf1 == cpf2).Should().BeTrue();
    }

    [Fact]
    public void Equals_DifferentCpfValue_ReturnsFalse()
    {
        // Arrange
        var cpf1 = Cpf.Create("529.982.247-25");
        var cpf2 = Cpf.Create("111.444.777-35");

        // Act & Assert
        cpf1.Should().NotBe(cpf2);
    }

    [Fact]
    public void ToFormattedString_ValidCpf_ReturnsFormattedValue()
    {
        // Arrange
        var cpf = Cpf.Create("52998224725");

        // Act
        string formatted = cpf.ToFormattedString();

        // Assert
        formatted.Should().Be("529.982.247-25");
    }
}
```

### 8.3 `MoneyAmount` Value Object Tests

```csharp
// File: tests/unit/CreditRisk.CreditAnalysis.Domain.Tests/ValueObjects/MoneyAmountTests.cs
using CreditRisk.Shared.Kernel.Exceptions;
using CreditRisk.Shared.Kernel.ValueObjects;
using FluentAssertions;

namespace CreditRisk.CreditAnalysis.Domain.Tests.ValueObjects;

public sealed class MoneyAmountTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(0.01)]
    [InlineData(500000)]
    [InlineData(1234567.89)]
    public void Create_NonNegativeAmount_ReturnsInstance(decimal amount)
    {
        // Arrange & Act
        var money = MoneyAmount.Create(amount);

        // Assert
        money.Should().NotBeNull();
        money.Currency.Should().Be("BRL");
    }

    [Fact]
    public void Create_NegativeAmount_ThrowsDomainException()
    {
        // Arrange & Act
        Action act = () => MoneyAmount.Create(-0.01m);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*non-negative*");
    }

    [Fact]
    public void Create_RoundsToTwoDecimalPlaces()
    {
        // Arrange & Act
        var money = MoneyAmount.Create(10.555m);

        // Assert
        money.Amount.Should().Be(10.56m); // MidpointRounding.AwayFromZero
    }

    [Fact]
    public void Add_TwoAmounts_ReturnsSumAmount()
    {
        // Arrange
        var a = MoneyAmount.Create(100m);
        var b = MoneyAmount.Create(50.50m);

        // Act
        var result = a.Add(b);

        // Assert
        result.Amount.Should().Be(150.50m);
    }

    [Fact]
    public void Subtract_ValidSubtraction_ReturnsDifference()
    {
        // Arrange
        var a = MoneyAmount.Create(100m);
        var b = MoneyAmount.Create(30m);

        // Act
        var result = a.Subtract(b);

        // Assert
        result.Amount.Should().Be(70m);
    }

    [Fact]
    public void Subtract_WouldProduceNegative_ThrowsDomainException()
    {
        // Arrange
        var a = MoneyAmount.Create(10m);
        var b = MoneyAmount.Create(20m);

        // Act
        Action act = () => a.Subtract(b);

        // Assert
        act.Should().Throw<DomainException>();
    }
}
```

### 8.4 `Result<T>` Tests

```csharp
// File: tests/unit/CreditRisk.IAM.Domain.Tests/Result/ResultTests.cs
using CreditRisk.Shared.Kernel.Result;
using FluentAssertions;

namespace CreditRisk.IAM.Domain.Tests.Result;

public sealed class ResultTests
{
    [Fact]
    public void Success_ReturnsSuccessfulResult_WithValue()
    {
        // Arrange & Act
        var result = Result<string>.Success("hello");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().Be("hello");
    }

    [Fact]
    public void Failure_ReturnsFailedResult_WithError()
    {
        // Arrange
        var error = Error.NotFound("Test.NotFound", "Not found");

        // Act
        var result = Result<string>.Failure(error);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Test.NotFound");
        result.Error.HttpStatusCode.Should().Be(404);
    }

    [Fact]
    public void Value_OnFailedResult_ThrowsInvalidOperationException()
    {
        // Arrange
        var result = Result<string>.Failure(Error.NotFound("X", "Y"));

        // Act
        Action act = () => _ = result.Value;

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Error_OnSuccessfulResult_ThrowsInvalidOperationException()
    {
        // Arrange
        var result = Result<string>.Success("value");

        // Act
        Action act = () => _ = result.Error;

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ImplicitConversion_FromValue_CreatesSuccessResult()
    {
        // Arrange & Act
        Result<int> result = 42;

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void ImplicitConversion_FromError_CreatesFailureResult()
    {
        // Arrange & Act
        Result<int> result = Error.Validation("X", "Y");

        // Assert
        result.IsFailure.Should().BeTrue();
    }
}
```

### 8.5 `AggregateRoot` Domain Events Tests

```csharp
// File: tests/unit/CreditRisk.CreditAnalysis.Domain.Tests/Domain/AggregateRootTests.cs
using CreditRisk.Shared.Kernel.Domain;
using FluentAssertions;

namespace CreditRisk.CreditAnalysis.Domain.Tests.Domain;

// Test aggregate for testing AggregateRoot behavior
internal sealed class TestAggregate : AggregateRoot
{
    public string Name { get; private set; } = string.Empty;

    public static TestAggregate Create(string name)
    {
        var aggregate = new TestAggregate { Name = name };
        aggregate.RaiseDomainEvent(new TestDomainEvent { Name = name });
        return aggregate;
    }

    public void Rename(string newName)
    {
        Name = newName;
        RaiseDomainEvent(new TestDomainEvent { Name = newName });
    }
}

internal sealed record TestDomainEvent : DomainEvent
{
    public required string Name { get; init; }
}

public sealed class AggregateRootTests
{
    [Fact]
    public void Create_RaisesDomainEvent_EventIsInCollection()
    {
        // Arrange & Act
        var aggregate = TestAggregate.Create("Test");

        // Assert
        aggregate.DomainEvents.Should().HaveCount(1);
        aggregate.DomainEvents[0].Should().BeOfType<TestDomainEvent>();
        ((TestDomainEvent)aggregate.DomainEvents[0]).Name.Should().Be("Test");
    }

    [Fact]
    public void ClearDomainEvents_RemovesAllEvents()
    {
        // Arrange
        var aggregate = TestAggregate.Create("Test");
        aggregate.Rename("NewName");

        // Act
        aggregate.ClearDomainEvents();

        // Assert
        aggregate.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void RaiseDomainEvent_UpdatesUpdatedAt()
    {
        // Arrange
        var aggregate = TestAggregate.Create("Test");
        var createdAt = aggregate.UpdatedAt;

        // Act — small delay to ensure timestamp difference
        Task.Delay(10).GetAwaiter().GetResult();
        aggregate.Rename("NewName");

        // Assert
        aggregate.UpdatedAt.Should().BeAfter(createdAt);
    }

    [Fact]
    public void DomainEvents_IsReadOnly_CannotBeModifiedExternally()
    {
        // Arrange
        var aggregate = TestAggregate.Create("Test");

        // Act & Assert — IReadOnlyList does not expose Add
        aggregate.DomainEvents.Should().BeAssignableTo<IReadOnlyList<DomainEvent>>();
    }
}
```

---

## 9. Acceptance Criteria and Definition of Done

The following checklist must be fully satisfied before this frente is considered complete:

### 9.1 Solution Scaffold

- [ ] `CreditRiskComplianceLab.sln` exists and includes all 16 projects
- [ ] `global.json` pins SDK version `10.0.100` with `rollForward: disable`
- [ ] `Directory.Build.props` sets `TargetFramework=net10.0`, `Nullable=enable`, `TreatWarningsAsErrors=true`, `LangVersion=14`
- [ ] `Directory.Build.props` enables `PublishAot=true` for `.Api` and `.Worker` projects
- [ ] `Directory.Packages.props` manages all package versions centrally — no version attributes in individual `.csproj` files
- [ ] `.editorconfig` enforces Allman brace style, 4-space indentation, LF line endings
- [ ] `dotnet build` succeeds with zero warnings on a clean checkout

### 9.2 Shared Kernel

- [ ] `Entity`, `AggregateRoot`, `ValueObject`, `DomainEvent` are implemented as specified in Section 4.5
- [ ] `Result<T>` and `Result` implement Success/Failure factory methods and implicit conversions
- [ ] `Error` record has `Validation`, `NotFound`, `Unauthorized`, `Conflict`, `BusinessRule` factory methods
- [ ] `Guard` implements `AgainstNull`, `AgainstNullOrWhiteSpace`, `AgainstEmpty`, `AgainstNegative`, `AgainstZeroOrNegative`, `AgainstExceedingMaximum`
- [ ] `Cpf.Create` validates check digits using the Receita Federal algorithm
- [ ] `Cnpj.Create` validates check digits using the Receita Federal algorithm
- [ ] `MoneyAmount.Create` rejects negative values and rounds to 2 decimal places
- [ ] `DomainException` extends `Exception` with `ErrorCode` property
- [ ] `ValidationBehavior<TCommand, TResult>` runs all registered validators before invoking the handler

### 9.3 Shared Contracts

- [ ] All 9 message contracts exist in the correct namespaces under `CreditRisk.Shared.Contracts`
- [ ] All contracts are immutable sealed records with `required` properties
- [ ] All contracts include `CorrelationId: Guid`
- [ ] `CreditRisk.Shared.Contracts.csproj` references only `MassTransit` — no infrastructure packages

### 9.4 Shared Observability

- [ ] `ObservabilityExtensions.AddCreditRiskObservability` configures tracing, metrics, and logging
- [ ] Tracing excludes `/health` and `/metrics` paths
- [ ] Prometheus exporter is registered
- [ ] OTLP exporter endpoint is read from `OTEL_EXPORTER_OTLP_ENDPOINT` configuration key

### 9.5 Native AOT Compliance

- [ ] `dotnet publish -r linux-x64 -p:PublishAot=true` succeeds for all API and Worker projects
- [ ] No AOT warnings (`ILC2026`, `ILC2067`, `ILC2072`) in the build output
- [ ] Each API project has a `JsonSerializerContext` registering all request/response types
- [ ] `JsonSerializerContext` is registered in `ConfigureHttpJsonOptions`

### 9.6 Test Coverage

- [ ] All unit tests in `CreditRisk.*.Domain.Tests` pass
- [ ] Line coverage on `CreditRisk.Shared.Kernel` is ≥ 80%
- [ ] `Cpf` and `Cnpj` validation tests cover valid inputs, all-same-digit inputs, wrong check digits, and null/empty inputs
- [ ] `Result<T>` tests cover success, failure, implicit conversions, and invalid access patterns
- [ ] `AggregateRoot` tests cover event raising, clearing, and `UpdatedAt` mutation

---

## 10. Local Execution Instructions

Execute these commands in order from a clean machine with .NET SDK 10.0.100, Docker 26+, and Git installed.

### Step 1: Clone and Verify SDK

```bash
git clone https://github.com/org/credit-risk-compliance-lab.git
cd credit-risk-compliance-lab
dotnet --version
# Expected output: 10.0.100
```

### Step 2: Restore All Packages

```bash
dotnet restore CreditRiskComplianceLab.sln
```

### Step 3: Build the Entire Solution

```bash
dotnet build CreditRiskComplianceLab.sln --no-restore -c Release
# Expected: Build succeeded. 0 Warning(s). 0 Error(s).
```

### Step 4: Run Shared Kernel Unit Tests

```bash
dotnet test tests/unit/CreditRisk.IAM.Domain.Tests/CreditRisk.IAM.Domain.Tests.csproj \
  --no-build \
  --collect:"XPlat Code Coverage" \
  --results-directory ./coverage/iam-domain \
  -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=cobertura

dotnet test tests/unit/CreditRisk.CreditAnalysis.Domain.Tests/CreditRisk.CreditAnalysis.Domain.Tests.csproj \
  --no-build \
  --collect:"XPlat Code Coverage" \
  --results-directory ./coverage/credit-domain \
  -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=cobertura

dotnet test tests/unit/CreditRisk.Compliance.Domain.Tests/CreditRisk.Compliance.Domain.Tests.csproj \
  --no-build \
  --collect:"XPlat Code Coverage" \
  --results-directory ./coverage/compliance-domain \
  -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=cobertura
```

### Step 5: Verify AOT Compilation (Shared Kernel)

```bash
# Verify Shared.Kernel is AOT-compatible (no ILC warnings)
dotnet publish src/modules/iam/CreditRisk.IAM.Api/CreditRisk.IAM.Api.csproj \
  -c Release \
  -r linux-x64 \
  --self-contained true \
  -p:PublishAot=true \
  -o /tmp/iam-aot-test
# Expected: no ILC2026/ILC2067/ILC2072 warnings
```

### Step 6: Generate Coverage Report

```bash
# Install ReportGenerator if not present
dotnet tool install -g dotnet-reportgenerator-globaltool

reportgenerator \
  -reports:"./coverage/**/coverage.cobertura.xml" \
  -targetdir:"./coverage/report" \
  -reporttypes:Html

# Open ./coverage/report/index.html in browser
# Verify CreditRisk.Shared.Kernel line coverage >= 80%
```

### Step 7: Start Full Infrastructure Stack

```bash
cp .env.example .env
# Edit .env and replace all CHANGE_ME values with secure passwords

# Use docker-compose (hyphenated) for Docker 24 and earlier compatibility.
# On Docker 25+ with the Compose plugin, docker compose (space) also works.
# Always clean up previous containers first to avoid name conflicts:
docker-compose down -v 2>/dev/null || true
docker-compose up -d postgres redis rabbitmq keycloak seq
docker-compose ps
# PostgreSQL, Redis, RabbitMQ: healthy within ~30 seconds
# Keycloak: may take up to 3 minutes on first run (database schema migration)
```

### Step 8: Verify Infrastructure Health

```bash
# PostgreSQL
docker exec crcl-postgres pg_isready -U crcl -d creditrisk

# Redis
docker exec crcl-redis redis-cli -a $REDIS_PASSWORD ping
# Expected: PONG

# RabbitMQ
docker exec crcl-rabbitmq rabbitmq-diagnostics ping
# Expected: Ping succeeded

# Keycloak (wait up to 90 seconds for startup)
curl -s http://localhost:8080/health/ready | grep -q '"status":"UP"' && echo "Keycloak ready"
```

### Expected Final State

After completing all steps:
- `dotnet build` produces 0 warnings and 0 errors
- All unit tests pass with ≥ 80% line coverage on `CreditRisk.Shared.Kernel`
- AOT publish succeeds without ILC warnings
- All infrastructure containers are healthy

---

## 11. Known Implementation Pitfalls

This section documents errors encountered during the initial SPEC-01 implementation. Use it as a pre-flight checklist before starting any implementation.

| # | Symptom | Root Cause | Resolution |
|---|---|---|---|
| 1 | Shell script not executable or has bad line endings | `write_to_file` used to create `.sh` files | Use `execute_command` with heredoc: `cat > file.sh << 'EOF'` then `chmod +x file.sh` |
| 2 | `docker-compose.yml` fails to parse | YAML key-value pairs concatenated without newlines | Validate with `docker-compose config` after every edit. Each key-value pair must be on its own line. |
| 3 | `unknown shorthand flag: 'd' in -d` | Script uses `docker compose` but environment has Docker 24 or earlier | Replace all `docker compose` with `docker-compose` in scripts |
| 4 | `Conflict. The container name "/crcl-xxx" is already in use` | `docker-compose up` run without cleaning up previous containers | Always run `docker-compose down -v` before `docker-compose up` in startup scripts |
| 5 | PostgreSQL container unhealthy / Keycloak fails to start | `init-db.sql` uses MySQL syntax which is invalid in PostgreSQL | Use `CREATE SCHEMA IF NOT EXISTS` for schemas. See §3.3 for correct syntax. |
| 6 | Keycloak remains unhealthy after 90 seconds | `start_period: 60s` / `retries: 5` too aggressive for first-run DB migration | Set `start_period: 180s` and `retries: 20` in the Keycloak healthcheck |
| 7 | `CS0246: JsonSourceGenerationContextAttribute not found` | `JsonSerializerContext` partial class missing `using System.Text.Json.Serialization;` | Add explicit `using System.Text.Json.Serialization;` to every `*JsonContext.cs` file. `ImplicitUsings` does not cover partial class attribute resolution. |
| 8 | EF Core migration fails: `DomainEvent requires a primary key` | EF Core discovers `DomainEvent` via `AggregateRoot.DomainEvents` navigation property | Add `modelBuilder.Ignore<DomainEvent>();` in every `DbContext.OnModelCreating()`. See SPEC-02 §4.8. |
| 9 | `ArgumentNullException: uriString` at startup in `ObservabilityExtensions` | `OTEL_EXPORTER_OTLP_ENDPOINT` absent; `new Uri(null)` throws | Null-check endpoint before constructing `Uri`. See §4.8 for the corrected implementation. |
| 10 | `appsettings.Development.json` never loaded; connection strings are null | `ASPNETCORE_ENVIRONMENT` not exported before `dotnet run` or `dotnet ef database update` | `export ASPNETCORE_ENVIRONMENT=Development` before every local run command. See `setup.md` §5.7.2. |
| 11 | `ACCESS_REFUSED` (RabbitMQ) or `RedisConnectionException` at startup | `appsettings.Development.json` uses Docker service names (`rabbitmq`, `redis`) instead of `localhost`; Redis missing `abortConnect=false` | Use `localhost` for all hostnames in `appsettings.Development.json`. Add `abortConnect=false` to Redis strings. See SPEC-02 §6.5. |
| 12 | APIs start on wrong ports (5050, 5012, 5052) | `dotnet new` auto-generates random ports in `launchSettings.json` | Set ports explicitly: IAM=5000, CreditAnalysis=5001, Compliance=5002, Operations=5003, BureauMock=8081. See SPEC-02 §6.6. |
| 13 | `RegexErrorStubRouteConstraint` / routes with `{id:guid}` return 500 | `CreateSlimBuilder` uses `AddRoutingCore()` — does not register built-in route constraints | Call `builder.Services.AddRouting()` in every API `Program.cs` after `CreateSlimBuilder`. See §5.7 invariant #7. |
| 14 | `Conflict. The container name "/crcl-xxx" is already in use by container "..."` | Containers created by another project/workspace holding container names/ports | Remove conflicting containers with `docker rm -f crcl-redis crcl-postgres crcl-rabbitmq crcl-keycloak crcl-grafana crcl-prometheus crcl-seq` before starting Docker infrastructure. |
| 15 | `Unable to create a 'DbContext' of type '...DbContext'. Unable to resolve service for type 'DbContextOptions<...>'` / `NOAUTH Returned` during `dotnet ef database update` | EF Core CLI attempts to build the Web Host (`Program.cs`) which executes synchronous service connections (e.g. Redis) that fail if unauthenticated or not ready. | Provide `IDesignTimeDbContextFactory<TContext>` in each module's Infrastructure layer (`IamDbContextFactory`, `CreditAnalysisDbContextFactory`, `ComplianceDbContextFactory`) to isolate design-time migrations from application startup dependencies. |
| 16 | `relation "outbox_messages" already exists` (SqlState: 42P07) during EF database update | Multiple module migrations define tables with identical names targeting the default `public` schema. | Ensure schema isolation: specify `SearchPath=<schema>` in the connection string and configure `.MigrationsHistoryTable("__EFMigrationsHistory", "<schema>")` in `UseNpgsql`. |
| 17 | `relation "users" does not exist` (SqlState: 42P01) on runtime API requests | Connection string in `Program.cs` lacked `SearchPath=iam`, defaulting queries to PostgreSQL `public` schema. | Add `SearchPath=<schema>` to DbContext connection strings in `Program.cs` and `ServiceCollectionExtensions.cs`. |
| 18 | `Bearer error="invalid_token"` / `SecurityTokenMalformedException: JWT is not well formed` | `KeycloakTokenService` returned raw mock string (`mock-jwt-token-{id}`) instead of compact RFC 7519 signed JWT. | Generate signed JWT with `System.IdentityModel.Tokens.Jwt` and configure `AddJwtBearer` with `MapInboundClaims = false` and matching signing key. |
| 19 | `405 Method Not Allowed` on `GET /api/v1/proposals` | Only POST route mapped; paged listing endpoint missing from `ProposalEndpoints.cs`. | Implement `ListProposalsQuery` and `ListProposalsQueryHandler`, add `ListAsync`/`CountAsync` to repository, and map `GET /` returning `PagedResult<ProposalListItemDto>`. |
| 20 | `404 Not Found` on `GET /statistics` (Bureau Mock) | Bureau Mock Minimal API omitted the `/statistics` endpoint. | Implement `GET /statistics` with `Interlocked` query counters in `CreditRisk.BureauMock.Service/Program.cs`. |
| 21 | `404 Not Found` on `POST|GET /api/v1/compliance/checks` | Compliance check endpoints omitted in Compliance API. | Map `ComplianceCheckEndpoints` in `CreditRisk.Compliance.Api` with screening integration and registration in `Program.cs`. |
| 22 | `403 Forbidden` on `GET /api/v1/users/{id}` | Endpoint restricted exclusively to `RequiresAdministrator`, blocking self-profile retrieval (`sub == id`) by operators and analysts. | Allow self-lookup where `sub == id` or require `RequiresAdministrator` for accessing third-party user profiles. |
| 23 | `relation "users" does not exist` (SqlState: 42P01) or other module tables missing on fresh startup | PostgreSQL container recreated/started with `init-db.sql` but EF Core migrations were not executed across modules | Execute `dotnet ef database update` for all module DbContexts (`iam`, `credit`, `compliance`) after containers start. Automated in `start-all-services.sh`. |
| 24 | `Access Denied` on Blazor WASM routes after Keycloak login | Keycloak sends roles in nested JSON arrays/objects (`roles`, `realm_access.roles`) which Blazor WASM does not map to `ClaimTypes.Role` by default | Implement `CustomUserFactory` extending `AccountClaimsPrincipalFactory<RemoteUserAccount>` to flatten and register roles into `ClaimTypes.Role`. |
| 25 | `NullReferenceException` at `CustomUserFactory.CreateUserAsync` / Blazor `#blazor-error-ui` | `account` parameter is `null` during anonymous state initialization in Blazor WASM | Check `if (account is null) return user;` before accessing `account.AdditionalProperties`. |

### 11.1 Pre-Implementation Checklist

Before implementing any SPEC that involves infrastructure, verify:

- [ ] Shell scripts are created with `execute_command` + heredoc, not `write_to_file`
- [ ] All YAML files are validated with `docker-compose config` or `yamllint`
- [ ] All `docker compose` references use `docker-compose` for compatibility
- [ ] `run-services.sh` includes `docker-compose down -v` before `docker-compose up`
- [ ] `infra/scripts/init-db.sql` uses only PostgreSQL syntax — no MySQL syntax
- [ ] Keycloak healthcheck uses `start_period: 180s` and `retries: 20`
- [ ] Keycloak healthcheck uses bash TCP check, not `curl` (not available in Keycloak image)
- [ ] Every module infrastructure project has `IDesignTimeDbContextFactory<T>` configured with schema isolation
- [ ] Runtime connection strings in `Program.cs` include `SearchPath=<schema>`
- [ ] Token services generate valid signed JWTs and `AddJwtBearer` sets `MapInboundClaims = false`
- [ ] EF Core database migrations across all modules (`iam`, `credit`, `compliance`) are executed during startup
- [ ] Blazor WASM client registers `CustomUserFactory` to map Keycloak realm roles to `ClaimTypes.Role` with null-check on `RemoteUserAccount account`
- [ ] Endpoints implement all expected CRUD and query verbs from specifications (e.g. GET list alongside POST create, `/statistics`, `/compliance/checks`)
- [ ] `GET /api/v1/users/{id}` allows self-lookup (`sub == id`) alongside admin access

---

*Cross-references: This document is the foundation for all other SPEC documents. SPEC-02 through SPEC-07 depend on the types, packages, and conventions defined here.*