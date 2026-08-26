# Test Suite Scripts - Quick Reference

## 🚀 Quick Start

### Bash/Linux/Mac
```bash
# Show all options
./run-tests.sh --help

# Run all tests
./run-tests.sh

# Run with coverage
./run-tests.sh --coverage

# Run specific tests
./run-tests.sh --filter "CreditScoring"

# CI/CD mode
./run-tests.sh --ci
```

### PowerShell/Windows
```powershell
# Show help
Get-Help .\run-tests.ps1

# Run all tests
.\run-tests.ps1

# Run with coverage
.\run-tests.ps1 -Coverage

# Run specific tests
.\run-tests.ps1 -Filter "CreditScoring"

# CI/CD mode
.\run-tests.ps1 -CI
```

### Make (Any Platform)
```bash
# Show all commands
make help

# Run all tests
make test

# Run with coverage
make test-coverage

# Run specific module
make test-scoring      # Credit scoring
make test-handlers     # Command handlers
make test-iam          # IAM module

# Coverage report
make coverage          # Open HTML report

# Cleanup
make clean
```

---

## 📋 Available Commands

### Bash Script (`./run-tests.sh`)

| Command | Description |
|---------|-------------|
| `--unit` | Run only unit tests (default) |
| `--integration` | Run only integration tests |
| `--coverage` | Generate coverage reports |
| `--watch` | Watch mode (continuous testing) |
| `--filter <pattern>` | Run tests matching pattern |
| `--verbose` | Detailed output |
| `--ci` | CI/CD mode (strict validation) |
| `--help` | Show help |

### PowerShell Script (`.\run-tests.ps1`)

| Parameter | Description |
|-----------|-------------|
| `-Unit` | Run only unit tests |
| `-Integration` | Run only integration tests |
| `-Coverage` | Generate coverage reports |
| `-Watch` | Watch mode |
| `-Filter <pattern>` | Run tests matching pattern |
| `-Verbose` | Detailed output |
| `-CI` | CI/CD mode |

### Make Targets

| Target | Description |
|--------|-------------|
| `make test` | Run all tests |
| `make test-coverage` | Tests + coverage |
| `make test-watch` | Watch mode |
| `make test-verbose` | Verbose output |
| `make test-ci` | CI/CD mode |
| `make test-filter F=<pattern>` | Filter tests |
| `make test-iam` | IAM tests |
| `make test-credit` | Credit Analysis tests |
| `make test-scoring` | Credit Scoring tests |
| `make test-handlers` | Command Handler tests |
| `make coverage` | Open coverage report |
| `make logs` | Show test logs |
| `make clean` | Clean artifacts |
| `make info` | Show environment info |

---

## 📊 Test Coverage

Current test count: **29 tests**

### By Module

| Module | Tests | Status |
|--------|-------|--------|
| **IAM** | 8 | ✅ All passing |
| **CreditAnalysis** | 21 | ✅ All passing |
| **Compliance** | 0 | ⏳ Pending |
| **Total** | **29** | **✅ 100% Pass** |

### IAM Tests (8)
- LoginCommandHandler: 4 tests
- LogoutCommandHandler: 4 tests

### Credit Analysis Tests (21)
- CreditScoringEngine: 21 tests
  - Rating A, B, C, D, E paths
  - Boundary conditions
  - Edge cases

---

## 🔍 Auto-Discovery

The runner **automatically discovers**:

✅ All test projects in `tests/unit/` directory  
✅ All test classes with `*Tests.cs` pattern  
✅ All test methods (xUnit `[Fact]` and `[Theory]`)  
✅ All coverage data during test runs

**To add new tests:** Simply create new files/projects - no configuration needed!

---

## 📁 Files Created

