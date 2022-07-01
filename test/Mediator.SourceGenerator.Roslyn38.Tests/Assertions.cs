using Microsoft.CodeAnalysis;
using System.Linq;

namespace Mediator.SourceGenerator.Tests;

public static class Assertions
{
    public static void NoMediatorImplGenerated(GeneratorResult result)
    {
        Assert.Single(result.RunResult.GeneratedTrees);
    }

    public static void AssertCommon(GeneratorResult result)
    {
        var analyzer = result.Generator.CompilationAnalyzer;

        Assert.NotNull(analyzer);
        Assert.True(
            analyzer!.ServiceLifetimeIsSingleton
                || analyzer.ServiceLifetimeIsScoped
                || analyzer.ServiceLifetimeIsTransient
        );
    }

    public static void CompilesWithoutDiagnostics(GeneratorResult result)
    {
        AssertCommon(result);

        Assert.True(result.Diagnostics.IsEmpty);
        Assert.True(result.RunResult.Diagnostics.IsEmpty);

        var outputCompilationDiagnostics = result.OutputCompilation.GetDiagnostics();
        Assert.Empty(outputCompilationDiagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
    }

    public static void CompilesWithoutErrorDiagnostics(GeneratorResult result)
    {
        AssertCommon(result);

        Assert.Empty(result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Empty(result.RunResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));

        var outputCompilationDiagnostics = result.OutputCompilation.GetDiagnostics();
        Assert.Empty(outputCompilationDiagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
    }
}
