using System.Threading.Tasks;
using VerifyXunit;

namespace Mediator.SourceGenerator.Tests;

[UsesVerify]
public sealed class SnapshotTests
{
    [Fact]
    public async Task Test_SimpleConsole()
    {
        var source = await Fixture.SourceFromResourceFile("SimpleConsoleProgram.cs");

        await Fixture.VerifyGenerator(source);
    }

    [Fact]
    public async Task Test_SimpleEndToEnd()
    {
        var source = await Fixture.SourceFromResourceFile("SimpleEndToEndProgram.cs");

        await Fixture.VerifyGenerator(source);
    }

    [Fact]
    public async Task Test_SimpleStreaming()
    {
        var source = await Fixture.SourceFromResourceFile("SimpleStreamingProgram.cs");

        await Fixture.VerifyGenerator(source);
    }

    [Fact]
    public async Task Test_Multiple_AddMediator_Calls()
    {
        var source = await Fixture.SourceFromResourceFile("MultipleAddMediatorCalls.cs");

        await Fixture.VerifyGenerator(source);
    }
}
