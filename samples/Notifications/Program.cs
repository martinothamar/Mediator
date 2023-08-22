using Mediator;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

var services = new ServiceCollection();

services.AddMediator();

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

public sealed record Notification(Guid Id) : INotification;

public sealed class GenericNotificationHandler<TNotification> : INotificationHandler<TNotification>
    where TNotification : INotification
{
    public ValueTask Handle(TNotification notification, CancellationToken cancellationToken)
    {
        if (notification is Notification concrete)
            Console.WriteLine($"{GetType().Name} - {concrete.Id}");
        return default;
    }
}

public sealed class CatchAllNotificationHandler : INotificationHandler<INotification>
{
    public ValueTask Handle(INotification notification, CancellationToken cancellationToken)
    {
        if (notification is Notification concrete)
            Console.WriteLine($"{GetType().Name} - {concrete.Id}");
        return default;
    }
}

public sealed class ConcreteNotificationHandler : INotificationHandler<Notification>
{
    public ValueTask Handle(Notification notification, CancellationToken cancellationToken)
    {
        Console.WriteLine($"{GetType().Name} - {notification.Id}");
        return default;
    }
}
