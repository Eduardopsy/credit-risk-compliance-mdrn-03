# Plan: SPEC-03 — Back-End Unit Tests Implementation

**Status:** Ready for Implementation (Revised)  
**Date:** 2026-08-25  
**Target Framework:** .NET 10.0  
**Governing Documents:**
- setup.md (v1.0.0, 2026-07-16) — **Authoritative** foundational spec
- SPEC-03 (v1.0.0) — Unit tests specialization

---

## ⚠️ Critical Constraint Resolution

**CONFLICT IDENTIFIED:** 
- **setup.md (Line 330):** Recommends xUnit + FluentAssertions + **NSubstitute**
- **SPEC-03 (Line 55):** Prohibits NSubstitute (incompatible with Native AOT) — **hand-written fakes only**

**RESOLUTION (AOT Priority):**
Since setup.md Section 3.4 mandates **Native AOT for all API/Worker projects** (`<PublishAot>true</PublishAot>`) and SPEC-03 is a specialized unit test specification that explicitly addresses AOT compatibility, the **SPEC-03 prohibition on dynamic proxies takes precedence**. Test projects do not publish AOT directly, but they test production code that MUST be AOT-compatible.

**DECISION:** Use **hand-written fakes only** (zero Moq/NSubstitute/FakeItEasy).

---

## Executive Summary

Implementação completa da suite de testes unitários para o projeto Credit Risk Compliance Lab, cobrindo Domain e Application layers de todos os 3 módulos (IAM, CreditAnalysis, Compliance).

**Constraints Applied:**
- ✅ .NET 10.0 LTS (setup.md 3.1)
- ✅ C# 14 with primary constructors (setup.md 2.7)
- ✅ xUnit + FluentAssertions (setup.md 2.9, SPEC-03 2)
- ✅ Hand-written fakes ONLY — NO dynamic proxies (SPEC-03 1.3, 6.1)
- ✅ Records for DTOs/Commands (setup.md 2.7)
- ✅ Result<T> pattern for failures (setup.md 2.8)
- ✅ Full XML docs on public/internal members (setup.md 2.6)
- ✅ Test naming: `MethodName_Scenario_ExpectedResult` (setup.md 2.2)

**Objetivo:** 80% linha coverage em Domain/Application + 100% branch coverage na Credit Scoring Matrix.

---

## Phase 1: Setup Estrutural & Verificação de Dependências (50 min)

### 1.1 Exploração de Interfaces em Produção

**CRITICAL STEP:** Antes de criar Fakes, explorar código fonte dos Ports/Repositories em produção para garantir 100% fidelidade:

**Tarefas:**
- [ ] Ler `CreditRisk.IAM.Domain.Repositories.IUserRepository` → identificar métodos exatos
- [ ] Ler `CreditRisk.IAM.Application.Ports.*` → identificar todas as dependencies
- [ ] Ler `CreditRisk.CreditAnalysis.Domain.Repositories.*` → ICreditProposalRepository, ICustomerRepository
- [ ] Ler `CreditRisk.CreditAnalysis.Application.Ports.*` → IUnitOfWork, etc.
- [ ] Ler `CreditRisk.Compliance.Domain.Repositories.*` → todas as interfaces
- [ ] Validar: NENHUMA interface deve conter `Reflection.Emit` ou dynamic types

**Output:** Documento de interfaces mapeadas (`INTERFACES_MAPPED.md`)

### 1.2 Criar Estrutura de Diretórios

```
tests/unit/
├── CreditRisk.IAM.Domain.Tests/
├── CreditRisk.CreditAnalysis.Domain.Tests/
└── CreditRisk.Compliance.Domain.Tests/
```

**Tarefas:**
- [ ] `mkdir -p tests/unit/{CreditRisk.IAM.Domain.Tests,CreditRisk.CreditAnalysis.Domain.Tests,CreditRisk.Compliance.Domain.Tests}`
- [ ] Dentro de cada: `{Entities,ValueObjects,Validators,Handlers,Services,Fakes,Builders,Middleware}`

### 1.3 Criar Arquivos de Configuração Global

**1.3.1 `tests/unit/xunit.runner.json`**

```json
{
  "$schema": "https://xunit.net/schema/current/xunit.runner.schema.json",
  "methodDisplay": "classAndMethod",
  "methodDisplayOptions": "all",
  "parallelizeAssembly": true,
  "parallelizeTestCollections": true,
  "maxParallelThreads": 4
}
```

**1.3.2 `tests/unit/coverage.runsettings`**

```xml
<?xml version="1.0" encoding="utf-8" ?>
<RunSettings>
  <DataCollectionRunSettings>
    <DataCollectors>
      <DataCollector friendlyName="XPlat Code Coverage">
        <Configuration>
          <Format>cobertura</Format>
          <Include>
            [CreditRisk.Shared.Kernel]*
            [CreditRisk.IAM.Domain]*
            [CreditRisk.IAM.Application]*
            [CreditRisk.CreditAnalysis.Domain]*
            [CreditRisk.CreditAnalysis.Application]*
            [CreditRisk.Compliance.Domain]*
            [CreditRisk.Compliance.Application]*
          </Include>
          <Exclude>
            [*Tests]*
            [*Migrations]*
          </Exclude>
          <ExcludeByAttribute>GeneratedCodeAttribute,CompilerGeneratedAttribute</ExcludeByAttribute>
          <SingleHit>false</SingleHit>
          <UseSourceLink>true</UseSourceLink>
          <IncludeTestAssembly>false</IncludeTestAssembly>
        </Configuration>
      </DataCollector>
    </DataCollectors>
  </DataCollectionRunSettings>
  <RunConfiguration>
    <ResultsDirectory>./coverage</ResultsDirectory>
  </RunConfiguration>
</RunSettings>
```

### 1.4 Criar `.csproj` Templates (3 projetos)

