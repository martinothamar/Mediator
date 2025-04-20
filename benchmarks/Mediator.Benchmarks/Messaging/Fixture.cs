namespace Mediator.Benchmarks.Messaging;

internal static class Fixture
{
    public static void Setup()
    {
        ConsoleLogger.Default.WriteLineError("--------------------------------------");
        ConsoleLogger.Default.WriteLineError("Mediator config:");
        ConsoleLogger.Default.WriteLineError($"  - Lifetime       = {Mediator.ServiceLifetime}");
        ConsoleLogger.Default.WriteLineError($"  - Publisher      = {Mediator.NotificationPublisherName}");
        ConsoleLogger.Default.WriteLineError($"  - Total messages = {Mediator.TotalMessages}");
        ConsoleLogger.Default.WriteLineError("--------------------------------------");

        var envIsLargeProject = Environment.GetEnvironmentVariable("IsLargeProject");
        if (envIsLargeProject is not "True" and not "False")
            throw new InvalidOperationException(
                $"Invalid IsLargeProject: {envIsLargeProject}. Expected: True or False"
            );

        if (envIsLargeProject == "True")
        {
            if (Mediator.TotalMessages <= 100)
                throw new InvalidOperationException(
                    $"Unexpected messages count: {Mediator.TotalMessages}. Expected: more than 100"
                );
        }
        else
        {
            if (Mediator.TotalMessages >= 100)
                throw new InvalidOperationException(
                    $"Unexpected messages count: {Mediator.TotalMessages}. Expected: less than 100"
                );
        }

        var envLifetime = Environment.GetEnvironmentVariable("ServiceLifetime");
        if (envLifetime != Mediator.ServiceLifetime.ToString())
            throw new InvalidOperationException(
                $"Invalid lifetime: {Mediator.ServiceLifetime}. Expected: {envLifetime}"
            );

        var envPublisher = Environment.GetEnvironmentVariable("NotificationPublisherName");
        if (!string.IsNullOrWhiteSpace(envPublisher) && envPublisher != Mediator.NotificationPublisherName)
            throw new InvalidOperationException(
                $"Invalid publisher: {Mediator.NotificationPublisherName}. Expected: {envPublisher}"
            );
    }
}
