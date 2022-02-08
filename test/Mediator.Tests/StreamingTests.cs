using Mediator.Tests.TestTypes;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Mediator.Tests
{
    public sealed class StreamingTests
    {
        [Fact]
        public async Task Test_ISender()
        {
            var (_, mediator) = Fixture.GetMediator();
            ISender sender = mediator;

            var id = Guid.NewGuid();

            int counter = 0;
            await foreach (var response in sender.CreateStream(new SomeStreamingQuery(id)))
            {
                Assert.Equal(id, response.Id);
                counter++;
            }

            Assert.Equal(3, counter);
        }

        [Fact]
        public async Task Test_IMediator()
        {
            var (_, mediator) = Fixture.GetMediator();

            var id = Guid.NewGuid();

            int counter = 0;
            await foreach (var response in mediator.CreateStream(new SomeStreamingQuery(id)))
            {
                Assert.Equal(id, response.Id);
                counter++;
            }

            Assert.Equal(3, counter);
        }

        [Fact]
        public async Task Test_Cancellation_Parameter()
        {
            var (_, mediator) = Fixture.GetMediator();

            var id = Guid.NewGuid();

            using var cts = new CancellationTokenSource();
            var token = cts.Token;

            int counter = 0;
            await foreach (var response in mediator.CreateStream(new SomeStreamingQuery(id), token))
            {
                Assert.Equal(id, response.Id);
                counter++;

                cts.Cancel();
            }

            Assert.Equal(1, counter);
        }

        [Fact]
        public async Task Test_Cancellation_WithCancellation_Method()
        {
            var (_, mediator) = Fixture.GetMediator();

            var id = Guid.NewGuid();

            using var cts = new CancellationTokenSource();
            var token = cts.Token;

            int counter = 0;
            await foreach (var response in mediator.CreateStream(new SomeStreamingQuery(id)).WithCancellation(token))
            {
                Assert.Equal(id, response.Id);
                counter++;

                cts.Cancel();
            }

            Assert.Equal(1, counter);
        }
    }
}
