using Microsoft.Extensions.DependencyInjection;

namespace Mediator.Benchmarks.Messaging.Comparison;

public sealed record Request(Guid Id) : IRequest<Response>, MediatR.IRequest<Response>;

public sealed record Notification(Guid Id) : INotification, MediatR.INotification;

public sealed record Response(Guid Id);

public sealed record StreamRequest(Guid Id) : IStreamRequest<Response>, MediatR.IStreamRequest<Response>;

public sealed class Handler
    : IRequestHandler<Request, Response>,
        MediatR.IRequestHandler<Request, Response>,
        IStreamRequestHandler<StreamRequest, Response>,
        MediatR.IStreamRequestHandler<StreamRequest, Response>,
        INotificationHandler<Notification>,
        MediatR.INotificationHandler<Notification>
{
    private static readonly Response _response = new Response(Guid.NewGuid());

    private static readonly Task<Response> _tResponse = Task.FromResult(_response);

    public ValueTask<Response> Handle(Request request, CancellationToken cancellationToken) =>
        new ValueTask<Response>(_response);

    Task<Response> MediatR.IRequestHandler<Request, Response>.Handle(
        Request request,
        CancellationToken cancellationToken
    ) => _tResponse;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    async IAsyncEnumerable<Response> _enumerate()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    {
        for (int i = 0; i < 3; i++)
        {
            yield return _response;
        }
    }

    public IAsyncEnumerable<Response> Handle(StreamRequest request, CancellationToken cancellationToken) =>
        _enumerate();

    IAsyncEnumerable<Response> MediatR.IStreamRequestHandler<StreamRequest, Response>.Handle(
        StreamRequest request,
        CancellationToken cancellationToken
    ) => _enumerate();

    public ValueTask Handle(Notification notification, CancellationToken cancellationToken) => default;

    Task MediatR.INotificationHandler<Notification>.Handle(
        Notification notification,
        CancellationToken cancellationToken
    ) => Task.CompletedTask;
}

[Config(typeof(Config))]
public class ComparisonBenchmarks
{
    private IServiceProvider _serviceProvider;
    private IServiceScope _serviceScope;
    private IMediator _mediator;
    private Mediator _concreteMediator;
    private MediatR.IMediator _mediatr;
    private Request _request;
    private object _requestObj;
    private StreamRequest _streamRequest;
    private object _streamRequestObj;
    private Notification _notification;
    private object _notificationObj;

    private sealed class Config : ManualConfig
    {
        public Config()
        {
            this.SummaryStyle = SummaryStyle.Default.WithRatioStyle(RatioStyle.Trend);
            this.AddDiagnoser(MemoryDiagnoser.Default);
            this.AddColumn(RankColumn.Arabic);
            this.AddColumn(CategoriesColumn.Default);
            this.AddLogicalGroupRules(BenchmarkLogicalGroupRule.ByCategory);
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
            opts.RegisterServicesFromAssembly(typeof(Handler).Assembly);
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
        _request = new(Guid.NewGuid());
        _requestObj = _request;
        _streamRequest = new StreamRequest(Guid.NewGuid());
        _streamRequestObj = _streamRequest;
        _notification = new Notification(Guid.NewGuid());
        _notificationObj = _notification;
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        if (_serviceScope is not null)
            _serviceScope.Dispose();
        else
            (_serviceProvider as IDisposable)?.Dispose();
    }

    // Normal requests

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Request", "Concrete")]
    public Task<Response> Request_MediatR()
    {
        return _mediatr.Send(_request, CancellationToken.None);
    }

    [Benchmark]
    [BenchmarkCategory("Request", "Concrete")]
    public ValueTask<Response> Request_IMediator()
    {
        return _mediator.Send(_request, CancellationToken.None);
    }

    [Benchmark]
    [BenchmarkCategory("Request", "Concrete")]
    public ValueTask<Response> Request_Mediator()
    {
        return _concreteMediator.Send(_request, CancellationToken.None);
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Request", "Object")]
    public Task<object> Request_MediatR_Object()
    {
        return _mediatr.Send(_requestObj, CancellationToken.None);
    }

    [Benchmark]
    [BenchmarkCategory("Request", "Object")]
    public ValueTask<object> Request_IMediator_Object()
    {
        return _mediator.Send(_requestObj, CancellationToken.None);
    }

    // Streaming requests

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("StreamRequest", "Concrete")]
    public async ValueTask StreamRequest_MediatR()
    {
        await foreach (var response in _mediatr.CreateStream(_streamRequest, CancellationToken.None))
        {
            _ = response;
        }
    }

    [Benchmark]
    [BenchmarkCategory("StreamRequest", "Concrete")]
    public async ValueTask StreamRequest_IMediator()
    {
        await foreach (var response in _mediator.CreateStream(_streamRequest, CancellationToken.None))
        {
            _ = response;
        }
    }

    [Benchmark]
    [BenchmarkCategory("StreamRequest", "Concrete")]
    public async ValueTask StreamRequest_Mediator()
    {
        await foreach (var response in _concreteMediator.CreateStream(_streamRequest, CancellationToken.None))
        {
            _ = response;
        }
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("StreamRequest", "Object")]
    public async ValueTask StreamRequest_MediatR_Object()
    {
        await foreach (var response in _mediatr.CreateStream(_streamRequestObj, CancellationToken.None))
        {
            _ = response;
        }
    }

    [Benchmark]
    [BenchmarkCategory("StreamRequest", "Object")]
    public async ValueTask StreamRequest_IMediator_Object()
    {
        await foreach (var response in _mediator.CreateStream(_streamRequestObj, CancellationToken.None))
        {
            _ = response;
        }
    }

    // Notifications

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Notification", "Concrete")]
    public Task Notification_MediatR()
    {
        return _mediatr.Publish(_notification, CancellationToken.None);
    }

    [Benchmark]
    [BenchmarkCategory("Notification", "Concrete")]
    public ValueTask Notification_IMediator()
    {
        return _mediator.Publish(_notification, CancellationToken.None);
    }

    [Benchmark]
    [BenchmarkCategory("Notification", "Concrete")]
    public ValueTask Notification_Mediator()
    {
        return _concreteMediator.Publish(_notification, CancellationToken.None);
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Notification", "Object")]
    public Task Notification_MediatR_Object()
    {
        return _mediatr.Publish(_notificationObj, CancellationToken.None);
    }

    [Benchmark]
    [BenchmarkCategory("Notification", "Object")]
    public ValueTask Notification_IMediator_Object()
    {
        return _mediator.Publish(_notificationObj, CancellationToken.None);
    }
}
