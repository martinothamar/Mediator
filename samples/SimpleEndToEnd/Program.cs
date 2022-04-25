using Mediator;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

var services = new ServiceCollection();

services.AddMediator();

// Ordering of pipeline behavior registrations matter!
services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(ErrorLoggerHandler<,>));
services.AddSingleton<IPipelineBehavior<Ping, Pong>, PingValidator>();

var sp = services.BuildServiceProvider();

var mediator = sp.GetRequiredService<IMediator>();

var ping = new Ping(Guid.NewGuid());
var pong = await mediator.Send(ping);
Debug.Assert(ping.Id == pong.Id);
Console.WriteLine("Got the right ID: " + (ping, pong));

ping = ping with { Id = default };
try
{
    pong = await mediator.Send(ping);
    Debug.Assert(false, "We don't expect to get here, the PingValidator should throw ArgumentException!");
}
catch (ArgumentException) // The ErrorLoggerHandler should handle the logging for this sample
{ }

var statsHandler = sp.GetRequiredService<StatsNotificationHandler>();
var (messageCount, messageErrorCount) = statsHandler.Stats;

// First Ping succeeded, second failed validation
Debug.Assert(messageCount == 2, "We sent 2 pings");
Debug.Assert(messageErrorCount == 1, "1 of them failed validation");

Console.WriteLine("Done!");

// Types used below

public sealed record Ping(Guid Id) : IRequest<Pong>;

public sealed record Pong(Guid Id);

public sealed class PingHandler : IRequestHandler<Ping, Pong>
{
    public ValueTask<Pong> Handle(Ping request, CancellationToken cancellationToken)
    {
        return new ValueTask<Pong>(new Pong(request.Id));
    }
}

public sealed class PingValidator : IPipelineBehavior<Ping, Pong>
{
    public ValueTask<Pong> Handle(
        Ping request,
        CancellationToken cancellationToken,
        MessageHandlerDelegate<Ping, Pong> next
    )
    {
        if (request is null || request.Id == default)
            throw new ArgumentException("Invalid input");

        return next(request, cancellationToken);
    }
}

public sealed record ErrorMessage(Exception Exception) : INotification;

public sealed record SuccessfulMessage() : INotification;

public sealed class ErrorLoggerHandler<TMessage, TResponse> : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage // Constrained to IMessage, or constrain to IBaseCommand or any custom interface you've implemented
{
    private readonly IMediator _mediator;

    public ErrorLoggerHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async ValueTask<TResponse> Handle(
        TMessage message,
        CancellationToken cancellationToken,
        MessageHandlerDelegate<TMessage, TResponse> next
    )
    {
        try
        {
            var response = await next(message, cancellationToken);
            await _mediator.Publish(new SuccessfulMessage());
            return response;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error handling message: " + ex.Message);
            await _mediator.Publish(new ErrorMessage(ex));
            throw;
        }
    }
}

// Notification handlers are automatically added to DI container

public sealed class ErrorNotificationHandler : INotificationHandler<ErrorMessage>
{
    public ValueTask Handle(ErrorMessage error, CancellationToken cancellationToken)
    {
        // Could log to application insights or something...
        return default;
    }
}

public sealed class StatsNotificationHandler : INotificationHandler<INotification> // or any other interface deriving from INotification
{
    private long _messageCount;
    private long _messageErrorCount;

    public (long MessageCount, long MessageErrorCount) Stats => (_messageCount, _messageErrorCount);

    public ValueTask Handle(INotification notification, CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref _messageCount);
        if (notification is ErrorMessage)
            Interlocked.Increment(ref _messageErrorCount);
        return default;
    }
}

public sealed class GenericNotificationHandler<TNotification> : INotificationHandler<TNotification>
    where TNotification : INotification // Notification handlers will be registered as open constrained types
{
    public ValueTask Handle(TNotification notification, CancellationToken cancellationToken)
    {
        return default;
    }
}
