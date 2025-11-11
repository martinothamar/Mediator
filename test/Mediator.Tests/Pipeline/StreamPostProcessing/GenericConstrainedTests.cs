using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Mediator.Tests.TestTypes;
using Microsoft.Extensions.DependencyInjection;

namespace Mediator.Tests.Pipeline.StreamPostProcessing;

public class GenericConstrainedTests
{
    [Fact]
    public async Task Test_StreamPostProcessor()
    {
        var ct = TestContext.Current.CancellationToken;
        var (sp, mediator) = Fixture.GetMediator(services =>
        {
            services.AddSingleton<PostProcessingState>();
            services.AddSingleton(typeof(IStreamPipelineBehavior<,>), typeof(TestStreamMessagePostProcessor<,>));
            services.AddSingleton(typeof(IStreamPipelineBehavior<,>), typeof(TestStreamMessagePostProcessor2<,>));
        });

        var id = Guid.NewGuid();
        var queryId = Guid.NewGuid();

        var state = sp.GetRequiredService<PostProcessingState>();

        Assert.NotNull(state);

        int counter = 0;
        await foreach (var response in mediator.CreateStream(new SomeStreamingRequest(id), ct))
        {
            Assert.Equal(id, response.Id);
            counter++;
        }
        Assert.Equal(3, counter);
        Assert.Equal(id, state!.Id);
        Assert.Equal(2, state.Calls); // Called once per stream * 2 processors

        counter = 0;
        await foreach (var response in mediator.CreateStream(new SomeStreamingQuery(queryId), ct))
        {
            Assert.Equal(queryId, response.Id);
            counter++;
        }
        Assert.Equal(3, counter);
        Assert.Equal(id, state!.Id); // Still the old id, query doesn't match the constraint
        Assert.Equal(2, state.Calls); // Still 2, query doesn't match the constraint

        counter = 0;
        await foreach (var response in mediator.CreateStream(new SomeStreamingRequest(id), ct))
        {
            Assert.Equal(id, response.Id);
            counter++;
        }
        Assert.Equal(3, counter);
        Assert.Equal(id, state!.Id);
        Assert.Equal(4, state.Calls); // 2 + 2
    }

    private sealed class PostProcessingState
    {
        public Guid Id;
        public int Calls;
    }

    private sealed class TestStreamMessagePostProcessor<TMessage, TResponse>
        : StreamMessagePostProcessor<TMessage, TResponse>
        where TMessage : notnull, IStreamRequest<TResponse>
    {
        private readonly PostProcessingState _state;

        public TestStreamMessagePostProcessor(PostProcessingState state) => _state = state;

        protected override ValueTask Handle(
            TMessage message,
            IReadOnlyList<TResponse> responses,
            CancellationToken cancellationToken
        )
        {
            _state.Id = (Guid)typeof(TMessage).GetProperty("Id")!.GetValue(message)!;
            _state.Calls++;
            return default;
        }
    }

    private sealed class TestStreamMessagePostProcessor2<TMessage, TResponse>
        : StreamMessagePostProcessor<TMessage, TResponse>
        where TMessage : notnull, IStreamRequest<TResponse>
    {
        private readonly PostProcessingState _state;

        public TestStreamMessagePostProcessor2(PostProcessingState state) => _state = state;

        protected override ValueTask Handle(
            TMessage message,
            IReadOnlyList<TResponse> responses,
            CancellationToken cancellationToken
        )
        {
            _state.Id = (Guid)typeof(TMessage).GetProperty("Id")!.GetValue(message)!;
            _state.Calls++;
            return default;
        }
    }
}
