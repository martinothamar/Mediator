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

public sealed class SomeStructHandler
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

[Config(typeof(Config))]
public class StructRequestBenchmarks
{
    private IServiceProvider _serviceProvider;
    private IServiceScope _serviceScope;
    private IMediator _mediator;
    private Mediator _concreteMediator;
    private MediatR.IMediator _mediatr;
    private SomeStructHandler _handler;
    private StructRequest _request;

    private sealed class Config : ManualConfig
    {
        public Config()
        {
            this.SummaryStyle = SummaryStyle.Default.WithRatioStyle(RatioStyle.Trend);
            this.AddDiagnoser(MemoryDiagnoser.Default);
            this.AddColumn(RankColumn.Arabic);
            this.Orderer = new DefaultOrderer(SummaryOrderPolicy.FastestToSlowest, MethodOrderPolicy.Declared);
        }
    }

    [Params(Mediator.ServiceLifetime)]
    public ServiceLifetime ServiceLifetime { get; set; }

    [GlobalSetup]
    public void Setup()
    {
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
        _handler = _serviceProvider.GetRequiredService<SomeStructHandler>();
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
