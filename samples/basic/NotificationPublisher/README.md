## NotificationPublisher

Simple showcase of using a custom notification publisher, by implementing `INotificationPublisher`.
The custom publisher catches all exceptions and logs them, a so called fire-and-forget implementation.

### Build and run

```console
$ dotnet run
Publishing!
-----------------------------------
MyNotificationHandler - 6ae7d56b-8a2f-404c-a24b-c5df1e6691d2
System.Exception: Something went wrong!
   at MyNotificationHandler.Handle(Notification notification, CancellationToken cancellationToken) in /home/martin/code/private/Mediator/samples/basic/NotificationPublisher/Program.cs:line 79
   at MyNotificationPublisher.Publish[TNotification](NotificationHandlers`1 handlers, TNotification notification, CancellationToken cancellationToken) in /home/martin/code/private/Mediator/samples/basic/NotificationPublisher/Program.cs:line 46
-----------------------------------
Finished publishing!
```

