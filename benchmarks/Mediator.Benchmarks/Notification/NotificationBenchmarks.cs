using MediatR;
using MessagePipe;
using Microsoft.Extensions.DependencyInjection;

#nullable disable

namespace Mediator.Benchmarks.Notification;

public sealed record SomeNotification(Guid Id) : INotification, MediatR.INotification;

public sealed class SomeHandlerClass :
    INotificationHandler<SomeNotification>,
    MediatR.INotificationHandler<SomeNotification>,
    IAsyncMessageHandler<SomeNotification>
{
    public ValueTask Handle(SomeNotification notification, CancellationToken cancellationToken) =>
        default;

    public ValueTask HandleAsync(SomeNotification message, CancellationToken cancellationToken) =>
        default;

    Task MediatR.INotificationHandler<SomeNotification>.Handle(SomeNotification notification, CancellationToken cancellationToken) =>
        Task.CompletedTask;
}

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest, MethodOrderPolicy.Declared)]
public class NotificationBenchmarks
{
    private IServiceProvider _serviceProvider;
    private IMediator _mediator;
    private Mediator _concreteMediator;
    private MediatR.IMediator _mediatr;
    private IAsyncSubscriber<SomeNotification> _messagePipeSubscriber;
    private IDisposable _subscription;
    private IAsyncPublisher<SomeNotification> _messagePipePublisher;
    private SomeHandlerClass _handler;
    private SomeNotification _notification;

    [GlobalSetup]
    public void Setup()
    {
        var services = new ServiceCollection();
        services.AddMediator();
        services.AddMediatR(config => config.AsSingleton(), typeof(SomeHandlerClass).Assembly);
        services.AddMessagePipe();

        _serviceProvider = services.BuildServiceProvider();
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