**Pattern obrigatório por setup.md 3.4:**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <AssemblyName>CreditRisk.{Module}.Domain.Tests</AssemblyName>
    <RootNamespace>CreditRisk.{Module}.Domain.Tests</RootNamespace>
    <!-- Inherited from Directory.Build.props: TargetFramework, Nullable, ImplicitUsings, etc. -->
  </PropertyGroup>
  
  <ItemGroup>
    <!-- Test frameworks per setup.md 2.9 -->
    <PackageReference Include="xunit" Version="2.9.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
    <PackageReference Include="FluentAssertions" Version="7.0.0" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.12.0" />
    <PackageReference Include="coverlet.collector" Version="6.0.2">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <!-- EXPLICITLY FORBIDDEN per SPEC-03 1.3 -->
    <!-- NO Moq, NO NSubstitute, NO FakeItEasy -->
  </ItemGroup>
  
  <ItemGroup>
    <ProjectReference Include="../../../src/shared/CreditRisk.Shared.Kernel/CreditRisk.Shared.Kernel.csproj" />
    <ProjectReference Include="../../../src/modules/{module}/CreditRisk.{Module}.Domain/CreditRisk.{Module}.Domain.csproj" />
    <ProjectReference Include="../../../src/modules/{module}/CreditRisk.{Module}.Application/CreditRisk.{Module}.Application.csproj" />
    <ProjectReference Include="../../../src/modules/{module}/CreditRisk.{Module}.Infrastructure/CreditRisk.{Module}.Infrastructure.csproj" />
  </ItemGroup>
</Project>
```

---

## Phase 2: Fakes (Hand-Written Test Doubles) — 70 min

### 2.1 Padrão de Fake Obrigatório

Todos os Fakes devem:
- ✅ Ser `internal sealed class`
- ✅ Implementar 100% da interface (ZERO `NotImplementedException`)
- ✅ Sem `Reflection.Emit`, sem proxies dinâmicos
- ✅ Documentados com XML docs (setup.md 2.6)
- ✅ Usar `async`/`await` para métodos async (setup.md 2.7)
- ✅ Incluir `CancellationToken` em todas as assinaturas async (setup.md 2.7)
- ✅ Test helpers prefixados com `// Test helper` comentário

### 2.2 IAM Fakes — 5 arquivos (25 min)

| Arquivo | Interface | Métodos | Responsabilidade |
|---------|-----------|---------|------------------|
| `FakeUserRepository.cs` | `IUserRepository` | GetByIdAsync, GetByEmailAsync, AddAsync, UpdateAsync | In-memory store com Dictionary<Guid, User> |
| `FakeTokenService.cs` | `ITokenService` | GenerateTokenAsync, ValidateTokenAsync | Retorna tokens configuráveis via SetTokenResponse() |
| `FakePasswordHasher.cs` | `IPasswordHasher` | HashAsync, VerifyAsync | Retorna bool configurável via SetVerifyResult() |
| `FakeTokenRevocationStore.cs` | `ITokenRevocationStore` | RevokeAsync, IsRevokedAsync | HashSet de JTIs + helper PreloadRevoked() |
| `FakeClock.cs` | `IClock` | UtcNowAsync, GetCurrentTimeAsync | DateTimeOffset configurável |

**Critical Pattern:**
```csharp
// ✅ REQUIRED — No dynamic proxies, explicit implementation
internal sealed class FakeUserRepository : IUserRepository
{
    private readonly Dictionary<Guid, User> _store = [];
    
    /// <summary>Retrieves user by ID or returns null.</summary>
    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _store.TryGetValue(id, out var user);
        return Task.FromResult(user);
    }
    
    // Test helper — not part of interface
    internal void Add(User user) => _store[user.Id] = user;
}
```

### 2.3 CreditAnalysis Fakes — 3 arquivos (20 min)

| Arquivo | Interface | Métodos | Responsabilidade |
|---------|-----------|---------|------------------|
| `FakeCreditProposalRepository.cs` | `ICreditProposalRepository` | GetByIdAsync, GetByCustomerIdAsync, GetByStatusAsync, AddAsync, UpdateAsync, CountByStatusAsync | In-memory store + GetAll() helper |
| `FakeCustomerRepository.cs` | `ICustomerRepository` | GetByIdAsync, GetByDocumentAsync | In-memory store + SetCustomerIdForDocument() helper |
| `FakeUnitOfWork.cs` | `IUnitOfWork` | CommitAsync | CommitCallCount counter + ShouldThrow flag |

### 2.4 Compliance Fakes — 2 arquivos (15 min)

| Arquivo | Interface | Métodos | Responsabilidade |
|---------|-----------|---------|------------------|
| `FakeTransactionRepository.cs` | `ITransactionRepository` | GetByIdAsync, GetByCustomerIdAsync, AddAsync | In-memory store + AddRange(), AddPepDocument() helpers |
| `FakeAmlAlertRepository.cs` | `IAmlAlertRepository` | CreateAsync, GetByTransactionIdAsync | In-memory store |

**Constraint:** Todos Fakes 100% implementations — setup.md exige XML docs, SPEC-03 exige zero NotImplementedException.

---

## Phase 3: Test Data Builders — 40 min

### 3.1 Padrão de Builder Obrigatório (setup.md 2.9, SPEC-03 6.2)

Todos builders devem:
- ✅ Ser `internal sealed class`
- ✅ Retornar objetos **válidos por default** (happy path sem config)
- ✅ Métodos fluentes (return `this`)
- ✅ Sem reflection (assignments explícitos per SPEC-03 5.3)
- ✅ `Build()` chama factory method (ex: `CreditProposal.Create`), não seta properties direto
- ✅ Nomeação: `With{Property}()`, `As{State}()`, `Build()`

### 3.2 IAM Builders — 1 arquivo (10 min)

**`UserBuilder.cs`**
```csharp
internal sealed class UserBuilder
{
    private Email _email = Email.Create("test@example.com");
    private string _role = "desk-operator";
    private HashedPassword _password = HashedPassword.Create("default-hashed");
    
    internal UserBuilder WithEmail(string email)
    {
        _email = Email.Create(email);
        return this;
    }
    
    internal UserBuilder WithRole(string role)
    {
        _role = role;
        return this;
    }
    
    internal User Build() => User.Create(_email, _role, _password);
}
```

