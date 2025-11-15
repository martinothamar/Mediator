using Microsoft.Extensions.DependencyInjection;

namespace Mediator.Benchmarks.Messaging.Comparison;

[ConfigSource]
public class HeadlineBenchmarks
{
    private sealed class ConfigSourceAttribute : Attribute, IConfigSource
    {
        public IConfig Config { get; }

        public ConfigSourceAttribute()
        {
            ServiceLifetime[] lifetimes = [ServiceLifetime.Singleton];
            bool[] largeProjectOptions = [true];

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
                    .WithCustomBuildConfiguration($"{lifetime}/{largeProject}");

            Config = ManualConfig
                .CreateEmpty()
                .AddJob(jobs.ToArray())
                .AddColumn(CategoriesColumn.Default)
                .AddLogicalGroupRules(BenchmarkLogicalGroupRule.ByCategory)
                .WithOption(ConfigOptions.KeepBenchmarkFiles, false)
                .HideColumns(
                    Column.Arguments,
                    Column.EnvironmentVariables,
                    Column.BuildConfiguration,
                    Column.Job,
                    Column.RatioSD,
                    Column.Rank,
                    Column.Gen0,
                    Column.Categories
                )
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
    private StreamRequest _streamRequest;
    private Notification _notification;

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
        _streamRequest = new StreamRequest(Guid.NewGuid());
        _notification = new Notification(Guid.NewGuid());
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        if (_serviceScope is not null)
            _serviceScope.Dispose();
        else
            (_serviceProvider as IDisposable)?.Dispose();
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
    [BenchmarkCategory("Request")]
    public Task<Response> Request_MediatR()
    {
        return _mediatr.Send(_request, CancellationToken.None);
    }

    [Benchmark]
    [BenchmarkCategory("Request")]
    public ValueTask<Response> Request_IMediator()
    {
        return _mediator.Send(_request, CancellationToken.None);
    }

    [Benchmark]
    [BenchmarkCategory("Request")]
    public ValueTask<Response> Request_Mediator()
    {
        return _concreteMediator.Send(_request, CancellationToken.None);
    }

    // Streaming requests

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("StreamRequest")]
    public async ValueTask StreamRequest_MediatR()
    {
        await foreach (var response in _mediatr.CreateStream(_streamRequest, CancellationToken.None))
        {
            _ = response;
        }
    }

    [Benchmark]
    [BenchmarkCategory("StreamRequest")]
    public async ValueTask StreamRequest_IMediator()
    {
        await foreach (var response in _mediator.CreateStream(_streamRequest, CancellationToken.None))
        {
            _ = response;
        }
    }

    [Benchmark]
    [BenchmarkCategory("StreamRequest")]
    public async ValueTask StreamRequest_Mediator()
    {
        await foreach (var response in _concreteMediator.CreateStream(_streamRequest, CancellationToken.None))
        {
            _ = response;
        }
    }

    // Notifications

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Notification")]
    public Task Notification_MediatR()
    {
        return _mediatr.Publish(_notification, CancellationToken.None);
    }

    [Benchmark]
    [BenchmarkCategory("Notification")]
    public ValueTask Notification_IMediator()
    {
        return _mediator.Publish(_notification, CancellationToken.None);
    }

    [Benchmark]
    [BenchmarkCategory("Notification")]
    public ValueTask Notification_Mediator()
    {
        return _concreteMediator.Publish(_notification, CancellationToken.None);
    }
}
