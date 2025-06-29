.PHONY: build test lint format clean watch check help

# Default target
help:
	@echo "Available targets:"
	@echo "  build   - Build the solution"
	@echo "  test    - Run all tests"
	@echo "  lint    - Check code formatting"
	@echo "  format  - Format code using Fantomas"
	@echo "  clean   - Clean build artifacts"
	@echo "  watch   - Run tests in watch mode"
	@echo "  check   - Run all checks before commit (build, test, lint)"

build:
	dotnet build

test:
	dotnet test

lint:
	dotnet fantomas . --check

format:
	dotnet fantomas .

clean:
	dotnet clean
	find . -type d -name bin -exec rm -rf {} + 2>/dev/null || true
	find . -type d -name obj -exec rm -rf {} + 2>/dev/null || true

watch:
	dotnet watch test --project tests/FunctionalDddMahjong.Domain.Tests

# Pre-commit check - runs build, test, and lint
check:
	@echo "🔍 Running pre-commit checks..."
	@echo ""
	@echo "📦 Building..."
	@$(MAKE) build
	@echo ""
	@echo "🧪 Running tests..."
	@$(MAKE) test
	@echo ""
	@echo "🎨 Checking code format..."
	@$(MAKE) lint
	@echo ""
	@echo "✅ All checks passed! Ready to commit."