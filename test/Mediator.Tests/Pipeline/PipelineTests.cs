using Mediator.Tests.TestTypes;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Mediator.Tests.Pipeline;

public sealed class PipelineTests
{
    [Fact]
    public async Task Test_Pipeline()
    {
        var (sp, mediator) = Fixture.GetMediator(
            services =>
            {
                services.AddSingleton<IPipelineBehavior<SomeRequest, SomeResponse>, SomePipeline>();
            }
        );

        var id = Guid.NewGuid();

        var pipelineStep =
            sp.GetServices<IPipelineBehavior<SomeRequest, SomeResponse>>().Single(s => s is SomePipeline)
            as SomePipeline;
        Assert.NotNull(pipelineStep);
        var response = await mediator.Send(new SomeRequest(id));
        Assert.Equal(id, response.Id);
        Assert.Equal(id, pipelineStep!.Id);
    }

    [Fact]
    public async Task Test_Generic_Pipeline()
    {
        var (sp, mediator) = Fixture.GetMediator(
            services =>
            {
                services.AddSingleton<GenericPipelineState>();
                services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(GenericPipeline<,>));
            }
        );

        var request = new SomeRequest(Guid.NewGuid());
        var requestWithoutResponse = new SomeRequestWithoutResponse(Guid.NewGuid());
        var command = new SomeCommand(Guid.NewGuid());
        var commandWithoutResponse = new SomeCommandWithoutResponse(Guid.NewGuid());
        var query = new SomeQuery(Guid.NewGuid());

        var pipelineState = sp.GetRequiredService<GenericPipelineState>();

        Assert.Equal(default, pipelineState.Id);
        Assert.Equal(default, pipelineState.Message);

        _ = await mediator.Send(request);
        Assert.Equal(request.Id, pipelineState.Id);
        Assert.Equal(request, pipelineState.Message);

        await mediator.Send(requestWithoutResponse);
        Assert.Equal(requestWithoutResponse.Id, pipelineState.Id);
        Assert.Equal(requestWithoutResponse, pipelineState.Message);

        _ = await mediator.Send(command);
        Assert.Equal(command.Id, pipelineState.Id);
        Assert.Equal(command, pipelineState.Message);

        await mediator.Send(commandWithoutResponse);
        Assert.Equal(commandWithoutResponse.Id, pipelineState.Id);
        Assert.Equal(commandWithoutResponse, pipelineState.Message);

        _ = await mediator.Send(query);
        Assert.Equal(query.Id, pipelineState.Id);
        Assert.Equal(query, pipelineState.Message);
    }

    [Fact]
    public async Task Test_Command_Specific_Pipeline()
    {
        var (sp, mediator) = Fixture.GetMediator(
            services =>
            {
                services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(CommandSpecificPipeline<,>));
            }
        );

        var id = Guid.NewGuid();

        var response = await mediator.Send(new SomeCommand(id));
        Assert.NotNull(response);
        Assert.Equal(id, response.Id);
        Assert.Equal(1, CommandSpecificPipeline<SomeCommand, SomeResponse>.CallCount);

        await mediator.Send(new SomeCommandWithoutResponse(id));
        Assert.Equal(1, CommandSpecificPipeline<SomeCommandWithoutResponse, Unit>.CallCount);
    }

    [Fact]
    public async Task Test_Pipeline_Ordering()
    {
        var (sp, mediator) = Fixture.GetMediator(
            services =>
            {
                services.AddSingleton<GenericPipelineState>();
                services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(GenericPipeline<,>));
                services.AddSingleton<IPipelineBehavior<SomeRequest, SomeResponse>, SomePipeline>();
            }
        );

        var id = Guid.NewGuid();

        var response = await mediator.Send(new SomeRequest(id));

        var pipelineSteps = sp.GetServices<IPipelineBehavior<SomeRequest, SomeResponse>>().Cast<IPipelineTestData>();

        var original = pipelineSteps.Select(p => p.LastMsgTimestamp).ToArray();
        var ordered = pipelineSteps.Select(p => p.LastMsgTimestamp).OrderBy(x => x).ToArray();
        Assert.True(original.SequenceEqual(ordered));
    }
}
