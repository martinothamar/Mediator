using System.Linq;
using System.Threading.Tasks;
using DICache = Mediator.Mediator.DICache;
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

        var threads = new Task<(DICache Cache, long State)>[concurrency];
        for (int i = 0; i < concurrency; i++)
        {
            threads[i] = Task.Run(Thread);
        }

        start.SetResult();
        var values = await Task.WhenAll(threads).ConfigureAwait(false);
        var states = values.Select(v => v.State).ToArray();

        Assert.DoesNotContain(LazyDICache.INVALID, states);
        Assert.Single(states.Where(s => s == LazyDICache.UNINIT));

        var handlers = values.Select(v => v.Cache.Wrapper_For_Mediator_Tests_TestTypes_SomeRequest).ToArray();
        var handler = handlers[0];

        Assert.All(handlers, h => Assert.Same(handler, h));

        async Task<(DICache Cache, long State)> Thread()
        {
            await start.Task.ConfigureAwait(false);

            return concrete._diCacheLazy.ValueInstrumented;
        }
    }
}
