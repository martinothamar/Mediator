using System;
using System.Threading;
using System.Threading.Tasks;
using Mediator.Tests.TestTypes;
using Microsoft.Extensions.DependencyInjection;

namespace Mediator.Tests.Pipeline.StreamPreProcessing;

public class GenericConstrainedTests
{
    [Fact]
    public async Task Test_StreamPreProcessor()
    {
        var ct = TestContext.Current.CancellationToken;
        var (sp, mediator) = Fixture.GetMediator(services =>
        {
            services.AddSingleton<PreProcessingState>();
            services.AddSingleton(typeof(IStreamPipelineBehavior<,>), typeof(TestStreamMessagePreProcessor<,>));
            services.AddSingleton(typeof(IStreamPipelineBehavior<,>), typeof(TestStreamMessagePreProcessor2<,>));
        });

        var id = Guid.NewGuid();
        var queryId = Guid.NewGuid();

        var state = sp.GetRequiredService<PreProcessingState>();

        Assert.NotNull(state);

        int counter = 0;
        await foreach (var response in mediator.CreateStream(new SomeStreamingRequest(id), ct))
        {
            Assert.Equal(id, response.Id);
            counter++;
        }
        Assert.Equal(3, counter);
        Assert.Equal(id, state!.Id);
        Assert.Equal(2, state.Calls);

        counter = 0;
        await foreach (var response in mediator.CreateStream(new SomeStreamingQuery(queryId), ct))
        {
            Assert.Equal(queryId, response.Id);
            counter++;
        }
        Assert.Equal(3, counter);
        Assert.Equal(id, state!.Id); // Still the old id, query doesn't match the constraint
        Assert.Equal(2, state.Calls); // Query doesn't match the constraint

        counter = 0;
        await foreach (var response in mediator.CreateStream(new SomeStreamingRequest(id), ct))
        {
            Assert.Equal(id, response.Id);
            counter++;
        }
        Assert.Equal(3, counter);
        Assert.Equal(id, state!.Id);
        Assert.Equal(4, state.Calls);
    }

    private sealed class PreProcessingState
    {
        public Guid Id;
        public int Calls;
    }

    private sealed class TestStreamMessagePreProcessor<TMessage, TResponse>
        : StreamMessagePreProcessor<TMessage, TResponse>
        where TMessage : notnull, IStreamRequest<TResponse>
    {
        private readonly PreProcessingState _state;

        public TestStreamMessagePreProcessor(PreProcessingState state) => _state = state;

        protected override ValueTask Handle(TMessage message, CancellationToken cancellationToken)
        {
            _state.Id = (Guid)typeof(TMessage).GetProperty("Id")!.GetValue(message)!;
            _state.Calls++;
            return default;
        }
    }

    private sealed class TestStreamMessagePreProcessor2<TMessage, TResponse>
        : StreamMessagePreProcessor<TMessage, TResponse>
        where TMessage : notnull, IStreamRequest<TResponse>
    {
        private readonly PreProcessingState _state;

        public TestStreamMessagePreProcessor2(PreProcessingState state) => _state = state;

        protected override ValueTask Handle(TMessage message, CancellationToken cancellationToken)
        {
            _state.Id = (Guid)typeof(TMessage).GetProperty("Id")!.GetValue(message)!;
            _state.Calls++;
            return default;
        }
    }
}
