using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mediator;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.AddMediator(
    (MediatorOptions options) =>
    {
        options.Assemblies = [typeof(Notification)];
        options.NotificationPublisherType = typeof(MyNotificationPublisher);
    }
);

var serviceProvider = services.BuildServiceProvider();

var mediator = serviceProvider.GetRequiredService<IMediator>();

var id = Guid.NewGuid();
var notification = new Notification(id);

Console.WriteLine("Publishing!");
Console.WriteLine("-----------------------------------");

await mediator.Publish(notification);

Console.WriteLine("-----------------------------------");
Console.WriteLine("Finished publishing!");

return 0;

//
// Here are the types used
//

public sealed class MyNotificationPublisher : INotificationPublisher
{
    public async ValueTask Publish<TNotification>(
        NotificationHandlers<TNotification> handlers,
        TNotification notification,
        CancellationToken cancellationToken
    )
        where TNotification : INotification
    {
        try
        {
            // IsSingleHandler is a convenience method to check if there is only one handler
            // so that we can early exist. Used for optimization purposes by the built in implementations.
            if (handlers.IsSingleHandler(out var singleHandler))
            {
                await singleHandler.Handle(notification, cancellationToken);
                return;
            }
            // IsArray is a convenience method to check if the handlers are an array (for the built-in DI container in BCL, this is the case)
            // so that we can iterate and/or index directly. Used for optimization purposes by the built in implementations.
            else if (handlers.IsArray(out var array))
            {
                foreach (var handler in array)
                {
                    await handler.Handle(notification, cancellationToken);
                }
            }
            else
            {
                // Or we can just box the tasks and await them all
                await Task.WhenAll(
                    handlers.Select(handler => handler.Handle(notification, cancellationToken).AsTask())
                );
            }
        }
        catch (Exception ex)
        {
            // Notifications should be fire-and-forget, we just need to log it!
            Console.Error.WriteLine(ex);
        }
    }
}

public sealed record Notification(Guid Id) : INotification;

public sealed class MyNotificationHandler : INotificationHandler<Notification>
{
    public ValueTask Handle(Notification notification, CancellationToken cancellationToken)
    {
        Console.WriteLine($"{GetType().Name} - {notification.Id}");
        throw new Exception("Something went wrong!");
    }
}
