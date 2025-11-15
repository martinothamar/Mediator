using System;
using System.Threading;
using System.Threading.Tasks;
using Mediator.Tests.TestTypes;
using Microsoft.Extensions.DependencyInjection;

namespace Mediator.Tests.Pipeline.StreamPreProcessing;

public class ConcreteTests
{
    [Fact]
    public async Task Test_StreamPreProcessor()
    {
        var ct = TestContext.Current.CancellationToken;
        var (sp, mediator) = Fixture.GetMediator(services =>
        {
            services.AddSingleton<PreProcessingState>();
            services.AddSingleton<
                IStreamPipelineBehavior<SomeStreamingRequest, SomeResponse>,
                TestStreamMessagePreProcessor
            >();
            services.AddSingleton<
                IStreamPipelineBehavior<SomeStreamingRequest, SomeResponse>,
                TestStreamMessagePreProcessor2
            >();
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

        // Test with SomeStreamingQuery - should not trigger the concrete preprocessors
        counter = 0;
        await foreach (var response in mediator.CreateStream(new SomeStreamingQuery(queryId), ct))
        {
            Assert.Equal(queryId, response.Id);
            counter++;
        }

        Assert.Equal(3, counter);
        Assert.Equal(id, state!.Id); // Should still be the old id
        Assert.Equal(2, state.Calls); // Should still be 2
    }

    private sealed class PreProcessingState
    {
        public Guid Id;
        public int Calls;
    }

    private sealed class TestStreamMessagePreProcessor : StreamMessagePreProcessor<SomeStreamingRequest, SomeResponse>
    {
        private readonly PreProcessingState _state;

        public TestStreamMessagePreProcessor(PreProcessingState state) => _state = state;

        protected override ValueTask Handle(SomeStreamingRequest message, CancellationToken cancellationToken)
        {
            _state.Id = message.Id;
            _state.Calls++;
            return default;
        }
    }

    private sealed class TestStreamMessagePreProcessor2 : StreamMessagePreProcessor<SomeStreamingRequest, SomeResponse>
    {
        private readonly PreProcessingState _state;

        public TestStreamMessagePreProcessor2(PreProcessingState state) => _state = state;

        protected override ValueTask Handle(SomeStreamingRequest message, CancellationToken cancellationToken)
        {
            _state.Id = message.Id;
            _state.Calls++;
            return default;
        }
    }
}
