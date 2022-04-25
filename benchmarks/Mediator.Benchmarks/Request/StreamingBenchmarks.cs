using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Mediator.Benchmarks.Request;

public sealed record SomeStreamRequest(Guid Id) : IStreamRequest<SomeResponse>, MediatR.IStreamRequest<SomeResponse>;

public sealed class SomeStreamHandlerClass
    : IStreamRequestHandler<SomeStreamRequest, SomeResponse>,
      MediatR.IStreamRequestHandler<SomeStreamRequest, SomeResponse>
{
    private static readonly SomeResponse _response = new SomeResponse(Guid.NewGuid());

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    async IAsyncEnumerable<SomeResponse> _enumerate()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    {
        for (int i = 0; i < 10; i++)
        {
            yield return _response;
        }
    }

    public IAsyncEnumerable<SomeResponse> Handle(SomeStreamRequest request, CancellationToken cancellationToken) =>
        _enumerate();

    IAsyncEnumerable<SomeResponse> MediatR.IStreamRequestHandler<SomeStreamRequest, SomeResponse>.Handle(
        SomeStreamRequest request,
        CancellationToken cancellationToken
    ) => _enumerate();
}

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest, MethodOrderPolicy.Declared)]
[RankColumn]
//[EventPipeProfiler(EventPipeProfile.CpuSampling)]
//[DisassemblyDiagnoser]
//[InliningDiagnoser(logFailuresOnly: true, allowedNamespaces: new[] { "Mediator" })]
public class StreamingBenchmarks
{
    private IServiceProvider _serviceProvider;
    private IServiceScope _serviceScope;
    private IMediator _mediator;
    private Mediator _concreteMediator;
    private MediatR.IMediator _mediatr;
    private SomeStreamHandlerClass _handler;
    private SomeStreamRequest _request;

    [Params(MediatorConfig.Lifetime)]
    public ServiceLifetime ServiceLifetime { get; set; } = MediatorConfig.Lifetime;

    [GlobalSetup]
    public void Setup()
    {
        var services = new ServiceCollection();
        services.AddMediator(opts => opts.ServiceLifetime = ServiceLifetime);
        services.AddMediatR(
            opts =>
            {
                _ = ServiceLifetime switch
                {
                    ServiceLifetime.Singleton => opts.AsSingleton(),
                    ServiceLifetime.Scoped => opts.AsScoped(),
                    ServiceLifetime.Transient => opts.AsTransient(),
                    _ => throw new InvalidOperationException(),
                };
            },
            typeof(SomeHandlerClass).Assembly
        );

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
        _handler = _serviceProvider.GetRequiredService<SomeStreamHandlerClass>();
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
