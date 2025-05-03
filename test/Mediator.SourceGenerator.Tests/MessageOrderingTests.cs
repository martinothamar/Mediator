using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;

namespace Mediator.SourceGenerator.Tests;

public sealed class MessageOrderingTests
{
    [Fact]
    public async Task Test_Notifications_Ordering()
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

            public record DomainEvent(DateTimeOffset Timestamp) : INotification;
            public record RoundCreated(long Id, DateTimeOffset Timestamp) : DomainEvent(Timestamp);
            public record RoundResulted(long Id, long Win, DateTimeOffset Timestamp) : DomainEvent(Timestamp);
            public record RoundSucceeded(long Id, DateTimeOffset Timestamp) : DomainEvent(Timestamp);
            public record RoundSucceededActually(long Id, string Because, DateTimeOffset Timestamp) : RoundSucceeded(Id, Timestamp);

            public sealed class DomainEventHandler : INotificationHandler<DomainEvent> { public ValueTask Handle(DomainEvent notification, CancellationToken cancellationToken) => default; }
            public sealed class RoundCreatedHandler : INotificationHandler<RoundCreated> { public ValueTask Handle(RoundCreated notification, CancellationToken cancellationToken) => default; }
            public sealed class RoundResultedHandler : INotificationHandler<RoundResulted> { public ValueTask Handle(RoundResulted notification, CancellationToken cancellationToken) => default; }
            public sealed class RoundSucceededHandler : INotificationHandler<RoundSucceeded> { public ValueTask Handle(RoundSucceeded notification, CancellationToken cancellationToken) => default; }
            public sealed class RoundSucceededActuallyHandler : INotificationHandler<RoundSucceededActually> { public ValueTask Handle(RoundSucceededActually notification, CancellationToken cancellationToken) => default; }
            """
        );

        await inputCompilation.AssertAndVerify(
            Assertions.CompilesWithoutDiagnostics,
            result =>
            {
                var model = result.Generator.CompilationModel;
                Assert.NotNull(model);
                var notifications = model.NotificationMessages.ToList();
                Assert.Equal(5, notifications.Count);

                var last = notifications[^1];
                last.Name.Should().Be("DomainEvent");

                var index0 = notifications.FindIndex(n => n.Name == "RoundSucceededActually");
                var index1 = notifications.FindIndex(n => n.Name == "RoundSucceeded");

                index0.Should().NotBe(-1);
                index1.Should().NotBe(-1);

                index0.Should().BeLessThan(index1);
            }
        );
    }

    [Fact]
    public async Task Test_Notifications_Ordering_Bigger()
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

            public record DomainEvent(DateTimeOffset Timestamp) : INotification;
            public record RoundCreated(long Id, DateTimeOffset Timestamp) : DomainEvent(Timestamp);
            public record RoundResulted(long Id, long Win, DateTimeOffset Timestamp) : DomainEvent(Timestamp);
            public record RoundSucceeded(long Id, DateTimeOffset Timestamp) : DomainEvent(Timestamp);
            public record RoundSucceededActually(long Id, string Because, DateTimeOffset Timestamp) : RoundSucceeded(Id, Timestamp);

            public record DomainEvent2(DateTimeOffset Timestamp) : INotification;
            public record Round2Created(long Id, DateTimeOffset Timestamp) : DomainEvent2(Timestamp);
            public record Round2Resulted(long Id, long Win, DateTimeOffset Timestamp) : DomainEvent2(Timestamp);
            public record Round2Succeeded(long Id, DateTimeOffset Timestamp) : DomainEvent2(Timestamp);
            public record Round2SucceededActually(long Id, string Because, DateTimeOffset Timestamp) : RoundSucceeded(Id, Timestamp);

            public sealed class DomainEventHandler : INotificationHandler<DomainEvent> { public ValueTask Handle(DomainEvent notification, CancellationToken cancellationToken) => default; }
            public sealed class RoundCreatedHandler : INotificationHandler<RoundCreated> { public ValueTask Handle(RoundCreated notification, CancellationToken cancellationToken) => default; }
            public sealed class RoundResultedHandler : INotificationHandler<RoundResulted> { public ValueTask Handle(RoundResulted notification, CancellationToken cancellationToken) => default; }
            public sealed class RoundSucceededHandler : INotificationHandler<RoundSucceeded> { public ValueTask Handle(RoundSucceeded notification, CancellationToken cancellationToken) => default; }
            public sealed class RoundSucceededActuallyHandler : INotificationHandler<RoundSucceededActually> { public ValueTask Handle(RoundSucceededActually notification, CancellationToken cancellationToken) => default; }

            public sealed class DomainEvent2Handler : INotificationHandler<DomainEvent2> { public ValueTask Handle(DomainEvent2 notification, CancellationToken cancellationToken) => default; }
            public sealed class Round2CreatedHandler : INotificationHandler<Round2Created> { public ValueTask Handle(Round2Created notification, CancellationToken cancellationToken) => default; }
            public sealed class Round2ResultedHandler : INotificationHandler<Round2Resulted> { public ValueTask Handle(Round2Resulted notification, CancellationToken cancellationToken) => default; }
            public sealed class Round2SucceededHandler : INotificationHandler<Round2Succeeded> { public ValueTask Handle(Round2Succeeded notification, CancellationToken cancellationToken) => default; }
            public sealed class Round2SucceededActuallyHandler : INotificationHandler<Round2SucceededActually> { public ValueTask Handle(Round2SucceededActually notification, CancellationToken cancellationToken) => default; }
            """
        );

        await inputCompilation.AssertAndVerify(
            Assertions.CompilesWithoutDiagnostics,
            result =>
            {
                var model = result.Generator.CompilationModel;
                Assert.NotNull(model);
                var notifications = model.NotificationMessages.ToList();
                Assert.Equal(5 * 2, notifications.Count);

                Assert.All(notifications.AsEnumerable().Take(2), n => n.Name.Should().EndWith("Actually"));
                Assert.All(
                    notifications.AsEnumerable().Reverse().Take(2),
                    n => n.Name.Should().StartWith("DomainEvent")
                );
            }
        );
    }
}
