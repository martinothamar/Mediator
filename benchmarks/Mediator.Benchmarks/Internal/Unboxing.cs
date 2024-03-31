using System.Runtime.CompilerServices;
using Mediator.Benchmarks.Notification;

namespace Mediator.Benchmarks.Internal;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest, MethodOrderPolicy.Declared)]
[RankColumn]
public class UnboxingBenchmarks
{
    private object _handler;
    private SomeNotification _notification;

    [GlobalSetup]
    public void Setup()
    {
        _handler = new SomeHandlerClass();
        _notification = new SomeNotification(Guid.NewGuid());
    }

    [Benchmark]
    public ValueTask Cast()
    {
        var handler = (INotificationHandler<SomeNotification>)_handler;
        return handler.Handle(_notification, default);
    }

    [Benchmark]
    public ValueTask Patternmatch()
    {
        if (_handler is not INotificationHandler<SomeNotification> handler)
            return default;
        return handler.Handle(_notification, default);
    }

    [Benchmark]
    public ValueTask UnsafeAs()
    {
        var handler = Unsafe.As<INotificationHandler<SomeNotification>>(_handler);
        return handler.Handle(_notification, default);
    }

    [Benchmark(Baseline = true)]
    public ValueTask UnsafeAsReinterpret()
    {
        var handler = Unsafe.As<object, INotificationHandler<SomeNotification>>(ref _handler);
        return handler.Handle(_notification, default);
    }
}
