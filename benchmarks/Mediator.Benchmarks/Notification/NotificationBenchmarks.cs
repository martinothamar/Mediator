using MediatR;
using MessagePipe;
using Microsoft.Extensions.DependencyInjection;

namespace Mediator.Benchmarks.Notification;

public sealed record SomeNotification(Guid Id) : INotification, MediatR.INotification;

public sealed class SomeHandlerClass
    : INotificationHandler<SomeNotification>,
      MediatR.INotificationHandler<SomeNotification>,
      IAsyncMessageHandler<SomeNotification>
{
    public ValueTask Handle(SomeNotification notification, CancellationToken cancellationToken) => default;

    public ValueTask HandleAsync(SomeNotification message, CancellationToken cancellationToken) => default;

    Task MediatR.INotificationHandler<SomeNotification>.Handle(
        SomeNotification notification,
        CancellationToken cancellationToken
    ) => Task.CompletedTask;
}

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest, MethodOrderPolicy.Declared)]
[RankColumn]
//[EventPipeProfiler(EventPipeProfile.CpuSampling)]
//[DisassemblyDiagnoser]
//[InliningDiagnoser(logFailuresOnly: true, allowedNamespaces: new[] { "Mediator" })]
public class NotificationBenchmarks
{
    private IServiceProvider _serviceProvider;
    private IServiceScope _serviceScope;
    private IMediator _mediator;
    private Mediator _concreteMediator;
    private MediatR.IMediator _mediatr;
    private IAsyncSubscriber<SomeNotification> _messagePipeSubscriber;
    private IDisposable _subscription;
    private IAsyncPublisher<SomeNotification> _messagePipePublisher;
    private SomeHandlerClass _handler;
    private SomeNotification _notification;

    [Params(MediatorConfig.Lifetime)]
    public ServiceLifetime ServiceLifetime { get; set; } = MediatorConfig.Lifetime;

    [GlobalSetup]
    public void Setup()
    {
        var services = new ServiceCollection();
        services.AddMediator(opts => opts.ServiceLifetime = ServiceLifetime);
        services.AddMediatR(
            opts =>
            {
                _ = ServiceLifetime switch
                {
                    ServiceLifetime.Singleton => opts.AsSingleton(),
                    ServiceLifetime.Scoped => opts.AsScoped(),
                    ServiceLifetime.Transient => opts.AsTransient(),
                    _ => throw new InvalidOperationException(),
                };
            },
            typeof(SomeHandlerClass).Assembly
        );
        services.AddMessagePipe(
            opts =>
            {
                opts.InstanceLifetime = ServiceLifetime switch
                {
                    ServiceLifetime.Singleton => InstanceLifetime.Singleton,
                    ServiceLifetime.Scoped => InstanceLifetime.Scoped,
                    ServiceLifetime.Transient => InstanceLifetime.Transient,
                    _ => throw new InvalidOperationException(),
                };
            }
        );

        _serviceProvider = services.BuildServiceProvider();
        if (ServiceLifetime == ServiceLifetime.Scoped)
        {
#pragma warning disable CS0162 // Unreachable code detected
            _serviceScope = _serviceProvider.CreateScope();
#pragma warning restore CS0162 // Unreachable code detected
            _serviceProvider = _serviceScope.ServiceProvider;
        }

        _mediator = _serviceProvider.GetRequiredService<IMediator>();
        _concreteMediator = _serviceProvider.GetRequiredService<Mediator>();
        _mediatr = _serviceProvider.GetRequiredService<MediatR.IMediator>();
        _handler = _serviceProvider.GetRequiredService<SomeHandlerClass>();
        _messagePipeSubscriber = _serviceProvider.GetRequiredService<IAsyncSubscriber<SomeNotification>>();
        _messagePipePublisher = _serviceProvider.GetRequiredService<IAsyncPublisher<SomeNotification>>();
        _subscription = _messagePipeSubscriber.Subscribe(_handler);
        _notification = new(Guid.NewGuid());
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _subscription?.Dispose();
        if (_serviceScope is not null)
            _serviceScope.Dispose();
        else
            (_serviceProvider as IDisposable)?.Dispose();
    }

    [Benchmark]
    public Task SendNotification_MediatR()
    {
        return _mediatr.Publish(_notification, CancellationToken.None);
    }

    [Benchmark]
    public ValueTask SendNotification_IMediator()
    {
        return _mediator.Publish(_notification, CancellationToken.None);
    }

    [Benchmark]
    public ValueTask SendNotification_Mediator()
    {
        return _concreteMediator.Publish(_notification, CancellationToken.None);
    }

    [Benchmark]
    public ValueTask SendNotification_MessagePipe()
    {
        return _messagePipePublisher.PublishAsync(_notification, CancellationToken.None);
    }

    [Benchmark(Baseline = true)]
    public ValueTask SendNotification_Baseline()
    {
        return _handler.Handle(_notification, CancellationToken.None);
    }
}
