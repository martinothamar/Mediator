namespace Mediator.SourceGenerator.Tests;

using System.Threading.Tasks;
using Verifier = CSharpSourceGeneratorVerifier<MediatorGenerator>;

public sealed class SnapshotTests
{
    [Fact]
    public async Task Test_Multiple_AddMediator_Calls()
    {
        var source = await Fixture.SourceFromResourceFile("MultipleAddMediatorCalls.cs");

        await Verifier.VerifySolution(source);
        await Fixture.VerifyGenerator(source);
    }

    // [Fact]
    // public async Task Test_Pipeline_And_AttributeNamespace()
    // {
    //     var source = await Fixture.SourceFromResourceFile("SimpleConsoleAOT.cs");

    //     await Fixture.VerifyGenerator(source);
    // }

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
