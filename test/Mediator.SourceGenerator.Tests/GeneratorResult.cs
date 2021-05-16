using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Mediator.SourceGenerator.Tests
{
    public sealed record GeneratorResult(ImmutableArray<Diagnostic> Diagnostics, GeneratorDriverRunResult RunResult, Compilation OutputCompilation);
}
