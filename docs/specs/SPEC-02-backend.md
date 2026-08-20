
# SPEC-02 — Back-End
## Credit Risk Compliance Lab — Technical Specification

> **Status:** Authoritative | **Version:** 1.0.0 | **Source:** [`setup.md`](../../setup.md)
> **Depends on:** [`SPEC-01-architecture-core.md`](SPEC-01-architecture-core.md)

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

This frente owns the three back-end API modules and their supporting layers:

- **IAM API** (`CreditRisk.IAM.*`): Authentication, MFA, RBAC, token lifecycle, user management, audit trail.
- **Credit Analysis API** (`CreditRisk.CreditAnalysis.*`): Customer onboarding, credit proposal creation, scoring engine, bureau integration orchestration, approval workflow.
- **Compliance API** (`CreditRisk.Compliance.*`): High-throughput transaction ingestion, AML/CFT rules engine, PEP/sanctions screening, alert management, regulatory report export.

Each module follows Clean Architecture: `Domain → Application → Infrastructure → Api`.

### 1.2 Boundaries

**Owns:** All `Domain`, `Application`, `Infrastructure`, and `Api` projects for IAM, CreditAnalysis, and Compliance modules.

**Does NOT own:** Worker projects (SPEC-04), Blazor frontend (SPEC-05), Docker/CI (SPEC-07), test projects (SPEC-03).

**Depends on (from SPEC-01):**
- `CreditRisk.Shared.Kernel`: `Entity`, `AggregateRoot`, `ValueObject`, `Result<T>`, `Guard`, `Cpf`, `Cnpj`, `MoneyAmount`, `DomainException`
- `CreditRisk.Shared.Contracts`: All message contracts
- `CreditRisk.Shared.Observability`: `ObservabilityExtensions`

---

## 2. Complete Technology Stack

All package versions are defined in `Directory.Packages.props` (see SPEC-01 Section 4.3).

| Component | Package | Version |
|---|---|---|
| Web framework | ASP.NET Core Minimal APIs | 10.0.0 |
| ORM | Microsoft.EntityFrameworkCore | 10.0.0 |
| PostgreSQL driver | Npgsql.EntityFrameworkCore.PostgreSQL | 10.0.0 |
| Micro-ORM | Dapper | 2.1.35 |
| Redis | StackExchange.Redis | 2.8.16 |
| JWT auth | Microsoft.AspNetCore.Authentication.JwtBearer | 10.0.0 |
| OpenAPI | Microsoft.AspNetCore.OpenApi | 10.0.0 |
| OpenAPI UI | Swagger UI (`Swashbuckle.AspNetCore`) | 6.9.0 |
| Validation | FluentValidation | 11.11.0 |
| Messaging | MassTransit | 8.3.6 |
| Messaging RabbitMQ | MassTransit.RabbitMQ | 8.3.6 |
| Resilience | Polly | 8.4.2 |
| HTTP resilience | Microsoft.Extensions.Http.Resilience | 9.3.0 |
| Observability | OpenTelemetry (full stack) | 1.9.0 |

---

## 3. Directory Structure and Naming Conventions

### 3.1 IAM Module

```
src/modules/iam/
├── CreditRisk.IAM.Domain/
│   ├── CreditRisk.IAM.Domain.csproj
│   ├── Entities/
│   │   ├── User.cs
│   │   └── RefreshToken.cs
│   ├── ValueObjects/
│   │   ├── Email.cs
│   │   └── HashedPassword.cs
│   ├── Events/
│   │   ├── UserCreatedDomainEvent.cs
│   │   ├── UserLoggedInDomainEvent.cs
│   │   └── UserLoggedOutDomainEvent.cs
│   ├── Repositories/
│   │   └── IUserRepository.cs
│   └── Enums/
│       └── UserRole.cs
├── CreditRisk.IAM.Application/
│   ├── CreditRisk.IAM.Application.csproj
│   ├── Commands/
│   │   ├── Login/LoginCommand.cs
│   │   ├── Login/LoginCommandHandler.cs
│   │   ├── Logout/LogoutCommand.cs
│   │   ├── Logout/LogoutCommandHandler.cs
│   │   ├── CreateUser/CreateUserCommand.cs
│   │   └── CreateUser/CreateUserCommandHandler.cs
│   ├── Queries/
│   │   ├── GetUserById/GetUserByIdQuery.cs
│   │   └── GetUserById/GetUserByIdQueryHandler.cs
│   ├── DTOs/
│   │   ├── LoginRequest.cs
│   │   ├── LoginResponse.cs
│   │   ├── CreateUserRequest.cs
│   │   └── UserDto.cs
│   ├── Validators/
│   │   ├── LoginRequestValidator.cs
│   │   └── CreateUserRequestValidator.cs
│   └── Ports/
│       ├── ITokenService.cs
│       ├── IPasswordHasher.cs
│       └── ITokenRevocationStore.cs
├── CreditRisk.IAM.Infrastructure/
│   ├── CreditRisk.IAM.Infrastructure.csproj
│   ├── Persistence/
│   │   ├── IamDbContext.cs
│   │   ├── Configurations/
│   │   │   ├── UserConfiguration.cs
│   │   │   └── RefreshTokenConfiguration.cs
│   │   ├── Migrations/
│   │   ├── Repositories/
│   │   │   └── UserRepository.cs
│   │   └── Interceptors/
│   │       └── AuditLogInterceptor.cs
│   ├── Services/
│   │   ├── KeycloakTokenService.cs
│   │   └── AspNetPasswordHasher.cs
│   └── Redis/
│       └── RedisTokenRevocationStore.cs
└── CreditRisk.IAM.Api/
    ├── CreditRisk.IAM.Api.csproj
    ├── Program.cs
    ├── Endpoints/
    │   ├── AuthEndpoints.cs
    │   └── UserEndpoints.cs
    ├── Middleware/
    │   ├── JwtRevocationMiddleware.cs
    │   └── GlobalExceptionMiddleware.cs
    ├── Serialization/
    │   └── IamApiJsonContext.cs
    └── Extensions/
        └── ServiceCollectionExtensions.cs
```

### 3.2 Credit Analysis Module

```
src/modules/credit-analysis/
├── CreditRisk.CreditAnalysis.Domain/
│   ├── Entities/
│   │   ├── CreditProposal.cs
│   │   └── Customer.cs
│   ├── ValueObjects/
│   │   └── RiskScore.cs
│   ├── Events/
│   │   ├── CreditProposalCreatedDomainEvent.cs
│   │   └── CreditProposalStatusChangedDomainEvent.cs
│   ├── Repositories/
│   │   ├── ICreditProposalRepository.cs
│   │   └── ICustomerRepository.cs
│   ├── Services/
│   │   └── ICreditScoringEngine.cs
│   └── Enums/
│       ├── RiskRating.cs
│       └── ProposalStatus.cs
├── CreditRisk.CreditAnalysis.Application/
│   ├── Commands/
│   │   ├── CreateProposal/CreateProposalCommand.cs
│   │   ├── CreateProposal/CreateProposalCommandHandler.cs
│   │   ├── SubmitProposal/SubmitProposalCommand.cs
│   │   └── SubmitProposal/SubmitProposalCommandHandler.cs
│   ├── Queries/
│   │   ├── GetProposalById/GetProposalByIdQuery.cs
│   │   ├── GetProposalById/GetProposalByIdQueryHandler.cs
│   │   ├── ListProposals/ListProposalsQuery.cs
│   │   └── ListProposals/ListProposalsQueryHandler.cs
│   ├── DTOs/
│   │   ├── CreateProposalRequest.cs
│   │   ├── CreditProposalDto.cs
│   │   └── ProposalListItemDto.cs
│   ├── Validators/
│   │   └── CreateProposalRequestValidator.cs
│   └── Ports/
│       ├── IUnitOfWork.cs
│       └── IBureauQueryService.cs
├── CreditRisk.CreditAnalysis.Infrastructure/
│   ├── Persistence/
│   │   ├── CreditAnalysisDbContext.cs
│   │   ├── Configurations/
│   │   │   ├── CreditProposalConfiguration.cs
│   │   │   └── CustomerConfiguration.cs
│   │   ├── Migrations/
│   │   ├── Repositories/
│   │   │   ├── CreditProposalRepository.cs
│   │   │   └── CustomerRepository.cs
│   │   └── UnitOfWork.cs
│   └── Services/
│       └── CreditScoringEngine.cs
└── CreditRisk.CreditAnalysis.Api/
    ├── Program.cs
    ├── Endpoints/
    │   ├── ProposalEndpoints.cs
    │   └── CustomerEndpoints.cs
    ├── Middleware/
    │   └── GlobalExceptionMiddleware.cs
    ├── Serialization/
    │   └── CreditAnalysisApiJsonContext.cs
    └── Extensions/
        └── ServiceCollectionExtensions.cs
```

### 3.3 Compliance Module

```
src/modules/compliance/
├── CreditRisk.Compliance.Domain/
│   ├── Entities/
│   │   ├── Transaction.cs
│   │   └── AmlAlert.cs
│   ├── Events/
│   │   ├── TransactionFlaggedDomainEvent.cs
│   │   └── AmlAlertCreatedDomainEvent.cs
│   ├── Repositories/
│   │   ├── ITransactionRepository.cs
│   │   └── IAmlAlertRepository.cs
│   └── Enums/
│       ├── TransactionStatus.cs
│       ├── AlertSeverity.cs
│       └── AlertStatus.cs              ← Required by AmlAlert entity and ReviewAlert command
├── CreditRisk.Compliance.Application/
│   ├── Commands/
│   │   ├── IngestTransaction/IngestTransactionCommand.cs
│   │   ├── IngestTransaction/IngestTransactionCommandHandler.cs
│   │   ├── ReviewAlert/ReviewAlertCommand.cs
│   │   └── ReviewAlert/ReviewAlertCommandHandler.cs
│   ├── Queries/
│   │   ├── ListAlerts/ListAlertsQuery.cs
│   │   └── ListAlerts/ListAlertsQueryHandler.cs
│   ├── DTOs/
│   │   ├── IngestTransactionRequest.cs
│   │   ├── TransactionDto.cs
│   │   └── AmlAlertDto.cs
│   └── Validators/
│       └── IngestTransactionRequestValidator.cs
├── CreditRisk.Compliance.Infrastructure/
│   ├── Persistence/
│   │   ├── ComplianceDbContext.cs
│   │   ├── Configurations/
│   │   │   ├── TransactionConfiguration.cs
│   │   │   └── AmlAlertConfiguration.cs
│   │   ├── Migrations/
│   │   └── Repositories/
│   │       ├── TransactionRepository.cs
│   │       └── AmlAlertRepository.cs
│   └── Services/
│       └── PepScreeningService.cs
└── CreditRisk.Compliance.Api/
    ├── Program.cs
    ├── Endpoints/
    │   ├── TransactionEndpoints.cs
    │   └── AlertEndpoints.cs
    ├── Middleware/
    │   └── GlobalExceptionMiddleware.cs  ← Each API module must have its OWN middleware
    ├── Serialization/
    │   └── ComplianceApiJsonContext.cs
    └── Extensions/
        └── ServiceCollectionExtensions.cs
```

---

## 4. Contracts and Interfaces

### 4.1 REST API Endpoints

#### IAM API — Base URL: `/api/v1`

