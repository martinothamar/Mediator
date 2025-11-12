using Microsoft.Extensions.DependencyInjection;

namespace Mediator.Benchmarks.Messaging.Comparison;

public sealed record Request(Guid Id) : IRequest<Response>, MediatR.IRequest<Response>;

public sealed record Notification(Guid Id) : INotification, MediatR.INotification;

public sealed record Response(Guid Id);

public sealed record StreamRequest(Guid Id) : IStreamRequest<Response>, MediatR.IStreamRequest<Response>;

public sealed class Handler
    : IRequestHandler<Request, Response>,
        MediatR.IRequestHandler<Request, Response>,
        IStreamRequestHandler<StreamRequest, Response>,
        MediatR.IStreamRequestHandler<StreamRequest, Response>,
        INotificationHandler<Notification>,
        MediatR.INotificationHandler<Notification>
{
    private static readonly Response _response = new Response(Guid.NewGuid());

    private static readonly Task<Response> _tResponse = Task.FromResult(_response);

    public ValueTask<Response> Handle(Request request, CancellationToken cancellationToken) =>
        new ValueTask<Response>(_response);

    Task<Response> MediatR.IRequestHandler<Request, Response>.Handle(
        Request request,
        CancellationToken cancellationToken
    ) => _tResponse;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    async IAsyncEnumerable<Response> _enumerate()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    {
        for (int i = 0; i < 3; i++)
        {
            yield return _response;
        }
    }

    public IAsyncEnumerable<Response> Handle(StreamRequest request, CancellationToken cancellationToken) =>
        _enumerate();

    IAsyncEnumerable<Response> MediatR.IStreamRequestHandler<StreamRequest, Response>.Handle(
        StreamRequest request,
        CancellationToken cancellationToken
    ) => _enumerate();

    public ValueTask Handle(Notification notification, CancellationToken cancellationToken) => default;

    Task MediatR.INotificationHandler<Notification>.Handle(
        Notification notification,
        CancellationToken cancellationToken
    ) => Task.CompletedTask;
}

[ConfigSource]
public class ComparisonBenchmarks
{
    private sealed class ConfigSourceAttribute : Attribute, IConfigSource
    {
        public IConfig Config { get; }

        public ConfigSourceAttribute()
        {
            var lifetimes = Enum.GetValues<ServiceLifetime>();
            bool[] largeProjectOptions = [false, true];

            // Local override
            // lifetimes = [ServiceLifetime.Singleton];
            // largeProjectOptions = [false, true];

            var jobs =
                from lifetime in lifetimes
                from largeProject in largeProjectOptions
                select Job
                    .Default.WithArguments([
                        new MsBuildArgument(
                            $"/p:ExtraDefineConstants=Mediator_Lifetime_{lifetime}"
                                + (largeProject ? $"%3BMediator_Large_Project" : "")
                        ),
                    ])
                    .WithEnvironmentVariable("ServiceLifetime", lifetime.ToString())
                    .WithEnvironmentVariable("IsLargeProject", $"{largeProject}")
                    .WithCustomBuildConfiguration($"{lifetime}/{largeProject}")
                    .WithId($"{lifetime}_{largeProject}");

            Config = ManualConfig
                .CreateEmpty()
                .AddJob(jobs.ToArray())
                .AddColumn(new CustomColumn("ServiceLifetime", (_, c) => c.Job.Id.Split('_')[0]))
                .AddColumn(
                    new CustomColumn("Project type", (_, c) => c.Job.Id.Split('_')[1] == "True" ? "Large" : "Small")
                )
                .AddColumn(CategoriesColumn.Default)
                .AddLogicalGroupRules(BenchmarkLogicalGroupRule.ByCategory)
                .WithOption(ConfigOptions.KeepBenchmarkFiles, false)
                .HideColumns(Column.Arguments, Column.EnvironmentVariables, Column.BuildConfiguration, Column.Job)
                .WithSummaryStyle(SummaryStyle.Default.WithRatioStyle(RatioStyle.Trend))
                .AddColumn(RankColumn.Arabic)
                .WithOrderer(new DefaultOrderer(SummaryOrderPolicy.FastestToSlowest, MethodOrderPolicy.Declared))
                .AddDiagnoser(MemoryDiagnoser.Default);
            // .AddDiagnoser(new DotTraceDiagnoser());
        }
    }

    private IServiceProvider _serviceProvider;
    private IServiceProvider _rootServiceProvider;
    private IServiceScope _serviceScope;
    private IMediator _mediator;
    private Mediator _concreteMediator;
    private MediatR.IMediator _mediatr;
    private Request _request;
    private object _requestObj;
    private StreamRequest _streamRequest;
    private object _streamRequestObj;
    private Notification _notification;
    private object _notificationObj;

    [GlobalSetup]
    public void Setup()
    {
        Fixture.Setup();

        var services = new ServiceCollection();
        services.AddMediator();
        services.AddMediatR(opts =>
        {
            opts.Lifetime = Mediator.ServiceLifetime;
            opts.RegisterServicesFromAssembly(typeof(Handler).Assembly);
        });

        _serviceProvider = services.BuildServiceProvider();
        _rootServiceProvider = _serviceProvider;
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
        _request = new(Guid.NewGuid());
        _requestObj = _request;
        _streamRequest = new StreamRequest(Guid.NewGuid());
        _streamRequestObj = _streamRequest;
        _notification = new Notification(Guid.NewGuid());
        _notificationObj = _notification;
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        if (_serviceScope is not null)
            _serviceScope.Dispose();
        else
            (_serviceProvider as IDisposable)?.Dispose();
    }

