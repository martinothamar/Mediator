using System.Threading.Tasks;
using Xunit;

namespace Mediator.SourceGenerator.Tests
{
    public sealed class SamplesTests
    {
        [Fact(Skip = "Test manually, programs containing top level statements must be executable")]
        public async Task Test_SimpleConsole()
        {
            var source = await Fixture.SourceFromResourceFile("SimpleConsoleProgram.cs");
            var inputCompilation = Fixture.CreateCompilation(source);

            inputCompilation.AssertGen(Assertions.CompilesWithoutDiagnostics);
        }

        [Fact(Skip = "Test manually, programs containing top level statements must be executable")]
        public async Task Test_SimpleEndToEnd()
        {
            var source = await Fixture.SourceFromResourceFile("SimpleEndToEndProgram.cs");
            var inputCompilation = Fixture.CreateCompilation(source);

            inputCompilation.AssertGen(Assertions.CompilesWithoutDiagnostics);
        }

        [Fact(Skip = "Test manually, programs containing top level statements must be executable")]
        public async Task Test_SimpleStreaming()
        {
            var source = await Fixture.SourceFromResourceFile("SimpleStreamingProgram.cs");
            var inputCompilation = Fixture.CreateCompilation(source);

            inputCompilation.AssertGen(Assertions.CompilesWithoutDiagnostics);
        }
    }
}
