using Mediator;

namespace AspNetCoreBlazor.Client.Pages;

public sealed record IncrementCounter() : IRequest<long>;

public sealed class IncrementCounterHandler : IRequestHandler<IncrementCounter, long>
{
    private long _counter = 0;

    public long Current => Interlocked.Read(ref _counter);

    public ValueTask<long> Handle(IncrementCounter request, CancellationToken cancellationToken)
    {
        var count = Interlocked.Increment(ref _counter);
        return new ValueTask<long>(count);
    }
}
