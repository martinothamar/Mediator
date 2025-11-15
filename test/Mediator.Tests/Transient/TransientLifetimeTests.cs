using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Mediator.Tests.TestTypes;
using Microsoft.Extensions.DependencyInjection;

namespace Mediator.Tests;

public sealed class TransientLifetimeTests
{
    [Fact(Skip = Mediator.ServiceLifetime != ServiceLifetime.Transient ? "Only tested for Transient lifetime" : null)]
    public void Test_Generated_Code_Lifetime()
    {
        Assert.Equal(ServiceLifetime.Transient, Mediator.ServiceLifetime);
    }

    [Fact(Skip = Mediator.ServiceLifetime != ServiceLifetime.Transient ? "Only tested for Transient lifetime" : null)]
    public async Task Test_Returns_Different_Handler_Instance_Every_Time()
    {
        using var _ = await TransientTestRequestHandler.LeaseForTesting();

        var (sp, _) = Fixture.GetMediator();

        var handler1 = sp.GetRequiredService<IRequestHandler<TransientTest, SomeResponse>>();
        var handler2 = sp.GetRequiredService<IRequestHandler<TransientTest, SomeResponse>>();

        Assert.NotNull(handler1);
        Assert.NotNull(handler2);
        Assert.NotEqual(handler1, handler2);
    }

    [Fact(Skip = Mediator.ServiceLifetime != ServiceLifetime.Transient ? "Only tested for Transient lifetime" : null)]
    public async Task Test_Disposes_Handler_Correctly_In_Scope()
    {
        using var _ = await TransientTestRequestHandler.LeaseForTesting();

        var services = new ServiceCollection();

        services.AddMediator();

        var ct = TestContext.Current.CancellationToken;

        await using (
            var sp = services.BuildServiceProvider(
                new ServiceProviderOptions() { ValidateOnBuild = true, ValidateScopes = true }
            )
        )
        {
            await using (var scope = sp.CreateAsyncScope())
            {
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                await mediator.Send(new TransientTest(Guid.NewGuid()), ct);
                await mediator.Send(new TransientTest(Guid.NewGuid()), ct);
                await mediator.Send(new TransientTest(Guid.NewGuid()), ct);

                Assert.All(TransientTestRequestHandler.CreatedHandlers, h => Assert.False(h.Disposed));
            }

            Assert.All(TransientTestRequestHandler.CreatedHandlers, h => Assert.True(h.Disposed));

            {
                var mediator = sp.GetRequiredService<IMediator>();
                await mediator.Send(new TransientTest(Guid.NewGuid()), ct);
                await mediator.Send(new TransientTest(Guid.NewGuid()), ct);
                await mediator.Send(new TransientTest(Guid.NewGuid()), ct);
            }
        }

        Assert.All(TransientTestRequestHandler.CreatedHandlers, h => Assert.True(h.Disposed));
    }

    [Fact(Skip = Mediator.ServiceLifetime != ServiceLifetime.Transient ? "Only tested for Transient lifetime" : null)]
    public async Task Test_Disposes_Handler_Correctly_Root_Scope()
    {
        using var _ = await TransientTestRequestHandler.LeaseForTesting();

        var services = new ServiceCollection();

        services.AddMediator();

        var ct = TestContext.Current.CancellationToken;

        await using (
            var sp = services.BuildServiceProvider(
                new ServiceProviderOptions() { ValidateOnBuild = true, ValidateScopes = true }
            )
        )
        {
            var mediator = sp.GetRequiredService<IMediator>();

            await mediator.Send(new TransientTest(Guid.NewGuid()), ct);
            await mediator.Send(new TransientTest(Guid.NewGuid()), ct);
            await mediator.Send(new TransientTest(Guid.NewGuid()), ct);

            Assert.All(TransientTestRequestHandler.CreatedHandlers, h => Assert.False(h.Disposed));
        }

        Assert.All(TransientTestRequestHandler.CreatedHandlers, h => Assert.True(h.Disposed));
    }
}

public sealed record TransientTest(Guid Id) : IRequest<SomeResponse>;

public sealed class TransientTestRequestHandler : IRequestHandler<TransientTest, SomeResponse>, IDisposable
{
    private static readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

    public bool Disposed { get; private set; }

    public static readonly ConcurrentBag<TransientTestRequestHandler> CreatedHandlers = new();

    public TransientTestRequestHandler() => CreatedHandlers.Add(this);

    public ValueTask<SomeResponse> Handle(TransientTest request, CancellationToken cancellationToken) =>
        new ValueTask<SomeResponse>(new SomeResponse(request.Id));

    public void Dispose() => Disposed = true;

    public static async ValueTask<IDisposable> LeaseForTesting()
    {
        await _lock.WaitAsync();
        CreatedHandlers.Clear();
        return new Lease();
    }

    private sealed record Lease() : IDisposable
    {
        public void Dispose() => _lock.Release();
    }
}
