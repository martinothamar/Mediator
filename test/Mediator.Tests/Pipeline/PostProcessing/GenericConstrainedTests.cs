using Mediator.Tests.TestTypes;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Mediator.Tests.Pipeline.PostProcessing;

public class GenericConstrainedTests
{
    [Fact]
    public async Task Test_PostProcessor()
    {
        var (sp, mediator) = Fixture.GetMediator(
            services =>
            {
                services.AddSingleton<PostProcessingState>();
                services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(TestMessagePostProcessor<,>));
                services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(TestMessagePostProcessor2<,>));
            }
        );

        var id = Guid.NewGuid();
        var queryId = Guid.NewGuid();

        var state = sp.GetRequiredService<PostProcessingState>();

        Assert.NotNull(state);
        var response = await mediator.Send(new SomeRequest(id));
        Assert.Equal(id, response.Id);
        Assert.Equal(id, state!.Id);
        Assert.Equal(2, state.Calls);

        response = await mediator.Send(new SomeQuery(queryId));
        Assert.Equal(queryId, response.Id);
        Assert.Equal(id, state!.Id);
        Assert.Equal(2, state.Calls);

        response = await mediator.Send(new SomeRequest(id));
        Assert.Equal(id, response.Id);
        Assert.Equal(id, state!.Id);
        Assert.Equal(4, state.Calls);
    }

    private sealed class PostProcessingState
    {
        public Guid Id;
        public int Calls;
    }

    private sealed class TestMessagePostProcessor<TMessage, TResponse> : MessagePostProcessor<TMessage, TResponse>
        where TMessage : notnull, IRequest<TResponse>
    {
        private readonly PostProcessingState _state;

        public TestMessagePostProcessor(PostProcessingState state) => _state = state;

        protected override ValueTask Handle(TMessage message, TResponse response, CancellationToken cancellationToken)
        {
            _state.Id = (Guid)typeof(TMessage).GetProperty("Id")!.GetValue(message)!;
            _state.Calls++;
            return default;
        }
    }

    private sealed class TestMessagePostProcessor2<TMessage, TResponse> : MessagePostProcessor<TMessage, TResponse>
        where TMessage : notnull, IRequest<TResponse>
    {
        private readonly PostProcessingState _state;

        public TestMessagePostProcessor2(PostProcessingState state) => _state = state;

        protected override ValueTask Handle(TMessage message, TResponse response, CancellationToken cancellationToken)
        {
            _state.Id = (Guid)typeof(TMessage).GetProperty("Id")!.GetValue(message)!;
            _state.Calls++;
            return default;
        }
    }
}
