using Mediator.Tests.TestTypes;
using LazyDICache = Mediator.Mediator.FastLazyValue<Mediator.Mediator.DICache>;

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
    public async Task Test_FastLazy(long concurrency)
    {
        var (_, mediator) = Fixture.GetMediator();
        var concrete = (Mediator)mediator;

        var start = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        var threads = new Task<long>[concurrency];
        for (int i = 0; i < concurrency; i++)
        {
            threads[i] = Task.Run(Thread);
        }

        start.SetResult();
        var states = await Task.WhenAll(threads).ConfigureAwait(false);
        Assert.DoesNotContain(LazyDICache.INVALID, states);
        Assert.Single(states.Where(s => s == LazyDICache.UNINIT));

        async Task<long> Thread()
        {
            await start.Task.ConfigureAwait(false);

            var (_, state) = concrete._diCacheLazy.ValueInstrumented;
            return state;
        }
    }
}
