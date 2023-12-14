using Mediator;
using InternalMessages.Domain;

namespace InternalMessages.Application;

internal sealed class PingHandler : IRequestHandler<Ping, Pong>
{
    private readonly IMediator _mediator;

    public PingHandler(IMediator mediator) => _mediator = mediator;

    public async ValueTask<Pong> Handle(Ping request, CancellationToken cancellationToken)
    {
        await _mediator.Publish(new PingPonged(request.Id), cancellationToken);
        return new Pong(request.Id);
    }
}

internal sealed class PingPongedHandler : INotificationHandler<PingPonged>
{
    public ValueTask Handle(PingPonged notification, CancellationToken cancellationToken)
    {
        Console.WriteLine($"PingPonged: {notification.Id}");
        return default;
    }
}
