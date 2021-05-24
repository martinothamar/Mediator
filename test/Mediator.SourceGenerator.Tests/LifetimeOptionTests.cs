using Xunit;

namespace Mediator.SourceGenerator.Tests
{
    public sealed class LifetimeOptionTests
    {
        [Fact]
        public void Test_Transient_Lifetime_With_Namespace_First_Arg()
        {
            var source = @"
using Mediator;
using Microsoft.Extensions.DependencyInjection;

[assembly: MediatorOptions(""Mediator2"", DefaultServiceLifetime = ServiceLifetime.Transient)]

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
            var inputCompilation = Fixture.CreateCompilation(source);

            inputCompilation.AssertGen(
                Assertions.CompilesWithoutDiagnostics
            );
        }

        [Fact]
        public void Test_Transient_Lifetime_With_Named_Namespace_Arg()
        {
            var source = @"
using Mediator;
using Microsoft.Extensions.DependencyInjection;

[assembly: MediatorOptions(Namespace = ""Mediator2"", DefaultServiceLifetime = ServiceLifetime.Transient)]

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
            var inputCompilation = Fixture.CreateCompilation(source);

            inputCompilation.AssertGen(
                Assertions.CompilesWithoutDiagnostics
            );
        }
    }
}
