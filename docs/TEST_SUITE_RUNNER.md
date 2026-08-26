# Test Suite Runner Documentation

## Overview

The SPEC-03 Test Suite Runner provides a comprehensive, self-discovering test execution system that automatically adapts to new tests as they are added to the codebase.

**Key Features:**
- ✅ **Auto-Discovery**: Dynamically finds all test projects
- ✅ **Multi-Platform**: Works on Linux/Mac (Bash) and Windows (PowerShell)
- ✅ **Coverage Reports**: Generates HTML coverage reports with metrics
- ✅ **CI/CD Ready**: Special CI mode with strict validation
- ✅ **Flexible Filtering**: Run specific tests by pattern
- ✅ **Watch Mode**: Continuous testing on file changes
- ✅ **Historical Tracking**: Records test metrics over time

---

## Quick Start

### Using Bash (Linux/Mac)

```bash
# Run all tests
./run-tests.sh

# Run with coverage report
./run-tests.sh --coverage

# Run specific tests
./run-tests.sh --filter "CreditScoring"

# CI/CD mode
./run-tests.sh --ci
```

### Using PowerShell (Windows)

```powershell
# Run all tests
.\run-tests.ps1

# Run with coverage report
.\run-tests.ps1 -Coverage

# Run specific tests
.\run-tests.ps1 -Filter "CreditScoring"

# CI/CD mode
.\run-tests.ps1 -CI
```

### Using Make

```bash
# List all available commands
make help

# Run all tests
make test

# Run with coverage
make test-coverage

# Run specific module
make test-scoring
```

---

## Script Options

### `./run-tests.sh` (Bash)

| Option | Description |
|--------|-------------|
| `--unit` | Run only unit tests (default) |
| `--integration` | Run only integration tests |
| `--coverage` | Generate coverage reports |
| `--watch` | Run in watch mode (continuous) |
| `--filter <pattern>` | Run tests matching pattern |
| `--verbose` | Show detailed output |
| `--ci` | CI/CD mode (strict, fail on violations) |
| `--help` | Show help message |

### `.\run-tests.ps1` (PowerShell)

| Parameter | Description |
|-----------|-------------|
| `-Unit` | Run only unit tests (default) |
| `-Integration` | Run only integration tests |
| `-Coverage` | Generate coverage reports |
| `-Watch` | Run in watch mode |
| `-Filter` | Run tests matching pattern |
| `-Verbose` | Show detailed output |
| `-CI` | CI/CD mode |

### Make Targets

| Target | Description |
|--------|-------------|
| `make help` | Show all available commands |
| `make test` | Run all unit tests |
| `make test-coverage` | Run tests + coverage |
| `make test-watch` | Watch mode |
| `make test-ci` | CI/CD mode |
| `make test-filter F=<pattern>` | Filter tests |
| `make test-iam` | IAM tests only |
| `make test-scoring` | Credit scoring tests |
| `make coverage` | Open coverage report |
| `make clean` | Clean test artifacts |
| `make info` | Show environment info |

---

## Usage Examples

### Example 1: Run All Tests

```bash
./run-tests.sh
```

Output:
```
[2026-08-26 09:30:00] ▶ Starting test run
[2026-08-26 09:30:01] ▶ Discovering Test Projects
[2026-08-26 09:30:01] ✓ Found: CreditRisk.IAM.Domain.Tests
[2026-08-26 09:30:01] ✓ Found: CreditRisk.CreditAnalysis.Domain.Tests
[2026-08-26 09:30:01] ✓ Found: CreditRisk.Compliance.Domain.Tests
[2026-08-26 09:30:02] ▶ Running Tests
[2026-08-26 09:30:02] Passed!  28/28 ✓
```

### Example 2: Run with Coverage

```bash
./run-tests.sh --coverage
```

Generates:
- `coverage/results/` - Raw coverage data
- `coverage/report/index.html` - Interactive HTML report
- Coverage metrics in console output

### Example 3: Run Specific Tests

```bash
./run-tests.sh --filter "CreditScoring"
```

Only runs tests containing "CreditScoring" in their name.

### Example 4: CI/CD Mode

```bash
./run-tests.sh --ci
```

In CI mode:
- ✅ Fails if tests fail
- ✅ Fails if coverage below threshold (80%)
- ✅ Generates coverage report
- ✅ Returns exit code 0 or 1

### Example 5: Watch Mode (Development)

```bash
./run-tests.sh --watch
```

Continuously watches for file changes and re-runs affected tests.

### Example 6: Make Commands

```bash
# Run tests by module
make test-iam          # IAM tests
make test-credit       # Credit Analysis tests
make test-scoring      # Credit Scoring tests

# Development workflow
make test-quick        # Fast test run
make test-full         # Full test + coverage
make coverage          # Open coverage report

# CI/CD pipeline
make ci-unit           # CI unit tests
make ci-coverage       # CI with coverage validation
```

---

## Configuration

### `test-config.json`

Central configuration file that auto-discovers and manages:

