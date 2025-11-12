using System.Runtime.CompilerServices;
using Mediator.Benchmarks.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace Mediator.Benchmarks.Notifications;

#pragma warning disable CS8524 // The switch expression does not handle some values of its input type (it is not exhaustive) involving an unnamed enum value.

public sealed record SingleHandlerNotification(Guid Id) : INotification, MediatR.INotification;

public sealed class SingleHandler
    : INotificationHandler<SingleHandlerNotification>,
        MediatR.INotificationHandler<SingleHandlerNotification>
{
    public ValueTask Handle(SingleHandlerNotification notification, CancellationToken cancellationToken) => default;

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

public sealed record MultiHandlersAsyncNotification(Guid Id) : INotification, MediatR.INotification;

public sealed class MultiHandlerAsync0
    : INotificationHandler<MultiHandlersAsyncNotification>,
        MediatR.INotificationHandler<MultiHandlersAsyncNotification>
{
    public async ValueTask Handle(MultiHandlersAsyncNotification notification, CancellationToken cancellationToken) =>
        await Task.Yield();

    async Task MediatR.INotificationHandler<MultiHandlersAsyncNotification>.Handle(
        MultiHandlersAsyncNotification notification,
        CancellationToken cancellationToken
    ) => await Task.Yield();
}

public sealed class MultiHandlerAsync1
    : INotificationHandler<MultiHandlersAsyncNotification>,
        MediatR.INotificationHandler<MultiHandlersAsyncNotification>
{
    public async ValueTask Handle(MultiHandlersAsyncNotification notification, CancellationToken cancellationToken) =>
        await Task.Yield();

    async Task MediatR.INotificationHandler<MultiHandlersAsyncNotification>.Handle(
        MultiHandlersAsyncNotification notification,
        CancellationToken cancellationToken
    ) => await Task.Yield();
}

public sealed class MultiHandlerAsync2
    : INotificationHandler<MultiHandlersAsyncNotification>,
        MediatR.INotificationHandler<MultiHandlersAsyncNotification>
{
    public async ValueTask Handle(MultiHandlersAsyncNotification notification, CancellationToken cancellationToken) =>
        await Task.Yield();

    async Task MediatR.INotificationHandler<MultiHandlersAsyncNotification>.Handle(
        MultiHandlersAsyncNotification notification,
        CancellationToken cancellationToken
    ) => await Task.Yield();
}

[ConfigSource]
public class NotificationBenchmarks
{
    private sealed class ConfigSourceAttribute : Attribute, IConfigSource
    {
        public IConfig Config { get; }

        public ConfigSourceAttribute()
        {
            var lifetimes = Enum.GetValues<ServiceLifetime>();
            string[] publishers = ["ForeachAwait", "TaskWhenAll"];
            bool[] largeProjectOptions = [false, true];
            var jobs =
                from lifetime in lifetimes
                from publisher in publishers
                from largeProject in largeProjectOptions
                select Job
                    .Default.WithArguments([
                        new MsBuildArgument(
                            $"/p:ExtraDefineConstants=Mediator_Lifetime_{lifetime}"
                                + $"%3BMediator_Publisher_{publisher}"
                                + (largeProject ? $"%3BMediator_Large_Project" : "")
                        ),
                    ])
                    .WithEnvironmentVariable("ServiceLifetime", lifetime.ToString())
                    .WithEnvironmentVariable("NotificationPublisherName", $"{publisher}Publisher")
                    .WithEnvironmentVariable("IsLargeProject", $"{largeProject}")
                    .WithCustomBuildConfiguration($"{lifetime}/{publisher}/{largeProject}")
                    .WithId($"{lifetime}_{publisher}_{largeProject}");

            Config = ManualConfig
                .CreateEmpty()
                .AddJob(jobs.ToArray())
                .AddColumn(new CustomColumn("ServiceLifetime", (_, c) => c.Job.Id.Split('_')[0]))
                .AddColumn(new CustomColumn("NotificationPublisher", (_, c) => c.Job.Id.Split('_')[1]))
                .AddColumn(
                    new CustomColumn("Project type", (_, c) => c.Job.Id.Split('_')[2] == "True" ? "Large" : "Small")
                )
                .WithOption(ConfigOptions.KeepBenchmarkFiles, false)
                .HideColumns(Column.Arguments, Column.EnvironmentVariables, Column.BuildConfiguration, Column.Job)
                .WithSummaryStyle(SummaryStyle.Default.WithRatioStyle(RatioStyle.Trend))
                .AddColumn(RankColumn.Arabic)
                .WithOrderer(new DefaultOrderer(SummaryOrderPolicy.FastestToSlowest, MethodOrderPolicy.Declared))
                .AddDiagnoser(MemoryDiagnoser.Default);
        }
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
    private MultiHandlerAsync0 _multiHandlerAsync0;
    private MultiHandlerAsync1 _multiHandlerAsync1;
    private MultiHandlerAsync2 _multiHandlerAsync2;
    private MultiHandlersAsyncNotification _multiHandlersAsyncNotification;

    public enum ScenarioType
    {
        SingleHandlerSync,
        MultiHandlersSync,
        MultiHandlersAsync,
    }

    [ParamsAllValues]
    public ScenarioType Scenario { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        Fixture.Setup();

        var services = new ServiceCollection();
        services.AddMediator();
        services.AddMediatR(opts =>
        {
            opts.Lifetime = Mediator.ServiceLifetime;
            opts.RegisterServicesFromAssembly(typeof(SingleHandler).Assembly);
        });

        _serviceProvider = services.BuildServiceProvider();
#pragma warning disable CS0162 // Unreachable code detected
        if (Mediator.ServiceLifetime == ServiceLifetime.Scoped)
        {
            _serviceScope = _serviceProvider.CreateScope();
            _serviceProvider = _serviceScope.ServiceProvider;
        }
#pragma warning restore CS0162 // Unreachable code detected

        _mediator = _serviceProvider.GetRequiredService<IMediator>();
        _concreteMediator = _serviceProvider.GetRequiredService<Mediator>();
        _mediatr = _serviceProvider.GetRequiredService<MediatR.IMediator>();

        _singleHandler = _serviceProvider.GetRequiredService<SingleHandler>();
        _singleHandlerNotification = new(Guid.NewGuid());

        _multiHandler0 = _serviceProvider.GetRequiredService<MultiHandler0>();
        _multiHandler1 = _serviceProvider.GetRequiredService<MultiHandler1>();
        _multiHandler2 = _serviceProvider.GetRequiredService<MultiHandler2>();
        _multiHandlersNotification = new(Guid.NewGuid());

        _multiHandlerAsync0 = _serviceProvider.GetRequiredService<MultiHandlerAsync0>();
        _multiHandlerAsync1 = _serviceProvider.GetRequiredService<MultiHandlerAsync1>();
        _multiHandlerAsync2 = _serviceProvider.GetRequiredService<MultiHandlerAsync2>();
        _multiHandlersAsyncNotification = new(Guid.NewGuid());
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
            ScenarioType.SingleHandlerSync => _mediatr.Publish(_singleHandlerNotification),
            ScenarioType.MultiHandlersSync => _mediatr.Publish(_multiHandlersNotification),
            ScenarioType.MultiHandlersAsync => _mediatr.Publish(_multiHandlersAsyncNotification),
        };
    }

    [Benchmark]
    public ValueTask Publish_Notification_IMediator()
    {
        return Scenario switch
        {
            ScenarioType.SingleHandlerSync => _mediator.Publish(_singleHandlerNotification),
            ScenarioType.MultiHandlersSync => _mediator.Publish(_multiHandlersNotification),
            ScenarioType.MultiHandlersAsync => _mediator.Publish(_multiHandlersAsyncNotification),
        };
    }

    [Benchmark]
    public ValueTask Publish_Notification_Mediator()
    {
        return Scenario switch
        {
            ScenarioType.SingleHandlerSync => _concreteMediator.Publish(_singleHandlerNotification),
            ScenarioType.MultiHandlersSync => _concreteMediator.Publish(_multiHandlersNotification),
            ScenarioType.MultiHandlersAsync => _concreteMediator.Publish(_multiHandlersAsyncNotification),
        };
    }

    [Benchmark(Baseline = true)]
    public ValueTask Publish_Notification_Baseline()
    {
        switch (Scenario)
        {
            case ScenarioType.SingleHandlerSync:
                return _singleHandler.Handle(_singleHandlerNotification, default);
            case ScenarioType.MultiHandlersSync:
                _multiHandler0.Handle(_multiHandlersNotification, default).GetAwaiter().GetResult();
                _multiHandler1.Handle(_multiHandlersNotification, default).GetAwaiter().GetResult();
                _multiHandler2.Handle(_multiHandlersNotification, default).GetAwaiter().GetResult();
                return default;
            case ScenarioType.MultiHandlersAsync:
                return AwaitMultipleHandlersAsync();
        }

        return default;

        async ValueTask AwaitMultipleHandlersAsync()
        {
            await _multiHandlerAsync0.Handle(_multiHandlersAsyncNotification, default);
            await _multiHandlerAsync1.Handle(_multiHandlersAsyncNotification, default);
            await _multiHandlerAsync2.Handle(_multiHandlersAsyncNotification, default);
        }
    }
}
