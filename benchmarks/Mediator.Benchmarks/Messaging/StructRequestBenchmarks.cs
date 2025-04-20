using Microsoft.Extensions.DependencyInjection;

namespace Mediator.Benchmarks.Messaging;

public readonly struct StructRequest : IRequest<Response>, MediatR.IRequest<Response>
{
    public readonly Guid Id;
    public readonly Guid CorrelationId;
    public readonly Guid CausationId;
    public readonly DateTimeOffset TimeStamp;
    public readonly uint Version;

    public StructRequest(Guid id)
    {
        Id = id;
        CorrelationId = Guid.NewGuid();
        CausationId = Guid.NewGuid();
        TimeStamp = DateTimeOffset.UtcNow;
        Version = 1;
    }
}

public sealed class StructHandler
    : IRequestHandler<StructRequest, Response>,
        MediatR.IRequestHandler<StructRequest, Response>
{
    private static readonly Response _response = new Response(Guid.NewGuid());

    private static readonly Task<Response> _tResponse = Task.FromResult(_response);

    public ValueTask<Response> Handle(StructRequest request, CancellationToken cancellationToken) =>
        new ValueTask<Response>(_response);

    Task<Response> MediatR.IRequestHandler<StructRequest, Response>.Handle(
        StructRequest request,
        CancellationToken cancellationToken
    ) => _tResponse;
}

[ConfigSource]
public class StructRequestBenchmarks
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
                    .Default.WithArguments(
                        [
                            new MsBuildArgument(
                                $"/p:ExtraDefineConstants=Mediator_Lifetime_{lifetime}"
                                    + (largeProject ? $"%3BMediator_Large_Project" : "")
                            )
                        ]
                    )
                    .WithEnvironmentVariable("ServiceLifetime", lifetime.ToString())
                    .WithEnvironmentVariable("IsLargeProject", $"{largeProject}")
                    .WithCustomBuildConfiguration($"{lifetime}/{largeProject}")
                    .WithId($"{lifetime}/{largeProject}");

            Config = ManualConfig
                .CreateEmpty()
                .AddJob(jobs.ToArray())
                .AddColumn(new CustomColumn("ServiceLifetime", (_, c) => c.Job.Id.Split('/')[0]))
                .AddColumn(
                    new CustomColumn("Project type", (_, c) => c.Job.Id.Split('/')[1] == "True" ? "Large" : "Small")
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
    private StructHandler _handler;
    private StructRequest _request;

    [GlobalSetup]
    public void Setup()
    {
        Fixture.Setup();

        var services = new ServiceCollection();
        services.AddMediator();
        services.AddMediatR(opts =>
        {
            opts.Lifetime = Mediator.ServiceLifetime;
            opts.RegisterServicesFromAssembly(typeof(RequestHandler).Assembly);
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
        _handler = _serviceProvider.GetRequiredService<StructHandler>();
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
    public Task<Response> StructRequest_MediatR()
    {
        return _mediatr.Send(_request, CancellationToken.None);
    }

    [Benchmark]
    public ValueTask<Response> StructRequest_IMediator()
    {
        return _mediator.Send(_request, CancellationToken.None);
    }

    [Benchmark]
    public ValueTask<Response> StructRequest_Mediator()
    {
        return _concreteMediator.Send(_request, CancellationToken.None);
    }

    [Benchmark(Baseline = true)]
    public ValueTask<Response> StructRequest_Baseline()
    {
        return _handler.Handle(_request, CancellationToken.None);
    }
}
