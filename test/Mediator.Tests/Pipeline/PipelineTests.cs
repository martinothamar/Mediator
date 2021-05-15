using Mediator.Tests.TestTypes;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Mediator.Tests.Pipeline
{
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

            var pipelineStep = sp.GetServices<IPipelineBehavior<SomeRequest, SomeResponse>>().Single(s => s is SomePipeline) as SomePipeline;
            Assert.NotNull(pipelineStep);
            var response = await mediator.Send(new SomeRequest(id));
            Assert.Equal(id, response.Id);
            Assert.Equal(id, pipelineStep!.Id);
        }

        [Fact]
        public async Task Test_Generic_Pipeline()
        {
            var (sp, mediator) = Fixture.GetMediator(services =>
            {
                services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(GenericPipeline<,>));
            });

            var requestId = Guid.NewGuid();
            var commandId = Guid.NewGuid();
            var queryId = Guid.NewGuid();

            var requestPipelineStep = sp.GetServices<IPipelineBehavior<SomeRequest, SomeResponse>>().Single(s => s is GenericPipeline<SomeRequest, SomeResponse>) as GenericPipeline<SomeRequest, SomeResponse>;
            var commandPipelineStep = sp.GetServices<IPipelineBehavior<SomeCommand, SomeResponse>>().Single(s => s is GenericPipeline<SomeCommand, SomeResponse>) as GenericPipeline<SomeCommand, SomeResponse>;
            var queryPipelineStep = sp.GetServices<IPipelineBehavior<SomeQuery, SomeResponse>>().Single(s => s is GenericPipeline<SomeQuery, SomeResponse>) as GenericPipeline<SomeQuery, SomeResponse>;

            Assert.NotNull(requestPipelineStep);
            Assert.NotNull(commandPipelineStep);
            Assert.NotNull(queryPipelineStep);

            for (int i = 0; i < 3; i++)
            {
                var response = await mediator.Send(new SomeRequest(requestId));
                Assert.Equal(requestId, response.Id);
                Assert.Equal(requestId, requestPipelineStep!.Id);

                response = await mediator.Send(new SomeCommand(commandId));
                Assert.Equal(commandId, response.Id);
                Assert.Equal(commandId, commandPipelineStep!.Id);

                response = await mediator.Send(new SomeQuery(queryId));
                Assert.Equal(queryId, response.Id);
                Assert.Equal(queryId, queryPipelineStep!.Id);
            }
        }

        [Fact]
        public async Task Test_Pipeline_Ordering()
        {
            var (sp, mediator) = Fixture.GetMediator(services =>
            {
                services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(GenericPipeline<,>));
                services.AddSingleton<IPipelineBehavior<SomeRequest, SomeResponse>, SomePipeline>();
            });

            var id = Guid.NewGuid();

            var response = await mediator.Send(new SomeRequest(id));

            var pipelineSteps = sp.GetServices<IPipelineBehavior<SomeRequest, SomeResponse>>().Cast<IPipelineTestData>();

            var original = pipelineSteps.Select(p => p.LastMsgTimestamp).ToArray();
            var ordered = pipelineSteps.Select(p => p.LastMsgTimestamp).OrderBy(x => x).ToArray();
            Assert.True(original.SequenceEqual(ordered));
        }
    }
}
