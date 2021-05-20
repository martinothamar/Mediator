using Microsoft.CodeAnalysis;
using System.Linq;
using Xunit;

namespace Mediator.SourceGenerator.Tests
{
    public static class Assertions
    {
        public static void NoMediatorImplGenerated(GeneratorResult result)
        {
            Assert.Single(result.RunResult.GeneratedTrees);
        }

        public static void CompilesWithoutDiagnostics(GeneratorResult result)
        {
            Assert.True(result.Diagnostics.IsEmpty);
            Assert.True(result.RunResult.Diagnostics.IsEmpty);

            var outputCompilationDiagnostics = result.OutputCompilation.GetDiagnostics();
            Assert.True(outputCompilationDiagnostics.IsEmpty);
        }

        public static void CompilesWithoutErrorDiagnostics(GeneratorResult result)
        {
            Assert.Empty(result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
            Assert.Empty(result.RunResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));

            var outputCompilationDiagnostics = result.OutputCompilation.GetDiagnostics();
            Assert.Empty(outputCompilationDiagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        }
    }
}