| Method | Route | Auth | Request Body | Response | HTTP Codes |
|---|---|---|---|---|---|
| `POST` | `/auth/login` | None | `LoginRequest` | `LoginResponse` | 200, 401, 422 |
| `POST` | `/auth/logout` | Bearer | None | None | 204, 401 |
| `POST` | `/auth/refresh` | None | `RefreshTokenRequest` | `LoginResponse` | 200, 401 |
| `GET` | `/auth/jwks` | None | None | JWKS JSON | 200 |
| `POST` | `/users` | Admin | `CreateUserRequest` | `UserDto` | 201, 400, 401, 403, 409, 422 |
| `GET` | `/users/{id}` | Admin | None | `UserDto` | 200, 401, 403, 404 |
| `PUT` | `/users/{id}/roles` | Admin | `AssignRoleRequest` | `UserDto` | 200, 401, 403, 404, 422 |
| `GET` | `/health` | None | None | Health JSON | 200, 503 |

#### Credit Analysis API — Base URL: `/api/v1`

| Method | Route | Auth | Request Body | Response | HTTP Codes |
|---|---|---|---|---|---|
| `POST` | `/proposals` | DeskOperator | `CreateProposalRequest` | `ProposalAcceptedResponse` | 202, 401, 403, 422 |
| `GET` | `/proposals/{id}` | DeskOperator | None | `CreditProposalDto` | 200, 401, 403, 404 |
| `GET` | `/proposals` | DeskOperator | None (query params) | `PagedResult<ProposalListItemDto>` | 200, 401, 403 |
| `PUT` | `/proposals/{id}/submit` | DeskOperator | None | `CreditProposalDto` | 200, 401, 403, 404, 409 |
| `GET` | `/customers/{id}/credit-history` | DeskOperator | None | `CreditHistoryDto` | 200, 401, 403, 404 |
| `GET` | `/health` | None | None | Health JSON | 200, 503 |

#### Compliance API — Base URL: `/api/v1`

| Method | Route | Auth | Request Body | Response | HTTP Codes |
|---|---|---|---|---|---|
| `POST` | `/transactions` | ServiceAccount | `IngestTransactionRequest` | `TransactionAcceptedResponse` | 202, 401, 422 |
| `GET` | `/alerts` | ComplianceAnalyst | None (query params) | `PagedResult<AmlAlertDto>` | 200, 401, 403 |
| `GET` | `/alerts/{id}` | ComplianceAnalyst | None | `AmlAlertDto` | 200, 401, 403, 404 |
| `PUT` | `/alerts/{id}/review` | ComplianceAnalyst | `ReviewAlertRequest` | `AmlAlertDto` | 200, 401, 403, 404, 409 |
| `GET` | `/reports/str` | ComplianceAnalyst | None (query params) | `StrReportDto` | 200, 401, 403 |
| `GET` | `/health` | None | None | Health JSON | 200, 503 |

### 4.2 DTOs and Request/Response Types

```csharp
// File: src/modules/iam/CreditRisk.IAM.Application/DTOs/LoginRequest.cs
namespace CreditRisk.IAM.Application.DTOs;

/// <summary>Request body for POST /auth/login.</summary>
public sealed record LoginRequest
{
    public required string Email { get; init; }
    public required string Password { get; init; }
    public required string TotpCode { get; init; }   // 6-digit TOTP code
}
```

```csharp
// File: src/modules/iam/CreditRisk.IAM.Application/DTOs/LoginResponse.cs
namespace CreditRisk.IAM.Application.DTOs;

/// <summary>Response body for successful authentication.</summary>
public sealed record LoginResponse
{
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
    public required int ExpiresIn { get; init; }     // seconds (900 = 15 min)
    public required string TokenType { get; init; }  // "Bearer"
    public required string[] Roles { get; init; }
}
```

```csharp
// File: src/modules/iam/CreditRisk.IAM.Application/DTOs/CreateUserRequest.cs
namespace CreditRisk.IAM.Application.DTOs;

/// <summary>Request body for POST /users.</summary>
public sealed record CreateUserRequest
{
    public required string Email { get; init; }
    public required string FullName { get; init; }
    public required string Role { get; init; }       // "desk-operator" | "compliance-analyst" | "administrator"
    public required string TemporaryPassword { get; init; }
}
```

```csharp
// File: src/modules/credit-analysis/CreditRisk.CreditAnalysis.Application/DTOs/CreateProposalRequest.cs
namespace CreditRisk.CreditAnalysis.Application.DTOs;

/// <summary>Request body for POST /proposals.</summary>
public sealed record CreateProposalRequest
{
    public required string CustomerDocument { get; init; }   // CPF or CNPJ digits
    public required string CustomerDocumentType { get; init; } // "CPF" | "CNPJ"
    public required string CustomerName { get; init; }
    public required string CustomerEmail { get; init; }
    public required decimal MonthlyIncome { get; init; }
    public required decimal RequestedLimit { get; init; }
    public required string ProposalType { get; init; }       // "Individual" | "LegalEntity"
    public required bool BureauConsentGiven { get; init; }
    public required string BureauConsentIpAddress { get; init; }
}
```

```csharp
// File: src/modules/credit-analysis/CreditRisk.CreditAnalysis.Application/DTOs/CreditProposalDto.cs
namespace CreditRisk.CreditAnalysis.Application.DTOs;

/// <summary>Full credit proposal representation returned by GET /proposals/{id}.</summary>
public sealed record CreditProposalDto
{
    public required Guid Id { get; init; }
    public required Guid CustomerId { get; init; }
    public required string CustomerDocument { get; init; }
    public required decimal RequestedLimit { get; init; }
    public required decimal? ApprovedLimit { get; init; }
    public required string Status { get; init; }             // ProposalStatus enum value
    public required string? RiskRating { get; init; }        // "A" | "B" | "C" | "D" | "E" | null
    public required bool RequiresManualReview { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required DateTimeOffset UpdatedAt { get; init; }
    public required string CreatedBy { get; init; }
}
```

```csharp
// File: src/modules/compliance/CreditRisk.Compliance.Application/DTOs/IngestTransactionRequest.cs
namespace CreditRisk.Compliance.Application.DTOs;

/// <summary>Request body for POST /transactions. Minimal validation — schema only.</summary>
public sealed record IngestTransactionRequest
{
    public required Guid TransactionId { get; init; }        // Idempotency key
    public required string CustomerDocument { get; init; }
    public required string CustomerDocumentType { get; init; }
    public required decimal Amount { get; init; }
    public required string TransactionType { get; init; }    // "Credit" | "Debit" | "Transfer"
    public required string Channel { get; init; }            // "ATM" | "Online" | "Branch" | "Mobile"
    public required DateTimeOffset TransactionDate { get; init; }
    public required string OriginAccountId { get; init; }
    public required string DestinationAccountId { get; init; }
}
```

### 4.3 Domain Entities

```csharp
// File: src/modules/credit-analysis/CreditRisk.CreditAnalysis.Domain/Entities/CreditProposal.cs
using CreditRisk.CreditAnalysis.Domain.Enums;
using CreditRisk.CreditAnalysis.Domain.Events;
using CreditRisk.Shared.Kernel.Domain;
using CreditRisk.Shared.Kernel.Exceptions;
using CreditRisk.Shared.Kernel.Guard;
using CreditRisk.Shared.Kernel.ValueObjects;

namespace CreditRisk.CreditAnalysis.Domain.Entities;

/// <summary>
/// Aggregate root representing a credit proposal submitted by a desk operator.
/// Manages the lifecycle from Draft through Approved/Rejected.
/// </summary>
public sealed class CreditProposal : AggregateRoot
{
    public Guid CustomerId { get; private init; }
    public MoneyAmount RequestedLimit { get; private init; } = MoneyAmount.Zero();
    public MoneyAmount? ApprovedLimit { get; private set; }
    public ProposalStatus Status { get; private set; }
    public RiskRating? Rating { get; private set; }
    public bool RequiresManualReview { get; private set; }
    public string CreatedBy { get; private init; } = string.Empty;
    public string ProposalType { get; private init; } = string.Empty;

    private CreditProposal() : base() { }

    /// <summary>Creates a new credit proposal in Draft status.</summary>
    public static CreditProposal Create(
        Guid customerId,
        MoneyAmount requestedLimit,
        string proposalType,
        string createdBy,
        Guid correlationId)
    {
        Guard.AgainstEmpty(customerId, nameof(customerId));
        Guard.AgainstNull(requestedLimit, nameof(requestedLimit));
        Guard.AgainstNullOrWhiteSpace(proposalType, nameof(proposalType));
        Guard.AgainstNullOrWhiteSpace(createdBy, nameof(createdBy));

        if (requestedLimit.Amount <= 0)
            throw new DomainException("Proposal.InvalidLimit", "Requested limit must be greater than zero.");

        if (requestedLimit.Amount > 500_000m)
            throw new DomainException("Proposal.ExceedsMaxLimit", "Requested limit cannot exceed R$ 500,000.");

        var proposal = new CreditProposal
        {
            CustomerId = customerId,
            RequestedLimit = requestedLimit,
            ProposalType = proposalType,
            CreatedBy = createdBy,
            Status = ProposalStatus.Draft
        };

        proposal.RaiseDomainEvent(new CreditProposalCreatedDomainEvent
        {
            ProposalId = proposal.Id,
            CustomerId = customerId,
            RequestedLimit = requestedLimit.Amount,
            ProposalType = proposalType,
            CorrelationId = correlationId
        });

        return proposal;
    }

    /// <summary>Transitions the proposal from Draft to PendingEvaluation.</summary>
    public void Submit()
    {
        if (Status != ProposalStatus.Draft)
            throw new DomainException("Proposal.InvalidTransition",
                $"Cannot submit a proposal in status {Status}. Only Draft proposals can be submitted.");

        Status = ProposalStatus.PendingEvaluation;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>Applies the result of the scoring engine evaluation.</summary>
    public void ApplyEvaluation(RiskRating rating, MoneyAmount approvedLimit, bool requiresManualReview)
    {
        if (Status != ProposalStatus.PendingEvaluation)
            throw new DomainException("Proposal.InvalidTransition",
                $"Cannot evaluate a proposal in status {Status}.");

        Rating = rating;
        ApprovedLimit = approvedLimit;
        RequiresManualReview = requiresManualReview;
        Status = requiresManualReview ? ProposalStatus.PendingReview : ProposalStatus.Approved;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>Manually approves a proposal that required review.</summary>
    public void ManuallyApprove(string approvedBy)
    {
        if (Status != ProposalStatus.PendingReview)
            throw new DomainException("Proposal.InvalidTransition",
                $"Cannot manually approve a proposal in status {Status}.");

        Status = ProposalStatus.Approved;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>Rejects the proposal.</summary>
    public void Reject(string rejectedBy, string reason)
    {
        if (Status is ProposalStatus.Approved or ProposalStatus.Rejected)
            throw new DomainException("Proposal.InvalidTransition",
                $"Cannot reject a proposal in status {Status}.");

        Status = ProposalStatus.Rejected;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
```

```csharp
// File: src/modules/credit-analysis/CreditRisk.CreditAnalysis.Domain/Enums/ProposalStatus.cs
namespace CreditRisk.CreditAnalysis.Domain.Enums;

/// <summary>Lifecycle states of a credit proposal.</summary>
public enum ProposalStatus
{
    Draft = 0,
    PendingEvaluation = 1,
    PendingReview = 2,
    Approved = 3,
    Rejected = 4
}
```

