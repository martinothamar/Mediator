# Contributing to Mediator

Thank you for your interest in contributing to Mediator! This guide will help you get started with building, testing, and contributing to the project.

## Table of Contents

- [Contributing to Mediator](#contributing-to-mediator)
  - [Table of Contents](#table-of-contents)
  - [Prerequisites](#prerequisites)
    - [Required](#required)
    - [Optional](#optional)
  - [Getting Started](#getting-started)
  - [Building the Project](#building-the-project)
  - [Testing](#testing)
    - [Quick Local Testing](#quick-local-testing)
    - [Full Test Suite](#full-test-suite)
    - [GitHub Actions Testing](#github-actions-testing)
    - [Test Projects](#test-projects)
  - [Project Structure](#project-structure)
  - [Code Style](#code-style)
  - [Questions or Issues?](#questions-or-issues)

## Prerequisites

### Required

- **.NET SDK 10.0.100+** (specified in `global.json`)
  - If you want to run all tests through `just test`, you would need 8.0 and 9.0 as well currently
  - The GitHub Actions workflows will handle testing across all supported frameworks (net8.0, net9.0, net10.0)

### Optional

- **[just](https://github.com/casey/just)** - Command runner for convenient build and test operations

## Getting Started

0. (Optional) Comment on the related issue you want to work on
1. Fork the repository
2. Make a branch
3. Make your changes
4. Push the branch
5. Make a pull request

- There is a [**good first issue**](https://github.com/martinothamar/Mediator/labels/good%20first%20issue) label for open issues

## Building the Project

```bash
# Builds the whole solution
dotnet build
```

## Testing

### Quick Local Testing

For most development work, you can simply run:

```bash
dotnet test
```

The GitHub Actions workflows will ensure comprehensive testing across all supported frameworks and Mediator configurations.
When making changes in in source generation, it might be most convenient to test changes using sample project(s) or only the `Mediator.SourceGenerator.Tests` project
where you exercise only your specific changes (and then run the full test suite when you are done).

### Full Test Suite

If you have the `just` command runner installed and want to run the full test suite locally (mimics CI):

```bash
# Run all tests (source generator, memory allocation, and full test matrix)
just test

# Test specific .NET framework
just test-framework net10.0
just test-framework net9.0
just test-framework net8.0

# Test specific configuration dimensions
just test-lifetime Singleton
just test-lifetime Scoped
just test-lifetime Transient

just test-publisher ForeachAwait
just test-publisher TaskWhenAll

# Test specific configuration combination
just test-config net10.0 Singleton ForeachAwait Default Eager
```

See the `Justfile` for more info

### GitHub Actions Testing

The project uses GitHub Actions for continuous integration:

1. **Quick PR Validation** (automatic)
   - Triggers automatically on pull requests
   - Runs all tests on a subset of configurations

2. **Full Test Matrix** (manual trigger)
   - A maintainer with write permissions will trigger the full test suite by commenting `/test-full` on your PR
   - Runs tests across all .NET versions (8.0, 9.0, 10.0) and Mediator configuration options

### Test Projects

The solution includes three test projects:

1. **Mediator.Tests** - Main "integration" tests
   - Located in `test/Mediator.Tests/`

2. **Mediator.SourceGenerator.Tests** - Source generator tests
   - Located in `test/Mediator.SourceGenerator.Tests/`
   - Run with `just test-sourcegen`

3. **Mediator.MemAllocationTests** - Memory allocation tests
   - Located in `test/Mediator.MemAllocationTests/`
   - Run with `just test-memory`

## Project Structure

```
Mediator/
├── src/
│   ├── Mediator/                    # Main library
│   └── Mediator.SourceGenerator/    # Source generator
├── test/
│   ├── Mediator.Tests/
│   ├── Mediator.SourceGenerator.Tests/
│   ├── Mediator.MemAllocationTests/
│   └── Mediator.Usings.Tests/
├── samples/                          # Example projects
├── benchmarks/                       # Performance benchmarks
├── Justfile                         # Build automation
└── Mediator.slnx                    # Solution file
```

## Code Style

The project enforces consistent code style:

- **Formatting**: [CSharpier](https://csharpier.com/) is used for code formatting, it is automated on build
- **Editor Configuration**: `.editorconfig` defines coding conventions

## Questions or Issues?

Check out [GitHub Issues](https://github.com/martinothamar/Mediator/issues) or [discussions](https://github.com/martinothamar/Mediator/discussions)

Thank you for contributing to Mediator!
