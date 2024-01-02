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

    [Fact]
    public async Task Test_Pipeline_And_AttributeNamespace()
    {
        var source = await Fixture.SourceFromResourceFile("SimpleConsoleAOT.cs");

        await Fixture.VerifyGenerator(source);
    }

    [Fact]
    public async Task Test_Message_With_Missing_Handler_Should_Diagnostic()
    {
        var source = """
                     using Mediator;
                     using Microsoft.Extensions.DependencyInjection;
                     using System;

                     var services = new ServiceCollection();

                     services.AddMediator();

                     public sealed record Ping(Guid Id) : IRequest<Pong>;

                     public sealed record Pong(Guid Id);
                     """;

        await Fixture.VerifyGenerator(source);
    }
}
