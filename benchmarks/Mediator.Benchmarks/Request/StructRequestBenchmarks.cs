using MediatR;
using MessagePipe;
using Microsoft.Extensions.DependencyInjection;

namespace Mediator.Benchmarks.Request;

public readonly struct SomeStructRequest : IRequest<SomeResponse>, MediatR.IRequest<SomeResponse>
{
    public readonly Guid Id;
    public readonly Guid CorrelationId;
    public readonly Guid CausationId;
    public readonly DateTimeOffset TimeStamp;
    public readonly uint Version;

    public SomeStructRequest(Guid id)
    {
        Id = id;
        CorrelationId = Guid.NewGuid();
        CausationId = Guid.NewGuid();
        TimeStamp = DateTimeOffset.UtcNow;
        Version = 1;
    }
}

public sealed class SomeStructHandler
    : IRequestHandler<SomeStructRequest, SomeResponse>,
      MediatR.IRequestHandler<SomeStructRequest, SomeResponse>,
      IAsyncRequestHandler<SomeStructRequest, SomeResponse>
{
    private static readonly SomeResponse _response = new SomeResponse(Guid.NewGuid());

    private static readonly Task<SomeResponse> _tResponse = Task.FromResult(_response);
    private static ValueTask<SomeResponse> _vtResponse => new ValueTask<SomeResponse>(_response);

    public ValueTask<SomeResponse> Handle(SomeStructRequest request, CancellationToken cancellationToken) =>
        _vtResponse;

    Task<SomeResponse> MediatR.IRequestHandler<SomeStructRequest, SomeResponse>.Handle(
        SomeStructRequest request,
        CancellationToken cancellationToken
    ) => _tResponse;

    public ValueTask<SomeResponse> InvokeAsync(SomeStructRequest request, CancellationToken cancellationToken) =>
        _vtResponse;
}

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest, MethodOrderPolicy.Declared)]
[RankColumn]
//[EventPipeProfiler(EventPipeProfile.CpuSampling)]
//[DisassemblyDiagnoser]
//[InliningDiagnoser(logFailuresOnly: true, allowedNamespaces: new[] { "Mediator" })]
public class StructRequestBenchmarks
{
    private IServiceProvider _serviceProvider;
    private IServiceScope _serviceScope;
    private IMediator _mediator;
    private Mediator _concreteMediator;
    private MediatR.IMediator _mediatr;
    private IAsyncRequestHandler<SomeStructRequest, SomeResponse> _messagePipeHandler;
    private SomeStructHandler _handler;
    private SomeStructRequest _request;

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
        _messagePipeHandler = _serviceProvider.GetRequiredService<
            IAsyncRequestHandler<SomeStructRequest, SomeResponse>
        >();
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
    public Task<SomeResponse> SendStructRequest_MediatR()
    {
        return _mediatr.Send(_request, CancellationToken.None);
    }

    [Benchmark]
    public ValueTask<SomeResponse> SendStructRequest_IMediator()
    {
        return _mediator.Send(_request, CancellationToken.None);
    }

    [Benchmark]
    public ValueTask<SomeResponse> SendStructRequest_Mediator()
    {
        return _concreteMediator.Send(_request, CancellationToken.None);
    }

    [Benchmark]
    public ValueTask<SomeResponse> SendStructRequest_MessagePipe()
    {
        return _messagePipeHandler.InvokeAsync(_request, CancellationToken.None);
    }

    [Benchmark(Baseline = true)]
    public ValueTask<SomeResponse> SendStructRequest_Baseline()
    {
        return _handler.Handle(_request, CancellationToken.None);
    }
}