```csharp
// File: src/modules/credit-analysis/CreditRisk.CreditAnalysis.Domain/Enums/RiskRating.cs
namespace CreditRisk.CreditAnalysis.Domain.Enums;

/// <summary>
/// Credit risk rating from A (lowest risk) to E (highest risk).
/// Follows BCB Resolução 4.557 risk classification framework.
/// </summary>
public enum RiskRating
{
    A = 1,   // Score >= 750: Excellent — auto-approve up to R$ 50,000
    B = 2,   // Score 650-749: Good — auto-approve up to R$ 20,000
    C = 3,   // Score 500-649: Fair — manual review required above R$ 5,000
    D = 4,   // Score 350-499: Poor — manual review always required
    E = 5    // Score < 350: Very Poor — auto-reject
}
```

```csharp
// File: src/modules/compliance/CreditRisk.Compliance.Domain/Enums/AlertStatus.cs
namespace CreditRisk.Compliance.Domain.Enums;

/// <summary>
/// Lifecycle states of an AML alert.
/// Used by AmlAlert entity and ReviewAlert command handler.
/// ⚠️ This enum MUST exist — it is referenced by AmlAlert, ReviewAlertCommand, and AmlAlertDto.
/// </summary>
public enum AlertStatus
{
    Open = 0,           // Alert created, awaiting analyst review
    UnderReview = 1,    // Analyst has started reviewing
    Confirmed = 2,      // Analyst confirmed as fraud/AML violation
    Dismissed = 3,      // Analyst dismissed as false positive
    Escalated = 4       // Escalated to senior compliance officer
}
```

### 4.4 Repository Interfaces

```csharp
// File: src/modules/credit-analysis/CreditRisk.CreditAnalysis.Domain/Repositories/ICreditProposalRepository.cs
using CreditRisk.CreditAnalysis.Domain.Entities;
using CreditRisk.CreditAnalysis.Domain.Enums;

namespace CreditRisk.CreditAnalysis.Domain.Repositories;

/// <summary>Repository interface for CreditProposal aggregate persistence.</summary>
public interface ICreditProposalRepository
{
    Task<CreditProposal?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CreditProposal>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CreditProposal>> GetByStatusAsync(ProposalStatus status, int page, int pageSize, CancellationToken cancellationToken = default);
    Task AddAsync(CreditProposal proposal, CancellationToken cancellationToken = default);
    Task UpdateAsync(CreditProposal proposal, CancellationToken cancellationToken = default);
    Task<int> CountByStatusAsync(ProposalStatus status, CancellationToken cancellationToken = default);
}
```

```csharp
// File: src/modules/credit-analysis/CreditRisk.CreditAnalysis.Application/Ports/IUnitOfWork.cs
namespace CreditRisk.CreditAnalysis.Application.Ports;

/// <summary>
/// Unit of Work abstraction. Wraps a database transaction.
/// CommitAsync dispatches domain events after the transaction commits.
/// </summary>
public interface IUnitOfWork
{
    Task<int> CommitAsync(CancellationToken cancellationToken = default);
}
```

### 4.5 JWT Revocation Middleware

```csharp
// File: src/modules/iam/CreditRisk.IAM.Api/Middleware/JwtRevocationMiddleware.cs
using CreditRisk.IAM.Application.Ports;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace CreditRisk.IAM.Api.Middleware;

/// <summary>
/// Middleware that checks every authenticated request against the Redis token revocation set.
/// If the JWT's jti claim is found in the revocation set, the request is rejected with 401.
/// Must be registered AFTER UseAuthentication() and BEFORE UseAuthorization().
/// </summary>
public sealed class JwtRevocationMiddleware(
    RequestDelegate next,
    ITokenRevocationStore revocationStore,
    ILogger<JwtRevocationMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            string? jti = context.User.FindFirst("jti")?.Value;

            if (string.IsNullOrEmpty(jti))
            {
                logger.LogWarning("Authenticated request missing jti claim. Rejecting.");
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            bool isRevoked = await revocationStore.IsRevokedAsync(jti, context.RequestAborted)
                .ConfigureAwait(false);

            if (isRevoked)
            {
                logger.LogWarning("Revoked token jti={Jti} rejected.", jti);
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(
                    new { error = "token_revoked", description = "The token has been revoked." },
                    context.RequestAborted)
                    .ConfigureAwait(false);
                return;
            }
        }

        await next(context).ConfigureAwait(false);

    }
}
```

### 4.6 Global Exception Middleware

> **⚠️ Module Isolation Rule:** Each API module (`IAM.Api`, `CreditAnalysis.Api`, `Compliance.Api`) must define its **own** `GlobalExceptionMiddleware` in its own `Middleware/` folder. **Never** reference another module's `GlobalExceptionMiddleware` — this creates a cross-module project dependency that violates Clean Architecture and causes build errors (`CS0246` / `CS0234`).

```csharp
// File: src/modules/credit-analysis/CreditRisk.CreditAnalysis.Api/Middleware/GlobalExceptionMiddleware.cs
using CreditRisk.Shared.Kernel.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace CreditRisk.CreditAnalysis.Api.Middleware;

/// <summary>
/// Catches unhandled exceptions and returns RFC 7807 Problem Details responses.
/// Never exposes stack traces. Logs full context for debugging.
/// </summary>
public sealed class GlobalExceptionMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context).ConfigureAwait(false);
        }
        catch (DomainException ex)
        {
            logger.LogWarning(ex, "Domain exception: {ErrorCode} — {Message}", ex.ErrorCode, ex.Message);
            await WriteProblemDetailsAsync(context, StatusCodes.Status422UnprocessableEntity,
                "Domain Rule Violation", ex.Message, ex.ErrorCode).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // Client disconnected — not an error
            context.Response.StatusCode = 499;
        }
        catch (Exception ex)
        {
            string correlationId = context.TraceIdentifier;
            logger.LogError(ex, "Unhandled exception. CorrelationId={CorrelationId}", correlationId);
            await WriteProblemDetailsAsync(context, StatusCodes.Status500InternalServerError,
                "Internal Server Error",
                "An unexpected error occurred. Please contact support with the correlation ID.",
                "InternalError",
                correlationId).ConfigureAwait(false);
        }
    }

    private static async Task WriteProblemDetailsAsync(
        HttpContext context,
        int statusCode,
        string title,
        string detail,
        string errorCode,
        string? correlationId = null)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        problem.Extensions["errorCode"] = errorCode;
        if (correlationId is not null)
            problem.Extensions["correlationId"] = correlationId;

        await context.Response.WriteAsJsonAsync(problem, context.RequestAborted).ConfigureAwait(false);
    }
}
```

```csharp
// File: src/modules/compliance/CreditRisk.Compliance.Api/Middleware/GlobalExceptionMiddleware.cs
// ⚠️ This is the Compliance-module-specific copy. Do NOT import from CreditRisk.CreditAnalysis.Api.
using CreditRisk.Shared.Kernel.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace CreditRisk.Compliance.Api.Middleware;

/// <summary>
/// Catches unhandled exceptions and returns RFC 7807 Problem Details responses.
/// Never exposes stack traces. Logs full context for debugging.
/// </summary>
public sealed class GlobalExceptionMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context).ConfigureAwait(false);
        }
        catch (DomainException ex)
        {
            logger.LogWarning(ex, "Domain exception: {ErrorCode} — {Message}", ex.ErrorCode, ex.Message);
            await WriteProblemDetailsAsync(context, StatusCodes.Status422UnprocessableEntity,
                "Domain Rule Violation", ex.Message, ex.ErrorCode).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            context.Response.StatusCode = 499;
        }
        catch (Exception ex)
        {
            string correlationId = context.TraceIdentifier;
            logger.LogError(ex, "Unhandled exception. CorrelationId={CorrelationId}", correlationId);
            await WriteProblemDetailsAsync(context, StatusCodes.Status500InternalServerError,
                "Internal Server Error",
                "An unexpected error occurred. Please contact support with the correlation ID.",
                "InternalError",
                correlationId).ConfigureAwait(false);
        }
    }

    private static async Task WriteProblemDetailsAsync(
        HttpContext context,
        int statusCode,
        string title,
        string detail,
        string errorCode,
        string? correlationId = null)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        problem.Extensions["errorCode"] = errorCode;
        if (correlationId is not null)
            problem.Extensions["correlationId"] = correlationId;

        await context.Response.WriteAsJsonAsync(problem, context.RequestAborted).ConfigureAwait(false);
    }
}
```

### 4.7 IAM API `Program.cs`

```csharp
// File: src/modules/iam/CreditRisk.IAM.Api/Program.cs
using CreditRisk.IAM.Api.Endpoints;
using CreditRisk.IAM.Api.Middleware;
using CreditRisk.IAM.Api.Serialization;
using CreditRisk.IAM.Infrastructure.Persistence;
using CreditRisk.Shared.Observability;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateSlimBuilder(args);

// REQUIRED: CreateSlimBuilder uses AddRoutingCore() internally, which does NOT register
// built-in route constraints like {id:guid}, {id:int}, {id:long}, etc.
// Without AddRouting(), any endpoint with a type constraint (e.g., MapGet("/{id:guid}", ...))
// will throw RegexErrorStubRouteConstraint at runtime and return 500 for all matching requests.
builder.Services.AddRouting();

// Observability
builder.Services.AddCreditRiskObservability(builder.Configuration, serviceName: "iam-api");

// JSON serialization — AOT-compatible
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, IamApiJsonContext.Default);
});

// Database
builder.Services.AddDbContext<IamDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")!));

// Authentication — Keycloak OIDC
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Keycloak__Authority"]!;
        options.Audience = builder.Configuration["Keycloak__Audience"] ?? "crcl-api";
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        options.TokenValidationParameters.ValidateIssuerSigningKey = true;
        options.TokenValidationParameters.ClockSkew = TimeSpan.FromSeconds(30);
    });

// Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequiresDeskOperator", policy =>
        policy.RequireAuthenticatedUser()
              .RequireClaim("roles", "desk-operator", "compliance-analyst", "administrator"));

    options.AddPolicy("RequiresComplianceAnalyst", policy =>
        policy.RequireAuthenticatedUser()
              .RequireClaim("roles", "compliance-analyst", "administrator"));

    options.AddPolicy("RequiresAdministrator", policy =>
        policy.RequireAuthenticatedUser()
              .RequireClaim("roles", "administrator"));
});

// Rate limiting
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("auth-endpoints", limiter =>
    {
        limiter.PermitLimit = 5;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
        limiter.QueueLimit = 0;
    });
});

// Health checks
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("Postgres")!, name: "postgres")
    .AddRedis(builder.Configuration.GetConnectionString("Redis")!, name: "redis");

// Infrastructure services
builder.Services.AddIamInfrastructure(builder.Configuration);

// OpenAPI
builder.Services.AddOpenApi();

WebApplication app = builder.Build();

// Middleware pipeline
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseAuthentication();
app.UseMiddleware<JwtRevocationMiddleware>();
app.UseAuthorization();
app.UseRateLimiter();

// Endpoints
app.MapAuthEndpoints();
app.MapUserEndpoints();
app.MapHealthChecks("/health");

app.MapSwagger();
app.UseSwaggerUI();

app.Run();
```

### 4.8 EF Core Configuration

