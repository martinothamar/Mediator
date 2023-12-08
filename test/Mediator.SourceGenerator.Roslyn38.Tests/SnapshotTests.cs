namespace Mediator.SourceGenerator.Tests;

using System.Threading.Tasks;
using VerifyXunit;
using Verifier = CSharpSourceGeneratorVerifier<MediatorGenerator>;

[UsesVerify]
public sealed class SnapshotTests
{
    [Fact]
    public async Task Test_SimpleConsole()
    {
        var source = await Fixture.SourceFromResourceFile("SimpleConsoleProgram.cs");

        await Verifier.VerifySolution(source);
        await Fixture.VerifyGenerator(source);
    }

    [Fact]
    public async Task Test_SimpleEndToEnd()
    {
        var source = await Fixture.SourceFromResourceFile("SimpleEndToEndProgram.cs");

        await Verifier.VerifySolution(source);
        await Fixture.VerifyGenerator(source);
    }

    [Fact]
    public async Task Test_SimpleStreaming()
    {
        var source = await Fixture.SourceFromResourceFile("SimpleStreamingProgram.cs");

        await Verifier.VerifySolution(source);
        await Fixture.VerifyGenerator(source);
    }

    [Fact]
    public async Task Test_Multiple_AddMediator_Calls()
    {
        var source = await Fixture.SourceFromResourceFile("MultipleAddMediatorCalls.cs");

        await Verifier.VerifySolution(source);
        await Fixture.VerifyGenerator(source);
    }
}
