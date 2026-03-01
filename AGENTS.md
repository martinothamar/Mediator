# AGENTS.md

This file provides guidance to AI agents when working with code in this repository.

## Mediator

This is a high performance .NET implementation of the Mediator pattern using source generators.
It provides a similar API to the great MediatR library while delivering better performance and full Native AOT support.

Goals for this library
* High performance
  * Efficient and fast by default, slower configurations are opt-in (in the spirit of non-pessimization)
* AOT friendly
  * Full Native AOT support without reflection or runtime code generation
  * Cold start performance matters for lots of scenarios (serverless, edge, apps, mobile)
* Build time errors instead of runtime errors
  * The generator includes diagnostics (example: if a handler is not defined for a request, a warning is emitted)
  * Catch configuration mistakes during development, not during runtime
* Stability
  * Stable API that only changes for good reason - fewer changes means less patching for users
  * Follows semver 2.0 strictly

## Development

- Build: `dotnet build` (formatting runs on build, using csharpier)
- Tests:
  - Run targeted tests, not all tests at once
  - Example using `dotnet test`: `dotnet test -f net10.0 ./test/Mediator.Tests/` (main integration tests for .NET 10)
  - Example using `just` (see @Justfile): `just test-sourcegen` (sourcegeneration tests)

For performance-sensitive changes, tell the user that benchmarks need to be run.