> **⚠️ Mandatory `OnModelCreating` pattern for all `DbContext` classes:** EF Core's model discovery scans all `IReadOnlyList<T>` properties on entities. Because `AggregateRoot` exposes `IReadOnlyList<DomainEvent> DomainEvents`, EF Core will attempt to map `DomainEvent` as a navigation entity and require a primary key — causing migrations to fail with `The entity type 'DomainEvent' requires a primary key to be defined`.
>
> **Every `DbContext` in every module must override `OnModelCreating` and call `modelBuilder.Ignore<DomainEvent>()` as the first statement:**
>
> ```csharp
> // ✅ MANDATORY pattern for ALL DbContext classes (IamDbContext, CreditAnalysisDbContext, ComplianceDbContext)
> using CreditRisk.Shared.Kernel.Domain;
> using System.Reflection;
>
> protected override void OnModelCreating(ModelBuilder modelBuilder)
> {
>     // MANDATORY: Prevent EF Core from discovering DomainEvent as a navigation entity
>     // via AggregateRoot.DomainEvents (IReadOnlyList<DomainEvent>). Without this,
>     // EF Core tries to create a DomainEvent table and requires a primary key,
>     // causing migrations to fail.
>     modelBuilder.Ignore<DomainEvent>();
>
>     base.OnModelCreating(modelBuilder);
>
>     // Apply all IEntityTypeConfiguration<T> classes in this assembly
>     modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
> }
> ```

```csharp
// File: src/modules/credit-analysis/CreditRisk.CreditAnalysis.Infrastructure/Persistence/Configurations/CreditProposalConfiguration.cs
using CreditRisk.CreditAnalysis.Domain.Entities;
using CreditRisk.CreditAnalysis.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CreditRisk.CreditAnalysis.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for the CreditProposal aggregate.</summary>
internal sealed class CreditProposalConfiguration : IEntityTypeConfiguration<CreditProposal>
{
    public void Configure(EntityTypeBuilder<CreditProposal> builder)
    {
        builder.ToTable("credit_proposals");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id").ValueGeneratedNever();

        builder.Property(p => p.CustomerId).HasColumnName("customer_id").IsRequired();
        builder.Property(p => p.CreatedBy).HasColumnName("created_by").HasMaxLength(100).IsRequired();
        builder.Property(p => p.ProposalType).HasColumnName("proposal_type").HasMaxLength(20).IsRequired();
        builder.Property(p => p.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(p => p.UpdatedAt).HasColumnName("updated_at").IsRequired();

        // Status stored as string for readability
        builder.Property(p => p.Status)
            .HasColumnName("status")
            .HasMaxLength(30)
            .HasConversion(
                v => v.ToString(),
                v => Enum.Parse<ProposalStatus>(v))
            .IsRequired();

        // RiskRating stored as string, nullable
        builder.Property(p => p.Rating)
            .HasColumnName("risk_rating")
            .HasMaxLength(1)
            .HasConversion(
                v => v.HasValue ? v.Value.ToString() : null,
                v => v != null ? Enum.Parse<RiskRating>(v) : (RiskRating?)null);

        // MoneyAmount owned type
        builder.OwnsOne(p => p.RequestedLimit, money =>
        {
            money.Property(m => m.Amount).HasColumnName("requested_limit").HasPrecision(18, 2).IsRequired();
            money.Ignore(m => m.Currency);
        });

        builder.OwnsOne(p => p.ApprovedLimit, money =>
        {
            money.Property(m => m.Amount).HasColumnName("approved_limit").HasPrecision(18, 2);
            money.Ignore(m => m.Currency);
        });

        builder.Property(p => p.RequiresManualReview).HasColumnName("requires_manual_review").IsRequired();

        // Optimistic concurrency via PostgreSQL xmin system column
        builder.UseXminAsConcurrencyToken();

        // Indexes
        builder.HasIndex(p => p.CustomerId).HasDatabaseName("ix_credit_proposals_customer_id");
        builder.HasIndex(p => p.Status).HasDatabaseName("ix_credit_proposals_status");
        builder.HasIndex(p => p.CreatedAt).HasDatabaseName("ix_credit_proposals_created_at");
    }
}
```

---

## 5. Business Rules and Invariants

### 5.1 Credit Scoring Engine — Risk Rating Matrix

The scoring engine computes a composite score (0–1000) from three inputs and maps it to a risk rating.

**Score Computation:**

```
CompositeScore = (BureauScore × 0.50) + (DebtRatioScore × 0.30) + (IncomeMultiplierScore × 0.20)
```

**Bureau Score Bands (weight: 50%):**

| Bureau Score Range | Points |
|---|---|
| 800 – 1000 | 1000 |
| 700 – 799 | 800 |
| 600 – 699 | 600 |
| 500 – 599 | 400 |
| 350 – 499 | 200 |
| 0 – 349 | 0 |

**Debt Ratio Score (weight: 30%):**

Debt Ratio = Total Monthly Debt Obligations / Monthly Income

| Debt Ratio | Points |
|---|---|
| < 20% | 1000 |
| 20% – 34% | 750 |
| 35% – 49% | 500 |
| 50% – 64% | 250 |
| ≥ 65% | 0 |

**Income Multiplier Score (weight: 20%):**

Income Multiplier = Requested Limit / Monthly Income

| Income Multiplier | Points |
|---|---|
| ≤ 2× | 1000 |
| 2× – 4× | 750 |
| 4× – 6× | 500 |
| 6× – 8× | 250 |
| > 8× | 0 |

**Risk Rating Mapping:**

| Composite Score | Rating | Auto-Approve Threshold | Manual Review Threshold |
|---|---|---|---|
| ≥ 750 | A | Up to R$ 50,000 | Above R$ 50,000 |
| 650 – 749 | B | Up to R$ 20,000 | Above R$ 20,000 |
| 500 – 649 | C | Up to R$ 5,000 | Always above R$ 5,000 |
| 350 – 499 | D | Never | Always manual review |
| < 350 | E | Auto-reject | N/A |

### 5.2 Credit Proposal Lifecycle Rules

1. A proposal can only be submitted when in `Draft` status.
2. A proposal can only be evaluated when in `PendingEvaluation` status.
3. A proposal with Rating E is automatically rejected — no manual override permitted.
4. A proposal with Rating D always requires manual review regardless of amount.
5. `ApprovedLimit` is always ≤ `RequestedLimit`.
6. `ApprovedLimit` is always ≤ R$ 500,000 (system-wide maximum).
7. Bureau consent must be recorded before a bureau query is initiated.
8. A customer may not have more than 3 active proposals simultaneously.

### 5.3 AML/CFT Rules Engine

The compliance worker applies these rules in order. Any match flags the transaction:

**Rule 1 — Smurfing Detection (Split Transaction):**
- Trigger: 3 or more transactions from the same customer within 24 hours, each below R$ 10,000, with a combined total exceeding R$ 30,000.
- Severity: High
- COAF reference: Circular BCB 3.978/2020, Art. 12

**Rule 2 — Velocity Anomaly:**
- Trigger: Transaction count in the last 1 hour exceeds 3× the customer's 30-day average hourly rate.
- Severity: Medium

**Rule 3 — Amount Anomaly:**
- Trigger: Transaction amount exceeds 5× the customer's average transaction amount in the last 90 days.
- Severity: Medium

**Rule 4 — PEP/Sanctions Match:**
- Trigger: Customer document matches any entry in the PEP list or OFAC/UN sanctions list.
- Severity: Critical
- Action: Block transaction immediately, create alert, notify compliance analyst via SignalR.

**Rule 5 — Round Amount Threshold:**
- Trigger: Transaction amount is exactly R$ 10,000 or a multiple thereof (R$ 20,000, R$ 30,000, etc.) via Cash channel.
- Severity: Low

### 5.4 Token Lifecycle Rules

1. Access tokens expire in exactly 900 seconds (15 minutes).
2. Refresh tokens expire in exactly 604,800 seconds (7 days).
3. On logout, the JWT `jti` claim is stored in Redis with TTL = remaining token lifetime.
4. Every authenticated API request checks the `jti` against the Redis revocation set.
5. Service accounts use Client Credentials flow — no refresh tokens, no MFA.
6. MFA (TOTP) is mandatory for all human operator logins.

### 5.5 LGPD Data Minimization Rules

1. CPF/CNPJ must never appear in log messages — use pseudonymous `CustomerId` (UUID).
2. Customer email and phone are stored encrypted using `pgcrypto` column-level encryption.
3. Bureau query results are cached in Redis for 24 hours, encrypted with AES-256-GCM.
4. Customer data anonymization replaces PII with `ANONYMIZED-{uuid}` — financial records are retained.

---

## 6. Design Decisions and Architectural Patterns

### 6.1 Minimal APIs over Controllers

**Decision:** All API endpoints use ASP.NET Core Minimal APIs, not MVC Controllers.

**Rationale:** Minimal APIs have lower overhead, are AOT-compatible without additional configuration, and produce smaller binaries. Controllers require reflection-based model binding that conflicts with Native AOT.

**Canonical endpoint definition:**

```csharp
// File: src/modules/credit-analysis/CreditRisk.CreditAnalysis.Api/Endpoints/ProposalEndpoints.cs
using CreditRisk.CreditAnalysis.Application.Commands.CreateProposal;
using CreditRisk.CreditAnalysis.Application.DTOs;
using CreditRisk.CreditAnalysis.Application.Queries.GetProposalById;
using Microsoft.AspNetCore.Mvc;

namespace CreditRisk.CreditAnalysis.Api.Endpoints;

/// <summary>Registers all credit proposal endpoints on the application.</summary>
public static class ProposalEndpoints
{
    public static IEndpointRouteBuilder MapProposalEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/v1/proposals")
            .WithTags("Credit Proposals")
            .RequireAuthorization("RequiresDeskOperator");

        group.MapPost("/", CreateProposalAsync)
            .WithName("CreateProposal")
            .WithSummary("Submit a new credit proposal")
            .Produces<ProposalAcceptedResponse>(StatusCodes.Status202Accepted)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapGet("/{id:guid}", GetProposalByIdAsync)
            .WithName("GetProposalById")
            .WithSummary("Get a credit proposal by ID")
            .Produces<CreditProposalDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{id:guid}/submit", SubmitProposalAsync)
            .WithName("SubmitProposal")
            .WithSummary("Submit a draft proposal for evaluation")
            .Produces<CreditProposalDto>()
            .ProducesProblem(StatusCodes.Status409Conflict);

        return app;
    }

    private static async Task<IResult> CreateProposalAsync(
        [FromBody] CreateProposalRequest request,
        ICreateProposalHandler handler,
        HttpContext context,
        CancellationToken ct)
    {
        string operatorId = context.User.FindFirst("sub")?.Value
            ?? throw new InvalidOperationException("sub claim missing from token.");

        var command = new CreateProposalCommand(
            CustomerDocument: request.CustomerDocument,
            CustomerDocumentType: request.CustomerDocumentType,
            CustomerName: request.CustomerName,
            CustomerEmail: request.CustomerEmail,
            MonthlyIncome: request.MonthlyIncome,
            RequestedLimit: request.RequestedLimit,
            ProposalType: request.ProposalType,
            BureauConsentGiven: request.BureauConsentGiven,
            BureauConsentIpAddress: request.BureauConsentIpAddress,
            CreatedBy: operatorId,
            CorrelationId: Guid.Parse(context.TraceIdentifier.Replace(":", "-").PadRight(36, '0')[..36]));

        var result = await handler.HandleAsync(command, ct).ConfigureAwait(false);

        return result.IsSuccess
            ? Results.Accepted($"/api/v1/proposals/{result.Value.ProposalId}",
                new ProposalAcceptedResponse { ProposalId = result.Value.ProposalId })
            : result.Error.HttpStatusCode switch
            {
                422 => Results.UnprocessableEntity(result.Error.ToProblemDetails()),
                409 => Results.Conflict(result.Error.ToProblemDetails()),
                _ => Results.BadRequest(result.Error.ToProblemDetails())
            };
    }

    private static async Task<IResult> GetProposalByIdAsync(
        Guid id,
        IGetProposalByIdHandler handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(new GetProposalByIdQuery(id), ct).ConfigureAwait(false);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.NotFound(result.Error.ToProblemDetails());
    }

    private static async Task<IResult> SubmitProposalAsync(
        Guid id,
        ISubmitProposalHandler handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(new SubmitProposalCommand(id), ct).ConfigureAwait(false);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : result.Error.HttpStatusCode switch
            {
                404 => Results.NotFound(result.Error.ToProblemDetails()),
                409 => Results.Conflict(result.Error.ToProblemDetails()),
                _ => Results.UnprocessableEntity(result.Error.ToProblemDetails())
            };
    }
}
```

