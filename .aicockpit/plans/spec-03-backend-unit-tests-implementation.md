# SPEC-03: Back-End Unit Tests Implementation Plan

**Status:** Planning  
**Target Spec:** `docs/specs/SPEC-03-backend-unit-tests.md`  
**Project:** Credit Risk Compliance Lab  
**Date:** 2026-08-26  

## Executive Summary

Implement a complete AOT-compatible unit test suite for all Domain and Application layers across three modules (IAM, CreditAnalysis, Compliance). This plan focuses on **hand-written fakes** instead of dynamic proxy libraries (Moq, NSubstitute), ensuring Native AOT compatibility.

**Key Constraints:**
- .NET 8.0.129 (no newer features required)
- Zero dynamic proxy dependencies (Castle.DynamicProxy incompatible with AOT)
- Hand-written test doubles with full interface implementation
- Minimum 80% line coverage on all Domain/Application layers
- 100% branch coverage on credit scoring matrix
- xUnit + FluentAssertions + Coverlet stack

---

## Phase 1: Scaffolding and Foundation Setup

### 1.1 Create Test Directory Structure

**Deliverables:**
- Create `tests/unit/` directory with three test projects:
  - `tests/unit/CreditRisk.IAM.Domain.Tests/`
  - `tests/unit/CreditRisk.CreditAnalysis.Domain.Tests/`
  - `tests/unit/CreditRisk.Compliance.Domain.Tests/`
- Each project follows the directory structure from SPEC-03 Section 3

**Files to Create:**
```
tests/unit/
├── CreditRisk.IAM.Domain.Tests/
│   ├── CreditRisk.IAM.Domain.Tests.csproj
│   ├── xunit.runner.json
│   ├── Entities/
│   ├── ValueObjects/
│   ├── Validators/
│   ├── Handlers/
│   ├── Fakes/
│   ├── Builders/
│   └── Middleware/
├── CreditRisk.CreditAnalysis.Domain.Tests/
│   ├── CreditRisk.CreditAnalysis.Domain.Tests.csproj
│   ├── xunit.runner.json
│   ├── Entities/
│   ├── ValueObjects/
│   ├── Services/
│   ├── Validators/
│   ├── Handlers/
│   ├── Fakes/
│   └── Builders/
├── CreditRisk.Compliance.Domain.Tests/
│   ├── CreditRisk.Compliance.Domain.Tests.csproj
│   ├── xunit.runner.json
│   ├── Entities/
│   ├── Services/
│   ├── Validators/
│   ├── Handlers/
│   ├── Fakes/
│   └── Builders/
└── coverage.runsettings
```

**Acceptance Criteria:**
- [ ] All three `.csproj` files created with proper SDK references
- [ ] All `.csproj` files reference correct Domain/Application/Infrastructure projects
- [ ] NuGet packages: xunit 2.9.2, FluentAssertions 7.0.0, Microsoft.NET.Test.Sdk 17.12.0, coverlet.collector 6.0.2
- [ ] No Moq, NSubstitute, or FakeItEasy references
- [ ] Solution file updated to include all three test projects
- [ ] xunit.runner.json configured for parallelization (maxParallelThreads: 4)
- [ ] coverage.runsettings created with proper exclusions

---

### 1.2 Configure Solution File

**Changes:**
- Add test project section folders under `tests` parent folder
- Add all three test projects to CreditRiskComplianceLab.sln
- Verify Visual Studio can load and build all projects

**Acceptance Criteria:**
- [ ] `dotnet build tests/unit/` compiles with 0 warnings
- [ ] Visual Studio solution loads without errors
- [ ] All test projects appear in Solution Explorer under `tests` folder

---

## Phase 2: Test Doubles Implementation (Fakes & Builders)

### 2.1 IAM Module Test Doubles

**Fakes to Implement:**

1. **FakeUserRepository** (IUserRepository)
   - In-memory dictionary storage
   - Methods: GetByIdAsync, GetByEmailAsync, AddAsync, UpdateAsync, DeleteAsync
   - Test helpers: GetAll(), SetupUser(user)

2. **FakeTokenService** (ITokenService)
   - Configurable token response
   - Methods: GenerateTokensAsync, ValidateTokenAsync, RefreshTokenAsync
   - Test helpers: SetTokenResponse(response), SetGenerateThrow(exception)

