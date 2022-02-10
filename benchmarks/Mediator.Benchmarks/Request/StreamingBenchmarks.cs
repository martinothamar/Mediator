using MediatR;
using Microsoft.Extensions.DependencyInjection;

#nullable disable

namespace Mediator.Benchmarks.Request;

public sealed record SomeStreamRequest(Guid Id) : IStreamRequest<SomeResponse>, MediatR.IStreamRequest<SomeResponse>;

public sealed class SomeStreamHandlerClass :
    IStreamRequestHandler<SomeStreamRequest, SomeResponse>,
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

    IAsyncEnumerable<SomeResponse> MediatR.IStreamRequestHandler<SomeStreamRequest, SomeResponse>.Handle(SomeStreamRequest request, CancellationToken cancellationToken) =>
        _enumerate();
}

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest, MethodOrderPolicy.Declared)]
public class StreamingBenchmarks
{
    private IServiceProvider _serviceProvider;
    private IMediator _mediator;
    private Mediator _concreteMediator;
    private MediatR.IMediator _mediatr;
    private SomeStreamHandlerClass _handler;
    private SomeStreamRequest _request;

    [GlobalSetup]
    public void Setup()
    {
        var services = new ServiceCollection();
        services.AddMediator();
        services.AddMediatR(config => config.AsSingleton(), typeof(SomeStreamHandlerClass).Assembly);

        _serviceProvider = services.BuildServiceProvider();
        _mediator = _serviceProvider.GetRequiredService<IMediator>();
        _concreteMediator = _serviceProvider.GetRequiredService<Mediator>();
        _mediatr = _serviceProvider.GetRequiredService<MediatR.IMediator>();
        _handler = _serviceProvider.GetRequiredService<SomeStreamHandlerClass>();
        _request = new(Guid.NewGuid());
    }

    [GlobalCleanup]
    public void Cleanup()
    {
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
