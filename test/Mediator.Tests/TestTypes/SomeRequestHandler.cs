using System.Collections.Concurrent;

namespace Mediator.Tests.TestTypes;

public sealed class SomeRequestHandler : IRequestHandler<SomeRequest, SomeResponse>
{
    public ValueTask<SomeResponse> Handle(SomeRequest request, CancellationToken cancellationToken) => new ValueTask<SomeResponse>(new SomeResponse(request.Id));
}

public sealed class SomeRequestWithoutResponseHandler : IRequestHandler<SomeRequestWithoutResponse>
{
    internal static readonly ConcurrentBag<Guid> Ids = new();

    public ValueTask<Unit> Handle(SomeRequestWithoutResponse request, CancellationToken cancellationToken)
    {
        Ids.Add(request.Id);
        return default;
    }
}

public static class SomeStaticClass
{
    public sealed record SomeStaticNestedRequest(Guid Id) : IRequest<SomeResponse>;

    public sealed class SomeStaticNestedHandler : IRequestHandler<SomeStaticNestedRequest, SomeResponse>
    {
        internal static readonly ConcurrentBag<Guid> Ids = new();

        public async ValueTask<SomeResponse> Handle(SomeStaticNestedRequest request, CancellationToken cancellationToken)
        {
            await Task.Yield();
            Ids.Add(request.Id);
            return new SomeResponse(request.Id);
        }
    }
}

public sealed class SomeOtherClass
{
    public sealed record SomeNestedRequest(Guid Id) : IRequest<SomeResponse>;

    public sealed class SomeNestedHandler : IRequestHandler<SomeNestedRequest, SomeResponse>
    {
        internal static readonly ConcurrentBag<Guid> Ids = new();

        public async ValueTask<SomeResponse> Handle(SomeNestedRequest request, CancellationToken cancellationToken)
        {
            await Task.Delay(1000, cancellationToken);
            Ids.Add(request.Id);
            return new SomeResponse(request.Id);
        }
    }
}