3. **FakePasswordHasher** (IPasswordHasher)
   - Configurable verify result
   - Methods: Hash(password), Verify(password, hash)
   - Test helpers: SetVerifyResult(bool), SetHashThrow(exception)

4. **FakeTokenRevocationStore** (ITokenRevocationStore)
   - In-memory HashSet of revoked JTIs
   - Methods: RevokeAsync, IsRevokedAsync
   - Test helpers: PreloadRevoked(jti), Contains(jti)

**File Locations:**
- `tests/unit/CreditRisk.IAM.Domain.Tests/Fakes/`

**Builders to Implement:**

1. **UserBuilder**
   - Default: valid active user with email, hashed password, desk-operator role
   - Fluent methods: WithEmail(), WithRole(), WithFullName(), WithIsActive()
   - Build() delegates to User.Create() factory

2. **RefreshTokenBuilder**
   - Default: valid unexpired refresh token
   - Fluent methods: WithExpiryDate(), WithUserIdAsync(), WithIsRevoked()
   - Build() creates RefreshToken via factory

**File Locations:**
- `tests/unit/CreditRisk.IAM.Domain.Tests/Builders/`

**Acceptance Criteria:**
- [ ] All fakes implement their interfaces completely
- [ ] No NotImplementedException or throw new NotImplementedException()
- [ ] Builders produce valid objects by default (no configuration needed for happy path)
- [ ] Builders are fluent (return `this`)
- [ ] No reflection in builders — all assignments explicit
- [ ] Build() methods use factory methods (User.Create, RefreshToken.Create)

---

### 2.2 CreditAnalysis Module Test Doubles

**Fakes to Implement:**

1. **FakeCreditProposalRepository** (ICreditProposalRepository)
   - Methods: GetByIdAsync, GetByCustomerIdAsync, GetByStatusAsync, AddAsync, UpdateAsync, CountByStatusAsync
   - Test helpers: GetAll()

2. **FakeCustomerRepository** (ICustomerRepository)
   - Methods: GetByIdAsync, GetByDocumentAsync, AddAsync, UpdateAsync
   - Test helpers: SetCustomerIdForDocument(doc, customerId)

3. **FakeUnitOfWork** (IUnitOfWork)
   - Methods: CommitAsync
   - Test helpers: CommitCallCount, ShouldThrow flag

**File Locations:**
- `tests/unit/CreditRisk.CreditAnalysis.Domain.Tests/Fakes/`

**Builders to Implement:**

1. **CreditProposalBuilder**
   - Default: valid Draft proposal (not submitted)
   - Fluent methods: WithCustomerId(), WithRequestedLimit(), WithProposalType(), WithCreatedBy(), AsSubmitted(), WithEvaluation()
   - Build() uses CreditProposal.Create() then applies state transitions

2. **CustomerBuilder**
   - Default: valid customer with CPF, name, email
   - Fluent methods: WithDocument(), WithName(), WithEmail(), WithMonthlyIncome()
   - Build() uses Customer.Create()

**File Locations:**
- `tests/unit/CreditRisk.CreditAnalysis.Domain.Tests/Builders/`

**Acceptance Criteria:**
- [ ] All fakes implement their interfaces completely
- [ ] Builders produce valid objects by default
- [ ] Builders are fluent and use factory methods
- [ ] CreditProposalBuilder can build proposals in Draft and Submitted states

---

### 2.3 Compliance Module Test Doubles

**Fakes to Implement:**

1. **FakeTransactionRepository** (ITransactionRepository)
   - Methods: GetByIdAsync, GetByCustomerIdAsync, AddAsync, AddRangeAsync, GetRecentAsync
   - Test helpers: GetAll(), AddPepDocument(cpf)

2. **FakeAmlAlertRepository** (IAmlAlertRepository)
   - Methods: GetByIdAsync, GetByTransactionIdAsync, AddAsync, UpdateAsync
   - Test helpers: GetAll()

**File Locations:**
- `tests/unit/CreditRisk.Compliance.Domain.Tests/Fakes/`

**Builders to Implement:**

