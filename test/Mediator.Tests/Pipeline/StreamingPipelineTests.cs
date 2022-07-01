using Mediator.Tests.TestTypes;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Mediator.Tests.Pipeline;

public sealed class StreamingPipelineTests
{
    [Fact]
    public async Task Test_Pipeline()
    {
        var (sp, mediator) = Fixture.GetMediator(
            services =>
            {
                services.AddSingleton<
                    IStreamPipelineBehavior<SomeStreamingQuery, SomeResponse>,
                    SomeStreamingPipeline
                >();
            }
        );

        var id = Guid.NewGuid();

        int counter = 0;
        await foreach (var response in mediator.CreateStream(new SomeStreamingQuery(id)))
        {
            Assert.Equal(id, response.Id);
            Assert.Equal(1, response.SomeStreamingData);
            counter++;
        }
    }

    [Fact]
    public async Task Test_Generic_Pipeline()
    {
        var (sp, mediator) = Fixture.GetMediator(
            services =>
            {
                services.AddSingleton<GenericPipelineState>();
                services.AddSingleton(typeof(IStreamPipelineBehavior<,>), typeof(GenericStreamPipeline<,>));
            }
        );

        var query = new SomeStreamingQuery(Guid.NewGuid());

        var pipelineState = sp.GetRequiredService<GenericPipelineState>();

        Assert.Equal(default, pipelineState.Id);
        Assert.Equal(default, pipelineState.Message);

        await foreach (var response in mediator.CreateStream(query))
        {
            Assert.Equal(query.Id, pipelineState.Id);
            Assert.Equal(query, pipelineState.Message);
        }
    }
}
