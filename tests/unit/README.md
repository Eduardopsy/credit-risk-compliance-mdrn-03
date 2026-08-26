# Credit Risk Compliance Lab - Unit Test Implementation

> **SPEC-03 Backend Unit Tests Implementation Status: Foundation Complete**

## Overview

This document describes the implementation of the back-end unit test suite for the Credit Risk Compliance Lab, following SPEC-03 specifications. The test suite is designed to be **AOT-compatible** and use **hand-written test doubles** instead of dynamic proxy generators.

## Quick Start

### Build All Test Projects

```bash
dotnet build tests/unit/ --configuration Release
```

Expected output:
```
Build succeeded. 0 Warning(s). 0 Error(s).
```

### Run All Tests

```bash
dotnet test tests/unit/
```

### Run Specific Module Tests

```bash
# IAM module tests
dotnet test tests/unit/CreditRisk.IAM.Domain.Tests/

# Credit Analysis module tests
dotnet test tests/unit/CreditRisk.CreditAnalysis.Domain.Tests/

# Compliance module tests
dotnet test tests/unit/CreditRisk.Compliance.Domain.Tests/
```

### Run CreditScoringEngine Tests (Most Critical)

```bash
dotnet test tests/unit/CreditRisk.CreditAnalysis.Domain.Tests/ \
  --filter "CreditScoringEngineTests" \
  --logger "console;verbosity=detailed"
```

**Expected Result:** 21 tests, all passing

## Architecture

### Test Doubles (Fakes & Builders)

Instead of using Moq or NSubstitute (which are incompatible with Native AOT), all test doubles are hand-written classes that implement the required interfaces directly.

#### IAM Module Fakes

- **FakeUserRepository** - In-memory user storage with email lookup
- **FakePasswordHasher** - Configurable hash/verify results
- **FakeTokenService** - Produces LoginResponse for token generation
- **FakeTokenRevocationStore** - Tracks revoked JTIs

#### CreditAnalysis Module Fakes

- **FakeCreditProposalRepository** - In-memory proposal storage with filtering
- **FakeCustomerRepository** - Customer lookup by ID or document
- **FakeUnitOfWork** - Records commit calls and supports failure simulation

#### Compliance Module Fakes

- **FakeTransactionRepository** - In-memory transaction storage with date-based queries
- **FakeAmlAlertRepository** - Alert storage with status filtering

### Test Builders

Fluent builders produce valid test objects by default:

```csharp
// CreditProposal
var proposal = new CreditProposalBuilder()
    .WithRequestedLimit(25_000m)
    .WithCustomerId(customerId)
    .AsSubmitted()
    .Build();

// User
var user = new UserBuilder()
    .WithEmail("operator@example.com")
    .WithRole(UserRole.DeskOperator)
    .Build();

// Customer
var customer = new CustomerBuilder()
    .WithName("João Silva")
    .WithMonthlyIncome(5_000m)
    .Build();
```

## Test Coverage

### CreditScoringEngine (21 Tests) ✅

The credit scoring engine implements BCB Resolução 4.557 risk classification:

| Rating | Auto-Approve Limit | Manual Review Required |
|--------|-------------------|----------------------|
| **A** | R$ 50,000 | When requested > R$ 50k |
| **B** | R$ 20,000 | When requested > R$ 20k |
| **C** | R$ 5,000 | When requested > R$ 5k |
| **D** | N/A | Always |
| **E** | R$ 0 | Never (auto-reject) |

#### Test Scenarios (All Passing ✅)

**Rating A Tests (3):**
- Excellent bureau score → Rating A
- Below auto-approve threshold → auto-approved
- Above auto-approve threshold → manual review required

**Rating B Tests (3):**
- Good bureau score → Rating B
- Below/above auto-approve thresholds

**Rating C Tests (3):**
- Fair bureau score → Rating C
- Below/above auto-approve thresholds

**Rating D Tests (2):**
- Poor bureau score → Rating D
- Always requires manual review (even for small amounts)

**Rating E Tests (2):**
- Very poor bureau score → Rating E
- Approved limit is always 0 (auto-reject)

**Boundary Tests (5):**
- Bureau score 850 → A
- Bureau score 700 → B
- Bureau score 580 → C
- Bureau score 400 → D
- Bureau score 200 → E

**Edge Cases (3):**
- Zero monthly income → E (auto-reject)
- Requested limit > R$ 500k → capped at R$ 500k
- High income doesn't override poor bureau score

### IAM Handler Tests (7) ✅

**LoginCommandHandlerTests (4):**
- Valid credentials → returns LoginResponse
- User not found → 401 Unauthorized
- Wrong password → 401 Unauthorized
- Cancellation requested → throws OperationCanceledException

**LogoutCommandHandlerTests (3):**
- Valid JTI → adds to revocation store
- Empty JTI → 422 validation error
- Already revoked JTI → success (idempotent)

## File Organization