1. **TransactionBuilder**
   - Default: valid clean transaction (5000 amount, Online channel)
   - Fluent methods: WithAmount(), WithChannel(), WithCustomerId(), WithDate(), WithCustomerDocument()
   - Build() uses Transaction.Create()

2. **AmlAlertBuilder**
   - Default: valid alert with High severity
   - Fluent methods: WithTransactionId(), WithRuleType(), WithSeverity()

**File Locations:**
- `tests/unit/CreditRisk.Compliance.Domain.Tests/Builders/`

**Acceptance Criteria:**
- [ ] All fakes implement their interfaces completely
- [ ] Builders produce valid objects by default
- [ ] TransactionBuilder supports date-based test scenarios (smurfing pattern)

---

## Phase 3: Domain & Value Object Tests

### 3.1 IAM Domain Tests

**Test Files:**

1. **tests/unit/CreditRisk.IAM.Domain.Tests/Entities/UserTests.cs**
   - Test User.Create() factory
   - Test ChangeRole() state mutation
   - Test domain event raising
   - Test invariants (email not null, fullName not empty)

2. **tests/unit/CreditRisk.IAM.Domain.Tests/Entities/RefreshTokenTests.cs**
   - Test RefreshToken.Create() factory
   - Test expiry validation
   - Test revocation behavior

3. **tests/unit/CreditRisk.IAM.Domain.Tests/ValueObjects/EmailTests.cs**
   - Test Email.Create() validation
   - Test valid email formats
   - Test invalid email rejection

**Acceptance Criteria:**
- [ ] All entity factory methods tested
- [ ] All value object validations tested
- [ ] All domain events verified to be raised
- [ ] All invariants tested (null checks, empty strings)
- [ ] Boundary conditions tested (max string lengths)

---

### 3.2 CreditAnalysis Domain Tests

**Test Files:**

1. **tests/unit/CreditRisk.CreditAnalysis.Domain.Tests/Entities/CreditProposalTests.cs**
   - Test CreditProposal.Create() factory
   - Test Submit() state transition
   - Test ApplyEvaluation() state transition
   - Test status transitions and invariants

2. **tests/unit/CreditRisk.CreditAnalysis.Domain.Tests/Entities/CustomerTests.cs**
   - Test Customer.Create() factory
   - Test document validation (CPF, CNPJ)

3. **tests/unit/CreditRisk.CreditAnalysis.Domain.Tests/ValueObjects/MoneyAmountTests.cs**
   - Test MoneyAmount.Create() validation
   - Test boundary values (0, negative, max limit)

**Acceptance Criteria:**
- [ ] All state transitions tested
- [ ] All invariants enforced (proposal limit boundaries)
- [ ] Document validation tested for valid/invalid formats

---

### 3.3 Compliance Domain Tests

**Test Files:**

1. **tests/unit/CreditRisk.Compliance.Domain.Tests/Entities/TransactionTests.cs**
   - Test Transaction.Create() factory
   - Test transaction validations

2. **tests/unit/CreditRisk.Compliance.Domain.Tests/Entities/AmlAlertTests.cs**
   - Test AmlAlert.Create() factory
   - Test alert states and transitions

**Acceptance Criteria:**
- [ ] All entity factories tested
- [ ] All validation rules tested

---

## Phase 4: Application Layer Tests (Handlers & Validators)

### 4.1 IAM Application Tests

**Test Files:**

1. **tests/unit/CreditRisk.IAM.Domain.Tests/Handlers/LoginCommandHandlerTests.cs**
   - ✓ Valid credentials → returns LoginResponse with tokens
   - ✓ User not found → returns 401 Unauthorized
   - ✓ Wrong password → returns 401 Unauthorized
   - ✓ User inactive → returns 401 Unauthorized
   - ✓ Cancellation requested → throws OperationCanceledException
   - ✓ Invalid email format → returns 422 validation error

2. **tests/unit/CreditRisk.IAM.Domain.Tests/Handlers/LogoutCommandHandlerTests.cs**
   - ✓ Valid JTI → adds to revocation store
   - ✓ Empty JTI → returns 422 validation error
   - ✓ Already revoked JTI → returns success (idempotent)

3. **tests/unit/CreditRisk.IAM.Domain.Tests/Handlers/CreateUserCommandHandlerTests.cs**
   - ✓ Valid command → creates user and commits
   - ✓ Duplicate email → returns 409 Conflict
   - ✓ Invalid email → returns 422 validation error
   - ✓ Weak password → returns 422 validation error

