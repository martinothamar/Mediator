using System;
using System.Linq;
using System.Threading.Tasks;
using Mediator.Tests.TestTypes;
using Microsoft.Extensions.DependencyInjection;

namespace Mediator.Tests.Pipeline;

public sealed class PipelineTests
{
    [Fact]
    public async Task Test_Pipeline()
    {
        var (sp, mediator) = Fixture.GetMediator(services =>
        {
            services.AddSingleton<IPipelineBehavior<SomeRequest, SomeResponse>, SomePipeline>();
        });

        var id = Guid.NewGuid();

        var pipelineStep =
            sp.GetServices<IPipelineBehavior<SomeRequest, SomeResponse>>().Single(s => s is SomePipeline)
            as SomePipeline;
        Assert.NotNull(pipelineStep);
        var response = await mediator.Send(new SomeRequest(id), TestContext.Current.CancellationToken);
        Assert.Equal(id, response.Id);
        Assert.Equal(id, pipelineStep!.Id);
    }

    [Fact]
    public async Task Test_Pipeline_Early_Return()
    {
        var (sp, mediator) = Fixture.GetMediator(services =>
        {
            services.AddSingleton<IPipelineBehavior<SomeRequest, SomeResponse>>(new SomePipeline(earlyReturn: true));
        });

        var id = Guid.NewGuid();

        var pipelineStep =
            sp.GetServices<IPipelineBehavior<SomeRequest, SomeResponse>>().Single(s => s is SomePipeline)
            as SomePipeline;
        Assert.NotNull(pipelineStep);
        var response = await mediator.Send(new SomeRequest(id), TestContext.Current.CancellationToken);
        Assert.Equal(id, response.Id);
        Assert.True(response.ReturnedEarly);
        Assert.NotEqual(id, pipelineStep!.Id);
    }

    [Fact]
    public async Task Test_Generic_Pipeline()
    {
        var ct = TestContext.Current.CancellationToken;
        var (sp, mediator) = Fixture.GetMediator(services =>
        {
            services.AddSingleton<GenericPipelineState>();
            services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(GenericPipeline<,>));
        });

        var request = new SomeRequest(Guid.NewGuid());
        var requestWithoutResponse = new SomeRequestWithoutResponse(Guid.NewGuid());
        var command = new SomeCommand(Guid.NewGuid());
        var commandWithoutResponse = new SomeCommandWithoutResponse(Guid.NewGuid());
        var query = new SomeQuery(Guid.NewGuid());

        var pipelineState = sp.GetRequiredService<GenericPipelineState>();

        Assert.Equal(default, pipelineState.Id);
        Assert.Equal(default, pipelineState.Message);

        _ = await mediator.Send(request, ct);
        Assert.Equal(request.Id, pipelineState.Id);
        Assert.Equal(request, pipelineState.Message);

        await mediator.Send(requestWithoutResponse, ct);
        Assert.Equal(requestWithoutResponse.Id, pipelineState.Id);
        Assert.Equal(requestWithoutResponse, pipelineState.Message);

        _ = await mediator.Send(command, ct);
        Assert.Equal(command.Id, pipelineState.Id);
        Assert.Equal(command, pipelineState.Message);

        await mediator.Send(commandWithoutResponse, ct);
        Assert.Equal(commandWithoutResponse.Id, pipelineState.Id);
        Assert.Equal(commandWithoutResponse, pipelineState.Message);

        _ = await mediator.Send(query, ct);
        Assert.Equal(query.Id, pipelineState.Id);
        Assert.Equal(query, pipelineState.Message);
    }

    [Fact]
    public async Task Test_Command_Specific_Pipeline()
    {
        var ct = TestContext.Current.CancellationToken;
        var (sp, mediator) = Fixture.GetMediator(services =>
        {
            services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(CommandSpecificPipeline<,>));
        });

        var id = Guid.NewGuid();

        var response = await mediator.Send(new SomeCommand(id), ct);
        Assert.NotNull(response);
        Assert.Equal(id, response.Id);
        Assert.Equal(1, CommandSpecificPipeline<SomeCommand, SomeResponse>.CallCount);

        await mediator.Send(new SomeCommandWithoutResponse(id), ct);
        Assert.Equal(1, CommandSpecificPipeline<SomeCommandWithoutResponse, Unit>.CallCount);
    }

    [Fact]
    public async Task Test_Pipeline_Ordering()
    {
        var (sp, mediator) = Fixture.GetMediator(services =>
        {
            services.AddSingleton<GenericPipelineState>();
            services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(GenericPipeline<,>));
            services.AddSingleton<IPipelineBehavior<SomeRequest, SomeResponse>, SomePipeline>();
        });

        var id = Guid.NewGuid();

        var response = await mediator.Send(new SomeRequest(id), TestContext.Current.CancellationToken);

        var pipelineSteps = sp.GetServices<IPipelineBehavior<SomeRequest, SomeResponse>>().Cast<IPipelineTestData>();

        var original = pipelineSteps.Select(p => p.LastMsgTimestamp).ToArray();
        var ordered = pipelineSteps.Select(p => p.LastMsgTimestamp).OrderBy(x => x).ToArray();
        Assert.True(original.SequenceEqual(ordered));
    }
}
