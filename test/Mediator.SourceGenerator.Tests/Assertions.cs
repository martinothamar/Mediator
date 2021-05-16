using Xunit;

namespace Mediator.SourceGenerator.Tests
{
    public static class Assertions
    {
        public static void NoMediatorImplGenerated(GeneratorResult result)
        {
            Assert.Single(result.RunResult.GeneratedTrees);
        }

        public static void NoDiagnostics(GeneratorResult result)
        {
            Assert.True(result.Diagnostics.IsEmpty);
            Assert.True(result.RunResult.Diagnostics.IsEmpty);
        }
    }
}