4. **tests/unit/CreditRisk.IAM.Domain.Tests/Validators/LoginRequestValidatorTests.cs**
   - ✓ Valid login request passes
   - ✓ Empty email fails
   - ✓ Invalid email format fails
   - ✓ Empty password fails
   - ✓ Empty TOTP fails

5. **tests/unit/CreditRisk.IAM.Domain.Tests/Validators/CreateUserRequestValidatorTests.cs**
   - ✓ Valid create user request passes
   - ✓ Empty email fails
   - ✓ Weak password fails
   - ✓ Empty role fails

**Acceptance Criteria:**
- [ ] All handler success paths tested
- [ ] All handler error paths tested (with correct HTTP status codes)
- [ ] All validators test valid and invalid inputs
- [ ] Cancellation tokens properly propagated and tested

---

### 4.2 CreditAnalysis Application Tests

**Test Files:**

1. **tests/unit/CreditRisk.CreditAnalysis.Domain.Tests/Handlers/CreateProposalCommandHandlerTests.cs**
   - ✓ Valid command → creates proposal and commits
   - ✓ Invalid CPF → returns 422 validation error
   - ✓ Customer has 3 active proposals → returns 409 Conflict (MaxActiveProposals)
   - ✓ Database failure → throws infrastructure exception
   - ✓ Invalid requested limit → returns 422 validation error

2. **tests/unit/CreditRisk.CreditAnalysis.Domain.Tests/Handlers/SubmitProposalCommandHandlerTests.cs**
   - ✓ Draft proposal submitted → transitions to PendingEvaluation
   - ✓ Already submitted → returns 409 Conflict
   - ✓ Proposal not found → returns 404 NotFound

3. **tests/unit/CreditRisk.CreditAnalysis.Domain.Tests/Handlers/GetProposalByIdQueryHandlerTests.cs**
   - ✓ Existing proposal → returns proposal
   - ✓ Nonexistent proposal → returns 404 NotFound

4. **tests/unit/CreditRisk.CreditAnalysis.Domain.Tests/Services/CreditScoringEngineTests.cs** (CRITICAL)
   - **All 5 Rating Paths (A, B, C, D, E):** at least 2 tests each
   - **Rating A tests:**
     - ✓ Excellent bureau, low debt, low multiplier → Rating A
     - ✓ Above auto-approve threshold → requires manual review
     - ✓ Below auto-approve threshold → auto-approved
   - **Rating B tests:**
     - ✓ Good bureau, moderate debt → Rating B
     - ✓ Above/below auto-approve threshold
   - **Rating C tests:**
     - ✓ Fair bureau, high debt → Rating C
     - ✓ Above/below auto-approve threshold
   - **Rating D tests:**
     - ✓ Poor bureau, high debt → Rating D
     - ✓ Always requires manual review (even for small amounts)
   - **Rating E tests:**
     - ✓ Very poor bureau, very high debt → Rating E
     - ✓ Approved limit is always 0
   - **Boundary Tests (Theory-based):**
     - ✓ Composite score 750 → Rating A boundary
     - ✓ Composite score 749 → Rating B
     - ✓ Composite score 650 → Rating B boundary
     - ✓ Composite score 649 → Rating C
     - ✓ Composite score 500 → Rating C boundary
     - ✓ Composite score 499 → Rating D
     - ✓ Composite score 350 → Rating D boundary
     - ✓ Composite score 349 → Rating E
   - **Edge Cases:**
     - ✓ Zero monthly income → Rating E
     - ✓ Requested limit exceeds 500k system max → capped at 500k

5. **tests/unit/CreditRisk.CreditAnalysis.Domain.Tests/Validators/CreateProposalRequestValidatorTests.cs**
   - ✓ Valid proposal request passes
   - ✓ Invalid CPF fails
   - ✓ Invalid CNPJ fails
   - ✓ Empty customer name fails
   - ✓ Invalid email fails
   - ✓ Negative monthly income fails
   - ✓ Negative requested limit fails

