using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Mediator.Tests;

public class MultipleHandlersTests
{
    public sealed record Request0(Guid Id) : IRequest<Response>;

    public sealed record Request1(Guid Id) : IRequest<Response>;

    public sealed record Request2(Guid Id) : IRequest<Response>;

    public sealed record Request3(Guid Id) : IRequest<Response>;

    public sealed record Response(Guid Id);

    public sealed record Notification0(Guid Id) : INotification;

    public sealed record Notification1(Guid Id) : INotification;

    public sealed record Notification2(Guid Id) : INotification;

    public sealed record Notification3(Guid Id) : INotification;

    public sealed class NotificationHandler : INotificationHandler<Notification0>, INotificationHandler<Notification1>
    {
        public static readonly ConcurrentDictionary<Guid, int> InstanceIds = new();

        public ValueTask Handle(Notification0 notification, CancellationToken cancellationToken)
        {
            InstanceIds.AddOrUpdate(notification.Id, 1, (_, count) => count + 1);
            return default;
        }

        public ValueTask Handle(Notification1 notification, CancellationToken cancellationToken)
        {
            InstanceIds.AddOrUpdate(notification.Id, 1, (_, count) => count + 1);
            return default;
        }
    }

    public sealed class RequestHandler : IRequestHandler<Request0, Response>, IRequestHandler<Request1, Response>
    {
        public static readonly ConcurrentDictionary<Guid, int> InstanceIds = new();

        public ValueTask<Response> Handle(Request0 request, CancellationToken cancellationToken)
        {
            InstanceIds.AddOrUpdate(request.Id, 1, (_, count) => count + 1);
            return new ValueTask<Response>(new Response(request.Id));
        }

        public ValueTask<Response> Handle(Request1 request, CancellationToken cancellationToken)
        {
            InstanceIds.AddOrUpdate(request.Id, 1, (_, count) => count + 1);
            return new ValueTask<Response>(new Response(request.Id));
        }
    }

    public sealed class MultipleHandlers
        : INotificationHandler<Notification2>,
            INotificationHandler<Notification3>,
            IRequestHandler<Request2, Response>,
            IRequestHandler<Request3, Response>
    {
        public static readonly ConcurrentDictionary<Guid, int> InstanceIds = new();

        public ValueTask Handle(Notification2 notification, CancellationToken cancellationToken)
        {
            InstanceIds.AddOrUpdate(notification.Id, 1, (_, count) => count + 1);
            return default;
        }

        public ValueTask Handle(Notification3 notification, CancellationToken cancellationToken)
        {
            InstanceIds.AddOrUpdate(notification.Id, 1, (_, count) => count + 1);
            return default;
        }

        public ValueTask<Response> Handle(Request2 request, CancellationToken cancellationToken)
        {
            InstanceIds.AddOrUpdate(request.Id, 1, (_, count) => count + 1);
            return new ValueTask<Response>(new Response(request.Id));
        }

        public ValueTask<Response> Handle(Request3 request, CancellationToken cancellationToken)
        {
            InstanceIds.AddOrUpdate(request.Id, 1, (_, count) => count + 1);
            return new ValueTask<Response>(new Response(request.Id));
        }
    }

    [Fact]
    public async Task Multiple_Notification_Handlers_One_Class()
    {
        var ct = TestContext.Current.CancellationToken;
        var (sp, mediator) = Fixture.GetMediator();

        var id0 = Guid.NewGuid();
        var id1 = Guid.NewGuid();

        await mediator.Publish(new Notification0(id0), ct);
        await mediator.Publish(new Notification1(id1), ct);

        Assert.Equal(2, NotificationHandler.InstanceIds.Count);
        Assert.Equal(1, NotificationHandler.InstanceIds[id0]);
        Assert.Equal(1, NotificationHandler.InstanceIds[id1]);
    }

    [Fact]
    public async Task Multiple_Request_Handlers_One_Class()
    {
        var ct = TestContext.Current.CancellationToken;
        var (sp, mediator) = Fixture.GetMediator();

        var id0 = Guid.NewGuid();
        var id1 = Guid.NewGuid();

        _ = await mediator.Send(new Request0(id0), ct);
        _ = await mediator.Send(new Request1(id1), ct);

        Assert.Equal(2, RequestHandler.InstanceIds.Count);
        Assert.Equal(1, RequestHandler.InstanceIds[id0]);
        Assert.Equal(1, RequestHandler.InstanceIds[id1]);
    }

    [Fact]
    public async Task Multiple_Handlers_One_Class()
    {
        var ct = TestContext.Current.CancellationToken;
        var (sp, mediator) = Fixture.GetMediator();

        var id0 = Guid.NewGuid();
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var id3 = Guid.NewGuid();

        _ = await mediator.Send(new Request2(id0), ct);
        _ = await mediator.Send(new Request3(id1), ct);
        await mediator.Publish(new Notification2(id2), ct);
        await mediator.Publish(new Notification3(id3), ct);

        Assert.Equal(4, MultipleHandlers.InstanceIds.Count);
        Assert.Equal(1, MultipleHandlers.InstanceIds[id0]);
        Assert.Equal(1, MultipleHandlers.InstanceIds[id1]);
        Assert.Equal(1, MultipleHandlers.InstanceIds[id2]);
        Assert.Equal(1, MultipleHandlers.InstanceIds[id3]);
    }
}