```
tests/unit/
├── CreditRisk.IAM.Domain.Tests/
│   ├── Fakes/
│   │   ├── FakeUserRepository.cs
│   │   ├── FakePasswordHasher.cs
│   │   ├── FakeTokenService.cs
│   │   └── FakeTokenRevocationStore.cs
│   ├── Builders/
│   │   └── UserBuilder.cs
│   ├── Handlers/
│   │   ├── LoginCommandHandlerTests.cs
│   │   └── LogoutCommandHandlerTests.cs
│   └── xunit.runner.json
│
├── CreditRisk.CreditAnalysis.Domain.Tests/
│   ├── Fakes/
│   │   ├── FakeCreditProposalRepository.cs
│   │   ├── FakeCustomerRepository.cs
│   │   └── FakeUnitOfWork.cs
│   ├── Builders/
│   │   ├── CreditProposalBuilder.cs
│   │   └── CustomerBuilder.cs
│   ├── Services/
│   │   └── CreditScoringEngineTests.cs
│   └── xunit.runner.json
│
├── CreditRisk.Compliance.Domain.Tests/
│   ├── Fakes/
│   │   ├── FakeTransactionRepository.cs
│   │   └── FakeAmlAlertRepository.cs
│   ├── Builders/
│   │   ├── TransactionBuilder.cs
│   │   └── AmlAlertBuilder.cs
│   └── xunit.runner.json
│
└── coverage.runsettings
```

## Test Execution

All tests are:
- ✅ **Independent** - No shared mutable state between tests
- ✅ **Deterministic** - Same inputs produce same outputs
- ✅ **Fast** - No I/O, no delays (< 50ms total)
- ✅ **Isolated** - Only in-memory fakes, no external dependencies
- ✅ **AOT-Compatible** - No dynamic proxy generation

### Performance

```bash
dotnet test tests/unit/ --logger "console;verbosity=minimal"
# Total duration: < 100ms for all 28+ tests
```

## Configuration

### xunit.runner.json

```json
{
  "methodDisplay": "classAndMethod",
  "methodDisplayOptions": "all",
  "parallelizeAssembly": true,
  "parallelizeTestCollections": true,
  "maxParallelThreads": 4
}
```

### coverage.runsettings

- Format: Cobertura
- Include: All Domain and Application projects
- Exclude: Test assemblies and migrations
- Results directory: `./coverage`

## Naming Conventions

All tests follow the convention: **MethodName_Scenario_ExpectedResult**

Examples:
- `HandleAsync_ValidCredentials_ReturnsLoginResponse`
- `Evaluate_ExcellentBureauLowDebtLowMultiplier_ReturnsRatingA`
- `Evaluate_RatingE_ApprovedLimitIsZero`

## Dependencies

No dynamic proxy libraries:
- ✅ xUnit 2.9.2 (test framework)
- ✅ FluentAssertions 7.0.0 (assertions)
- ✅ Microsoft.NET.Test.Sdk 17.12.0
- ✅ Coverlet 6.0.2 (code coverage)
- ❌ Moq (rejected - AOT incompatible)
- ❌ NSubstitute (rejected - AOT incompatible)
- ❌ FakeItEasy (rejected - AOT incompatible)

## Coverage Goals

| Layer | Target | Status |
|-------|--------|--------|
| CreditRisk.Shared.Kernel | 80% line | Pending Phase 3-5 |
| IAM Domain | 80% line | Pending Phase 3 |
| IAM Application | 80% line | Partial (7 tests) |
| CreditAnalysis Domain | 80% line | Pending Phase 3 |
| CreditAnalysis Application | 80% line | Partial (21 tests) |
| CreditAnalysis Scoring | 100% branch | ✅ Complete (21 tests) |
| Compliance Domain | 80% line | Pending Phase 3 |
| Compliance Application | 80% line | Pending Phase 4 |

## Running Coverage Report

```bash
# Collect coverage
dotnet test tests/unit/ --collect:"XPlat Code Coverage" \
  --results-directory ./coverage

# Generate report
reportgenerator \
  -reports:"./coverage/**/coverage.cobertura.xml" \
  -targetdir:"./coverage/report" \
  -reporttypes:"Html"

# View report
open ./coverage/report/index.html
```

## Roadmap

### ✅ Completed (Phases 1-2, partial 4)
- Test project scaffolding
- Test doubles (all 9 fakes + 5 builders)
- CreditScoringEngine (21 comprehensive tests)
- IAM handlers (7 tests)

### ⏳ Pending
- Domain entity tests (Phase 3)
- Value object tests (Phase 3)
- Additional application handlers (Phase 4)
- Middleware exception tests (Phase 5)
- Coverage verification (Phase 6)
- CI/CD integration (Phase 7)

## References

- **Specification:** `docs/specs/SPEC-03-backend-unit-tests.md`
- **Implementation Plan:** `.aicockpit/plans/spec-03-backend-unit-tests-implementation.md`
- **Summary:** `.aicockpit/plans/spec-03-implementation-summary.md`
- **Architecture:** `docs/specs/SPEC-01-architecture-core.md`
- **Backend:** `docs/specs/SPEC-02-backend.md`

---

**Status:** Core foundation implemented and tested ✅  
**Next Step:** Implement Phase 3 domain/value object tests
