using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Reports;
using Microsoft.Extensions.DependencyInjection;

namespace Mediator.Benchmarks.Notification;

#pragma warning disable CS8524 // The switch expression does not handle some values of its input type (it is not exhaustive) involving an unnamed enum value.

public sealed record SingleHandlerNotification(Guid Id) : INotification, MediatR.INotification;

public sealed class SingleHandler
    : INotificationHandler<SingleHandlerNotification>,
        MediatR.INotificationHandler<SingleHandlerNotification>
{
    public ValueTask Handle(SingleHandlerNotification notification, CancellationToken cancellationToken) => default;

    public ValueTask HandleAsync(SingleHandlerNotification message, CancellationToken cancellationToken) => default;

    Task MediatR.INotificationHandler<SingleHandlerNotification>.Handle(
        SingleHandlerNotification notification,
        CancellationToken cancellationToken
    ) => Task.CompletedTask;
}

public sealed record MultiHandlersNotification(Guid Id) : INotification, MediatR.INotification;

public sealed class MultiHandler0
    : INotificationHandler<MultiHandlersNotification>,
        MediatR.INotificationHandler<MultiHandlersNotification>
{
    public ValueTask Handle(MultiHandlersNotification notification, CancellationToken cancellationToken) => default;

    Task MediatR.INotificationHandler<MultiHandlersNotification>.Handle(
        MultiHandlersNotification notification,
        CancellationToken cancellationToken
    ) => Task.CompletedTask;
}

public sealed class MultiHandler1
    : INotificationHandler<MultiHandlersNotification>,
        MediatR.INotificationHandler<MultiHandlersNotification>
{
    public ValueTask Handle(MultiHandlersNotification notification, CancellationToken cancellationToken) => default;

    Task MediatR.INotificationHandler<MultiHandlersNotification>.Handle(
        MultiHandlersNotification notification,
        CancellationToken cancellationToken
    ) => Task.CompletedTask;
}

public sealed class MultiHandler2
    : INotificationHandler<MultiHandlersNotification>,
        MediatR.INotificationHandler<MultiHandlersNotification>
{
    public ValueTask Handle(MultiHandlersNotification notification, CancellationToken cancellationToken) => default;

    Task MediatR.INotificationHandler<MultiHandlersNotification>.Handle(
        MultiHandlersNotification notification,
        CancellationToken cancellationToken
    ) => Task.CompletedTask;
}

[ConfigSource]
public class NotificationBenchmarks
{
    private sealed class ConfigSourceAttribute : Attribute, IConfigSource
    {
        public IConfig Config { get; }

        public ConfigSourceAttribute() => Config = new SimpleConfig();
    }

    private IServiceProvider _serviceProvider;
    private IServiceScope _serviceScope;
    private IMediator _mediator;
    private Mediator _concreteMediator;
    private MediatR.IMediator _mediatr;
    private SingleHandler _singleHandler;
    private SingleHandlerNotification _singleHandlerNotification;
    private MultiHandler0 _multiHandler0;
    private MultiHandler1 _multiHandler1;
    private MultiHandler2 _multiHandler2;
    private MultiHandlersNotification _multiHandlersNotification;

    [Params(MediatorConfig.Lifetime)]
    public ServiceLifetime ServiceLifetime { get; set; } = MediatorConfig.Lifetime;

    public enum ScenarioType
    {
        SingleHandler,
        MultipleHandlers,
    }

    [ParamsAllValues]
    public ScenarioType Scenario { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var services = new ServiceCollection();
        services.AddMediator(opts => opts.ServiceLifetime = ServiceLifetime);
        services.AddMediatR(opts =>
        {
            opts.Lifetime = ServiceLifetime;
            opts.RegisterServicesFromAssembly(typeof(SingleHandler).Assembly);
        });

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

        _singleHandler = _serviceProvider.GetRequiredService<SingleHandler>();
        _singleHandlerNotification = new(Guid.NewGuid());

        _multiHandler0 = _serviceProvider.GetRequiredService<MultiHandler0>();
        _multiHandler1 = _serviceProvider.GetRequiredService<MultiHandler1>();
        _multiHandler2 = _serviceProvider.GetRequiredService<MultiHandler2>();
        _multiHandlersNotification = new(Guid.NewGuid());
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        if (_serviceScope is not null)
            _serviceScope.Dispose();
        else
            (_serviceProvider as IDisposable)?.Dispose();
    }

    [Benchmark]
    public Task Publish_Notification_MediatR()
    {
        return Scenario switch
        {
            ScenarioType.SingleHandler => _mediatr.Publish(_singleHandlerNotification, CancellationToken.None),
            ScenarioType.MultipleHandlers => _mediatr.Publish(_multiHandlersNotification, CancellationToken.None),
        };
    }

    [Benchmark]
    public ValueTask Publish_Notification_IMediator()
    {
        return Scenario switch
        {
            ScenarioType.SingleHandler => _mediator.Publish(_singleHandlerNotification, CancellationToken.None),
            ScenarioType.MultipleHandlers => _mediator.Publish(_multiHandlersNotification, CancellationToken.None),
        };
    }

    [Benchmark]
    public ValueTask Publish_Notification_Mediator()
    {
        return Scenario switch
        {
            ScenarioType.SingleHandler => _concreteMediator.Publish(_singleHandlerNotification, CancellationToken.None),
            ScenarioType.MultipleHandlers
                => _concreteMediator.Publish(_multiHandlersNotification, CancellationToken.None),
        };
    }

    [Benchmark(Baseline = true)]
    public async ValueTask Publish_Notification_Baseline()
    {
        switch (Scenario)
        {
            case ScenarioType.SingleHandler:
                await _singleHandler.Handle(_singleHandlerNotification, CancellationToken.None);
                break;
            case ScenarioType.MultipleHandlers:
                await _multiHandler0.Handle(_multiHandlersNotification, CancellationToken.None);
                await _multiHandler1.Handle(_multiHandlersNotification, CancellationToken.None);
                await _multiHandler2.Handle(_multiHandlersNotification, CancellationToken.None);
                break;
        }
    }
}
