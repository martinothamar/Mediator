using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Mediator.SourceGenerator.Tests;

public sealed record GeneratorResult(
    IncrementalMediatorGenerator Generator,
    ImmutableArray<Diagnostic> Diagnostics,
    GeneratorDriverRunResult RunResult,
    Compilation OutputCompilation
);