```
.
├── run-tests.sh              ← Main Bash script (Linux/Mac)
├── run-tests.ps1             ← Main PowerShell script (Windows)
├── Makefile                  ← Make targets for easy execution
├── test-config.json          ← Centralized test configuration
├── docs/
│   └── TEST_SUITE_RUNNER.md  ← Full documentation
├── logs/
│   └── test-run-*.log        ← Test execution logs
└── coverage/
    ├── results/              ← Raw coverage data
    ├── report/               ← HTML coverage reports
    └── .test-history.json    ← Historical test metrics
```

---

## 🎯 Common Workflows

### Development: Quick Testing
```bash
# Run only relevant tests while coding
./run-tests.sh --filter "CreditScoring"

# Or use watch mode
./run-tests.sh --watch
```

### Pre-Commit: Local Validation
```bash
# Run all tests locally before committing
./run-tests.sh --coverage

# Check coverage report
make coverage
```

### CI/CD Pipeline: Strict Mode
```bash
# In your CI/CD pipeline
./run-tests.sh --ci

# Fails if tests fail OR coverage below 80%
```

### Coverage Analysis
```bash
# Generate detailed coverage report
./run-tests.sh --coverage

# View the report
make coverage  # Opens in browser
```

---

## 📈 Test History & Metrics

The runner tracks historical test metrics in `coverage/.test-history.json`:

- Total tests run
- Passed/failed counts
- Duration
- Coverage percentages
- Timestamps

This enables:
- Trend analysis
- Performance tracking
- Coverage regression detection

---

## ✅ Test Results Format

### Console Output
```
[2026-08-26 09:30:00] ▶ Discovering Test Projects
[2026-08-26 09:30:01] ✓ Found: CreditRisk.IAM.Domain.Tests
[2026-08-26 09:30:02] ▶ Running Tests
[2026-08-26 09:30:03] Passed: 29/29 ✓
[2026-08-26 09:30:04] ▶ Test Run Complete ✓
[2026-08-26 09:30:04] ✓ All tests passed successfully!
```

### Log Files
Located in `logs/test-run-YYYYMMDD_HHMMSS.log`
- Full test output
- Coverage metrics
- Performance data
- Any errors/warnings

### Coverage Report
Located in `coverage/report/index.html`
- Interactive HTML dashboard
- File-by-file coverage breakdown
- Line and branch coverage
- Highlighted source code

---

## 🔧 Configuration

Edit `test-config.json` to customize:

```json
{
  "coverage": {
    "thresholds": {
      "lineCoverage": 80,
      "branchCoverage": 100
    }
  },
  "testProjects": [
    {
      "name": "CreditRisk.IAM.Domain.Tests",
      "enabled": true
    }
  ]
}
```

---

## 🐛 Troubleshooting

### Tests Not Found
```bash
# Verify test projects exist
find tests/ -name "*Tests.csproj"

# Rebuild
dotnet clean tests/unit/
dotnet build tests/unit/
```

### Coverage Tool Missing
```bash
# Install reportgenerator
dotnet tool install -g dotnet-reportgenerator-globaltool
```

### Tests Hanging
```bash
# Exit watch mode: Ctrl+C
# Try without watch mode first
./run-tests.sh
```

---

## 📚 Additional Resources

- **Full Documentation**: `docs/TEST_SUITE_RUNNER.md`
- **Test Configuration**: `test-config.json`
- **SPEC-03**: `docs/specs/SPEC-03-backend-unit-tests.md`
- **Logs**: `logs/test-run-*.log`

---

## 🎓 Key Features

✅ **Auto-Discovery**: Finds all tests automatically  
✅ **Multi-Platform**: Works on Linux, Mac, and Windows  
✅ **Coverage Reports**: HTML reports with metrics  
✅ **CI/CD Ready**: Special validation mode for pipelines  
✅ **Watch Mode**: Continuous testing during development  
✅ **Flexible Filtering**: Run specific tests by pattern  
✅ **Historical Tracking**: Trends and metrics over time  
✅ **Zero Config**: Works out of the box  

---

Last Updated: 2026-08-26  
Test Count: 29  
All Tests: ✅ Passing
