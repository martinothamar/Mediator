using System;
using System.Threading;
using System.Threading.Tasks;

namespace Mediator.Tests.TestTypes
{
    public sealed class SomeCommandHandler : ICommandHandler<SomeCommand, SomeResponse>
    {
        internal static Guid Id;

        public ValueTask<SomeResponse> Handle(SomeCommand command, CancellationToken cancellationToken)
        {
            Id = command.Id;
            return new ValueTask<SomeResponse>(new SomeResponse(Id));
        }
    }

    public sealed class SomeCommandWithoutResponseHandler : ICommandHandler<SomeCommandWithoutResponse>
    {
        internal static Guid Id;

        public ValueTask<Unit> Handle(SomeCommandWithoutResponse command, CancellationToken cancellationToken)
        {
            Id = command.Id;
            return default;
        }
    }

    public sealed class SomeStructCommandHandler : ICommandHandler<SomeStructCommand>
    {
        internal static Guid Id;

        public ValueTask<Unit> Handle(SomeStructCommand command, CancellationToken cancellationToken)
        {
            Id = command.Id;
            return default;
        }
    }
}
