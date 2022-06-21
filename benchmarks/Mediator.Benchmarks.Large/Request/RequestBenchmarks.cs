using Microsoft.Extensions.DependencyInjection;

namespace Mediator.Benchmarks.Request;

public sealed record SomeRequest(Guid Id) : IRequest<SomeResponse>;

public sealed record SomeResponse(Guid Id);

public sealed class SomeHandlerClass : IRequestHandler<SomeRequest, SomeResponse>
{
    private static readonly SomeResponse _response = new SomeResponse(Guid.NewGuid());
    private static ValueTask<SomeResponse> _vtResponse => new ValueTask<SomeResponse>(_response);

    public ValueTask<SomeResponse> Handle(SomeRequest request, CancellationToken cancellationToken) => _vtResponse;
}

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest, MethodOrderPolicy.Declared)]
[RankColumn]
//[EventPipeProfiler(EventPipeProfile.CpuSampling)]
//[DisassemblyDiagnoser]
//[InliningDiagnoser(logFailuresOnly: true, allowedNamespaces: new[] { "Mediator" })]
public class RequestBenchmarks
{
    private IServiceProvider _serviceProvider;
    private IServiceScope _serviceScope;
    private IMediator _mediator;
    private Mediator _concreteMediator;
    private SomeHandlerClass _handler;
    private SomeRequest _request;

    [Params(MediatorConfig.Lifetime)]
    public ServiceLifetime ServiceLifetime { get; set; } = MediatorConfig.Lifetime;

    [GlobalSetup]
    public void Setup()
    {
        var services = new ServiceCollection();
        services.AddMediator(opts => opts.ServiceLifetime = ServiceLifetime);

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
        _handler = _serviceProvider.GetRequiredService<SomeHandlerClass>();
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
    public ValueTask<SomeResponse> SendRequest_IMediator()
    {
        return _mediator.Send(_request, CancellationToken.None);
    }

    [Benchmark]
    public ValueTask<SomeResponse> SendRequest_Mediator()
    {
        return _concreteMediator.Send(_request, CancellationToken.None);
    }

    [Benchmark(Baseline = true)]
    public ValueTask<SomeResponse> SendRequest_Baseline()
    {
        return _handler.Handle(_request, CancellationToken.None);
    }
}