**Acceptance Criteria:**
- [ ] CreditScoringEngineTests has 100% branch coverage
- [ ] All 5 rating paths (A-E) covered with multiple tests each
- [ ] All boundary values tested (750, 749, 650, 649, 500, 499, 350, 349)
- [ ] All edge cases tested (0 income, max limit)
- [ ] Theory tests for boundaries use [Theory] + [InlineData]

---

### 4.3 Compliance Application Tests

**Test Files:**

1. **tests/unit/CreditRisk.Compliance.Domain.Tests/Handlers/IngestTransactionCommandHandlerTests.cs**
   - ✓ Valid transaction → ingested successfully
   - ✓ Invalid amount → returns 422 validation error
   - ✓ PEP customer → triggers AML alert
   - ✓ Suspicious pattern → flags for review

2. **tests/unit/CreditRisk.Compliance.Domain.Tests/Services/AmlRulesEngineTests.cs**
   - ✓ Clean transaction → no flags
   - ✓ Smurfing pattern (3 txns < 10k in 24h, combined > 30k) → High severity flag
   - ✓ Round amount cash transaction → Low severity flag (10k ATM)
   - ✓ PEP match → Critical severity flag
   - ✓ Multiple rules triggered → returns all applicable flags

3. **tests/unit/CreditRisk.Compliance.Domain.Tests/Validators/IngestTransactionRequestValidatorTests.cs**
   - ✓ Valid transaction request passes
   - ✓ Negative amount fails
   - ✓ Invalid channel fails
   - ✓ Invalid beneficiary account fails

**Acceptance Criteria:**
- [ ] All AML rules tested with realistic scenarios
- [ ] Smurfing pattern detection tested with multiple transactions
- [ ] PEP matching tested
- [ ] All validators test valid and invalid inputs

---

## Phase 5: Middleware & Global Exception Handling Tests

### 5.1 Global Exception Middleware Tests

**File:** `tests/unit/CreditRisk.IAM.Domain.Tests/Middleware/GlobalExceptionMiddlewareTests.cs`

**Test Scenarios:**

1. **DomainException → 422 with RFC 7807 Problem Details**
   - ✓ Status code 422 (Unprocessable Entity)
   - ✓ Content-Type: application/problem+json
   - ✓ ProblemDetails includes: status, detail, extensions.errorCode
   - ✓ No stack trace in response

2. **Unhandled Exception → 500 without Stack Trace**
   - ✓ Status code 500
   - ✓ Response does not include exception type name
   - ✓ Response does not include stack trace
   - ✓ Response includes correlation ID

3. **OperationCanceledException → 499**
   - ✓ Status code 499 (Client Closed Request)
   - ✓ Not logged as error

4. **No Exception → Pass-through**
   - ✓ Next middleware called
   - ✓ Status code remains 200 (unchanged)

**Acceptance Criteria:**
- [ ] All exception types return correct HTTP status codes
- [ ] RFC 7807 Problem Details format verified
- [ ] No sensitive information (stack traces) in error responses
- [ ] Correlation IDs included for traceability

---

## Phase 6: Coverage & Validation

### 6.1 Code Coverage Configuration

**Deliverables:**
- Configure `coverage.runsettings` with:
  - Format: Cobertura
  - Include: CreditRisk.Shared.Kernel, all Domain/Application layers
  - Exclude: *Tests, *Migrations, compiler-generated code
  - Results directory: `./coverage`

**Coverage Targets:**
- Shared.Kernel: ≥ 80% line coverage
- All Domain layers: ≥ 80% line coverage
- All Application layers: ≥ 80% line coverage
- Credit scoring matrix: 100% branch coverage (every rating path A-E)

**Acceptance Criteria:**
- [ ] Coverage report generated via `dotnet-reportgenerator-globaltool`
- [ ] All targets meet minimum coverage thresholds
- [ ] CreditScoringEngine shows 100% branch coverage

---

### 6.2 Test Execution & Verification

**Local Execution Commands:**

1. **Build all test projects:**
   ```bash
   dotnet build tests/unit/ --configuration Release
   # Expected: 0 warnings, 0 errors
   ```

2. **Run all unit tests:**
   ```bash
   dotnet test tests/unit/ --configuration Release --no-build
   # Expected: all tests pass, < 5 seconds total
   ```