### 3.3 CreditAnalysis Builders — 2 arquivos (20 min)

**`CreditProposalBuilder.cs`**
- Default: Draft status, R$ 10k limit, Type "Individual"
- Methods: WithCustomerId, WithRequestedLimit, WithProposalType, AsSubmitted, WithEvaluation
- Build() calls `CreditProposal.Create()` then conditionally Submit/ApplyEvaluation

**`CustomerBuilder.cs`**
- Default: Valid CPF, email, income
- Methods: WithDocument, WithEmail, WithIncome

### 3.4 Compliance Builders — 1 arquivo (10 min)

**`TransactionBuilder.cs`**
- Default: Amount R$ 5k, Channel "Online", timestamp agora
- Methods: WithAmount, WithChannel, WithCustomerId, WithDate, WithCustomerDocument

---

## Phase 4: Unit Tests — 200 min

### 4.1 Nomenclatura & Padrão (setup.md 2.2 + SPEC-03 3.1)

**Test Class:** `{SubjectClass}Tests` (ex: `CreditProposalTests`)
**Test Method:** `MethodName_Scenario_ExpectedResult` (ex: `Submit_FromDraftStatus_TransitionsToPendingEvaluation`)
**Fact (single scenario):** `[Fact]` per xUnit
**Theory (parameterized):** `[Theory]` + `[InlineData]` per xUnit

### 4.2 IAM Tests — 70 min

#### 4.2.1 Entity Tests (10 min)
- `UserTests.cs` (4 testes): Create, validation, password hashing
- `RefreshTokenTests.cs` (3 testes): Create, expiry, revocation

#### 4.2.2 ValueObject Tests (8 min)
- `EmailTests.cs` (3 testes): Valid, invalid, equality
- `CpfTests.cs` (2 testes): Valid, invalid
- `CnpjTests.cs` (2 testes): Valid, invalid

#### 4.2.3 Validator Tests (12 min)
- `LoginRequestValidatorTests.cs` (5 testes): Valid email, invalid email, empty password, valid
- `CreateUserRequestValidatorTests.cs` (4 testes): Valid, duplicate email, weak password, TOTP

#### 4.2.4 Handler Tests (30 min)

**`LoginCommandHandlerTests.cs`** (11 testes — coverage per SPEC-03 8.1)
- ValidCredentials → LoginResponse ✅
- UserNotFound → 401 Unauthorized ✅
- WrongPassword → 401 ✅
- CancellationRequested → OperationCanceledException ✅
- InvalidEmail → 422 Validation
- TotpInvalid → 401
- UserLocked → 409
- MultipleAttempts (rate limit) → 429
- SuccessWithMFA → LoginResponse
- PasswordExpired → 422
- UserDisabled → 403

**`LogoutCommandHandlerTests.cs`** (5 testes — coverage per SPEC-03 8.2)
- ValidJti → Success + revocation store updated ✅
- EmptyJti → 422 Validation ✅
- AlreadyRevoked → Success (idempotent) ✅
- CancellationRequested → OperationCanceledException
- InvalidJti → 400 Bad Request

**`CreateUserCommandHandlerTests.cs`** (5 testes)
- ValidCommand → UserCreated
- DuplicateEmail → 409 Conflict
- WeakPassword → 422 Validation
- InvalidCpf → 422 Validation
- DatabaseFailure → propagates exception

#### 4.2.5 Middleware Tests (10 min)

**`GlobalExceptionMiddlewareTests.cs`** (6 testes — coverage per SPEC-03 8.5)
- DomainException → 422 + ProblemDetails RFC 7807 ✅
- UnhandledException → 500 + NO stack trace ✅
- OperationCanceled → 499 + NO error log ✅
- NoException → PassThrough ✅
- ValidationException → 422 with details
- UnauthorizedException → 401

### 4.3 CreditAnalysis Tests — 90 min

#### 4.3.1 Entity Tests (12 min)
- `CreditProposalTests.cs` (6 testes): Create, Submit, ApplyEvaluation, state transitions, invariants
- `CustomerTests.cs` (3 testes): Create, validation

#### 4.3.2 ValueObject Tests (8 min)
- `MoneyAmountTests.cs` (5 testes): Create, validation, comparisons, arithmetic

#### 4.3.3 Service Tests (35 min) — **CRITICAL: 100% BRANCH COVERAGE**

**`CreditScoringEngineTests.cs`** (25+ testes per SPEC-03 8.3)

**Rating A (3 testes):**
- ExcellentBureau_LowDebt_LowMultiplier → RatingA ✅
- RatingA_BelowAutoApprove → ManualReview=false ✅
- RatingA_AboveAutoApprove → ManualReview=true ✅

**Rating B (3 testes):**
- GoodBureau_ModerateDebt → RatingB ✅
- RatingB_BelowAutoApprove → ManualReview=false ✅
- RatingB_AboveAutoApprove → ManualReview=true ✅

**Rating C (3 testes):**
- FairBureau_HighDebt → RatingC ✅
- RatingC_BelowAutoApprove → ManualReview=false ✅
- RatingC_AboveAutoApprove → ManualReview=true ✅

**Rating D (2 testes):**
- PoorBureau_HighDebt → RatingD ✅
- RatingD_AlwaysManualReview → true (even small) ✅

**Rating E (2 testes):**
- VeryPoorBureau_VeryHighDebt → RatingE ✅
- RatingE_ApprovedLimitZero → 0m ✅

