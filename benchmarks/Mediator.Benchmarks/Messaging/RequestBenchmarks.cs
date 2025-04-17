using Microsoft.Extensions.DependencyInjection;

namespace Mediator.Benchmarks.Messaging;

public sealed record Request(Guid Id) : IRequest<Response>, MediatR.IRequest<Response>;

public sealed record Response(Guid Id);

public sealed class RequestHandler : IRequestHandler<Request, Response>, MediatR.IRequestHandler<Request, Response>
{
    private static readonly Response _response = new Response(Guid.NewGuid());

    private static readonly Task<Response> _tResponse = Task.FromResult(_response);

    public ValueTask<Response> Handle(Request request, CancellationToken cancellationToken) =>
        new ValueTask<Response>(_response);

    Task<Response> MediatR.IRequestHandler<Request, Response>.Handle(
        Request request,
        CancellationToken cancellationToken
    ) => _tResponse;
}

[Config(typeof(Config))]
public class RequestBenchmarks
{
    private IServiceProvider _serviceProvider;
    private IServiceScope _serviceScope;
    private IMediator _mediator;
    private Mediator _concreteMediator;
    private MediatR.IMediator _mediatr;
    private RequestHandler _handler;
    private Request _request;

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
        _handler = _serviceProvider.GetRequiredService<RequestHandler>();
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
    public Task<Response> SendRequest_MediatR()
    {
        return _mediatr.Send(_request, CancellationToken.None);
    }

    [Benchmark]
    public ValueTask<Response> SendRequest_IMediator()
    {
        return _mediator.Send(_request, CancellationToken.None);
    }

    [Benchmark]
    public ValueTask<Response> SendRequest_Mediator()
    {
        return _concreteMediator.Send(_request, CancellationToken.None);
    }

    [Benchmark(Baseline = true)]
    public ValueTask<Response> SendRequest_Baseline()
    {
        return _handler.Handle(_request, CancellationToken.None);
    }
}