### 6.2 Repository + Unit of Work Pattern

```csharp
// File: src/modules/credit-analysis/CreditRisk.CreditAnalysis.Infrastructure/Persistence/UnitOfWork.cs
using CreditRisk.CreditAnalysis.Application.Ports;
using CreditRisk.Shared.Kernel.Domain;
using MassTransit;

namespace CreditRisk.CreditAnalysis.Infrastructure.Persistence;

/// <summary>
/// Unit of Work implementation. Saves EF Core changes and dispatches domain events
/// via MassTransit after the transaction commits.
/// </summary>
public sealed class UnitOfWork(
    CreditAnalysisDbContext dbContext,
    IPublishEndpoint publishEndpoint,
    ILogger<UnitOfWork> logger) : IUnitOfWork
{
    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        // Collect domain events before saving (saving clears them)
        var domainEvents = dbContext.ChangeTracker
            .Entries<AggregateRoot>()
            .SelectMany(e => e.Entity.DomainEvents)
            .ToList();

        int rowsAffected = await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // Clear events after successful save
        foreach (var entry in dbContext.ChangeTracker.Entries<AggregateRoot>())
            entry.Entity.ClearDomainEvents();

        // Dispatch domain events as integration events via MassTransit
        foreach (var domainEvent in domainEvents)
        {
            logger.LogDebug("Dispatching domain event {EventType} with EventId={EventId}",
                domainEvent.GetType().Name, domainEvent.EventId);

            await publishEndpoint.Publish(domainEvent, domainEvent.GetType(), cancellationToken)
                .ConfigureAwait(false);
        }

        return rowsAffected;
    }
}
```

### 6.3 Credit Scoring Engine Implementation

```csharp
// File: src/modules/credit-analysis/CreditRisk.CreditAnalysis.Infrastructure/Services/CreditScoringEngine.cs
using CreditRisk.CreditAnalysis.Domain.Enums;
using CreditRisk.CreditAnalysis.Domain.Services;

namespace CreditRisk.CreditAnalysis.Infrastructure.Services;

/// <summary>
/// Implements the credit scoring algorithm defined in SPEC-02 Section 5.1.
/// Computes a composite score from bureau score, debt ratio, and income multiplier.
/// </summary>
public sealed class CreditScoringEngine : ICreditScoringEngine
{
    public ScoringResult Evaluate(
        int bureauScore,
        decimal monthlyIncome,
        decimal totalMonthlyDebt,
        decimal requestedLimit)
    {
        decimal bureauPoints = ComputeBureauPoints(bureauScore);
        decimal debtRatioPoints = ComputeDebtRatioPoints(monthlyIncome, totalMonthlyDebt);
        decimal incomeMultiplierPoints = ComputeIncomeMultiplierPoints(monthlyIncome, requestedLimit);

        decimal compositeScore = (bureauPoints * 0.50m)
                               + (debtRatioPoints * 0.30m)
                               + (incomeMultiplierPoints * 0.20m);

        RiskRating rating = compositeScore switch
        {
            >= 750 => RiskRating.A,
            >= 650 => RiskRating.B,
            >= 500 => RiskRating.C,
            >= 350 => RiskRating.D,
            _ => RiskRating.E
        };

        decimal approvedLimit = ComputeApprovedLimit(rating, requestedLimit);
        bool requiresManualReview = DeterminesManualReview(rating, approvedLimit);

        return new ScoringResult(
            CompositeScore: compositeScore,
            Rating: rating,
            ApprovedLimit: approvedLimit,
            RequiresManualReview: requiresManualReview);
    }

    private static decimal ComputeBureauPoints(int bureauScore)
    {
        return bureauScore switch
        {
            >= 800 => 1000m,
            >= 700 => 800m,
            >= 600 => 600m,
            >= 500 => 400m,
            >= 350 => 200m,
            _ => 0m
        };
    }

    private static decimal ComputeDebtRatioPoints(decimal monthlyIncome, decimal totalMonthlyDebt)
    {
        if (monthlyIncome <= 0) return 0m;
        decimal ratio = totalMonthlyDebt / monthlyIncome;
        return ratio switch
        {
            < 0.20m => 1000m,
            < 0.35m => 750m,
            < 0.50m => 500m,
            < 0.65m => 250m,
            _ => 0m
        };
    }

    private static decimal ComputeIncomeMultiplierPoints(decimal monthlyIncome, decimal requestedLimit)
    {
        if (monthlyIncome <= 0) return 0m;
        decimal multiplier = requestedLimit / monthlyIncome;
        return multiplier switch
        {
            <= 2m => 1000m,
            <= 4m => 750m,
            <= 6m => 500m,
            <= 8m => 250m,
            _ => 0m
        };
    }

    private static decimal ComputeApprovedLimit(RiskRating rating, decimal requestedLimit)
    {
        return rating switch
        {
            RiskRating.A => Math.Min(requestedLimit, 500_000m),
            RiskRating.B => Math.Min(requestedLimit, 500_000m),
            RiskRating.C => Math.Min(requestedLimit, 500_000m),
            RiskRating.D => Math.Min(requestedLimit, 500_000m),
            RiskRating.E => 0m,
            _ => 0m
        };
    }

    private static bool DeterminesManualReview(RiskRating rating, decimal approvedLimit)
    {
        return rating switch
        {
            RiskRating.A => approvedLimit > 50_000m,
            RiskRating.B => approvedLimit > 20_000m,
            RiskRating.C => approvedLimit > 5_000m,
            RiskRating.D => true,
            RiskRating.E => false,
            _ => true
        };
    }
}

/// <summary>Result of the credit scoring evaluation.</summary>
public sealed record ScoringResult(
    decimal CompositeScore,
    RiskRating Rating,
    decimal ApprovedLimit,
    bool RequiresManualReview);
```

### 6.4 `ServiceCollectionExtensions` Pattern

Each API module must have an `Extensions/ServiceCollectionExtensions.cs` that registers all module-specific services. This keeps `Program.cs` clean and avoids registering services from other modules.

> **⚠️ Critical rules for `ServiceCollectionExtensions`:**
> - Do NOT register `IPipelineBehavior<,>` — this is a MediatR interface that does not exist in this codebase.
> - Do NOT use `StatusCodes.*` constants in Application layer projects — use integer literals (422, 409, etc.) instead.
> - Classes registered via DI must be `public` — `internal sealed` causes `InvalidOperationException` at runtime.
> - Add `using CreditRisk.Shared.Kernel.CQRS;` to resolve `ICommandHandler<,>` and `IQueryHandler<,>`.
> - Add `using CreditRisk.Shared.Kernel.Result;` to resolve `PagedResult<T>` — never redefine it locally.
> - Add `using Microsoft.EntityFrameworkCore.Diagnostics;` to resolve `SaveChangesInterceptor`.

```csharp
// File: src/modules/credit-analysis/CreditRisk.CreditAnalysis.Api/Extensions/ServiceCollectionExtensions.cs
using CreditRisk.CreditAnalysis.Application.Commands.CreateProposal;
using CreditRisk.CreditAnalysis.Application.Commands.SubmitProposal;
using CreditRisk.CreditAnalysis.Application.DTOs;
using CreditRisk.CreditAnalysis.Application.Queries.GetProposalById;
using CreditRisk.CreditAnalysis.Application.Queries.ListProposals;
using CreditRisk.CreditAnalysis.Application.Validators;
using CreditRisk.CreditAnalysis.Domain.Repositories;
using CreditRisk.CreditAnalysis.Domain.Services;
using CreditRisk.CreditAnalysis.Infrastructure.Persistence;
using CreditRisk.CreditAnalysis.Infrastructure.Persistence.Repositories;
using CreditRisk.CreditAnalysis.Infrastructure.Services;
using CreditRisk.Shared.Kernel.CQRS;
using CreditRisk.Shared.Kernel.Result;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace CreditRisk.CreditAnalysis.Api.Extensions;

/// <summary>
/// Registers all CreditAnalysis module services.
/// Called from Program.cs: builder.Services.AddCreditAnalysisModule(builder.Configuration);
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCreditAnalysisModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        services.AddDbContext<CreditAnalysisDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Postgres")!));

        // Repositories
        services.AddScoped<ICreditProposalRepository, CreditProposalRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();

        // Unit of Work — must be public sealed, not internal sealed
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Domain Services — must be public sealed, not internal sealed
        services.AddScoped<ICreditScoringEngine, CreditScoringEngine>();

        // Command Handlers — use ICommandHandler<,> from CreditRisk.Shared.Kernel.CQRS
        services.AddScoped<ICommandHandler<CreateProposalCommand, CreateProposalResult>, CreateProposalCommandHandler>();
        services.AddScoped<ICommandHandler<SubmitProposalCommand, CreditProposalDto>, SubmitProposalCommandHandler>();

        // Query Handlers — use IQueryHandler<,> from CreditRisk.Shared.Kernel.CQRS
        services.AddScoped<IQueryHandler<GetProposalByIdQuery, CreditProposalDto>, GetProposalByIdQueryHandler>();
        services.AddScoped<IQueryHandler<ListProposalsQuery, PagedResult<ProposalListItemDto>>, ListProposalsQueryHandler>();

        // Validators — explicit registration (no assembly scanning — AOT incompatible)
        services.AddScoped<IValidator<CreateProposalRequest>, CreateProposalRequestValidator>();

        // ❌ NEVER register IPipelineBehavior<,> — it does not exist in this codebase
        // services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>)); // WRONG

        return services;
    }
}
```

### 6.5 `appsettings.Development.json` — Required `ConnectionStrings` Section

> **⚠️ Critical:** Every API project must have an `appsettings.Development.json` file with a `ConnectionStrings` section. Without it, `builder.Configuration.GetConnectionString("Postgres")` returns `null`, causing `ArgumentNullException` at startup.
>
> The `!` (null-forgiving) operator in `GetConnectionString("Postgres")!` suppresses the compiler warning but does NOT prevent a runtime crash if the key is missing.
>
> **⚠️ Use `localhost` — not Docker service names:** When running APIs locally (outside Docker), all hostnames must be `localhost`. Using Docker service names (`rabbitmq`, `redis`, `postgres`) will cause connection failures because those names only resolve inside the Docker network.
>
> **⚠️ Redis — always include `abortConnect=false`:** Without `abortConnect=false`, StackExchange.Redis throws `RedisConnectionException` at startup if Redis is briefly unavailable or slow to respond. With `abortConnect=false`, the client retries in the background and the service starts normally.

