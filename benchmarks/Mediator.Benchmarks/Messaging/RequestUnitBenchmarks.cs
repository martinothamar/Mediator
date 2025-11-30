using Microsoft.Extensions.DependencyInjection;

namespace Mediator.Benchmarks.Messaging;

public sealed record RequestWithoutResponse : IRequest, MediatR.IRequest;

public sealed record RequestAsyncWithoutResponse : IRequest, MediatR.IRequest;

public sealed class RequestWithoutResponseHandler
    : IRequestHandler<RequestWithoutResponse>,
        MediatR.IRequestHandler<RequestWithoutResponse>
{
    public ValueTask Handle(RequestWithoutResponse request, CancellationToken cancellationToken) =>
        ValueTask.CompletedTask;

    Task MediatR.IRequestHandler<RequestWithoutResponse>.Handle(
        RequestWithoutResponse request,
        CancellationToken cancellationToken
    ) => Task.CompletedTask;
}

public sealed class RequestAsyncWithoutResponseHandler
    : IRequestHandler<RequestAsyncWithoutResponse>,
        MediatR.IRequestHandler<RequestAsyncWithoutResponse>
{
    public async ValueTask Handle(RequestAsyncWithoutResponse request, CancellationToken cancellationToken) =>
        await ValueTask.CompletedTask;

    async Task MediatR.IRequestHandler<RequestAsyncWithoutResponse>.Handle(
        RequestAsyncWithoutResponse request,
        CancellationToken cancellationToken
    ) => await Task.CompletedTask;
}

[ConfigSource]
public class RequestWithoutResponseBenchmarks
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
                            ),
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
    private RequestWithoutResponseHandler _handler;
    private RequestWithoutResponse _request;
    private RequestAsyncWithoutResponseHandler _handlerAsync;
    private RequestAsyncWithoutResponse _requestAsync;

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
        _handler = _serviceProvider.GetRequiredService<RequestWithoutResponseHandler>();
        _handlerAsync = _serviceProvider.GetRequiredService<RequestAsyncWithoutResponseHandler>();
        _request = new();
        _requestAsync = new();
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
    public Task SendRequest_MediatR()
    {
        return _mediatr.Send(_request, CancellationToken.None);
    }

    [Benchmark]
    public ValueTask SendRequest_IMediator()
    {
        return _mediator.Send(_request, CancellationToken.None);
    }

    [Benchmark]
    public ValueTask SendRequest_Mediator()
    {
        return _concreteMediator.Send(_request, CancellationToken.None);
    }

    [Benchmark(Baseline = true)]
    public ValueTask SendRequest_Baseline()
    {
        return _handler.Handle(_request, CancellationToken.None);
    }

    [Benchmark]
    public Task SendRequest_Async_MediatR()
    {
        return _mediatr.Send(_requestAsync, CancellationToken.None);
    }

    [Benchmark]
    public ValueTask SendRequest_Async_IMediator()
    {
        return _mediator.Send(_requestAsync, CancellationToken.None);
    }

    [Benchmark]
    public ValueTask SendRequest_Async_Mediator()
    {
        return _concreteMediator.Send(_requestAsync, CancellationToken.None);
    }

    [Benchmark]
    public ValueTask SendRequest_Async_Baseline()
    {
        return _handlerAsync.Handle(_requestAsync, CancellationToken.None);
    }
}