**Boundary Tests (8 testes) — 100% BRANCH PER SPEC-03 5.2:**
```csharp
[Theory]
[InlineData(750, RiskRating.A)]    // Exact boundary A/B
[InlineData(749, RiskRating.B)]    // Just below A
[InlineData(650, RiskRating.B)]    // Exact boundary B/C
[InlineData(649, RiskRating.C)]    // Just below B
[InlineData(500, RiskRating.C)]    // Exact boundary C/D
[InlineData(499, RiskRating.D)]    // Just below C
[InlineData(350, RiskRating.D)]    // Exact boundary D/E
[InlineData(349, RiskRating.E)]    // Just below D
public void Evaluate_BoundaryScores_ReturnsCorrectRating(...)
```

**Edge Cases (3 testes):**
- ZeroMonthlyIncome → RatingE ✅
- RequestedLimit > 500k → ApprovedLimit capped ✅
- NegativeDebtRatio → normalized to 0

#### 4.3.4 Handler Tests (25 min)

**`CreateProposalCommandHandlerTests.cs`** (6 testes per SPEC-03 8.6)
- ValidCommand → ProposalCreated + CommitCalled ✅
- InvalidCpf → 422 Validation ✅
- MaxActiveProposals (3) → 409 Conflict ✅
- DatabaseFailure → propagates exception ✅
- CancellationRequested → OperationCanceledException
- BureauConsentMissing → 422 Validation

**`GetProposalByIdQueryHandlerTests.cs`** (3 testes)
- ProposalFound → returns dto
- ProposalNotFound → 404
- CancellationRequested → OperationCanceledException

#### 4.3.5 Validator Tests (10 min)
- `CreateProposalRequestValidatorTests.cs` (5 testes): Valid, invalid fields, CPF validation

### 4.4 Compliance Tests — 60 min

#### 4.4.1 Entity Tests (10 min)
- `TransactionTests.cs` (4 testes): Create, validation
- `AmlAlertTests.cs` (3 testes): Create, severity levels

#### 4.4.2 Service Tests (30 min)

**`AmlRulesEngineTests.cs`** (8 testes per SPEC-03 8.4)
- CleanTransaction → NoFlags ✅
- SmurfingPattern (3 txns < 24h, combined > 30k) → High severity ✅
- RoundAmountCash (10k ATM) → Low severity ✅
- PepMatch → Critical severity ✅
- MultipleFlags → All triggered
- HighAmountThreshold → Medium severity
- SuspiciousBehavior → High severity
- NoRulesTriggered → Empty list

#### 4.4.3 Handler Tests (20 min)
- `IngestTransactionCommandHandlerTests.cs` (5 testes): Valid, invalid, AML flags, saved, error handling

---

## Phase 5: Validação & Coverage — 50 min

### 5.1 Build & Compilation (15 min)
- [ ] `dotnet build tests/unit/ --configuration Release`
- [ ] Verificar: sucesso, 0 warnings
- [ ] Verificar: nenhuma referência a Moq/NSubstitute/FakeItEasy/Castle

### 5.2 Execute Tests (20 min)
- [ ] `dotnet test tests/unit/ --configuration Release --no-build --logger "console;verbosity=normal"`
- [ ] Todos passam: 0 failures, 0 skipped
- [ ] Tempo total < 5 segundos

### 5.3 Coverage Report (15 min)
- [ ] Instalar: `dotnet tool install -g dotnet-reportgenerator-globaltool --version 5.3.11`
- [ ] Rodar coverage com settings
- [ ] Gerar HTML report
- [ ] Verificar: Domain ≥80%, Application ≥80%, Credit Scoring 100% branch

---

## Phase 6: Integração & Finalização — 30 min

### 6.1 Atualizar Solution (10 min)
- [ ] Adicionar 3 projetos test ao `CreditRiskComplianceLab.sln`
- [ ] Verificar: solution compila

### 6.2 Validação de Padrões (10 min)
- [ ] ✅ Test classes: `{SubjectClass}Tests`
- [ ] ✅ Test methods: `MethodName_Scenario_ExpectedResult`
- [ ] ✅ Fake classes: `Fake{InterfaceName}`
- [ ] ✅ Builder classes: `{EntityName}Builder`
- [ ] ✅ XML docs on all public/internal members (setup.md 2.6)

### 6.3 Documentação (10 min)
- [ ] Criar `docs/TESTING.md` com:
  - Padrões de builder
  - Como adicionar novo teste
  - Como rodar coverage
  - Links para SPEC-03 e setup.md
- [ ] Atualizar README com instruções `dotnet test`

---

## Estimativa de Esforço Revisada

| Phase | Duração | Tarefas | Constraints |
|-------|---------|---------|-------------|
| 1. Setup + Exploração | 50 min | Dirs + config + .csproj + interfaces mapping | setup.md 3.4 |
| 2. Fakes | 70 min | 10 arquivos hand-written | SPEC-03 1.3, setup.md 2.6 |
| 3. Builders | 40 min | 4 arquivos fluent | SPEC-03 5.3, setup.md 2.7 |
| 4. Tests | 200 min | 30+ test classes, 100+ testes | SPEC-03 8.x, setup.md 2.9 |
| 5. Validação | 50 min | Build, test, coverage | SPEC-03 5.2 |
| 6. Integração | 30 min | Solution + docs | setup.md 2.3 |
| **TOTAL** | **440 min** | **~7.3 horas** | All constraints |

---

## Artifacts Entregáveis (47 arquivos)

### Config Files (3 arquivos)
- `tests/unit/xunit.runner.json`
- `tests/unit/coverage.runsettings`
- `docs/TESTING.md`

### Project Files (3 .csproj)
- `tests/unit/CreditRisk.IAM.Domain.Tests/CreditRisk.IAM.Domain.Tests.csproj`
- `tests/unit/CreditRisk.CreditAnalysis.Domain.Tests/CreditRisk.CreditAnalysis.Domain.Tests.csproj`
- `tests/unit/CreditRisk.Compliance.Domain.Tests/CreditRisk.Compliance.Domain.Tests.csproj`

### Fakes (10 arquivos)
**IAM:**
- `Fakes/FakeUserRepository.cs`
- `Fakes/FakeTokenService.cs`
- `Fakes/FakePasswordHasher.cs`
- `Fakes/FakeTokenRevocationStore.cs`
- `Fakes/FakeClock.cs`

