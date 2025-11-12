using Microsoft.Extensions.DependencyInjection;

namespace Mediator.Benchmarks.Messaging;

public sealed record StreamRequest(Guid Id) : IStreamRequest<Response>, MediatR.IStreamRequest<Response>;

public sealed class StreamRequestHandler
    : IStreamRequestHandler<StreamRequest, Response>,
        MediatR.IStreamRequestHandler<StreamRequest, Response>
{
    private static readonly Response _response = new Response(Guid.NewGuid());

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
}

[ConfigSource]
public class StreamingBenchmarks
{
    private sealed class ConfigSourceAttribute : Attribute, IConfigSource
    {
        public IConfig Config { get; }

        public ConfigSourceAttribute()
        {
            var lifetimes = Enum.GetValues<ServiceLifetime>();
            bool[] largeProjectOptions = [false, true];
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
    private StreamRequestHandler _handler;
    private StreamRequest _request;

    [GlobalSetup]
    public void Setup()
    {
        Fixture.Setup();

        var services = new ServiceCollection();
        services.AddMediator();
        services.AddMediatR(opts =>
        {
            opts.Lifetime = Mediator.ServiceLifetime;
            opts.RegisterServicesFromAssembly(typeof(StreamRequestHandler).Assembly);
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
        _handler = _serviceProvider.GetRequiredService<StreamRequestHandler>();
        _request = new(Guid.NewGuid());
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
    public async ValueTask Stream_MediatR()
    {
        await foreach (var response in _mediatr.CreateStream(_request, CancellationToken.None))
        {
            _ = response;
        }
    }

    [Benchmark]
    public async ValueTask Stream_IMediator()
    {
        await foreach (var response in _mediator.CreateStream(_request, CancellationToken.None))
        {
            _ = response;
        }
    }

    [Benchmark]
    public async ValueTask Stream_Mediator()
    {
        await foreach (var response in _concreteMediator.CreateStream(_request, CancellationToken.None))
        {
            _ = response;
        }
    }

    [Benchmark(Baseline = true)]
    public async ValueTask Stream_Baseline()
    {
        await foreach (var response in _handler.Handle(_request, CancellationToken.None))
        {
            _ = response;
        }
    }
}
