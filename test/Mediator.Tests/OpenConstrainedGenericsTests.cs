using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mediator.Tests.TestTypes;
using Microsoft.Extensions.DependencyInjection;

namespace Mediator.Tests;

public sealed class OpenConstrainedGenericsTests
{
    public sealed record SomeNotificationWithoutConcreteHandler(Guid Id) : INotification;

    public sealed class CatchAllPolymorphicNotificationHandler : INotificationHandler<INotification>
    {
        internal static readonly ConcurrentBag<Guid> Ids = new();
        internal readonly ConcurrentDictionary<Guid, int> InstanceIds = new();

        public ValueTask Handle(INotification notification, CancellationToken cancellationToken)
        {
            if (notification is SomeNotificationWithoutConcreteHandler n)
            {
                Ids.Add(n.Id);
                InstanceIds.AddOrUpdate(n.Id, 1, (_, count) => count + 1);
            }
            return default;
        }
    }

    public sealed class SomeGenericConstrainedNotificationHandler<TNotification> : INotificationHandler<TNotification>
        where TNotification : ISomeNotification
    {
        internal static readonly ConcurrentBag<Guid> Ids = new();
        internal readonly ConcurrentDictionary<Guid, int> InstanceIds = new();

        public ValueTask Handle(TNotification notification, CancellationToken cancellationToken)
        {
            Ids.Add(notification.Id);
            InstanceIds.AddOrUpdate(notification.Id, 1, (_, count) => count + 1);
            return default;
        }
    }

    public sealed class SomeGenericConstrainedPipeline<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IBaseRequest // Only requests, not commands or queries
    {
        public async ValueTask<TResponse> Handle(
            TRequest message,
            MessageHandlerDelegate<TRequest, TResponse> next,
            CancellationToken cancellationToken
        )
        {
            var response = await next(message, cancellationToken);
            if (response is SomeResponse someResponse)
                return (TResponse)(object)(someResponse with { Id = Guid.NewGuid() });
            else
                return response;
        }
    }

    [Fact]
    public async Task Test_Constrained_Generic_Argument_Handler()
    {
        var (sp, mediator) = Fixture.GetMediator();

        var notification1 = new SomeNotification(Guid.NewGuid());
        var notification2 = new SomeOtherNotification(Guid.NewGuid());

        var handler1 =
            (SomeGenericConstrainedNotificationHandler<SomeNotification>)
                sp.GetServices<INotificationHandler<SomeNotification>>()
                    .Single(h => h is SomeGenericConstrainedNotificationHandler<SomeNotification>);
        var handler2 =
            (SomeGenericConstrainedNotificationHandler<SomeOtherNotification>)
                sp.GetServices<INotificationHandler<SomeOtherNotification>>()
                    .Single(h => h is SomeGenericConstrainedNotificationHandler<SomeOtherNotification>);

        Assert.NotNull(handler1);
        Assert.NotNull(handler2);

        await mediator.Publish(notification1, TestContext.Current.CancellationToken);
        Assert.Contains(notification1.Id, SomeGenericConstrainedNotificationHandler<SomeNotification>.Ids);

        await mediator.Publish(notification2, TestContext.Current.CancellationToken);
        Assert.Contains(notification2.Id, SomeGenericConstrainedNotificationHandler<SomeOtherNotification>.Ids);
    }

    [Fact]
    public async Task Test_Notification_Without_Concrete_Handler()
    {
        var (sp, mediator) = Fixture.GetMediator();

        var id = Guid.NewGuid();
        var notification = new SomeNotificationWithoutConcreteHandler(id);

        await mediator.Publish(notification, TestContext.Current.CancellationToken);

        var handler = (CatchAllPolymorphicNotificationHandler)
            sp.GetRequiredService<INotificationHandler<SomeNotificationWithoutConcreteHandler>>();
        Assert.NotNull(handler);
        Assert.Contains(id, CatchAllPolymorphicNotificationHandler.Ids);
        AssertInstanceIdCount(1, handler.InstanceIds, id);
    }

    [Fact]
    public async Task Test_Notification_Without_Concrete_Handler_As_Objct()
    {
        var (sp, mediator) = Fixture.GetMediator();

        var id = Guid.NewGuid();
        var notification = new SomeNotificationWithoutConcreteHandler(id);

        await mediator.Publish((object)notification, TestContext.Current.CancellationToken);

        var handler = (CatchAllPolymorphicNotificationHandler)
            sp.GetRequiredService<INotificationHandler<SomeNotificationWithoutConcreteHandler>>();
        Assert.NotNull(handler);
        Assert.Contains(id, CatchAllPolymorphicNotificationHandler.Ids);
        AssertInstanceIdCount(1, handler.InstanceIds, id);
    }

    [Fact]
    public async Task Test_Constrained_Generic_Argument_Pipeline()
    {
        var ct = TestContext.Current.CancellationToken;
        var (sp, mediator) = Fixture.GetMediator(services =>
        {
            services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(SomeGenericConstrainedPipeline<,>));
        });

        var request = new SomeRequest(Guid.NewGuid());
        var command = new SomeCommand(Guid.NewGuid());

        var response = await mediator.Send(command, ct);
        Assert.Equal(command.Id, response.Id);

        response = await mediator.Send(request, ct);
        Assert.NotEqual(command.Id, response.Id);
        Assert.NotEqual(default, response.Id);
    }
}
