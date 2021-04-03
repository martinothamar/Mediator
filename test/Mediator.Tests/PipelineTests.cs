using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Mediator.Tests
{
    public sealed class SomePipeline : IPipelineBehavior<SomeRequest, SomeResponse>
    {
        internal Guid Id;

        public ValueTask<SomeResponse> Handle(SomeRequest request, CancellationToken cancellationToken, MessageHandlerDelegate<SomeRequest, SomeResponse> next)
        {
            if (request is null || request.Id == default)
                throw new ArgumentException("Invalid input");

            Id = request.Id;

            return next(request!, cancellationToken);
        }
    }

    public sealed class PipelineTests
    {
        [Fact]
        public async Task Test_Pipeline()
        {
            var (sp, mediator) = Fixture.GetMediator(services => services.AddSingleton<IPipelineBehavior<SomeRequest, SomeResponse>, SomePipeline>());

            var id = Guid.NewGuid();

            var pipelineStep = sp.GetServices<IPipelineBehavior<SomeRequest, SomeResponse>>().Single(s => s is SomePipeline) as SomePipeline;
            Assert.NotNull(pipelineStep);
            var response = await mediator.Send(new SomeRequest(id));
            Assert.Equal(id, response.Id);
            Assert.Equal(id, pipelineStep!.Id);
        }
    }
}
