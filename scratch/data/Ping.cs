using Mediator;

namespace data;

public sealed record Ping : ICommand;

public sealed class PingHandler : ICommandHandler<Ping>
{
    public ValueTask<Unit> Handle(Ping command, CancellationToken cancellationToken)
    {
        Console.WriteLine("Ping");
        return default;
    }
}
