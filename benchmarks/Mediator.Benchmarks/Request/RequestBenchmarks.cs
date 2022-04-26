using MediatR;
using MessagePipe;
using Microsoft.Extensions.DependencyInjection;

namespace Mediator.Benchmarks.Request;

public sealed record SomeRequest(Guid Id) : IRequest<SomeResponse>, MediatR.IRequest<SomeResponse>;

public sealed record SomeResponse(Guid Id);

public sealed class SomeHandlerClass
    : IRequestHandler<SomeRequest, SomeResponse>,
      MediatR.IRequestHandler<SomeRequest, SomeResponse>,
      IAsyncRequestHandler<SomeRequest, SomeResponse>
{
    private static readonly SomeResponse _response = new SomeResponse(Guid.NewGuid());

    private static readonly Task<SomeResponse> _tResponse = Task.FromResult(_response);
    private static ValueTask<SomeResponse> _vtResponse => new ValueTask<SomeResponse>(_response);

    public ValueTask<SomeResponse> Handle(SomeRequest request, CancellationToken cancellationToken) => _vtResponse;

    Task<SomeResponse> MediatR.IRequestHandler<SomeRequest, SomeResponse>.Handle(
        SomeRequest request,
        CancellationToken cancellationToken
    ) => _tResponse;

    public ValueTask<SomeResponse> InvokeAsync(SomeRequest request, CancellationToken cancellationToken) => _vtResponse;
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
    private MediatR.IMediator _mediatr;
    private IAsyncRequestHandler<SomeRequest, SomeResponse> _messagePipeHandler;
    private SomeHandlerClass _handler;
    private SomeRequest _request;

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
        services.AddMessagePipe(
            opts =>
            {
                opts.InstanceLifetime = ServiceLifetime switch
                {
                    ServiceLifetime.Singleton => InstanceLifetime.Singleton,
                    ServiceLifetime.Scoped => InstanceLifetime.Scoped,
                    ServiceLifetime.Transient => InstanceLifetime.Transient,
                    _ => throw new InvalidOperationException(),
                };
            }
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
        _messagePipeHandler = _serviceProvider.GetRequiredService<IAsyncRequestHandler<SomeRequest, SomeResponse>>();
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
    public Task<SomeResponse> SendRequest_MediatR()
    {
        return _mediatr.Send(_request, CancellationToken.None);
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

    [Benchmark]
    public ValueTask<SomeResponse> SendRequest_MessagePipe()
    {
        return _messagePipeHandler.InvokeAsync(_request, CancellationToken.None);
    }

    [Benchmark(Baseline = true)]
    public ValueTask<SomeResponse> SendRequest_Baseline()
    {
        return _handler.Handle(_request, CancellationToken.None);
    }
}