**CreditAnalysis:**
- `Fakes/FakeCreditProposalRepository.cs`
- `Fakes/FakeCustomerRepository.cs`
- `Fakes/FakeUnitOfWork.cs`

**Compliance:**
- `Fakes/FakeTransactionRepository.cs`
- `Fakes/FakeAmlAlertRepository.cs`

### Builders (4 arquivos)
- `Builders/UserBuilder.cs`
- `Builders/CreditProposalBuilder.cs`
- `Builders/CustomerBuilder.cs`
- `Builders/TransactionBuilder.cs`

### Tests (30 arquivos)
**IAM (11):**
- `Entities/UserTests.cs`, `RefreshTokenTests.cs`
- `ValueObjects/EmailTests.cs`, `CpfTests.cs`, `CnpjTests.cs`
- `Validators/LoginRequestValidatorTests.cs`, `CreateUserRequestValidatorTests.cs`
- `Handlers/LoginCommandHandlerTests.cs`, `LogoutCommandHandlerTests.cs`, `CreateUserCommandHandlerTests.cs`
- `Middleware/GlobalExceptionMiddlewareTests.cs`

**CreditAnalysis (10):**
- `Entities/CreditProposalTests.cs`, `CustomerTests.cs`
- `ValueObjects/MoneyAmountTests.cs`
- `Services/CreditScoringEngineTests.cs`
- `Validators/CreateProposalRequestValidatorTests.cs`
- `Handlers/CreateProposalCommandHandlerTests.cs`, `GetProposalByIdQueryHandlerTests.cs`
- (Additional validators/handlers as per phase 4.3)

**Compliance (9):**
- `Entities/TransactionTests.cs`, `AmlAlertTests.cs`
- `Services/AmlRulesEngineTests.cs`
- `Validators/IngestTransactionRequestValidatorTests.cs`
- `Handlers/IngestTransactionCommandHandlerTests.cs`
- (Additional as per phase 4.4)

---

## Acceptance Criteria (Per SPEC-03 Section 9)

### Build & Compilation ✅
- [ ] `dotnet build tests/unit/` → sucesso, 0 warnings
- [ ] `dotnet list tests/unit/ package | grep -iE "moq|nsubstitute|castle|fakeiteasy"` → ZERO output
- [ ] Todos Fakes implementam 100% interface (ZERO NotImplementedException)

### Test Coverage per SPEC-03
- [ ] `CreditScoringEngineTests`: 5 ratings (A-E) ✅ ≥2 testes cada (min 10, atual 13)
- [ ] `CreditScoringEngineTests`: Boundaries 100% branch ✅ (8 boundary tests)
- [ ] `LoginCommandHandlerTests`: valid, not found, wrong password, cancelled ✅ (11 testes)
- [ ] `LogoutCommandHandlerTests`: valid JTI, empty, already revoked ✅ (5 testes)
- [ ] `GlobalExceptionMiddlewareTests`: 422, 500 no trace, 499, passthrough ✅ (6 testes)
- [ ] `CreateProposalCommandHandlerTests`: valid, invalid CPF, max proposals, DB failure ✅ (6 testes)
- [ ] `AmlRulesEngineTests`: clean, smurfing, round amount, PEP, multiple ✅ (8 testes)

### Builders ✅
- [ ] Produzem objetos válidos por default (happy path sem config)
- [ ] Métodos fluentes (return `this`)
- [ ] Sem reflection (assignments explícitos)
- [ ] `Build()` chama factory method, não seta properties

### Execution ✅
- [ ] Todos testes passam (0 failures, 0 skipped)
- [ ] Tempo total < 5 segundos (ZERO I/O, ZERO delays)
- [ ] Nomenclatura: `MethodName_Scenario_ExpectedResult` ✅

### Coverage Targets ✅
- [ ] `CreditRisk.Shared.Kernel` ≥80% linha
- [ ] Domain layers ≥80% linha
- [ ] Application layers ≥80% linha
- [ ] Credit Scoring Matrix 100% branch ✅

---

## Próximos Passos

1. ✅ **Plan Finalizado** — Review constraints vs setup.md
2. 🚀 **Phase 1-6** — Sequential implementation
3. 📊 **Coverage Report** — Gerar ao final
4. ✅ **Acceptance** — Validar todos AC

---

## Notas Importantes (setup.md + SPEC-03)

**Fundamental Constraints:**
- **Financial-grade correctness:** ACID, immutable audit (setup.md 1.3)
- **Explicit over implicit:** All rules in code, NOT framework defaults (setup.md 1.3)
- **Test Isolation:** ZERO I/O, ZERO delays, ZERO shared state (SPEC-03 5.1)
- **Determinismo:** Same inputs → same outputs ALWAYS (SPEC-03 5.1)
- **Nomeação:** RIGID adherence to conventions (setup.md 2.2, SPEC-03 3.1)
- **Builders:** ALWAYS valid by default (SPEC-03 5.3)
- **AOT:** CRITICAL — hand-written fakes ONLY (SPEC-03 1.3, setup.md 3.4)
- **XML Docs:** MANDATORY on all public/internal (setup.md 2.6)
- **CancellationToken:** Thread through EVERY async call (setup.md 2.7)
- **Result Pattern:** Use Result<T> for expected failures (setup.md 2.8)
- **Async Only:** Never `.Result` or `.Wait()` (setup.md 2.7, 3.2)
- **ConfigureAwait(false):** On library/infrastructure code (setup.md 2.7)

---

**Plan Status:** ✅ Ready for Implementation  
**Last Updated:** 2026-08-25  
**Constraint Alignment:** setup.md v1.0.0 + SPEC-03 v1.0.0

### 1.1 Criar Estrutura de Diretórios

```
tests/unit/
├── CreditRisk.IAM.Domain.Tests/
├── CreditRisk.CreditAnalysis.Domain.Tests/
└── CreditRisk.Compliance.Domain.Tests/
```

