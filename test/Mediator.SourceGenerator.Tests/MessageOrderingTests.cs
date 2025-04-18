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
