# Testing Guide - Credit Risk Compliance System

## Overview

The project uses a comprehensive testing strategy with multiple layers:

- **Unit Tests** (29 tests): Domain and application logic in isolation
- **Integration Tests** (7 tests): End-to-end flows with real databases (Testcontainers)
- **E2E Tests** (Future): Full system workflows via APIs

## Unit Tests (29 tests)

### Test Projects

```
tests/unit/
├── CreditRisk.IAM.Domain.Tests/         (8 tests)
└── CreditRisk.CreditAnalysis.Domain.Tests/
    └── CreditRisk.Compliance.Domain.Tests/ (21 tests)
```

### Running Unit Tests

```bash
# Run all unit tests
dotnet test --filter "FullyQualifiedName!~Integration" -c Debug

# Run specific test class
dotnet test --filter "FullyQualifiedName~CreditScoringEngine" -c Debug

# Run with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Run in parallel (faster)
dotnet test -p:ParallelizeTestCollections=true
```

### Test Coverage

- **IAM Domain:** 8 tests (user roles, permissions)
- **Credit Analysis Domain:** 21 tests
  - CreditProposal entity creation (4 tests)
  - CreditScoringEngine scoring logic (8 tests)
  - RiskRating value object (3 tests)
  - MoneyAmount value object (6 tests)

**Target:** 80% line coverage, 100% branch coverage on critical paths

### Example: Credit Scoring Engine Tests

```csharp
public class CreditScoringEngineTests
{
    private readonly ICreditScoringEngine _engine;

    public CreditScoringEngineTests()
    {
        _engine = new CreditScoringEngine();
    }

    [Theory]
    [InlineData(750, RiskRating.A)]    // >=750 = A
    [InlineData(650, RiskRating.B)]    // >=650 = B
    [InlineData(500, RiskRating.C)]    // >=500 = C
    [InlineData(350, RiskRating.D)]    // >=350 = D
    [InlineData(100, RiskRating.E)]    // <350 = E
    public async Task Evaluate_VariousBureauScores_ReturnCorrectRating(
        int bureauScore, RiskRating expectedRating)
    {
        // Act
        var result = await _engine.EvaluateAsync(bureauScore);

        // Assert
        result.Should().Be(expectedRating);
    }

    [Fact]
    public async Task Evaluate_ZeroScore_ReturnsRejectionRating()
    {
        // Arrange
        const int zeroScore = 0;

        // Act
        var result = await _engine.EvaluateAsync(zeroScore);

        // Assert
        result.Should().Be(RiskRating.E, "zero score indicates rejection");
    }
}
```

## Integration Tests (7 tests + future)

### Test Project

```
tests/integration/
├── Infrastructure/
│   └── IntegrationTestFixture.cs     # Testcontainers setup
└── EndToEnd/
    └── TransactionToAlertFlowTests.cs # 7 integration tests
```

### Testcontainers Setup

Each integration test uses:

- **PostgreSQL 16:** For database tests
- **RabbitMQ 3.13:** For messaging tests
- **Redis 7:** For PEP cache tests

Containers are created/destroyed per test collection for isolation.

### Running Integration Tests

```bash
# Run all integration tests
dotnet test tests/integration -c Debug

# Run specific test
dotnet test --filter "TransactionToAlertFlowTests.PepScreening*" -c Debug

# Run with detailed output
dotnet test tests/integration -v detailed

# Run with custom timeout (tests need time for containers to start)
dotnet test tests/integration --test-timeout 30000
```

### Integration Tests

1. **PEP Screening** (`PepScreening_DetectsPepCustomer_CreatesAlert`)
   - Tests PEP detection with pattern heuristic (documents starting with "000")
   - Verifies alert creation and persistence

2. **AML Structuring** (`AmlRules_DetectsStructuring_CreatesAlert`)
   - Tests detection of 5+ transactions <10K in 7 days
   - Verifies structuring pattern recognition

3. **Large Transaction** (`AmlRules_DetectsLargeTransaction_CreatesHighSeverityAlert`)
   - Tests detection of single large transaction >100K
   - Verifies high severity alert creation

4. **Outbox Delivery** (`AlertCreation_PublishesToOutbox_GuaranteesDelivery`)
   - Tests outbox message persistence
   - Verifies guaranteed delivery semantics

5. **Retry Logic** (`OutboxMessage_OnFailure_IncrementRetryCount`)
   - Tests outbox retry counter incrementing
   - Verifies failure handling

6. **Data Isolation** (`AlertIsolation_CustomerAlertsNotVisible_ToOtherCustomers`)
   - Tests customer data separation
   - Verifies no data leakage between customers

7. **PEP Cache** (`PepCache_CachesResults_ReducesQueries`)
   - Tests Redis caching of PEP screening results
   - Verifies 1-hour TTL and cache consistency

## Test Performance

### Unit Tests
- **Total Time:** ~45ms
- **Parallelization:** 4 test classes in parallel
- **No I/O:** All in-memory, no database access

### Integration Tests
- **Total Time:** ~30-60 seconds (includes container startup)
- **First run:** ~30s (container image pull)
- **Subsequent runs:** ~10s (containers cached)