**Tarefas:**
- [x] Criar pasta `tests/unit/`
- [x] Criar subpastas para cada módulo
- [x] Criar subdiretorios: `Entities/`, `ValueObjects/`, `Validators/`, `Handlers/`, `Services/`, `Fakes/`, `Builders/`, `Middleware/`

### 1.2 Criar Arquivos de Configuração Global

**Arquivos a criar em `tests/unit/`:**

1. **`xunit.runner.json`** — Configuração paralela de testes
   - `methodDisplay`: "classAndMethod"
   - `parallelizeAssembly`: true
   - `maxParallelThreads`: 4

2. **`coverage.runsettings`** — Cobertura via Coverlet
   - Include: todos Domain/Application das 3 módulos
   - Exclude: *Tests, *Migrations
   - Formato: Cobertura XML

### 1.3 Criar `.csproj` Templates para 3 Projetos

Cada projeto (.csproj) deve ter:
- PackageReferences: xunit 2.9.2, FluentAssertions 7.0.0, Microsoft.NET.Test.Sdk 17.12.0, coverlet.collector 6.0.2
- ProjectReferences: Shared.Kernel + respectivos Domain/Application/Infrastructure do módulo
- NO Moq, NSubstitute, FakeItEasy

**Padrão .csproj:**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <AssemblyName>CreditRisk.{Module}.Domain.Tests</AssemblyName>
    <RootNamespace>CreditRisk.{Module}.Domain.Tests</RootNamespace>
  </PropertyGroup>
  <ItemGroup>
    <!-- xunit, FluentAssertions, coverlet -->
  </ItemGroup>
  <ItemGroup>
    <!-- Shared.Kernel + Module layers -->
  </ItemGroup>
