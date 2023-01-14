using Mediator.Tests.TestTypes;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Mediator.Tests.Pipeline.PreProcessing;

public class ConcreteTests
{
    [Fact]
    public async Task Test_PreProcessor()
    {
        var (sp, mediator) = Fixture.GetMediator(
            services =>
            {
                services.AddSingleton<PreProcessingState>();
                services.AddSingleton<IPipelineBehavior<SomeRequest, SomeResponse>, TestMessagePreProcessor>();
                services.AddSingleton<IPipelineBehavior<SomeRequest, SomeResponse>, TestMessagePreProcessor2>();
            }
        );

        var id = Guid.NewGuid();
        var queryId = Guid.NewGuid();

        var state = sp.GetRequiredService<PreProcessingState>();

        Assert.NotNull(state);
        var response = await mediator.Send(new SomeRequest(id));
        Assert.Equal(id, response.Id);
        Assert.Equal(id, state!.Id);
        Assert.Equal(2, state.Calls);

        response = await mediator.Send(new SomeQuery(queryId));
        Assert.Equal(queryId, response.Id);
        Assert.Equal(id, state!.Id);
        Assert.Equal(2, state.Calls);
    }

    private sealed class PreProcessingState
    {
        public Guid Id;
        public int Calls;
    }

    private sealed class TestMessagePreProcessor : MessagePreProcessor<SomeRequest, SomeResponse>
    {
        private readonly PreProcessingState _state;

        public TestMessagePreProcessor(PreProcessingState state) => _state = state;

        protected override ValueTask Handle(SomeRequest message, CancellationToken cancellationToken)
        {
            _state.Id = message.Id;
            _state.Calls++;
            return default;
        }
    }

    private sealed class TestMessagePreProcessor2 : MessagePreProcessor<SomeRequest, SomeResponse>
    {
        private readonly PreProcessingState _state;

        public TestMessagePreProcessor2(PreProcessingState state) => _state = state;

        protected override ValueTask Handle(SomeRequest message, CancellationToken cancellationToken)
        {
            _state.Id = message.Id;
            _state.Calls++;
            return default;
        }
    }
}
