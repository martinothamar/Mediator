using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DICache = Mediator.Mediator.DICache;
using LazyDICache = Mediator.Mediator.FastLazyValue<Mediator.Mediator.DICache>;

await Host.CreateDefaultBuilder()
    .ConfigureLogging(
        (context, logging) =>
        {
            logging.ClearProviders();
            logging.AddSimpleConsole(
                options =>
                {
                    options.SingleLine = true;
                }
            );
        }
    )
    .ConfigureServices(
        services =>
        {
            services.AddHostedService<Work>();
        }
    )
    .Build()
    .RunAsync();

public sealed class Work : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var concurrency = Environment.ProcessorCount;
        var threads = new Task<(DICache Cache, long State)>[concurrency];

        var services = new ServiceCollection();
        services.AddMediator();

        var iteration = 0;
        const int maxIterations = 1_000_000;

        Console.WriteLine(
            $"Starting smoketests - "
                + $"{nameof(concurrency)}={concurrency}"
                + $", {nameof(maxIterations)}={maxIterations}"
        );

        while (!stoppingToken.IsCancellationRequested && iteration < maxIterations)
        {
            await using var sp = services.BuildServiceProvider(validateScopes: true);

            var mediator = sp.GetRequiredService<Mediator.Mediator>();

            var start = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

            for (int i = 0; i < concurrency; i++)
                threads[i] = Task.Run(Thread);

            start.SetResult();
            var values = await Task.WhenAll(threads).ConfigureAwait(false);
            var states = values.Select(v => v.State).ToArray();
            var firstHandlers = values.Select(v => v.Cache.Wrapper_For_Request).ToArray();
            var firstHandler = firstHandlers[0];
            var lastHandlers = values.Select(v => v.Cache.Wrapper_For_Request5).ToArray();
            var lastHandler = lastHandlers[0];

            var hasInvalidHandler =
                firstHandlers.Any(h => !ReferenceEquals(h, firstHandler))
                || lastHandlers.Any(h => !ReferenceEquals(h, lastHandler));
            var hasInvalid = states.Any(s => s == LazyDICache.INVALID);
            var wasUninitCount = states.Count(s => s == LazyDICache.UNINIT);
            var wasInitingCount = states.Count(s => s == LazyDICache.INITING);
            var wasInitedCount = states.Count(s => s == LazyDICache.INITD);
            var wasCachedCount = states.Count(s => s == LazyDICache.CACHED);

            Console.WriteLine(
                $"Ran smoketest iteration {++iteration} - "
                    + $"{nameof(hasInvalidHandler)}={hasInvalidHandler}"
                    + $", {nameof(hasInvalid)}={hasInvalid}"
                    + $", {nameof(wasUninitCount)}={wasUninitCount}"
                    + $", {nameof(wasInitingCount)}={wasInitingCount}"
                    + $", {nameof(wasInitedCount)}={wasInitedCount}"
                    + $", {nameof(wasCachedCount)}={wasCachedCount}"
            );

            if (hasInvalidHandler || hasInvalid || wasUninitCount != 1)
            {
                Console.WriteLine("Error condition, exiting...");
                break;
            }

            async Task<(DICache Cache, long State)> Thread()
            {
                await start.Task.ConfigureAwait(false);

                return mediator._diCacheLazy.ValueInstrumented;
            }
        }

        Console.WriteLine("------------------");
        Console.WriteLine(
            $"Done smoketesting! - " + $"{nameof(concurrency)}={concurrency}, {nameof(maxIterations)}={maxIterations}"
        );
    }
}

public sealed record Request() : IRequest;

public sealed class RequestHandler : IRequestHandler<Request>
{
    public ValueTask<Unit> Handle(Request request, CancellationToken cancellationToken) => default;
}

public sealed record Request2() : IRequest;

public sealed class Request2Handler : IRequestHandler<Request2>
{
    public ValueTask<Unit> Handle(Request2 request, CancellationToken cancellationToken) => default;
}

public sealed record Request3() : IRequest;

public sealed class Request3Handler : IRequestHandler<Request3>
{
    public ValueTask<Unit> Handle(Request3 request, CancellationToken cancellationToken) => default;
}

public sealed record Request4() : IRequest;

public sealed class Request4Handler : IRequestHandler<Request4>
{
    public ValueTask<Unit> Handle(Request4 request, CancellationToken cancellationToken) => default;
}

public sealed record Request5() : IRequest;

public sealed class Request5Handler : IRequestHandler<Request5>
{
    public ValueTask<Unit> Handle(Request5 request, CancellationToken cancellationToken) => default;
}
