using Mediator.Tests.TestTypes;
using System;
using System.Threading.Tasks;

namespace Mediator.Tests;

public partial class SmokeTests
{
    [Theory]
    [InlineData(1L << 2)]
    [InlineData(1L << 3)]
    [InlineData(1L << 4)]
    [InlineData(1L << 5)]
    [InlineData(1L << 6)]
    [InlineData(1L << 7)]
    [InlineData(1L << 8)]
    [InlineData(1L << 9)]
    [InlineData(1L << 10)]
    public async Task Test_Concurrent_Messages(long concurrency)
    {
        var (_, mediator) = Fixture.GetMediator();
        var concrete = (Mediator)mediator;

        var id = Guid.NewGuid();
        var message = new SomeRequest(id);

        var start = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        var threads = new Task[concurrency];
        for (int i = 0; i < concurrency; i++)
        {
            threads[i] = Task.Run(Thread);
        }

        start.SetResult();
        await Task.WhenAll(threads).ConfigureAwait(false);

        async Task Thread()
        {
            await start.Task.ConfigureAwait(false);

            const int count = 1000;
            for (int i = 0; i < count; i++)
            {
                var response = await concrete.Send(message).ConfigureAwait(false);
                Assert.Equal(id, response.Id);
            }
        }
    }
}