```json
// File: src/modules/iam/CreditRisk.IAM.Api/appsettings.Development.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  },
  "ConnectionStrings": {
    "Postgres": "Host=localhost;Port=5432;Database=creditrisk;Username=crcl;Password=CHANGE_ME;SearchPath=iam",
    "Redis": "localhost:6379,password=CHANGE_ME_REDIS_PASSWORD,ssl=false,abortConnect=false"
  },
  "Keycloak": {
    "Authority": "http://localhost:8080/realms/crcl",
    "Audience": "crcl-api"
  },
  "OTEL_EXPORTER_OTLP_ENDPOINT": "http://localhost:4317",
  "RabbitMQ": {
    "Host": "localhost",
    "VHost": "crcl",
    "Username": "crcl_broker",
    "Password": "CHANGE_ME_RABBITMQ_PASSWORD"
  }
}
```

```json
// File: src/modules/credit-analysis/CreditRisk.CreditAnalysis.Api/appsettings.Development.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  },
  "ConnectionStrings": {
    "Postgres": "Host=localhost;Port=5432;Database=creditrisk;Username=crcl;Password=CHANGE_ME;SearchPath=credit"
  },
  "OTEL_EXPORTER_OTLP_ENDPOINT": "http://localhost:4317",
  "RabbitMQ": {
    "Host": "localhost",
    "VHost": "crcl",
    "Username": "crcl_broker",
    "Password": "CHANGE_ME_RABBITMQ_PASSWORD"
  }
}
```

```json
// File: src/modules/compliance/CreditRisk.Compliance.Api/appsettings.Development.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  },
  "ConnectionStrings": {
    "Postgres": "Host=localhost;Port=5432;Database=creditrisk;Username=crcl;Password=CHANGE_ME;SearchPath=compliance"
  },
  "OTEL_EXPORTER_OTLP_ENDPOINT": "http://localhost:4317",
  "RabbitMQ": {
    "Host": "localhost",
    "VHost": "crcl",
    "Username": "crcl_broker",
    "Password": "CHANGE_ME_RABBITMQ_PASSWORD"
  }
}
```

### 6.6 `launchSettings.json` — Required Port Configuration

> **⚠️ Critical:** `dotnet new` auto-generates random ports in `launchSettings.json`. These must be overridden to the canonical ports before running any API locally. Using wrong ports will cause health check commands and integration tests to fail silently.

The required ports for local development are:

| Module | Port | URL |
|---|---|---|
| IAM API | **5001** | `http://localhost:5001` |
| Credit Analysis API | **5002** | `http://localhost:5002` |
| Compliance API | **5003** | `http://localhost:5003` |

Each API project's `Properties/launchSettings.json` must use these exact ports:

