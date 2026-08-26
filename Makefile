.PHONY: help test test-unit test-coverage test-watch test-filter test-verbose test-ci test-clean logs open-coverage

# Default target
.DEFAULT_GOAL := help

# Colors for output
BLUE := \033[0;34m
GREEN := \033[0;32m
YELLOW := \033[1;33m
NC := \033[0m

# Detect OS
ifeq ($(OS),Windows_NT)
    SHELL := pwsh
    TEST_RUNNER := .\run-tests.ps1
else
    SHELL := bash
    TEST_RUNNER := ./run-tests.sh
endif

##@ Help
help: ## Show this help message
	@echo "$(BLUE)════════════════════════════════════════════════════════════$(NC)"
	@echo "$(BLUE)SPEC-03 Test Suite - Available Commands$(NC)"
	@echo "$(BLUE)════════════════════════════════════════════════════════════$(NC)"
	@echo ""
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | sort | awk 'BEGIN {FS = ":.*?## "}; {printf "$(GREEN)%-20s$(NC) %s\n", $$1, $$2}'
	@echo ""
	@echo "$(YELLOW)Examples:$(NC)"
	@echo "  make test                    # Run all unit tests"
	@echo "  make test-coverage           # Run tests with coverage report"
	@echo "  make test-filter F=Scoring   # Run specific test filter"
	@echo "  make test-watch              # Run tests in watch mode"
	@echo "  make test-ci                 # CI/CD mode (strict validation)"
	@echo ""

##@ Test Execution
test: ## Run all unit tests
	@echo "$(BLUE)▶ Running all unit tests...$(NC)"
	@$(TEST_RUNNER)

test-unit: ## Run only unit tests
	@echo "$(BLUE)▶ Running unit tests...$(NC)"
	@$(TEST_RUNNER) --unit

test-coverage: ## Run tests with coverage report
	@echo "$(BLUE)▶ Running tests with coverage...$(NC)"
	@$(TEST_RUNNER) --coverage

test-watch: ## Run tests in watch mode (file changes trigger re-runs)
	@echo "$(BLUE)▶ Running tests in watch mode...$(NC)"
	@$(TEST_RUNNER) --watch

test-verbose: ## Run tests with verbose output
	@echo "$(BLUE)▶ Running tests with verbose output...$(NC)"
	@$(TEST_RUNNER) --verbose

test-ci: ## Run in CI/CD mode (strict validation, fail on coverage violations)
	@echo "$(BLUE)▶ Running in CI/CD mode...$(NC)"
	@$(TEST_RUNNER) --ci

test-filter: ## Run specific tests by filter (usage: make test-filter F=CreditScoring)
	@if [ -z "$(F)" ]; then \
		echo "$(YELLOW)Error: Filter pattern required. Usage: make test-filter F=<pattern>$(NC)"; \
		exit 1; \
	fi
	@echo "$(BLUE)▶ Running tests matching: $(F)$(NC)"
	@$(TEST_RUNNER) --filter "$(F)"

test-iam: ## Run IAM module tests
	@echo "$(BLUE)▶ Running IAM tests...$(NC)"
	@$(TEST_RUNNER) --filter "IAM"

test-credit: ## Run Credit Analysis tests
	@echo "$(BLUE)▶ Running Credit Analysis tests...$(NC)"
	@$(TEST_RUNNER) --filter "CreditAnalysis"

test-scoring: ## Run Credit Scoring tests
	@echo "$(BLUE)▶ Running Credit Scoring tests...$(NC)"
	@$(TEST_RUNNER) --filter "CreditScoringEngine"

test-handlers: ## Run Command Handler tests
	@echo "$(BLUE)▶ Running Command Handler tests...$(NC)"
	@$(TEST_RUNNER) --filter "CommandHandler"

##@ Reporting & Analysis
coverage: ## Generate coverage report (requires previous test run with --coverage)
	@echo "$(BLUE)▶ Opening coverage report...$(NC)"
	@if [ -f "coverage/report/index.html" ]; then \
		open coverage/report/index.html || xdg-open coverage/report/index.html || start coverage/report/index.html; \
	else \
		echo "$(YELLOW)Coverage report not found. Run 'make test-coverage' first.$(NC)"; \
		exit 1; \
	fi

logs: ## Show last test log
	@echo "$(BLUE)▶ Recent test logs:$(NC)"
	@ls -lthS logs/test-run-*.log | head -10 || echo "No logs found"

logs-show: ## Show last test log content
	@echo "$(BLUE)▶ Last test log:$(NC)"
	@tail -100 $$(ls -t logs/test-run-*.log | head -1)

##@ Cleanup
test-clean: ## Clean test artifacts and coverage data
	@echo "$(YELLOW)▶ Cleaning test artifacts...$(NC)"
	@rm -rf coverage/results coverage/report tests/unit/**/bin tests/unit/**/obj
	@echo "$(GREEN)✓ Cleanup complete$(NC)"

clean: test-clean ## Alias for test-clean
	@echo "$(GREEN)✓ All cleaned$(NC)"

##@ Development
test-quick: ## Quick test run (no coverage)
	@$(TEST_RUNNER)

test-full: ## Full test run with all reports
	@$(TEST_RUNNER) --coverage --verbose

test-quick-coverage: ## Quick coverage check (minimal overhead)
	@$(TEST_RUNNER) --coverage

##@ Configuration
show-config: ## Show test configuration
	@echo "$(BLUE)Test Configuration (test-config.json):$(NC)"
	@cat test-config.json | grep -E '^\s*"(name|description|count)"' | head -20

list-tests: ## List all discovered test projects
	@echo "$(BLUE)▶ Available Test Projects:$(NC)"
	@find tests/unit -name "*Tests.csproj" -type f | while read f; do \
		echo "  - $$(basename $$(dirname $$f))"; \
	done

##@ CI/CD Integration
ci-unit: ## CI: Run unit tests
	@$(TEST_RUNNER) --ci

ci-coverage: ## CI: Run with coverage validation
	@$(TEST_RUNNER) --ci --coverage

##@ Advanced
rebuild: ## Rebuild all test projects
	@echo "$(BLUE)▶ Rebuilding test projects...$(NC)"
	@dotnet clean tests/unit/ -c Release
	@dotnet build tests/unit/ -c Release

test-all: rebuild test-coverage ## Full rebuild and test with coverage
	@echo "$(GREEN)✓ Full test suite complete$(NC)"

# Development utilities
watch-tests: test-watch ## Alias for test-watch

check-coverage: ## Check if coverage meets thresholds
	@echo "$(BLUE)▶ Validating coverage thresholds...$(NC)"
	@$(TEST_RUNNER) --coverage

# Info targets
info: ## Show test environment info
	@echo "$(BLUE)════════════════════════════════════════════════════════════$(NC)"
	@echo "$(BLUE)SPEC-03 Test Environment Info$(NC)"
	@echo "$(BLUE)════════════════════════════════════════════════════════════$(NC)"
	@echo ""
	@echo "$(GREEN)Dotnet Version:$(NC)"
	@dotnet --version
	@echo ""
	@echo "$(GREEN)Test Projects:$(NC)"
	@find tests/unit -name "*Tests.csproj" -type f | wc -l | xargs echo "  Total:"
	@echo ""
	@echo "$(GREEN)Test Classes:$(NC)"
	@find tests/unit -name "*Tests.cs" -type f | wc -l | xargs echo "  Total:"
	@echo ""
	@echo "$(GREEN)Fakes & Builders:$(NC)"
	@find tests/unit -name "Fake*.cs" -o -name "*Builder.cs" | wc -l | xargs echo "  Total:"
	@echo ""

version: ## Show script versions
	@echo "$(BLUE)Script Versions:$(NC)"
	@grep -E "^# |VERSION" run-tests.sh | head -3
	@echo ""

.PHONY: all
