using Mediator;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

var services = new ServiceCollection();

// This extensions method is generated, and is put in the "Microsoft.Extensions.DependencyInjection" namespace.
// We override the namespace in the "MediatorOptions" attribute above.
services.AddMediator(
    options =>
    {
        options.Namespace = null;
        options.ServiceLifetime = ServiceLifetime.Singleton;
    }
);

// Standard handlers are added by default, but we need to add pipeline steps manually.
// Here are two examples.
services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(GenericLoggerHandler<,>)); // This will run 1st
services.AddSingleton<IPipelineBehavior<Ping, Pong>, PingValidator>(); // This will run 2nd

var serviceProvider = services.BuildServiceProvider();

var mediator = serviceProvider.GetRequiredService<IMediator>();

var id = Guid.NewGuid();
var request = new Ping(id);

var response = await mediator.Send(request);

return response.Id == id ? 0 : 1;

//
// Here are the types used
//

public sealed record Ping(Guid Id) : IRequest<Pong>;

public sealed record Pong(Guid Id);

public sealed class GenericLoggerHandler<TMessage, TResponse> : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
    public async ValueTask<TResponse> Handle(
        TMessage message,
        CancellationToken cancellationToken,
        MessageHandlerDelegate<TMessage, TResponse> next
    )
    {
        try
        {
            var response = await next(message, cancellationToken);
            return response;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
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
        return next(request, cancellationToken);
    }
}

public sealed class PingHandler : IRequestHandler<Ping, Pong>
{
    public ValueTask<Pong> Handle(Ping request, CancellationToken cancellationToken)
    {
        return new ValueTask<Pong>(new Pong(request.Id));
    }
}


public sealed record Pinged(Ping Ping) : INotification;

public sealed class AllNotificationsHandler : INotificationHandler<INotification>
{
    public ValueTask Handle(INotification notification, CancellationToken cancellationToken)
    {
        return default;
    }
}

public sealed record IntegrationEvent<T>(T Message) : INotification
    where T : IMessage;

public sealed class PingedHandler : INotificationHandler<Pinged>
{
    public ValueTask Handle(Pinged notification, CancellationToken cancellationToken)
    {
        return default;
    }
}