    // Initialization

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Initialization")]
    public MediatR.IMediator Initialization_MediatR()
    {
#if Mediator_Lifetime_Scoped
        using var scope = _rootServiceProvider.CreateScope();
        return scope.ServiceProvider.GetRequiredService<MediatR.IMediator>();
#else
        return _rootServiceProvider.GetRequiredService<MediatR.IMediator>();
#endif
    }

    [Benchmark]
    [BenchmarkCategory("Initialization")]
    public IMediator Initialization_IMediator()
    {
#if Mediator_Lifetime_Scoped
        using var scope = _rootServiceProvider.CreateScope();
        return scope.ServiceProvider.GetRequiredService<IMediator>();
#else
        return _rootServiceProvider.GetRequiredService<IMediator>();
#endif
    }

    // ColdStart (mostly makes sense for transient and scoped registration)

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("ColdStart")]
    public Task<Response> ColdStart_MediatR()
    {
#if Mediator_Lifetime_Scoped
        using var scope = _rootServiceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<MediatR.IMediator>();
#else
        var mediator = _rootServiceProvider.GetRequiredService<MediatR.IMediator>();
#endif
        return mediator.Send(_request, CancellationToken.None);
    }

    [Benchmark]
    [BenchmarkCategory("ColdStart")]
    public ValueTask<Response> ColdStart_IMediator()
    {
#if Mediator_Lifetime_Scoped
        using var scope = _rootServiceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
#else
        var mediator = _rootServiceProvider.GetRequiredService<IMediator>();
#endif
        return mediator.Send(_request, CancellationToken.None);
    }

    // Normal requests

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Request", "Concrete")]
    public Task<Response> Request_MediatR()
    {
        return _mediatr.Send(_request, CancellationToken.None);
    }

    [Benchmark]
    [BenchmarkCategory("Request", "Concrete")]
    public ValueTask<Response> Request_IMediator()
    {
        return _mediator.Send(_request, CancellationToken.None);
    }

    [Benchmark]
    [BenchmarkCategory("Request", "Concrete")]
    public ValueTask<Response> Request_Mediator()
    {
        return _concreteMediator.Send(_request, CancellationToken.None);
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Request", "Object")]
    public Task<object> Request_MediatR_Object()
    {
        return _mediatr.Send(_requestObj, CancellationToken.None);
    }

    [Benchmark]
    [BenchmarkCategory("Request", "Object")]
    public ValueTask<object> Request_IMediator_Object()
    {
        return _mediator.Send(_requestObj, CancellationToken.None);
    }

    // Streaming requests

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("StreamRequest", "Concrete")]
    public async ValueTask StreamRequest_MediatR()
    {
        await foreach (var response in _mediatr.CreateStream(_streamRequest, CancellationToken.None))
        {
            _ = response;
        }
    }

    [Benchmark]
    [BenchmarkCategory("StreamRequest", "Concrete")]
    public async ValueTask StreamRequest_IMediator()
    {
        await foreach (var response in _mediator.CreateStream(_streamRequest, CancellationToken.None))
        {
            _ = response;
        }
    }

    [Benchmark]
    [BenchmarkCategory("StreamRequest", "Concrete")]
    public async ValueTask StreamRequest_Mediator()
    {
        await foreach (var response in _concreteMediator.CreateStream(_streamRequest, CancellationToken.None))
        {
            _ = response;
        }
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("StreamRequest", "Object")]
    public async ValueTask StreamRequest_MediatR_Object()
    {
        await foreach (var response in _mediatr.CreateStream(_streamRequestObj, CancellationToken.None))
        {
            _ = response;
        }
    }

    [Benchmark]
    [BenchmarkCategory("StreamRequest", "Object")]
    public async ValueTask StreamRequest_IMediator_Object()
    {
        await foreach (var response in _mediator.CreateStream(_streamRequestObj, CancellationToken.None))
        {
            _ = response;
        }
    }

    // Notifications

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Notification", "Concrete")]
    public Task Notification_MediatR()
    {
        return _mediatr.Publish(_notification, CancellationToken.None);
    }

    [Benchmark]
    [BenchmarkCategory("Notification", "Concrete")]
    public ValueTask Notification_IMediator()
    {
        return _mediator.Publish(_notification, CancellationToken.None);
    }

    [Benchmark]
    [BenchmarkCategory("Notification", "Concrete")]
    public ValueTask Notification_Mediator()
    {
        return _concreteMediator.Publish(_notification, CancellationToken.None);
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Notification", "Object")]
    public Task Notification_MediatR_Object()
    {
        return _mediatr.Publish(_notificationObj, CancellationToken.None);
    }

    [Benchmark]
    [BenchmarkCategory("Notification", "Object")]
    public ValueTask Notification_IMediator_Object()
    {
        return _mediator.Publish(_notificationObj, CancellationToken.None);
    }
}
