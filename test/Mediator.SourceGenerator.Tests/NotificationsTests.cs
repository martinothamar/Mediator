using System.Threading.Tasks;

namespace Mediator.SourceGenerator.Tests;

public sealed class NotificationsTests
{
    [Fact]
    public async Task Test_Notifications_DI()
    {
        var inputCompilation = Fixture.CreateLibrary(
            """
            using Mediator;
            using System.Threading;
            using System.Threading.Tasks;
            using System;

            namespace TestCode;

            public class Program
            {
                public static void Main()
                {
                }
            }

            public sealed record Notification0() : INotification;
            public readonly record struct Notification1() : INotification;
            public interface ISpecialNotification : INotification { }
            public sealed record Notification2() : ISpecialNotification;
            public readonly record struct Notification3() : ISpecialNotification;

            public sealed class GenericNotificationHandler<TNotification> : INotificationHandler<TNotification>
                where TNotification : INotification
            {
                public ValueTask Handle(TNotification notification, CancellationToken cancellationToken) => default;
            }

            public sealed class CatchAllNotificationHandler : INotificationHandler<INotification>
            {
                public ValueTask Handle(INotification notification, CancellationToken cancellationToken) => default;
            }

            public sealed class SpecialGenericNotificationHandler<TNotification> : INotificationHandler<TNotification>
                where TNotification : ISpecialNotification
            {
                public ValueTask Handle(TNotification notification, CancellationToken cancellationToken) => default;
            }

            public sealed class SpecialCatchAllNotificationHandler : INotificationHandler<ISpecialNotification>
            {
                public ValueTask Handle(ISpecialNotification notification, CancellationToken cancellationToken) => default;
            }

            public sealed class Notification0Handler : INotificationHandler<Notification0>
            {
                public ValueTask Handle(Notification0 notification, CancellationToken cancellationToken) => default;
            }
            public sealed class Notification1Handler : INotificationHandler<Notification1>
            {
                public ValueTask Handle(Notification1 notification, CancellationToken cancellationToken) => default;
            }
            public sealed class Notification2Handler : INotificationHandler<Notification2>
            {
                public ValueTask Handle(Notification2 notification, CancellationToken cancellationToken) => default;
            }
            public sealed class Notification3Handler : INotificationHandler<Notification3>
            {
                public ValueTask Handle(Notification3 notification, CancellationToken cancellationToken) => default;
            }
            """
        );

        await inputCompilation.AssertAndVerify(Assertions.CompilesWithoutDiagnostics);
    }
}
