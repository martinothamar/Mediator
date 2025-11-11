using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Mediator.Tests.TestTypes;
using Microsoft.Extensions.DependencyInjection;

namespace Mediator.Tests.Pipeline.StreamPostProcessing;

public class ConcreteTests
{
    [Fact]
    public async Task Test_StreamPostProcessor()
    {
        var ct = TestContext.Current.CancellationToken;
        var (sp, mediator) = Fixture.GetMediator(services =>
        {
            services.AddSingleton<PostProcessingState>();
            services.AddSingleton<
                IStreamPipelineBehavior<SomeStreamingRequest, SomeResponse>,
                TestStreamMessagePostProcessor
            >();
            services.AddSingleton<
                IStreamPipelineBehavior<SomeStreamingRequest, SomeResponse>,
                TestStreamMessagePostProcessor2
            >();
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

        // Test with SomeStreamingQuery - should not trigger the concrete postprocessors
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

    private sealed class PostProcessingState
    {
        public Guid Id;
        public int Calls;
    }

    private sealed class TestStreamMessagePostProcessor : StreamMessagePostProcessor<SomeStreamingRequest, SomeResponse>
    {
        private readonly PostProcessingState _state;

        public TestStreamMessagePostProcessor(PostProcessingState state) => _state = state;

        protected override ValueTask Handle(
            SomeStreamingRequest message,
            IReadOnlyList<SomeResponse> responses,
            CancellationToken cancellationToken
        )
        {
            _state.Id = message.Id;
            _state.Calls++;
            return default;
        }
    }

    private sealed class TestStreamMessagePostProcessor2
        : StreamMessagePostProcessor<SomeStreamingRequest, SomeResponse>
    {
        private readonly PostProcessingState _state;

        public TestStreamMessagePostProcessor2(PostProcessingState state) => _state = state;

        protected override ValueTask Handle(
            SomeStreamingRequest message,
            IReadOnlyList<SomeResponse> responses,
            CancellationToken cancellationToken
        )
        {
            _state.Id = message.Id;
            _state.Calls++;
            return default;
        }
    }
}
