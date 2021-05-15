using System;
using System.Threading;
using System.Threading.Tasks;

namespace Mediator.Tests.TestTypes
{
    public sealed class SomeCommandHandler : ICommandHandler<SomeCommand, SomeResponse>
    {
        internal Guid Id;

        public ValueTask<SomeResponse> Handle(SomeCommand command, CancellationToken cancellationToken)
        {
            Id = command.Id;
            return new ValueTask<SomeResponse>(new SomeResponse(Id));
        }
    }

    public sealed class SomeCommandWithoutResponseHandler : ICommandHandler<SomeCommandWithoutResponse>
    {
        internal Guid Id;

        public ValueTask Handle(SomeCommandWithoutResponse command, CancellationToken cancellationToken)
        {
            Id = command.Id;
            return default;
        }
    }
}
