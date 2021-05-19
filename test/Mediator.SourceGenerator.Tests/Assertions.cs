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
    }
}
