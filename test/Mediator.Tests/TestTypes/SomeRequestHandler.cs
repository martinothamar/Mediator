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
}