</Project>
```

---

## Phase 2: Fakes (Hand-Written Test Doubles) — 60 min

### 2.1 IAM Fakes — 5 arquivos

| Arquivo | Interface | Responsabilidade |
|---------|-----------|------------------|
| `FakeUserRepository.cs` | `IUserRepository` | In-memory store com GetByIdAsync, GetByEmailAsync, AddAsync, UpdateAsync |
| `FakeTokenService.cs` | `ITokenService` | Retorna tokens configuráveis via `SetTokenResponse()` |
| `FakePasswordHasher.cs` | `IPasswordHasher` | Retorna bool configurável via `SetVerifyResult()` |
| `FakeTokenRevocationStore.cs` | `ITokenRevocationStore` | HashSet de JTIs revogados + helper `PreloadRevoked()` |
| `FakeClock.cs` | `IClock` | Retorna DateTimeOffset configurável |

### 2.2 CreditAnalysis Fakes — 3 arquivos

| Arquivo | Interface | Responsabilidade |
|---------|-----------|------------------|
| `FakeCreditProposalRepository.cs` | `ICreditProposalRepository` | In-memory store com GetByIdAsync, GetByCustomerIdAsync, GetByStatusAsync, AddAsync, UpdateAsync, CountByStatusAsync + GetAll() |
| `FakeCustomerRepository.cs` | `ICustomerRepository` | In-memory store com GetByIdAsync, GetByDocumentAsync + helper SetCustomerIdForDocument() |
| `FakeUnitOfWork.cs` | `IUnitOfWork` | CommitCallCount counter + ShouldThrow flag |

### 2.3 Compliance Fakes — 2 arquivos

| Arquivo | Interface | Responsabilidade |
|---------|-----------|------------------|
| `FakeTransactionRepository.cs` | `ITransactionRepository` | In-memory store com GetByIdAsync, GetByCustomerIdAsync + AddRange(), AddPepDocument() |
| `FakeAmlAlertRepository.cs` | `IAmlAlertRepository` | In-memory store com CreateAsync, GetByTransactionIdAsync |

**Constraint:** Todos Fakes devem ser `internal sealed` e implementar 100% da interface (sem NotImplementedException).

---

## Phase 3: Test Data Builders — 50 min

### 3.1 IAM Builders — 1 arquivo

**`UserBuilder.cs`**
- Default: Email válido, Role "desk-operator", Senha hashed
- Methods: `WithEmail()`, `WithRole()`, `WithPassword()`, `Build()`
- Constraints: Retorna objeto válido por padrão (happy path)

### 3.2 CreditAnalysis Builders — 2 arquivos

**`CreditProposalBuilder.cs`**
- Default: Draft proposal, Requested limit R$ 10k, Type "Individual"
- Methods: `WithCustomerId()`, `WithRequestedLimit()`, `WithProposalType()`, `WithCreatedBy()`, `AsSubmitted()`, `WithEvaluation()`, `Build()`
- Constraint: Build() chama `CreditProposal.Create()`, não seta properties direto

**`CustomerBuilder.cs`**
- Default: CPF/CNPJ válido, Email válido
- Methods: `WithDocument()`, `WithEmail()`, `WithIncome()`, `Build()`

### 3.3 Compliance Builders — 1 arquivo

**`TransactionBuilder.cs`**
- Default: Amount R$ 5k, Channel "Online", Timestamp agora
- Methods: `WithAmount()`, `WithChannel()`, `WithCustomerId()`, `WithDate()`, `WithCustomerDocument()`, `Build()`

---

## Phase 4: Unit Tests — 180 min

### 4.1 IAM Tests — 70 min

#### 4.1.1 Entity Tests (15 min)
- `UserTests.cs`: User creation, email validation, password hashing
- `RefreshTokenTests.cs`: Token creation, expiry validation, revocation

#### 4.1.2 ValueObject Tests (10 min)
- `EmailTests.cs`: Valid email, invalid email, equality
- Others por conveniência da spec (OPTIONAL neste MVP)

#### 4.1.3 Handler Tests (35 min)
- **`LoginCommandHandlerTests.cs`** (9 testes)
  - ✅ ValidCredentials → LoginResponse
  - ✅ UserNotFound → 401 Unauthorized
  - ✅ WrongPassword → 401 Unauthorized
  - ✅ CancellationRequested → OperationCanceledException
  - ✅ InvalidEmail → 422 Validation
  - ✅ TotpInvalid → 401
  - ✅ UserLocked → 409
  - ✅ Multiple attempts (rate limit)
  - ✅ Success + MFA

- **`LogoutCommandHandlerTests.cs`** (4 testes)
  - ✅ ValidJti → Success + revocation store updated
  - ✅ EmptyJti → 422 Validation
  - ✅ AlreadyRevoked → Success (idempotent)
  - ✅ CancellationRequested → OperationCanceledException

#### 4.1.4 Middleware Tests (10 min)
- `GlobalExceptionMiddlewareTests.cs` (4 testes)
  - ✅ DomainException → 422 + ProblemDetails (RFC 7807)
  - ✅ UnhandledException → 500 + NO stack trace
  - ✅ OperationCanceled → 499 + NO error log
  - ✅ NoException → PassThrough

### 4.2 CreditAnalysis Tests — 80 min

#### 4.2.1 Entity Tests (15 min)
- `CreditProposalTests.cs`: Create, Submit, ApplyEvaluation, state transitions
- `CustomerTests.cs`: Create, validation

#### 4.2.2 ValueObject Tests (10 min)
- `MoneyAmountTests.cs`: Create, validation, comparisons

#### 4.2.3 Service Tests — Credit Scoring Engine (35 min)
- **`CreditScoringEngineTests.cs`** (20+ testes)
  - **Rating A** (3 testes)
    - ExcellentBureau → Rating A
    - BelowAutoApprove → false
    - AboveAutoApprove → true
  - **Rating B** (3 testes) — similar pattern
  - **Rating C** (3 testes) — similar pattern
  - **Rating D** (2 testes)
    - PoorBureau → Rating D
    - AlwaysRequiresReview
  - **Rating E** (2 testes)
    - VeryPoorBureau → Rating E
    - ApprovedLimitIsZero
  - **Boundary Tests** (2 testes)
    - CompositeScore=750 → A
    - CompositeScore=749 → B
    - (etc. para 650, 649, 500, 499, 350, 349)
  - **Edge Cases** (2 testes)
    - ZeroMonthlyIncome → E
    - RequestedLimit > 500k → capped at 500k

#### 4.2.4 Handler Tests (20 min)
- **`CreateProposalCommandHandlerTests.cs`** (5 testes)
  - ✅ ValidCommand → ProposalCreated + CommitCalled
  - ✅ InvalidCPF → 422 Validation
  - ✅ MaxActiveProposals (3) → 409 Conflict
  - ✅ DatabaseFailure → propagates exception
  - ✅ CancellationRequested → OperationCanceledException

#### 4.2.5 Query/Validator Tests (10 min)
- `GetProposalByIdQueryHandlerTests.cs`: Found, NotFound
- `CreateProposalRequestValidatorTests.cs`: Valid, invalid fields

### 4.3 Compliance Tests — 60 min

#### 4.3.1 Entity Tests (15 min)
- `TransactionTests.cs`: Create, validation
- `AmlAlertTests.cs`: Create, severity levels

#### 4.3.2 Service Tests (30 min)
- **`AmlRulesEngineTests.cs`** (5 testes)
  - ✅ CleanTransaction → NoFlags
  - ✅ SmurfingPattern (3 txns < 24h) → High severity
  - ✅ RoundAmountCash (10k ATM) → Low severity
  - ✅ PepMatch → Critical severity
  - ✅ MultipleFlags → All triggered

#### 4.3.3 Handler Tests (15 min)
- `IngestTransactionCommandHandlerTests.cs`: Valid, invalid, AML flags

---

## Phase 5: Validação e Cobertura — 50 min

### 5.1 Build e Testes (20 min)
- `dotnet build tests/unit/` → sem warnings
- `dotnet test tests/unit/` → todos passam

### 5.2 Coverage Report (20 min)
- Instalar: `dotnet tool install -g dotnet-reportgenerator-globaltool --version 5.3.11`
- Rodar coverage: `dotnet test --collect:"XPlat Code Coverage"`
- Gerar report: `reportgenerator` → verificar ≥80% em Domain + Application

### 5.3 Validação AOT (10 min)
- `dotnet list tests/unit/ package | grep -iE "moq|nsubstitute|castle|fakeiteasy"` → ZERO resultados
- Confirmar: Todos Fakes são hand-written, sem dynamic proxy

---

## Phase 6: Integração e Finalização — 40 min

### 6.1 Atualizar Solution (5 min)
- Adicionar 3 projetos test ao `CreditRiskComplianceLab.sln`

### 6.2 Validação de Nomenclatura (10 min)
- ✅ Test classes: `{SubjectClass}Tests`
- ✅ Test methods: `MethodName_Scenario_ExpectedResult`
- ✅ Fake classes: `Fake{InterfaceName}`
- ✅ Builder classes: `{EntityName}Builder`

### 6.3 Documentação (15 min)
- Atualizar README com instruções de execução
- Criar TESTING.md com padrões e convenções
- Documentar builder pattern com exemplos

### 6.4 Validação Final (10 min)
- ✅ Tempo execução < 5s
- ✅ 0 falhas, 0 skipped
- ✅ Cobertura ≥80%
- ✅ Credit Scoring 100% branch

---

## Estimativa de Esforço

| Phase | Duração | Tarefas |
|-------|---------|---------|
| 1. Setup | 40 min | Estrutura + config |
| 2. Fakes | 60 min | 10 arquivos hand-written |
| 3. Builders | 50 min | 4 arquivos |
| 4. Tests | 180 min | 20+ test classes, 80+ testes |
| 5. Validação | 50 min | Build, coverage, AOT check |
| 6. Finalização | 40 min | Integração + docs |
| **TOTAL** | **420 min** | **~7 horas** |

---

## Artifacts Entregáveis

### Estrutura de Arquivos (45 arquivos total)

```
tests/unit/
├── xunit.runner.json
├── coverage.runsettings
├── CreditRisk.IAM.Domain.Tests/
│   ├── CreditRisk.IAM.Domain.Tests.csproj
│   ├── Entities/
│   │   ├── UserTests.cs
│   │   └── RefreshTokenTests.cs
│   ├── ValueObjects/
│   │   ├── CpfTests.cs
│   │   ├── CnpjTests.cs
│   │   └── EmailTests.cs
│   ├── Validators/
│   │   ├── LoginRequestValidatorTests.cs
│   │   └── CreateUserRequestValidatorTests.cs
│   ├── Handlers/
│   │   ├── LoginCommandHandlerTests.cs
│   │   ├── LogoutCommandHandlerTests.cs
│   │   └── CreateUserCommandHandlerTests.cs
│   ├── Fakes/
│   │   ├── FakeUserRepository.cs
│   │   ├── FakeTokenService.cs
│   │   ├── FakePasswordHasher.cs
│   │   ├── FakeTokenRevocationStore.cs
│   │   └── FakeClock.cs
│   ├── Builders/
│   │   └── UserBuilder.cs
│   └── Middleware/
│       └── GlobalExceptionMiddlewareTests.cs
│
├── CreditRisk.CreditAnalysis.Domain.Tests/
│   ├── CreditRisk.CreditAnalysis.Domain.Tests.csproj
│   ├── Entities/
│   │   ├── CreditProposalTests.cs
│   │   └── CustomerTests.cs
│   ├── ValueObjects/
│   │   └── MoneyAmountTests.cs
│   ├── Services/
│   │   └── CreditScoringEngineTests.cs
│   ├── Validators/
│   │   └── CreateProposalRequestValidatorTests.cs
│   ├── Handlers/
│   │   ├── CreateProposalCommandHandlerTests.cs
│   │   └── GetProposalByIdQueryHandlerTests.cs
│   ├── Fakes/
│   │   ├── FakeCreditProposalRepository.cs
│   │   ├── FakeCustomerRepository.cs
│   │   └── FakeUnitOfWork.cs
│   └── Builders/
│       ├── CreditProposalBuilder.cs
│       └── CustomerBuilder.cs
│
└── CreditRisk.Compliance.Domain.Tests/
    ├── CreditRisk.Compliance.Domain.Tests.csproj
    ├── Entities/
    │   ├── TransactionTests.cs
    │   └── AmlAlertTests.cs
    ├── Services/
    │   └── AmlRulesEngineTests.cs
    ├── Validators/
    │   └── IngestTransactionRequestValidatorTests.cs
    ├── Handlers/
    │   └── IngestTransactionCommandHandlerTests.cs
    ├── Fakes/
    │   ├── FakeTransactionRepository.cs
    │   └── FakeAmlAlertRepository.cs
    └── Builders/
        └── TransactionBuilder.cs
