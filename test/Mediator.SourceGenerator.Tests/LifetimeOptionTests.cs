namespace Mediator.SourceGenerator.Tests;

public sealed class LifetimeOptionTests
{
    [Fact]
    public void Test_No_Args()
    {
        var source =
            @"
using Mediator;
using Microsoft.Extensions.DependencyInjection;

[assembly: MediatorOptions]

namespace Something
{
    public static class Program
    {
        public static void Main()
        {
        }
    }
}
";
        var inputCompilation = Fixture.CreateLibrary(source);

        inputCompilation.AssertGen(
            Assertions.CompilesWithoutDiagnostics,
            result =>
            {
                var analyzer = result.Generator.CompilationAnalyzer;
                Assert.True(analyzer?.ServiceLifetimeIsSingleton);
                Assert.Equal("Mediator", analyzer?.MediatorNamespace);
            }
        );
    }

    [Fact]
    public void Test_Transient_Lifetime_With_Named_Namespace_Arg()
    {
        var source =
            @"
using Mediator;
using Microsoft.Extensions.DependencyInjection;

[assembly: MediatorOptions(Namespace = ""Mediator2"", ServiceLifetime = ServiceLifetime.Transient)]

namespace Something
{
    public static class Program
    {
        public static void Main()
        {
        }
    }
}
";
        var inputCompilation = Fixture.CreateLibrary(source);

        inputCompilation.AssertGen(
            Assertions.CompilesWithoutDiagnostics,
            result =>
            {
                var analyzer = result.Generator.CompilationAnalyzer;
                Assert.True(analyzer?.ServiceLifetimeIsTransient);
                Assert.Equal("Mediator2", analyzer?.MediatorNamespace);
            }
        );
    }
}
