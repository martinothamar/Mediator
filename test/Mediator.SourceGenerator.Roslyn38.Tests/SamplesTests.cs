using System.Threading.Tasks;

namespace Mediator.SourceGenerator.Tests;

using Microsoft.CodeAnalysis.Testing;
using Verifier = CSharpSourceGeneratorVerifier<MediatorGenerator>;

public sealed class SamplesTests
{
    [Fact]
    public async Task Test_SimpleConsole()
    {
        var source = await Fixture.SourceFromResourceFile("SimpleConsoleProgram.cs");

        var tester = new Verifier.Test
        {
            TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck,
            TestState = { Sources = { source }, },
        };
        await tester.RunAsync();
    }

    [Fact]
    public async Task Test_SimpleEndToEnd()
    {
        var source = await Fixture.SourceFromResourceFile("SimpleEndToEndProgram.cs");

        await new Verifier.Test
        {
            TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck,
            TestState = { Sources = { source }, },
        }.RunAsync();
    }

    [Fact]
    public async Task Test_SimpleStreaming()
    {
        var source = await Fixture.SourceFromResourceFile("SimpleStreamingProgram.cs");

        await new Verifier.Test
        {
            TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck,
            TestState = { Sources = { source }, },
        }.RunAsync();
    }

    [Fact]
    public async Task Test_Multiple_AddMediator_Calls()
    {
        var source = await Fixture.SourceFromResourceFile("MultipleAddMediatorCalls.cs");

        await new Verifier.Test
        {
            TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck,
            TestState = { Sources = { source }, },
        }.RunAsync();
    }
}
