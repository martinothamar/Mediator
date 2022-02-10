using MediatR;
using MessagePipe;
using Microsoft.Extensions.DependencyInjection;

#nullable disable

namespace Mediator.Benchmarks.Request;

public sealed record SomeRequest(Guid Id) : IRequest<SomeResponse>, MediatR.IRequest<SomeResponse>;

public sealed record SomeResponse(Guid Id);

public sealed class SomeHandlerClass :
    IRequestHandler<SomeRequest, SomeResponse>,
    MediatR.IRequestHandler<SomeRequest, SomeResponse>,
    IAsyncRequestHandler<SomeRequest, SomeResponse>
{
    private static readonly SomeResponse _response = new SomeResponse(Guid.NewGuid());

    private static readonly Task<SomeResponse> _tResponse = Task.FromResult(_response);
    private static ValueTask<SomeResponse> _vtResponse => new ValueTask<SomeResponse>(_response);

    public ValueTask<SomeResponse> Handle(SomeRequest request, CancellationToken cancellationToken) => _vtResponse;

    Task<SomeResponse> MediatR.IRequestHandler<SomeRequest, SomeResponse>.Handle(SomeRequest request, CancellationToken cancellationToken) => _tResponse;

    public ValueTask<SomeResponse> InvokeAsync(SomeRequest request, CancellationToken cancellationToken) => _vtResponse;
}

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest, MethodOrderPolicy.Declared)]
public class RequestBenchmarks
{
    private IServiceProvider _serviceProvider;
    private IMediator _mediator;
    private Mediator _concreteMediator;
    private MediatR.IMediator _mediatr;
    private IAsyncRequestHandler<SomeRequest, SomeResponse> _messagePipeHandler;
    private SomeHandlerClass _handler;
    private SomeRequest _request;

    [GlobalSetup]
    public void Setup()
    {
        var services = new ServiceCollection();
        services.AddMediator();
        services.AddMediatR(config => config.AsSingleton(), typeof(SomeHandlerClass).Assembly);
        services.AddMessagePipe();

        _serviceProvider = services.BuildServiceProvider();
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
