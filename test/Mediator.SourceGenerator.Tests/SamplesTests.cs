using System.Threading.Tasks;
using Xunit;

namespace Mediator.SourceGenerator.Tests
{
    public sealed class SamplesTests
    {
        [Fact]
        public async Task Test_SimpleConsole()
        {
            var source = await Fixture.SourceFromResourceFile("SimpleConsoleProgram.cs");
            var inputCompilation = Fixture.CreateCompilation(source);

            inputCompilation.AssertGen(Assertions.NoDiagnostics);
        }
    }
}