```json
{
  "testConfiguration": {
    "coverage": {
      "thresholds": {
        "lineCoverage": 80,
        "branchCoverage": 100
      }
    },
    "testProjects": [
      {
        "name": "CreditRisk.IAM.Domain.Tests",
        "path": "tests/unit/CreditRisk.IAM.Domain.Tests/...",
        "type": "unit",
        "enabled": true
      }
    ]
  }
}
```

**To Add a New Test Project:**
1. Create the `.csproj` file in `tests/unit/`
2. The runner auto-discovers it
3. (Optional) Update `test-config.json` with metadata

---

## Output & Logging

### Log Files

Logs are saved to: `logs/test-run-YYYYMMDD_HHMMSS.log`

### Coverage Report

After running with `--coverage`:
- Open: `coverage/report/index.html`
- Contains:
  - Line coverage percentage
  - Branch coverage percentage
  - File-by-file breakdown
  - Highlighted source code

### Test Metrics

The runner tracks:
- Total tests run
- Passed/failed count
- Test duration
- Coverage percentages
- Timestamp

---

## Auto-Discovery How It Works

The runner automatically discovers:

1. **Test Projects**: Searches for `*Tests.csproj` files in `tests/` directory
2. **Test Classes**: Finds `*Tests.cs` files in each project
3. **Test Methods**: xUnit discovers test methods using `[Fact]` and `[Theory]` attributes
4. **Coverage**: Collects code coverage from test runs

### Adding New Tests

Simply create a new test file or project:
- The runner will automatically discover it
- No configuration needed
- It will be included in the next run

**Example:**

```bash
# Create new test project
mkdir -p tests/unit/CreditRisk.SomeModule.Domain.Tests
cd tests/unit/CreditRisk.SomeModule.Domain.Tests

# Create .csproj (dotnet new will handle this)
dotnet new xunit

# Create test files
touch SomeTests.cs

# Next time you run the runner, it will auto-discover this project
```

---

## Coverage Thresholds

Default thresholds (configurable in `test-config.json`):

| Metric | Threshold | Type |
|--------|-----------|------|
| Line Coverage | 80% | Error if below |
| Branch Coverage | 100% | Warning if below |
| Overall Coverage | 80% | Error if below |

**In CI mode:** Fails the build if thresholds not met

**In Normal mode:** Warns but continues

---

## Continuous Integration

### GitHub Actions Integration

The runner supports CI/CD pipelines:

```bash
# In your CI pipeline
./run-tests.sh --ci

# This will:
# 1. Exit with code 0 if all tests pass and coverage OK
# 2. Exit with code 1 if any test fails or coverage below threshold
# 3. Generate coverage reports as artifacts
```

### Example GitHub Actions Workflow

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
          dotnet-version: 8.0.x
      
      - name: Run Tests
        run: ./run-tests.sh --ci
      
      - name: Upload Coverage
        uses: codecov/codecov-action@v3
        with:
          files: ./coverage/**/coverage.cobertura.xml
```

---

## Troubleshooting

### Issue: Tests Not Discovered

```bash
# Verify test projects exist
find tests/ -name "*Tests.csproj" -type f

# Rebuild to ensure compilation
dotnet clean tests/unit/
dotnet build tests/unit/
```

### Issue: Coverage Report Not Generated

```bash
# Ensure reportgenerator is installed
dotnet tool list -g | grep reportgenerator

# Install if missing
dotnet tool install -g dotnet-reportgenerator-globaltool
```

### Issue: Tests Hanging in Watch Mode

```bash
# Ctrl+C to stop
# Try without watch mode first
./run-tests.sh
```

### Issue: Coverage Below Threshold in CI

```bash
# Review coverage report
make coverage

# Check which files/methods need coverage
# Add tests for uncovered code
```

---

## Performance Tips

1. **Use Filters for Development**
   ```bash
   ./run-tests.sh --filter "CreditScoring"  # Fast, focused tests
   ```

2. **Watch Mode for TDD**
   ```bash
   ./run-tests.sh --watch  # Auto re-run on changes
   ```

3. **Skip Coverage in Development**
   ```bash
   ./run-tests.sh  # Fast (no coverage overhead)
   ```

4. **Full Coverage Only Before Commit**
   ```bash
   ./run-tests.sh --coverage  # More time, more info
   ```

---

## Maintenance

### Updating Scripts

When adding new test types (e.g., integration tests):

1. Create the test project
2. Scripts auto-discover it
3. Update `test-config.json` with metadata
4. Update `Makefile` targets if needed

### Updating Configuration

Edit `test-config.json` to:
- Add new test projects
- Update coverage thresholds
- Add test groups/filters
- Configure CI/CD

---

## Reference

- **Specification**: `docs/specs/SPEC-03-backend-unit-tests.md`
- **Configuration**: `test-config.json`
- **Bash Script**: `run-tests.sh`
- **PowerShell Script**: `run-tests.ps1`
- **Make Targets**: `Makefile`