3. **Generate coverage report:**
   ```bash
   dotnet test tests/unit/ --collect:"XPlat Code Coverage" --results-directory ./coverage
   reportgenerator -reports:"./coverage/**/coverage.cobertura.xml" -targetdir:"./coverage/report"
   ```

4. **Verify no forbidden packages:**
   ```bash
   dotnet list tests/unit/ package | grep -iE "moq|nsubstitute|castle|fakeiteasy"
   # Expected: no output
   ```

**Acceptance Criteria:**
- [ ] All tests pass (0 failures, 0 skipped)
- [ ] Total execution time < 5 seconds
- [ ] Coverage report shows ≥ 80% for all layers
- [ ] Credit scoring matrix: 100% branch coverage
- [ ] No dynamic proxy libraries in dependencies
- [ ] All test method names follow `MethodName_Scenario_ExpectedResult` convention

---

## Phase 7: Integration & Final Checks

### 7.1 Solution Integration

**Changes:**
- [ ] Add test project folder to solution hierarchy
- [ ] All test projects compile with solution build
- [ ] Solution builds successfully: `dotnet build`

### 7.2 CI/CD Readiness

**Preparation:**
- [ ] Create `.github/workflows/unit-tests.yml` (if applicable) that:
  - Builds all test projects
  - Runs full test suite
  - Collects coverage
  - Verifies coverage thresholds
  - Fails if dynamic proxy libraries detected

### 7.3 Documentation

**Files to Update/Create:**
- [ ] `setup.md`: Add section on running unit tests locally
- [ ] `tests/unit/README.md`: Document test project structure and conventions
- [ ] Add comments in builder and fake classes explaining test patterns

---

## Acceptance Criteria Checklist (Definition of Done)

### Configuration & Setup
- [ ] All unit test projects compile with `dotnet build` — zero warnings
- [ ] Solution file includes all three test projects
- [ ] No Moq, NSubstitute, or FakeItEasy references in any test project
- [ ] xunit.runner.json configured for parallelization
- [ ] coverage.runsettings configured for Cobertura format

### Fakes & Builders
- [ ] All fakes implement their interfaces completely — no `NotImplementedException`
- [ ] All builders produce valid objects by default
- [ ] All builders use fluent API pattern (return `this`)
- [ ] All builders use factory methods (User.Create, etc.), not direct property assignment

### Test Coverage
- [ ] `LoginCommandHandlerTests` covers: valid credentials, user not found, wrong password, cancelled token
- [ ] `LogoutCommandHandlerTests` covers: valid JTI, empty JTI, already-revoked JTI
- [ ] `CreateProposalCommandHandlerTests` covers: valid command, invalid CPF, max active proposals, database failure
- [ ] `CreditScoringEngineTests` covers all 5 rating paths (A, B, C, D, E) — ≥ 2 tests each
- [ ] `CreditScoringEngineTests` covers all boundary values: 750, 749, 650, 649, 500, 499, 350, 349
- [ ] `GlobalExceptionMiddlewareTests` covers: DomainException → 422, unhandled → 500 (no stack trace), cancelled → 499, no exception → pass-through
- [ ] `AmlRulesEngineTests` covers: clean transaction, smurfing pattern, round amount, PEP match

### Coverage Metrics
- [ ] Line coverage on CreditRisk.Shared.Kernel ≥ 80%
- [ ] Line coverage on all Domain projects ≥ 80%
- [ ] Line coverage on all Application projects ≥ 80%
- [ ] Credit scoring matrix: 100% branch coverage
- [ ] All `DomainException` throw sites have corresponding tests

### Performance & Quality
- [ ] All tests run in < 5 seconds total (no I/O, no delays)
- [ ] Test method names follow `MethodName_Scenario_ExpectedResult` convention
- [ ] No `Thread.Sleep` or `Task.Delay` in tests
- [ ] Each test is independent — no shared mutable state
- [ ] All tests are deterministic — same inputs → same outputs

### Verification Commands
- [ ] `dotnet build tests/unit/ --configuration Release` succeeds with 0 warnings
- [ ] `dotnet test tests/unit/ --configuration Release` passes all tests
- [ ] `dotnet list tests/unit/ package | grep -iE "moq|nsubstitute|castle"` returns no output
- [ ] Coverage report: `./coverage/report/index.html` shows all thresholds met