```json
// File: src/modules/iam/CreditRisk.IAM.Api/Properties/launchSettings.json
{
  "profiles": {
    "Development": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": false,
      "applicationUrl": "http://localhost:5001",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

```json
// File: src/modules/credit-analysis/CreditRisk.CreditAnalysis.Api/Properties/launchSettings.json
{
  "profiles": {
    "Development": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": false,
      "applicationUrl": "http://localhost:5002",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

```json
// File: src/modules/compliance/CreditRisk.Compliance.Api/Properties/launchSettings.json
{
  "profiles": {
    "Development": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": false,
      "applicationUrl": "http://localhost:5003",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

> **Note:** The `environmentVariables` section in `launchSettings.json` sets `ASPNETCORE_ENVIRONMENT=Development` automatically when using `dotnet run --launch-profile Development`. However, when running `dotnet ef database update` (which does not use `launchSettings.json`), you must still export `ASPNETCORE_ENVIRONMENT=Development` manually. See `setup.md` §5.7.2.

---

## 7. Configuration and Environment Variables

### 7.1 IAM API

| Variable | Type | Dev Default | Description |
|---|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | `string` | `Development` | Controls Swagger, CORS, log level |
| `ConnectionStrings__Postgres` | `string` | (from `.env`) | PostgreSQL connection string |
| `ConnectionStrings__Redis` | `string` | (from `.env`) | Redis connection string |
| `Keycloak__Authority` | `string` | `http://keycloak:8080/realms/crcl` | Keycloak realm URL |
| `Keycloak__Audience` | `string` | `crcl-api` | Expected JWT audience |
| `OTEL_EXPORTER_OTLP_ENDPOINT` | `string` | `http://otel-collector:4317` | OTel collector endpoint |
| `RateLimiting__AuthEndpoints__PermitLimit` | `int` | `5` | Max auth requests per minute per IP |
| `TokenRevocation__RedisKeyPrefix` | `string` | `token:revoked:` | Redis key prefix for revoked JTIs |

### 7.2 Credit Analysis API

| Variable | Type | Dev Default | Description |
|---|---|---|---|
| `ConnectionStrings__Postgres` | `string` | (from `.env`) | PostgreSQL connection string |
| `RabbitMQ__Host` | `string` | `rabbitmq` | RabbitMQ hostname |
| `RabbitMQ__VHost` | `string` | `crcl` | RabbitMQ virtual host |
| `RabbitMQ__Username` | `string` | `crcl_broker` | RabbitMQ username |
| `RabbitMQ__Password` | `string` | (secret) | RabbitMQ password |
| `CreditScoring__MaxCreditLimit` | `decimal` | `500000` | System-wide maximum credit limit |
| `CreditScoring__AutoApproveThresholdA` | `decimal` | `50000` | Auto-approve threshold for Rating A |
| `CreditScoring__AutoApproveThresholdB` | `decimal` | `20000` | Auto-approve threshold for Rating B |
| `CreditScoring__AutoApproveThresholdC` | `decimal` | `5000` | Auto-approve threshold for Rating C |
| `CreditScoring__MaxActiveProposalsPerCustomer` | `int` | `3` | Max simultaneous active proposals |

### 7.3 Compliance API

| Variable | Type | Dev Default | Description |
|---|---|---|---|
| `ConnectionStrings__Postgres` | `string` | (from `.env`) | PostgreSQL connection string |
| `RabbitMQ__Host` | `string` | `rabbitmq` | RabbitMQ hostname |
| `AmlRules__SmurfingWindowHours` | `int` | `24` | Time window for smurfing detection |
| `AmlRules__SmurfingThresholdAmount` | `decimal` | `10000` | Per-transaction threshold for smurfing |
| `AmlRules__SmurfingCombinedThreshold` | `decimal` | `30000` | Combined amount threshold for smurfing |
| `AmlRules__VelocityWindowMinutes` | `int` | `60` | Time window for velocity checks |
| `AmlRules__VelocityMultiplier` | `decimal` | `3.0` | Multiplier over average for velocity alert |
| `AmlRules__AmountAnomalyMultiplier` | `decimal` | `5.0` | Multiplier over average for amount alert |
| `PepScreening__CacheTtlHours` | `int` | `24` | Redis TTL for PEP screening results |

---

## 8. Detailed Test Scenarios

### 8.1 Credit Proposal Domain Tests

```csharp
// File: tests/unit/CreditRisk.CreditAnalysis.Domain.Tests/Entities/CreditProposalTests.cs
using CreditRisk.CreditAnalysis.Domain.Entities;
using CreditRisk.CreditAnalysis.Domain.Enums;
using CreditRisk.Shared.Kernel.Exceptions;
using CreditRisk.Shared.Kernel.ValueObjects;
using FluentAssertions;

namespace CreditRisk.CreditAnalysis.Domain.Tests.Entities;

public sealed class CreditProposalTests
{
    [Fact]
    public void Create_ValidInputs_ReturnsDraftProposal()
    
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var limit = MoneyAmount.Create(10_000m);

        // Act
        var proposal = CreditProposal.Create(customerId, limit, "Individual", "operator-1", Guid.NewGuid());

        // Assert
        proposal.Status.Should().Be(ProposalStatus.Draft);
        proposal.CustomerId.Should().Be(customerId);
        proposal.RequestedLimit.Amount.Should().Be(10_000m);
        proposal.DomainEvents.Should().HaveCount(1);
        proposal.DomainEvents[0].Should().BeOfType<CreditProposalCreatedDomainEvent>();
    }

    [Fact]
    public void Create_ExceedsMaxLimit_ThrowsDomainException()
    {
        // Arrange
        var limit = MoneyAmount.Create(500_001m);

        // Act
        Action act = () => CreditProposal.Create(Guid.NewGuid(), limit, "Individual", "op", Guid.NewGuid());

        // Assert
        act.Should().Throw<DomainException>().WithMessage("*500,000*");
    }

    [Fact]
    public void Submit_FromDraftStatus_TransitionsToPendingEvaluation()
    {
        // Arrange
        var proposal = CreditProposal.Create(Guid.NewGuid(), MoneyAmount.Create(5_000m), "Individual", "op", Guid.NewGuid());

        // Act
        proposal.Submit();

        // Assert
        proposal.Status.Should().Be(ProposalStatus.PendingEvaluation);
    }

    [Fact]
    public void Submit_FromNonDraftStatus_ThrowsDomainException()
    {
        // Arrange
        var proposal = CreditProposal.Create(Guid.NewGuid(), MoneyAmount.Create(5_000m), "Individual", "op", Guid.NewGuid());
        proposal.Submit();

        // Act
        Action act = () => proposal.Submit();

        // Assert
        act.Should().Throw<DomainException>().WithMessage("*Draft*");
    }

    [Theory]
    [InlineData(RiskRating.A, 10_000, false)]   // A, below 50k threshold — auto-approve
    [InlineData(RiskRating.A, 60_000, true)]    // A, above 50k threshold — manual review
    [InlineData(RiskRating.B, 15_000, false)]   // B, below 20k threshold — auto-approve
    [InlineData(RiskRating.B, 25_000, true)]    // B, above 20k threshold — manual review
    [InlineData(RiskRating.D, 1_000, true)]     // D — always manual review
    public void ApplyEvaluation_SetsRatingAndReviewFlag(
        RiskRating rating, decimal approvedAmount, bool expectedManualReview)
    {
        // Arrange
        var proposal = CreditProposal.Create(Guid.NewGuid(), MoneyAmount.Create(approvedAmount), "Individual", "op", Guid.NewGuid());
        proposal.Submit();

        // Act
        proposal.ApplyEvaluation(rating, MoneyAmount.Create(approvedAmount), expectedManualReview);

        // Assert
        proposal.Rating.Should().Be(rating);
        proposal.RequiresManualReview.Should().Be(expectedManualReview);
        proposal.Status.Should().Be(expectedManualReview ? ProposalStatus.PendingReview : ProposalStatus.Approved);
    }
}
```

### 8.2 Credit Scoring Engine Tests

```csharp
// File: tests/unit/CreditRisk.CreditAnalysis.Domain.Tests/Services/CreditScoringEngineTests.cs
using CreditRisk.CreditAnalysis.Domain.Enums;
using CreditRisk.CreditAnalysis.Infrastructure.Services;
using FluentAssertions;

namespace CreditRisk.CreditAnalysis.Domain.Tests.Services;

public sealed class CreditScoringEngineTests
{
    private readonly CreditScoringEngine _engine = new();

    [Theory]
    [InlineData(850, 5_000, 1_000, 10_000, RiskRating.A)]   // High bureau, low debt, low multiplier
    [InlineData(720, 8_000, 2_000, 15_000, RiskRating.B)]   // Good bureau, moderate debt
    [InlineData(580, 6_000, 2_400, 20_000, RiskRating.C)]   // Fair bureau, moderate debt
    [InlineData(400, 5_000, 3_000, 25_000, RiskRating.D)]   // Poor bureau, high debt
    [InlineData(200, 3_000, 2_700, 30_000, RiskRating.E)]   // Very poor bureau, very high debt
    public void Evaluate_ScenarioMatrix_ReturnsExpectedRating(
        int bureauScore, decimal monthlyIncome, decimal totalDebt, decimal requestedLimit, RiskRating expectedRating)
    {
        // Act
        var result = _engine.Evaluate(bureauScore, monthlyIncome, totalDebt, requestedLimit);

        // Assert
        result.Rating.Should().Be(expectedRating);
    }

    [Fact]
    public void Evaluate_RatingE_ApprovedLimitIsZero()
    {
        // Arrange — very poor score
        // Act
        var result = _engine.Evaluate(bureauScore: 100, monthlyIncome: 2_000, totalDebt: 1_900, requestedLimit: 50_000);

        // Assert
        result.Rating.Should().Be(RiskRating.E);
        result.ApprovedLimit.Should().Be(0m);
        result.RequiresManualReview.Should().BeFalse();
    }

    [Fact]
    public void Evaluate_RatingA_BelowAutoApproveThreshold_NoManualReview()
    {
        // Arrange — excellent score, low amount
        // Act
        var result = _engine.Evaluate(bureauScore: 900, monthlyIncome: 20_000, totalDebt: 1_000, requestedLimit: 30_000);

        // Assert
        result.Rating.Should().Be(RiskRating.A);
        result.RequiresManualReview.Should().BeFalse();
    }

    [Fact]
    public void Evaluate_RatingA_AboveAutoApproveThreshold_RequiresManualReview()
    {
        // Arrange — excellent score, high amount
        // Act
        var result = _engine.Evaluate(bureauScore: 900, monthlyIncome: 50_000, totalDebt: 2_000, requestedLimit: 80_000);

        // Assert
        result.Rating.Should().Be(RiskRating.A);
        result.RequiresManualReview.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_RatingD_AlwaysRequiresManualReview()
    {
        // Arrange — poor score, very low amount
        // Act
        var result = _engine.Evaluate(bureauScore: 400, monthlyIncome: 5_000, totalDebt: 3_000, requestedLimit: 500);

        // Assert
        result.Rating.Should().Be(RiskRating.D);
        result.RequiresManualReview.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_ZeroMonthlyIncome_ReturnsRatingE()
    {
        // Arrange — zero income is invalid for scoring
        // Act
        var result = _engine.Evaluate(bureauScore: 800, monthlyIncome: 0, totalDebt: 0, requestedLimit: 10_000);

        // Assert
        result.Rating.Should().Be(RiskRating.E);
    }
}
```

### 8.3 FluentValidation Tests

```csharp
// File: tests/unit/CreditRisk.CreditAnalysis.Domain.Tests/Validators/CreateProposalRequestValidatorTests.cs
using CreditRisk.CreditAnalysis.Application.DTOs;
using CreditRisk.CreditAnalysis.Application.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace CreditRisk.CreditAnalysis.Domain.Tests.Validators;

public sealed class CreateProposalRequestValidatorTests
{
    private readonly CreateProposalRequestValidator _validator = new();

    [Fact]
    public void Validate_ValidRequest_PassesValidation()
    {
        // Arrange
        var request = new CreateProposalRequest
        {
            CustomerDocument = "52998224725",
            CustomerDocumentType = "CPF",
            CustomerName = "João Silva",
            CustomerEmail = "joao@example.com",
            MonthlyIncome = 5_000m,
            RequestedLimit = 10_000m,
            ProposalType = "Individual",
            BureauConsentGiven = true,
            BureauConsentIpAddress = "192.168.1.1"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_InvalidCpf_FailsWithCpfError()
    {
        // Arrange
        var request = new CreateProposalRequest
        {
            CustomerDocument = "111.111.111-11",
            CustomerDocumentType = "CPF",
            CustomerName = "João Silva",
            CustomerEmail = "joao@example.com",
            MonthlyIncome = 5_000m,
            RequestedLimit = 10_000m,
            ProposalType = "Individual",
            BureauConsentGiven = true,
            BureauConsentIpAddress = "192.168.1.1"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(r => r.CustomerDocument);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(500_001)]
    public void Validate_InvalidRequestedLimit_FailsWithLimitError(decimal limit)
    {
        // Arrange
        var request = new CreateProposalRequest
        {
            CustomerDocument = "52998224725",
            CustomerDocumentType = "CPF",
            CustomerName = "João Silva",
            CustomerEmail = "joao@example.com",
            MonthlyIncome = 5_000m,
            RequestedLimit = limit,
            ProposalType = "Individual",
            BureauConsentGiven = true,
            BureauConsentIpAddress = "192.168.1.1"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(r => r.RequestedLimit);
    }

    [Fact]
    public void Validate_BureauConsentNotGiven_FailsValidation()
    {
        // Arrange
        var request = new CreateProposalRequest
        {
            CustomerDocument = "52998224725",
            CustomerDocumentType = "CPF",
            CustomerName = "João Silva",
            CustomerEmail = "joao@example.com",
            MonthlyIncome = 5_000m,
            RequestedLimit = 10_000m,
            ProposalType = "Individual",
            BureauConsentGiven = false,
            BureauConsentIpAddress = "192.168.1.1"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(r => r.BureauConsentGiven);
    }
}
```

---

## 9. Acceptance Criteria and Definition of Done

### 9.1 IAM API

- [ ] `POST /auth/login` returns 200 with `LoginResponse` for valid credentials + TOTP
- [ ] `POST /auth/login` returns 401 for invalid credentials
- [ ] `POST /auth/logout` adds JWT `jti` to Redis revocation set with correct TTL
- [ ] `POST /auth/refresh` returns new access token for valid refresh token
- [ ] `GET /auth/jwks` returns Keycloak JWKS JSON without authentication
- [ ] `POST /users` creates user and returns 201 — requires `administrator` role
- [ ] `PUT /users/{id}/roles` assigns role and publishes `UserRoleChangedEvent`
- [ ] `JwtRevocationMiddleware` returns 401 for revoked tokens on all protected endpoints
- [ ] Rate limiter returns 429 after 5 auth requests per minute per IP
- [ ] All endpoints return RFC 7807 Problem Details for error responses
- [ ] `GET /health` returns 200 when PostgreSQL and Redis are healthy

### 9.2 Credit Analysis API

- [ ] `POST /proposals` returns 202 Accepted with `ProposalId` and publishes `CreditProposalCreatedEvent`
- [ ] `POST /proposals` returns 422 for invalid CPF/CNPJ
- [ ] `POST /proposals` returns 422 when `BureauConsentGiven = false`
- [ ] `POST /proposals` returns 409 when customer already has 3 active proposals
- [ ] `GET /proposals/{id}` returns 404 for non-existent proposal
- [ ] `GET /proposals/{id}` returns 403 when desk-operator requests another operator's proposal
- [ ] `PUT /proposals/{id}/submit` transitions Draft → PendingEvaluation and returns 200
- [ ] `PUT /proposals/{id}/submit` returns 409 when proposal is not in Draft status
- [ ] EF Core optimistic concurrency (`xmin`) prevents lost updates on concurrent modifications
- [ ] All database calls are async — no synchronous EF Core calls

### 9.3 Compliance API

- [ ] `POST /transactions` returns 202 Accepted within 50ms (schema validation only, no business rules)
- [ ] `POST /transactions` is idempotent — duplicate `TransactionId` returns 202 without reprocessing
- [ ] `GET /alerts` returns paginated list — requires `compliance-analyst` role
- [ ] `PUT /alerts/{id}/review` updates alert status and publishes appropriate event
- [ ] `GET /reports/str` returns STR report data in BCB-compliant format
- [ ] Rate limiter is configurable per client for transaction ingestion endpoint

### 9.4 Cross-Cutting

- [ ] All API projects compile with `PublishAot=true` without ILC warnings
- [ ] All endpoints emit OpenTelemetry spans with correct service name
- [ ] All error responses are RFC 7807 Problem Details — no stack traces exposed
- [ ] Structured logs never contain CPF, CNPJ, email, or passwords
- [ ] `dotnet build` produces 0 warnings with `TreatWarningsAsErrors=true`

---

## 10. Local Execution Instructions

Execute in order from a clean machine with .NET SDK 10.0.100 and Docker 26+ installed.

### Step 1: Start Infrastructure

```bash
cd credit-risk-compliance-lab
cp .env.example .env
# Edit .env with secure passwords

docker compose up -d postgres redis rabbitmq keycloak
# Wait for all services to be healthy (up to 90 seconds for Keycloak)
docker compose ps
```

### Step 2: Apply Database Migrations

```bash
# IAM module
dotnet ef database update \
  --project src/modules/iam/CreditRisk.IAM.Infrastructure/CreditRisk.IAM.Infrastructure.csproj \
  --startup-project src/modules/iam/CreditRisk.IAM.Api/CreditRisk.IAM.Api.csproj

# Credit Analysis module
dotnet ef database update \
  --project src/modules/credit-analysis/CreditRisk.CreditAnalysis.Infrastructure/CreditRisk.CreditAnalysis.Infrastructure.csproj \
  --startup-project src/modules/credit-analysis/CreditRisk.CreditAnalysis.Api/CreditRisk.CreditAnalysis.Api.csproj

# Compliance module
dotnet ef database update \
  --project src/modules/compliance/CreditRisk.Compliance.Infrastructure/CreditRisk.Compliance.Infrastructure.csproj \
  --startup-project src/modules/compliance/CreditRisk.Compliance.Api/CreditRisk.Compliance.Api.csproj
```

### Step 3: Run All Back-End APIs Locally

```bash
# Terminal 1 — IAM API
dotnet run --project src/modules/iam/CreditRisk.IAM.Api/CreditRisk.IAM.Api.csproj \
  --launch-profile Development

# Terminal 2 — Credit Analysis API
dotnet run --project src/modules/credit-analysis/CreditRisk.CreditAnalysis.Api/CreditRisk.CreditAnalysis.Api.csproj \
  --launch-profile Development

# Terminal 3 — Compliance API
dotnet run --project src/modules/compliance/CreditRisk.Compliance.Api/CreditRisk.Compliance.Api.csproj \
  --launch-profile Development
```

### Step 4: Verify Health Endpoints

```bash
curl -s http://localhost:5001/health | python3 -m json.tool   # IAM
curl -s http://localhost:5002/health | python3 -m json.tool   # Credit Analysis
curl -s http://localhost:5003/health | python3 -m json.tool   # Compliance
# All should return {"status":"Healthy"}
```

### Step 5: Run Back-End Unit Tests

```bash
dotnet test tests/unit/CreditRisk.CreditAnalysis.Domain.Tests/ \
  --collect:"XPlat Code Coverage" \
  --results-directory ./coverage/credit-domain

dotnet test tests/unit/CreditRisk.IAM.Domain.Tests/ \
  --collect:"XPlat Code Coverage" \
  --results-directory ./coverage/iam-domain

dotnet test tests/unit/CreditRisk.Compliance.Domain.Tests/ \
  --collect:"XPlat Code Coverage" \
  --results-directory ./coverage/compliance-domain
```

### Step 6: Run Integration Tests

```bash
# Integration tests use Testcontainers — Docker must be running
dotnet test tests/integration/CreditRisk.CreditAnalysis.Integration.Tests/ \
  --logger "console;verbosity=normal"

dotnet test tests/integration/CreditRisk.IAM.Integration.Tests/ \
  --logger "console;verbosity=normal"
```

### Step 7: Verify AOT Compilation

```bash
dotnet publish src/modules/iam/CreditRisk.IAM.Api/CreditRisk.IAM.Api.csproj \
  -c Release -r linux-x64 --self-contained true -p:PublishAot=true -o /tmp/iam-aot

dotnet publish src/modules/credit-analysis/CreditRisk.CreditAnalysis.Api/CreditRisk.CreditAnalysis.Api.csproj \
  -c Release -r linux-x64 --self-contained true -p:PublishAot=true -o /tmp/credit-aot

dotnet publish src/modules/compliance/CreditRisk.Compliance.Api/CreditRisk.Compliance.Api.csproj \
  -c Release -r linux-x64 --self-contained true -p:PublishAot=true -o /tmp/compliance-aot
# All three must complete with 0 ILC warnings
```

### Expected Final State

- All three APIs start and respond to `/health` with `{"status":"Healthy"}`
- All unit tests pass with ≥ 80% line coverage on Domain and Application layers
- All integration tests pass (requires Docker)
- All three APIs publish successfully with Native AOT

---

*Cross-references: Depends on types from [`SPEC-01-architecture-core.md`](SPEC-01-architecture-core.md). Worker consumers are specified in [`SPEC-04-integration.md`](SPEC-04-integration.md). Test suite is specified in [`SPEC-03-backend-unit-tests.md`](SPEC-03-backend-unit-tests.md).*
---

## 11. Known Implementation Pitfalls — Back-End

This section documents all build and runtime errors encountered during the initial SPEC-02 implementation. Use it as a pre-flight checklist before starting any implementation.

### 11.1 Build Errors Reference Table

| # | Error Code | Symptom | Root Cause | Resolution |
|---|---|---|---|---|
| 1 | `CS0246` | `'AlertStatus' not found` | `AlertStatus.cs` not created in Compliance domain | Create `src/modules/compliance/CreditRisk.Compliance.Domain/Enums/AlertStatus.cs` (see §4.3) |
| 2 | `CS0246` | `'ICommandHandler<,>' not found` | Missing `using CreditRisk.Shared.Kernel.CQRS;` | Add using to endpoint files and `ServiceCollectionExtensions.cs` |
| 3 | `CS0246` | `'IQueryHandler<,>' not found` | Missing `using CreditRisk.Shared.Kernel.CQRS;` | Add using to endpoint files and `ServiceCollectionExtensions.cs` |
| 4 | `CS0104` | `'PagedResult<>' is ambiguous` | `PagedResult<T>` redefined in Application query namespace | Remove local definition; use `using CreditRisk.Shared.Kernel.Result;` only |
| 5 | `CS0246` | `'DomainException' not found` | Missing `using CreditRisk.Shared.Kernel.Exceptions;` | Add using to domain entity files |
| 6 | `CS0246` | `'SaveChangesInterceptor' not found` | Missing `using Microsoft.EntityFrameworkCore.Diagnostics;` | Add using to interceptor files |
| 7 | `CS1998` | `async method lacks await` | Repository method marked `async` but has no `await` | Remove `async` keyword; return `Task.FromResult(...)` or `Task.CompletedTask` |
| 8 | `CA1860` | `Do not use Enumerable.Any()` | `!list.Any()` used instead of `list.Count == 0` | Replace `!.Any()` with `.Count == 0` |
| 9 | `CS9113` | `Parameter 'X' is unread` | Primary constructor parameter declared but never used | Remove unused parameter from primary constructor |
| 10 | `IDE0008` | `Use explicit type instead of 'var'` | `var` used for built-in types (int, string, bool, decimal) | Replace `var` with explicit type |
| 11 | `IDE0022` | `Use block body for method` | Expression-bodied method (`=>`) used | Convert to block body with `{ return ...; }` |
| 12 | `CS0122` | `'UnitOfWork' is inaccessible` | `internal sealed class UnitOfWork` — DI requires `public` | Change to `public sealed class UnitOfWork` |
| 13 | `CS0122` | `'CreditScoringEngine' is inaccessible` | `internal sealed class CreditScoringEngine` — DI requires `public` | Change to `public sealed class CreditScoringEngine` |
| 14 | `CS0234` | `'GlobalExceptionMiddleware' not found` | Cross-module middleware reference (e.g., `CreditRisk.IAM.Api.Middleware`) | Each API module must have its own `GlobalExceptionMiddleware` — never reference another module's |
| 15 | `CS0246` | `'JsonSourceGenerationContextAttribute' not found` | Non-existent attribute used for JSON source generation | Use `[JsonSerializable(typeof(T))]` per type; `[JsonSourceGenerationOptions(...)]` on context class |
| 16 | `CS0246` | `'JsonContextSerializationMode' not found` | Non-existent enum used in JSON context | Remove entirely; use `[JsonSourceGenerationOptions]` attributes only |
| 17 | `CS1503` | `TypedResults.Accepted()` argument mismatch | `TypedResults.Accepted()` requires a URI argument in .NET 8/10 | Use `TypedResults.Accepted((string?)null, value)` for no location header |
| 18 | `CS0305` | `Results<T1..T7>` — too many type arguments | `Results<>` has a maximum of 6 type parameters | Split endpoint into multiple handlers or use `IResult` return type |
| 19 | `CS0246` | `'ProblemHttpResult' not found` | `TypedResults.Problem()` used but `ProblemHttpResult` not in `Results<>` union | Add `ProblemHttpResult` to the `Results<>` type union |
| 20 | `CS0246` | `'StatusCodes' not found in Application layer` | `StatusCodes` is in `Microsoft.AspNetCore.Http` — not available in Application projects | Use integer literals: `422`, `409`, `404`, `401` instead |
| 21 | `CS0246` | `'MassTransit' types not found in Application layer` | `IPublishEndpoint` used but `MassTransit` not referenced in `.csproj` | Add `<PackageReference Include="MassTransit" />` to Application layer `.csproj` |
| 22 | `ArgumentNullException` at startup | `connectionString` is null | `appsettings.Development.json` missing `ConnectionStrings` section | Create `appsettings.Development.json` with `ConnectionStrings.Postgres` (see §6.5) |
| 23 | `CS0246` | `'JsonSourceGenerationContextAttribute' not found` in `*JsonContext.cs` | Missing `using System.Text.Json.Serialization;` in the partial class file | Add `using System.Text.Json.Serialization;` explicitly — `ImplicitUsings` does not cover partial class attribute resolution |
| 24 | EF Core migration fails: `DomainEvent requires a primary key` | EF Core discovers `DomainEvent` via `AggregateRoot.DomainEvents` navigation property | Add `modelBuilder.Ignore<DomainEvent>();` as first statement in every `DbContext.OnModelCreating()`. See §4.8. |
| 25 | `ArgumentNullException: uriString` in `ObservabilityExtensions` | `OTEL_EXPORTER_OTLP_ENDPOINT` absent; `new Uri(null)` throws | Null-check endpoint before constructing `Uri`. See SPEC-01 §4.8 for corrected implementation. |
| 26 | `appsettings.Development.json` not loaded; all connection strings null | `ASPNETCORE_ENVIRONMENT` not exported before `dotnet run` or `dotnet ef database update` | `export ASPNETCORE_ENVIRONMENT=Development` before every local command. See `setup.md` §5.7.2. |
| 27 | `ACCESS_REFUSED` (RabbitMQ) or `RedisConnectionException` | `appsettings.Development.json` uses Docker service names; Redis missing `abortConnect=false` | Use `localhost` for all hostnames. Add `abortConnect=false` to Redis connection strings. See §6.5. |
| 28 | APIs start on wrong ports (5050, 5012, 5052) | `dotnet new` auto-generates random ports in `launchSettings.json` | Set ports: IAM=5001, CreditAnalysis=5002, Compliance=5003 in `launchSettings.json`. See §6.6. |
| 29 | `RegexErrorStubRouteConstraint` / routes with `{id:guid}` return 500 | `CreateSlimBuilder` uses `AddRoutingCore()` — does not register built-in route constraints | Call `builder.Services.AddRouting()` in every API `Program.cs` after `CreateSlimBuilder`. See §4.7. |

### 11.2 `.csproj` Package Reference Requirements

Each project layer has specific package requirements. Missing packages cause `CS0246` errors.

**Application layer `.csproj` must include:**
```xml
<PackageReference Include="MassTransit" />
<PackageReference Include="FluentValidation" />
<PackageReference Include="Microsoft.Extensions.Logging.Abstractions" />
```

**API layer `.csproj` must include:**
```xml
<PackageReference Include="Swashbuckle.AspNetCore" />
<PackageReference Include="AspNetCore.HealthChecks.NpgSql" />
<PackageReference Include="MassTransit.RabbitMQ" />
```

**Infrastructure layer `.csproj` must include:**
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" />
<PackageReference Include="MassTransit" />
```

### 11.3 Pre-Implementation Checklist

Before implementing any module, verify:

- [ ] `AlertStatus.cs` exists in `CreditRisk.Compliance.Domain/Enums/`
- [ ] All domain entity files have `using CreditRisk.Shared.Kernel.Exceptions;`
- [ ] All endpoint files have `using CreditRisk.Shared.Kernel.CQRS;`
- [ ] All files using `PagedResult<T>` have `using CreditRisk.Shared.Kernel.Result;` — no local redefinition
- [ ] All interceptor files have `using Microsoft.EntityFrameworkCore.Diagnostics;`
- [ ] `UnitOfWork` and `CreditScoringEngine` are `public sealed` (not `internal sealed`)
- [ ] No expression-bodied methods (`=>`) — all methods use block bodies
- [ ] No `async` methods without `await` — remove `async` keyword and return `Task.CompletedTask`
- [ ] No `!.Any()` — use `.Count == 0` instead
- [ ] No unused primary constructor parameters
- [ ] No `var` for built-in types (int, string, bool, decimal)
- [ ] No `StatusCodes.*` in Application layer — use integer literals
- [ ] No `IPipelineBehavior<,>` registrations — this is a MediatR interface
- [ ] Each API module has its own `GlobalExceptionMiddleware` — no cross-module references
- [ ] Each API project has `appsettings.Development.json` with `ConnectionStrings` section
- [ ] `[JsonSerializable(typeof(T))]` used per type — no `[JsonSourceGenerationContext(...)]`
- [ ] `TypedResults.Accepted((string?)null, value)` used — not `TypedResults.Accepted(value)`
- [ ] `Results<>` unions include `ProblemHttpResult` when `TypedResults.Problem()` is returned
- [ ] `Results<>` unions have at most 6 type parameters
- [ ] Every `*JsonContext.cs` file has `using System.Text.Json.Serialization;` explicitly at the top
- [ ] Every `DbContext.OnModelCreating()` calls `modelBuilder.Ignore<DomainEvent>()` as first statement
- [ ] `ObservabilityExtensions` null-checks `OTEL_EXPORTER_OTLP_ENDPOINT` before constructing `Uri`
- [ ] `ASPNETCORE_ENVIRONMENT=Development` exported before running migrations and `dotnet run`
- [ ] All hostnames in `appsettings.Development.json` use `localhost` (not Docker service names)
- [ ] Redis connection strings include `abortConnect=false`
- [ ] `launchSettings.json` ports: IAM=5001, CreditAnalysis=5002, Compliance=5003
- [ ] Every API `Program.cs` calls `builder.Services.AddRouting()` after `WebApplication.CreateSlimBuilder(args)`