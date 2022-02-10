namespace Mediator.Tests.TestTypes;

public interface ISomeNotification : INotification
{
    Guid Id { get; }
}

public sealed record SomeNotification(Guid Id) : ISomeNotification;

public sealed record SomeOtherNotification(Guid Id) : ISomeNotification;