---

## Implementation Order (Recommended)

1. **Phase 1:** Create directory structure & projects (high-level scaffolding)
2. **Phase 2:** Implement test doubles (fakes & builders) — foundational
3. **Phase 3:** Domain & value object tests (isolated domain logic)
4. **Phase 4a:** IAM Application tests (handlers & validators)
5. **Phase 4b:** CreditAnalysis Application tests (handlers, validators, **CreditScoringEngine**)
6. **Phase 4c:** Compliance Application tests (handlers, validators, AmlRulesEngine)
7. **Phase 5:** Global exception middleware tests (cross-cutting concern)
8. **Phase 6:** Coverage configuration & verification
9. **Phase 7:** Solution integration & CI/CD readiness

---

## Key Architectural Decisions

### Decision 1: Hand-Written Fakes vs. Dynamic Proxies

**Choice:** Hand-written fakes implementing interfaces directly

**Rationale:**
- Moq/NSubstitute use `Castle.DynamicProxy` (Reflection.Emit) — incompatible with Native AOT
- Hand-written fakes are explicit, testable, and AOT-safe
- Enables testing production code with AOT trimming in mind

### Decision 2: Builder Pattern for Test Data

**Choice:** Fluent builder pattern with factory delegation

**Rationale:**
- Builders produce valid objects by default (DRY for happy-path tests)
- Fluent API makes test setup readable
- Builders delegate to domain factories, ensuring domain invariants are enforced
- No reflection — all property assignments are explicit

### Decision 3: Test Isolation via In-Memory Fakes

**Choice:** All dependencies replaced with in-memory fakes (no external resources)

**Rationale:**
- Unit tests must not access databases, Redis, RabbitMQ, or HTTP endpoints
- In-memory fakes enable fast, deterministic execution (< 5 seconds)
- Easier debugging — no network latency or timeouts

### Decision 4: Coverage Thresholds

**Choice:** Minimum 80% line coverage + 100% branch coverage on scoring matrix

**Rationale:**
- 80% is industry-standard minimum for business logic
- Credit scoring matrix (A-E ratings) requires 100% branch coverage due to compliance/risk sensitivity
- Enforced via coverage reporting gate

---

## Risks & Mitigation

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|-----------|
| Fakes don't fully implement interfaces | Medium | High | Code review fakes + static analysis |
| Test doubles drift from production contracts | Medium | High | Integration tests (separate phase) will catch |
| Coverage targets not met | Low | Medium | Define clear acceptance criteria per layer |
| Tests fail due to domain model changes | Low | Low | Tests catch breaking changes (by design) |
| Execution time exceeds 5 seconds | Low | Low | Parallelize with xunit.runner.json, use fast fakes |

---

## Success Criteria

**Green Light for Phase Completion:**
1. ✅ All test projects build successfully
2. ✅ All tests pass (0 failures, 0 skipped)
3. ✅ Coverage report shows ≥ 80% for all layers + 100% for scoring matrix
4. ✅ No dynamic proxy dependencies in test projects
5. ✅ Test execution time < 5 seconds
6. ✅ All acceptance criteria checked off

---

## Timeline Estimate

| Phase | Tasks | Est. Duration |
|-------|-------|---------------|
| Phase 1 | Scaffolding & configuration | 1-2 hours |
| Phase 2 | Test doubles (fakes & builders) | 3-4 hours |
| Phase 3 | Domain & value object tests | 2-3 hours |
| Phase 4 | Application layer tests (handlers & validators) | 6-8 hours |
| Phase 5 | Middleware & exception tests | 1-2 hours |
| Phase 6 | Coverage configuration & verification | 1-2 hours |
| Phase 7 | Integration & final checks | 1 hour |
| **Total** | | **15-22 hours** |

---

## References

- **Spec:** `docs/specs/SPEC-03-backend-unit-tests.md`
- **Architecture:** `docs/specs/SPEC-01-architecture-core.md`
- **Backend:** `docs/specs/SPEC-02-backend.md`
- **Stack:** xUnit 2.9.2, FluentAssertions 7.0.0, Coverlet 6.0.2, ReportGenerator 5.3.11
- **.NET Version:** 8.0.129
- **AOT Target:** Native AOT compatibility (trimming-safe code)
