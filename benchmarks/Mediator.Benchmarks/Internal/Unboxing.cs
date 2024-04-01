using System.Runtime.CompilerServices;
using Mediator.Benchmarks.Notification;

namespace Mediator.Benchmarks.Internal;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest, MethodOrderPolicy.Declared)]
[RankColumn]
public class UnboxingBenchmarks
{
    private object _handler;
    private SingleHandlerNotification _notification;

    [GlobalSetup]
    public void Setup()
    {
        _handler = new SingleHandler();
        _notification = new SingleHandlerNotification(Guid.NewGuid());
    }

    [Benchmark]
    public ValueTask Cast()
    {
        var handler = (INotificationHandler<SingleHandlerNotification>)_handler;
        return handler.Handle(_notification, default);
    }

    [Benchmark]
    public ValueTask Patternmatch()
    {
        if (_handler is not INotificationHandler<SingleHandlerNotification> handler)
            return default;
        return handler.Handle(_notification, default);
    }

    [Benchmark]
    public ValueTask UnsafeAs()
    {
        var handler = Unsafe.As<INotificationHandler<SingleHandlerNotification>>(_handler);
        return handler.Handle(_notification, default);
    }

    [Benchmark(Baseline = true)]
    public ValueTask UnsafeAsReinterpret()
    {
        var handler = Unsafe.As<object, INotificationHandler<SingleHandlerNotification>>(ref _handler);
        return handler.Handle(_notification, default);
    }
}