### Performance Targets
- Unit tests < 100ms
- Integration tests < 60s
- All tests deterministic (no time-based flakiness)

## Test Isolation & Ordering

### Unit Tests
- Independent test methods (xUnit runs in parallel)
- No shared state between tests
- Test classes can run in any order

### Integration Tests
- Shared fixture per collection (`IntegrationTestCollection`)
- Containers created once per collection
- Data cleared before each test (`ClearDataAsync`)
- Tests run sequentially (container resource constraints)

## Fakes vs Mocks

This project uses **hand-written fakes** for AOT compatibility (no Moq/NSubstitute):

```csharp
// ✅ Fake implementation (AOT-compatible)
public sealed class FakeCreditScoringEngine : ICreditScoringEngine
{
    private readonly RiskRating _responseRating;

    public FakeCreditScoringEngine(RiskRating rating = RiskRating.C)
        => _responseRating = rating;

    public Task<RiskRating> EvaluateAsync(decimal score)
        => Task.FromResult(_responseRating);
}

// Usage in test
var fakeEngine = new FakeCreditScoringEngine(RiskRating.A);
var service = new MyService(fakeEngine);
// Test service behavior...
```

## Assertion Helpers

All tests use **FluentAssertions** for readable assertions:

```csharp
// Domain object assertions
alert.AlertType.Should().Be("PepMatch");
alert.Severity.Should().Be(AlertSeverity.High);
alert.CustomerId.Should().NotBeEmpty();

// Collection assertions
alerts.Should().HaveCount(3);
alerts.Should().AllSatisfy(a => a.Status == AlertStatus.Open);

// Exception assertions
action.Should().Throw<ArgumentNullException>()
    .WithParameterName("customerId");

// Async assertions
await func.Should().ThrowAsync<InvalidOperationException>();
```

## Test Naming Convention

Tests follow the pattern: `MethodUnderTest_StateUnderTest_ExpectedResult`

Examples:
- `CreditProposal_WhenCreated_HasReceivedStatus`
- `CreditScoringEngine_ScoreAbove750_ReturnsRatingA`
- `PepScreening_DetectsPepCustomer_CreatesAlert`

## Debugging Tests

### Visual Studio

1. Open test file
2. Right-click test method
3. Select "Debug Test"

### Command Line

```bash
# Run with verbose output
dotnet test --logger "console;verbosity=detailed"

# Run with test timeout disabled (for debugging)
dotnet test --test-timeout 60000

# Break on first failure
dotnet test --fail-on-test-failure
```

### Inspecting Test State

```csharp
[Fact]
public async Task MyTest()
{
    // ... arrange & act ...
    
    // Inspect state before assertion
    var alerts = await _alertRepository.GetAllAsync();
    var json = JsonSerializer.Serialize(alerts, new JsonSerializerOptions { WriteIndented = true });
    _output.WriteLine($"Alerts: {json}");
    
    // Assert
    alerts.Should().NotBeEmpty();
}
```

## Coverage Goals

### Minimum Targets

- Domain logic: **80% line coverage**
- Credit scoring: **100% branch coverage**
- Outbox pattern: **90% coverage**
- API validators: **85% coverage**

### Coverage Report

```bash
# Generate coverage report
dotnet test /p:CollectCoverage=true \
    /p:CoverletOutputFormat=cobertura \
    /p:CoverletOutput=./coverage/

# View HTML report
open coverage/index.html

# Check specific namespace
reportgenerator -reports:coverage/coverage.cobertura.xml \
    -targetdir:coverage-report
```

## CI/CD Integration

### GitHub Actions

```yaml
name: Tests
on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'
      
      - run: dotnet test --filter "FullyQualifiedName!~Integration"
      
      - run: dotnet test tests/integration --timeout 60000
      
      - uses: codecov/codecov-action@v3
        with:
          files: ./coverage/coverage.cobertura.xml
```

## Best Practices

1. **One assertion per test** (ideally, or grouped logically)
2. **Arrange-Act-Assert** pattern
3. **Descriptive test names**
4. **No test interdependencies**
5. **No sleep/delays** in tests (use events/completion sources)
6. **Use generators** for parameterized tests (`[Theory]`)
7. **Test behavior, not implementation**
8. **Keep tests fast** (target < 5 seconds per test)
9. **Test happy path and edge cases**
10. **Mock external dependencies only**

## Future Test Enhancements

1. **Property-Based Testing** with FsCheck
2. **Performance Benchmarks** with BenchmarkDotNet
3. **Load Testing** with k6 or Apache JMeter
4. **Mutation Testing** with Stryker.NET
5. **Security Testing** with OWASP ZAP
6. **Accessibility Testing** for SignalR UI

## Test Metrics Dashboard

Track these metrics in CI/CD:

- Test execution time
- Test pass rate
- Code coverage percentage
- Flaky test detection
- Test failure trends
- Performance regression detection

Run:
```bash
dotnet test --logger "trx;LogFileName=test-results.trx"
```

Upload to:
- Azure DevOps Test Results
- GitHub Actions Workflow Summary
- SonarQube Quality Gate
