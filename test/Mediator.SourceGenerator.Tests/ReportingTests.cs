using Microsoft.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Mediator.SourceGenerator.Tests
{
    public sealed class ReportingTests
    {
        [Fact]
        public void Test_Empty_Program()
        {
            var inputCompilation = Fixture.CreateCompilation(@"
namespace MyCode
{
    public class Program
    {
        public static void Main(string[] args)
        {
        }
    }
}
");

            inputCompilation.AssertGen(
                Assertions.CompilesWithoutDiagnostics,
                result =>
                {
                    Assert.True(result.OutputCompilation.SyntaxTrees.Count() == 3); // Original + attribute + mediator impl
                    Assert.True(result.RunResult.GeneratedTrees.Length == 2); // attribute + mediator impl
                }
            );
        }

        [Fact]
        public async Task Test_Deep_Namespace_Program()
        {
            var source = await Fixture.SourceFromResourceFile("DeepNamespaceProgram.cs");
            var inputCompilation = Fixture.CreateCompilation(source);

            inputCompilation.AssertGen(
                Assertions.CompilesWithoutDiagnostics
            );
        }

        [Fact]
        public async Task Test_Static_Nested_Handler_Program()
        {
            var source = await Fixture.SourceFromResourceFile("StaticNestedHandlerProgram.cs");
            var inputCompilation = Fixture.CreateCompilation(source);

            inputCompilation.AssertGen(
                Assertions.CompilesWithoutDiagnostics
            );
        }

        [Fact]
        public async Task Test_Abstract_Handler_Program()
        {
            var source = await Fixture.SourceFromResourceFile("AbstractHandlerClass.cs");
            var inputCompilation = Fixture.CreateCompilation(source);

            inputCompilation.AssertGen(
                Assertions.CompilesWithoutDiagnostics
            );
        }

        [Fact]
        public async Task Test_Duplicate_Handlers()
        {
            var source = await Fixture.SourceFromResourceFile("DuplicateHandlersProgram.cs");
            var inputCompilation = Fixture.CreateCompilation(source);

            inputCompilation.AssertGen(
                Assertions.NoMediatorImplGenerated,
                result =>
                {
                    Assert.Contains(result.Diagnostics, d => d.Id == Diagnostics.MultipleHandlersError.Id);
                }
            );
        }

        [Fact]
        public async Task Test_Invalid_Handler_Type()
        {
            var source = await Fixture.SourceFromResourceFile("StructHandlerProgram.cs");
            var inputCompilation = Fixture.CreateCompilation(source);

            inputCompilation.AssertGen(
                Assertions.NoMediatorImplGenerated,
                result =>
                {
                    Assert.Contains(result.Diagnostics, d => d.Id == Diagnostics.InvalidHandlerTypeError.Id);
                }
            );
        }

        [Fact]
        public async Task Test_Multiple_Errors()
        {
            var source = await Fixture.SourceFromResourceFile("MultipleErrorsProgram.cs");
            var inputCompilation = Fixture.CreateCompilation(source);

            inputCompilation.AssertGen(
                Assertions.NoMediatorImplGenerated,
                result =>
                {
                    Assert.Contains(result.Diagnostics, d => d.Id == Diagnostics.MultipleHandlersError.Id);
                    Assert.Contains(result.Diagnostics, d => d.Id == Diagnostics.InvalidHandlerTypeError.Id);
                }
            );
        }
    }
}
