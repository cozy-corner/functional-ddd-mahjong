# Functional DDD Mahjong

A learning project to explore functional Domain-Driven Design (DDD) concepts using F# and Japanese Mahjong as the problem domain.

## Overview

This project implements a simplified version of Japanese Mahjong using functional programming principles from Scott Wlaschin's "Domain Modeling Made Functional" approach.

## Documentation

- **[Learning Guide](doc/guide.md)** - Main guide explaining the project's purpose, approach, and functional DDD concepts
- **[TODO List](doc/todo.md)** - Structured learning path with 4 progressive phases
- **[Setup Guide](docs/setup.md)** - Development environment setup instructions
- **[Development Workflow](CLAUDE.md)** - Guidelines for development with Claude Code

## Quick Start

### Prerequisites

- .NET 8 SDK ([Installation guide](docs/setup.md))
- Make (for task automation)

### Setup

```bash
# Clone the repository
git clone <repository-url>
cd functional-ddd-mahjong

# Restore tools
dotnet tool restore

# Build the project
make build

# Run tests
make test
```

### Development Commands

```bash
make help     # Show all available commands
make build    # Build the solution
make test     # Run all tests
make lint     # Check code formatting
make format   # Auto-format code
make check    # Run all pre-commit checks
make watch    # Run tests in watch mode
make clean    # Clean build artifacts
```

### Project Structure

```
functional-ddd-mahjong/
├── src/
│   └── FunctionalDddMahjong.Domain/      # Domain logic
├── tests/
│   └── FunctionalDddMahjong.Domain.Tests/ # Unit tests
├── doc/                                   # Learning documentation
│   ├── guide.md                          # Main learning guide
│   └── todo.md                           # Learning progress TODO
├── docs/                                  # Technical documentation
│   └── setup.md                          # Setup instructions
└── CLAUDE.md                             # Claude Code workflow
```

## Learning Path

The project is structured in 4 phases, each building on functional DDD concepts:

1. **Phase 1**: Tile types and Hand management - Type safety and domain expression
2. **Phase 2**: Meld detection and winning judgment - Function composition
3. **Phase 3**: Yaku detection - Railway-Oriented Programming
4. **Phase 4**: Score calculation - Workflows and domain events

See [todo.md](doc/todo.md) for detailed tasks and progress tracking.

## Development Workflow

Before committing any changes:

```bash
make check  # Runs build, tests, and lint
```

This ensures code quality and all tests pass. See [CLAUDE.md](CLAUDE.md) for detailed development guidelines.

## Technologies

- **Language**: F# 8
- **Framework**: .NET 8
- **Testing**: xUnit
- **Formatter**: Fantomas
- **Build**: MSBuild + Makefile

## Domain Simplifications

This learning project uses a simplified version of Japanese Mahjong:
- Basic melds only (Chow, Pung, Kong)
- Standard yaku patterns
- Simplified scoring
- No special hands (7 pairs, 13 orphans)
- No game state dependencies

For complete details, see the [learning guide](doc/guide.md).

## Contributing

This is a learning project. The focus is on understanding functional DDD concepts rather than implementing a complete Mahjong game.

## License

This project is for educational purposes.