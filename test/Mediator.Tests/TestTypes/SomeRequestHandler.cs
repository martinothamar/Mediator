using System;
using System.Threading;
using System.Threading.Tasks;

namespace Mediator.Tests.TestTypes
{
    public sealed class SomeRequestHandler : IRequestHandler<SomeRequest, SomeResponse>
    {
        public ValueTask<SomeResponse> Handle(SomeRequest request, CancellationToken cancellationToken) => new ValueTask<SomeResponse>(new SomeResponse(request.Id));
    }

    public sealed class SomeRequestWithoutResponseHandler : IRequestHandler<SomeRequestWithoutResponse>
    {
        internal Guid Id;

        public ValueTask<Unit> Handle(SomeRequestWithoutResponse request, CancellationToken cancellationToken)
        {
            Id = request.Id;
            return default;
        }
    }

    public static class SomeStaticClass
    {
        public sealed record SomeStaticNestedRequest(Guid Id) : IRequest<SomeResponse>;

        public sealed class SomeStaticNestedHandler : IRequestHandler<SomeStaticNestedRequest, SomeResponse>
        {
            public Guid Id;

            public async ValueTask<SomeResponse> Handle(SomeStaticNestedRequest request, CancellationToken cancellationToken)
            {
                await Task.Yield();
                Id = request.Id;
                return new SomeResponse(Id);
            }
        }
    }

    public sealed class SomeOtherClass
    {
        public sealed record SomeNestedRequest(Guid Id) : IRequest<SomeResponse>;

        public sealed class SomeNestedHandler : IRequestHandler<SomeNestedRequest, SomeResponse>
        {
            public Guid Id;

            public async ValueTask<SomeResponse> Handle(SomeNestedRequest request, CancellationToken cancellationToken)
            {
                await Task.Delay(1000, cancellationToken);
                Id = request.Id;
                return new SomeResponse(Id);
            }
        }
    }
}