```

---

## Acceptance Criteria

### Build & Compilation
- [ ] `dotnet build tests/unit/` → sucesso, 0 warnings
- [ ] Nenhuma referência a Moq/NSubstitute/FakeItEasy
- [ ] Todos Fakes implementam 100% da interface

### Test Coverage
- [ ] CreditScoringEngineTests: 5 ratings (A-E), ≥2 testes cada, boundaries 100% coverage
- [ ] LoginCommandHandlerTests: 4+ testes (valid, not found, wrong password, cancelled)
- [ ] LogoutCommandHandlerTests: 3+ testes (valid, empty, already revoked)
- [ ] GlobalExceptionMiddlewareTests: 4 testes (422, 500, 499, passthrough)
- [ ] CreateProposalCommandHandlerTests: 5 testes (valid, invalid CPF, max proposals, DB failure, cancelled)
- [ ] AmlRulesEngineTests: 5 testes (clean, smurfing, round amount, PEP, multiple)

### Builders
- [ ] Produzem objetos válidos por default (happy path sem config)
- [ ] Métodos fluentes (return this)
- [ ] Sem reflection (assignments explícitos)
- [ ] Build() chama factory method, não seta properties

### Execution
- [ ] Todos testes passam (0 failures, 0 skipped)
- [ ] Tempo total < 5 segundos
- [ ] Nomenclatura: MethodName_Scenario_ExpectedResult

### Coverage Targets
- [ ] CreditRisk.Shared.Kernel ≥80% linha
- [ ] Todos Domain layers ≥80% linha
- [ ] Todos Application layers ≥80% linha
- [ ] Credit Scoring Matrix 100% branch

---

## Riscos e Mitigações

| Risco | Probabilidade | Impacto | Mitigação |
|-------|---------------|--------|-----------|
| Interfaces de produção incompletas | Média | Alto | Explorar código source antes de implementar Fakes |
| Testes flaky (delay/timing) | Baixa | Médio | Usar fakes sincronos, NO Thread.Sleep |
| Cobertura < 80% | Média | Alto | Priorizar critical paths, builders helpers |
| AOT incompatibility | Baixa | Crítico | Hand-written fakes only, sem packages dinamicos |

---

## Próximos Passos

1. ✅ **Validar Plano** — Revisar com stakeholder
2. 🚀 **Phase 1-6** — Implementação sequencial
3. 📊 **Coverage Report** — Gerar relatório final
4. ✅ **Acceptance** — Validar todos AC

---

## Notas Importantes

- **Test Isolation:** ZERO I/O, ZERO delays, ZERO shared state between tests
- **Determinismo:** Mesmos inputs sempre → mesmos outputs
- **Nomeação:** Seguir rigorosamente conventions (MethodName_Scenario_ExpectedResult)
- **Builders:** SEMPRE produzem objetos válidos por default (happy path)
- **AOT:** CRITICAL — sem dynamic proxies em qualquer lugar
- **Hand-Written Fakes:** Implementação 100%, sem NotImplementedException

---

**Plan Status:** ✅ Ready for Implementation  
**Last Updated:** 2026-08-25  
**Next Review:** Post-Phase 1 completion
