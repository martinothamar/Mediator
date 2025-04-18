using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Mediator;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.AddMediator(options =>
{
    options.ServiceLifetime = ServiceLifetime.Scoped;
    // options.NotificationPublisherType = typeof(FireAndForgetNotificationPublisher);
});

// Ordering of pipeline behavior registrations matter!
services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(ErrorLoggerHandler<,>));
services.AddSingleton<IPipelineBehavior<Ping, Pong>, PingValidator>();

var sp = services.BuildServiceProvider();

var mediator = sp.GetRequiredService<IMediator>();

var ping = new Ping(Guid.NewGuid());
var pong = await mediator.Send(ping);
Debug.Assert(ping.Id == pong.Id);
Console.WriteLine("Got the right ID: " + (ping, pong));

ping = ping with { Id = default };
try
{
    pong = await mediator.Send(ping);
    Debug.Assert(false, "We don't expect to get here, the PingValidator should throw ArgumentException!");
}
catch (ArgumentException) // The ErrorLoggerHandler should handle the logging for this sample
{ }

var statsHandler = sp.GetRequiredService<StatsNotificationHandler>();
var (messageCount, messageErrorCount) = statsHandler.Stats;

// First Ping succeeded, second failed validation
Debug.Assert(messageCount == 2, "We sent 2 pings");
Debug.Assert(messageErrorCount == 1, "1 of them failed validation");

int r;
r = await mediator.Send(new TestRequest1());
Debug.Assert(r == 1);
r = await ((Mediator.Mediator)mediator).Send(new TestRequest1());
Debug.Assert(r == 1);
r = (int)(await mediator.Send((object)new TestRequest1()))!;
Debug.Assert(r == 1);

r = await mediator.Send(new TestCommand1());
Debug.Assert(r == 1);
r = await ((Mediator.Mediator)mediator).Send(new TestCommand1());
Debug.Assert(r == 1);
r = (int)(await mediator.Send((object)new TestCommand1()))!;
Debug.Assert(r == 1);

r = await mediator.Send(new TestQuery1());
Debug.Assert(r == 1);
r = await ((Mediator.Mediator)mediator).Send(new TestQuery1());
Debug.Assert(r == 1);
r = (int)(await mediator.Send((object)new TestQuery1()))!;
Debug.Assert(r == 1);

const int expected = 1;
int count = 0;
const int expectedCount = 3;
await foreach (var i in mediator.CreateStream(new TestStreamRequest1()))
{
    Debug.Assert(i == expected);
    count++;
}
Debug.Assert(count == expectedCount);

count = 0;
await foreach (var i in ((Mediator.Mediator)mediator).CreateStream(new TestStreamRequest1()))
{
    Debug.Assert(i == expected);
    count++;
}
Debug.Assert(count == expectedCount);

count = 0;
await foreach (var i in mediator.CreateStream((object)new TestStreamRequest1()))
{
    Debug.Assert((int)i! == expected);
    count++;
}
Debug.Assert(count == expectedCount);

Console.WriteLine("Done!");

// Types used below

public sealed record Ping(Guid Id) : IRequest<Pong>;

public sealed record Pong(Guid Id);

public sealed class PingHandler : IRequestHandler<Ping, Pong>
{
    public ValueTask<Pong> Handle(Ping request, CancellationToken cancellationToken)
    {
        return new ValueTask<Pong>(new Pong(request.Id));
    }
}

public sealed class PingValidator : IPipelineBehavior<Ping, Pong>
{
    public ValueTask<Pong> Handle(
        Ping request,
        MessageHandlerDelegate<Ping, Pong> next,
        CancellationToken cancellationToken
    )
    {
        if (request is null || request.Id == default)
            throw new ArgumentException("Invalid input");

        return next(request, cancellationToken);
    }
}

public sealed record ErrorMessage(Exception Exception) : INotification;

public sealed record SuccessfulMessage() : INotification;

public sealed class ErrorLoggerHandler<TMessage, TResponse> : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage // Constrained to IMessage, or constrain to IBaseCommand or any custom interface you've implemented
{
    private readonly IMediator _mediator;

    public ErrorLoggerHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var response = await next(message, cancellationToken);
            await _mediator.Publish(new SuccessfulMessage());
            return response;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error handling message: " + ex.Message);
            await _mediator.Publish(new ErrorMessage(ex));
            throw;
        }
    }
}

// Notification handlers are automatically added to DI container

public sealed class ErrorNotificationHandler : INotificationHandler<ErrorMessage>
{
    public ValueTask Handle(ErrorMessage error, CancellationToken cancellationToken)
    {
        // Could log to application insights or something...
        return default;
    }
}

public sealed class StatsNotificationHandler : INotificationHandler<INotification> // or any other interface deriving from INotification
{
    private static long _messageCount;
    private static long _messageErrorCount;

    public (long MessageCount, long MessageErrorCount) Stats => (_messageCount, _messageErrorCount);

    public ValueTask Handle(INotification notification, CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref _messageCount);
        if (notification is ErrorMessage)
            Interlocked.Increment(ref _messageErrorCount);
        return default;
    }
}

public sealed class GenericNotificationHandler<TNotification> : INotificationHandler<TNotification>
    where TNotification : INotification // Notification handlers will be registered as open constrained types
{
    public ValueTask Handle(TNotification notification, CancellationToken cancellationToken)
    {
        return default;
    }
}

public sealed class FireAndForgetNotificationPublisher : INotificationPublisher
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
            await Task.WhenAll(handlers.Select(handler => handler.Handle(notification, cancellationToken).AsTask()));
        }
        catch (Exception ex)
        {
            // Notifications should be fire-and-forget, we just need to log it!
            // This way we don't have to worry about exceptions bubbling up when publishing notifications
            Console.Error.WriteLine(ex);
        }
    }
}

// csharpier-ignore-start
public sealed record TestRequest0() : IRequest<int>; public sealed record TestRequest0Handler : IRequestHandler<TestRequest0, int> { public ValueTask<int> Handle(TestRequest0 request, CancellationToken cancellationToken) => new ValueTask<int>(0); }
public sealed record TestRequest1() : IRequest<int>; public sealed record TestRequest1Handler : IRequestHandler<TestRequest1, int> { public ValueTask<int> Handle(TestRequest1 request, CancellationToken cancellationToken) => new ValueTask<int>(1); }
public sealed record TestRequest2() : IRequest<int>; public sealed record TestRequest2Handler : IRequestHandler<TestRequest2, int> { public ValueTask<int> Handle(TestRequest2 request, CancellationToken cancellationToken) => new ValueTask<int>(2); }
public sealed record TestRequest3() : IRequest<int>; public sealed record TestRequest3Handler : IRequestHandler<TestRequest3, int> { public ValueTask<int> Handle(TestRequest3 request, CancellationToken cancellationToken) => new ValueTask<int>(3); }
public sealed record TestRequest4() : IRequest<int>; public sealed record TestRequest4Handler : IRequestHandler<TestRequest4, int> { public ValueTask<int> Handle(TestRequest4 request, CancellationToken cancellationToken) => new ValueTask<int>(4); }
public sealed record TestRequest5() : IRequest<int>; public sealed record TestRequest5Handler : IRequestHandler<TestRequest5, int> { public ValueTask<int> Handle(TestRequest5 request, CancellationToken cancellationToken) => new ValueTask<int>(5); }
public sealed record TestRequest6() : IRequest<int>; public sealed record TestRequest6Handler : IRequestHandler<TestRequest6, int> { public ValueTask<int> Handle(TestRequest6 request, CancellationToken cancellationToken) => new ValueTask<int>(6); }
public sealed record TestRequest7() : IRequest<int>; public sealed record TestRequest7Handler : IRequestHandler<TestRequest7, int> { public ValueTask<int> Handle(TestRequest7 request, CancellationToken cancellationToken) => new ValueTask<int>(7); }
public sealed record TestRequest8() : IRequest<int>; public sealed record TestRequest8Handler : IRequestHandler<TestRequest8, int> { public ValueTask<int> Handle(TestRequest8 request, CancellationToken cancellationToken) => new ValueTask<int>(8); }
public sealed record TestRequest9() : IRequest<int>; public sealed record TestRequest9Handler : IRequestHandler<TestRequest9, int> { public ValueTask<int> Handle(TestRequest9 request, CancellationToken cancellationToken) => new ValueTask<int>(9); }
public sealed record TestRequest10() : IRequest<int>; public sealed record TestRequest10Handler : IRequestHandler<TestRequest10, int> { public ValueTask<int> Handle(TestRequest10 request, CancellationToken cancellationToken) => new ValueTask<int>(10); }
public sealed record TestRequest11() : IRequest<int>; public sealed record TestRequest11Handler : IRequestHandler<TestRequest11, int> { public ValueTask<int> Handle(TestRequest11 request, CancellationToken cancellationToken) => new ValueTask<int>(11); }
public sealed record TestRequest12() : IRequest<int>; public sealed record TestRequest12Handler : IRequestHandler<TestRequest12, int> { public ValueTask<int> Handle(TestRequest12 request, CancellationToken cancellationToken) => new ValueTask<int>(12); }
public sealed record TestRequest13() : IRequest<int>; public sealed record TestRequest13Handler : IRequestHandler<TestRequest13, int> { public ValueTask<int> Handle(TestRequest13 request, CancellationToken cancellationToken) => new ValueTask<int>(13); }
public sealed record TestRequest14() : IRequest<int>; public sealed record TestRequest14Handler : IRequestHandler<TestRequest14, int> { public ValueTask<int> Handle(TestRequest14 request, CancellationToken cancellationToken) => new ValueTask<int>(14); }
public sealed record TestRequest15() : IRequest<int>; public sealed record TestRequest15Handler : IRequestHandler<TestRequest15, int> { public ValueTask<int> Handle(TestRequest15 request, CancellationToken cancellationToken) => new ValueTask<int>(15); }
public sealed record TestRequest16() : IRequest<int>; public sealed record TestRequest16Handler : IRequestHandler<TestRequest16, int> { public ValueTask<int> Handle(TestRequest16 request, CancellationToken cancellationToken) => new ValueTask<int>(16); }
public sealed record TestRequest17() : IRequest<int>; public sealed record TestRequest17Handler : IRequestHandler<TestRequest17, int> { public ValueTask<int> Handle(TestRequest17 request, CancellationToken cancellationToken) => new ValueTask<int>(17); }
public sealed record TestRequest18() : IRequest<int>; public sealed record TestRequest18Handler : IRequestHandler<TestRequest18, int> { public ValueTask<int> Handle(TestRequest18 request, CancellationToken cancellationToken) => new ValueTask<int>(18); }
public sealed record TestRequest19() : IRequest<int>; public sealed record TestRequest19Handler : IRequestHandler<TestRequest19, int> { public ValueTask<int> Handle(TestRequest19 request, CancellationToken cancellationToken) => new ValueTask<int>(19); }
public sealed record TestRequest20() : IRequest<int>; public sealed record TestRequest20Handler : IRequestHandler<TestRequest20, int> { public ValueTask<int> Handle(TestRequest20 request, CancellationToken cancellationToken) => new ValueTask<int>(20); }
public sealed record TestRequest21() : IRequest<int>; public sealed record TestRequest21Handler : IRequestHandler<TestRequest21, int> { public ValueTask<int> Handle(TestRequest21 request, CancellationToken cancellationToken) => new ValueTask<int>(21); }
public sealed record TestRequest22() : IRequest<int>; public sealed record TestRequest22Handler : IRequestHandler<TestRequest22, int> { public ValueTask<int> Handle(TestRequest22 request, CancellationToken cancellationToken) => new ValueTask<int>(22); }
public sealed record TestRequest23() : IRequest<int>; public sealed record TestRequest23Handler : IRequestHandler<TestRequest23, int> { public ValueTask<int> Handle(TestRequest23 request, CancellationToken cancellationToken) => new ValueTask<int>(23); }
public sealed record TestRequest24() : IRequest<int>; public sealed record TestRequest24Handler : IRequestHandler<TestRequest24, int> { public ValueTask<int> Handle(TestRequest24 request, CancellationToken cancellationToken) => new ValueTask<int>(24); }
public sealed record TestRequest25() : IRequest<int>; public sealed record TestRequest25Handler : IRequestHandler<TestRequest25, int> { public ValueTask<int> Handle(TestRequest25 request, CancellationToken cancellationToken) => new ValueTask<int>(25); }
public sealed record TestRequest26() : IRequest<int>; public sealed record TestRequest26Handler : IRequestHandler<TestRequest26, int> { public ValueTask<int> Handle(TestRequest26 request, CancellationToken cancellationToken) => new ValueTask<int>(26); }
public sealed record TestRequest27() : IRequest<int>; public sealed record TestRequest27Handler : IRequestHandler<TestRequest27, int> { public ValueTask<int> Handle(TestRequest27 request, CancellationToken cancellationToken) => new ValueTask<int>(27); }
public sealed record TestRequest28() : IRequest<int>; public sealed record TestRequest28Handler : IRequestHandler<TestRequest28, int> { public ValueTask<int> Handle(TestRequest28 request, CancellationToken cancellationToken) => new ValueTask<int>(28); }
public sealed record TestRequest29() : IRequest<int>; public sealed record TestRequest29Handler : IRequestHandler<TestRequest29, int> { public ValueTask<int> Handle(TestRequest29 request, CancellationToken cancellationToken) => new ValueTask<int>(29); }

public sealed record TestQuery0() : IQuery<int>; public sealed record TestQuery0Handler : IQueryHandler<TestQuery0, int> { public ValueTask<int> Handle(TestQuery0 query, CancellationToken cancellationToken) => new ValueTask<int>(0); }
public sealed record TestQuery1() : IQuery<int>; public sealed record TestQuery1Handler : IQueryHandler<TestQuery1, int> { public ValueTask<int> Handle(TestQuery1 query, CancellationToken cancellationToken) => new ValueTask<int>(1); }
public sealed record TestQuery2() : IQuery<int>; public sealed record TestQuery2Handler : IQueryHandler<TestQuery2, int> { public ValueTask<int> Handle(TestQuery2 query, CancellationToken cancellationToken) => new ValueTask<int>(2); }
public sealed record TestQuery3() : IQuery<int>; public sealed record TestQuery3Handler : IQueryHandler<TestQuery3, int> { public ValueTask<int> Handle(TestQuery3 query, CancellationToken cancellationToken) => new ValueTask<int>(3); }
public sealed record TestQuery4() : IQuery<int>; public sealed record TestQuery4Handler : IQueryHandler<TestQuery4, int> { public ValueTask<int> Handle(TestQuery4 query, CancellationToken cancellationToken) => new ValueTask<int>(4); }
public sealed record TestQuery5() : IQuery<int>; public sealed record TestQuery5Handler : IQueryHandler<TestQuery5, int> { public ValueTask<int> Handle(TestQuery5 query, CancellationToken cancellationToken) => new ValueTask<int>(5); }
public sealed record TestQuery6() : IQuery<int>; public sealed record TestQuery6Handler : IQueryHandler<TestQuery6, int> { public ValueTask<int> Handle(TestQuery6 query, CancellationToken cancellationToken) => new ValueTask<int>(6); }
public sealed record TestQuery7() : IQuery<int>; public sealed record TestQuery7Handler : IQueryHandler<TestQuery7, int> { public ValueTask<int> Handle(TestQuery7 query, CancellationToken cancellationToken) => new ValueTask<int>(7); }
public sealed record TestQuery8() : IQuery<int>; public sealed record TestQuery8Handler : IQueryHandler<TestQuery8, int> { public ValueTask<int> Handle(TestQuery8 query, CancellationToken cancellationToken) => new ValueTask<int>(8); }
public sealed record TestQuery9() : IQuery<int>; public sealed record TestQuery9Handler : IQueryHandler<TestQuery9, int> { public ValueTask<int> Handle(TestQuery9 query, CancellationToken cancellationToken) => new ValueTask<int>(9); }
public sealed record TestQuery10() : IQuery<int>; public sealed record TestQuery10Handler : IQueryHandler<TestQuery10, int> { public ValueTask<int> Handle(TestQuery10 query, CancellationToken cancellationToken) => new ValueTask<int>(10); }
public sealed record TestQuery11() : IQuery<int>; public sealed record TestQuery11Handler : IQueryHandler<TestQuery11, int> { public ValueTask<int> Handle(TestQuery11 query, CancellationToken cancellationToken) => new ValueTask<int>(11); }
public sealed record TestQuery12() : IQuery<int>; public sealed record TestQuery12Handler : IQueryHandler<TestQuery12, int> { public ValueTask<int> Handle(TestQuery12 query, CancellationToken cancellationToken) => new ValueTask<int>(12); }
public sealed record TestQuery13() : IQuery<int>; public sealed record TestQuery13Handler : IQueryHandler<TestQuery13, int> { public ValueTask<int> Handle(TestQuery13 query, CancellationToken cancellationToken) => new ValueTask<int>(13); }
public sealed record TestQuery14() : IQuery<int>; public sealed record TestQuery14Handler : IQueryHandler<TestQuery14, int> { public ValueTask<int> Handle(TestQuery14 query, CancellationToken cancellationToken) => new ValueTask<int>(14); }
public sealed record TestQuery15() : IQuery<int>; public sealed record TestQuery15Handler : IQueryHandler<TestQuery15, int> { public ValueTask<int> Handle(TestQuery15 query, CancellationToken cancellationToken) => new ValueTask<int>(15); }
public sealed record TestQuery16() : IQuery<int>; public sealed record TestQuery16Handler : IQueryHandler<TestQuery16, int> { public ValueTask<int> Handle(TestQuery16 query, CancellationToken cancellationToken) => new ValueTask<int>(16); }
public sealed record TestQuery17() : IQuery<int>; public sealed record TestQuery17Handler : IQueryHandler<TestQuery17, int> { public ValueTask<int> Handle(TestQuery17 query, CancellationToken cancellationToken) => new ValueTask<int>(17); }
public sealed record TestQuery18() : IQuery<int>; public sealed record TestQuery18Handler : IQueryHandler<TestQuery18, int> { public ValueTask<int> Handle(TestQuery18 query, CancellationToken cancellationToken) => new ValueTask<int>(18); }
public sealed record TestQuery19() : IQuery<int>; public sealed record TestQuery19Handler : IQueryHandler<TestQuery19, int> { public ValueTask<int> Handle(TestQuery19 query, CancellationToken cancellationToken) => new ValueTask<int>(19); }
public sealed record TestQuery20() : IQuery<int>; public sealed record TestQuery20Handler : IQueryHandler<TestQuery20, int> { public ValueTask<int> Handle(TestQuery20 query, CancellationToken cancellationToken) => new ValueTask<int>(20); }
public sealed record TestQuery21() : IQuery<int>; public sealed record TestQuery21Handler : IQueryHandler<TestQuery21, int> { public ValueTask<int> Handle(TestQuery21 query, CancellationToken cancellationToken) => new ValueTask<int>(21); }
public sealed record TestQuery22() : IQuery<int>; public sealed record TestQuery22Handler : IQueryHandler<TestQuery22, int> { public ValueTask<int> Handle(TestQuery22 query, CancellationToken cancellationToken) => new ValueTask<int>(22); }
public sealed record TestQuery23() : IQuery<int>; public sealed record TestQuery23Handler : IQueryHandler<TestQuery23, int> { public ValueTask<int> Handle(TestQuery23 query, CancellationToken cancellationToken) => new ValueTask<int>(23); }
public sealed record TestQuery24() : IQuery<int>; public sealed record TestQuery24Handler : IQueryHandler<TestQuery24, int> { public ValueTask<int> Handle(TestQuery24 query, CancellationToken cancellationToken) => new ValueTask<int>(24); }
public sealed record TestQuery25() : IQuery<int>; public sealed record TestQuery25Handler : IQueryHandler<TestQuery25, int> { public ValueTask<int> Handle(TestQuery25 query, CancellationToken cancellationToken) => new ValueTask<int>(25); }
public sealed record TestQuery26() : IQuery<int>; public sealed record TestQuery26Handler : IQueryHandler<TestQuery26, int> { public ValueTask<int> Handle(TestQuery26 query, CancellationToken cancellationToken) => new ValueTask<int>(26); }
public sealed record TestQuery27() : IQuery<int>; public sealed record TestQuery27Handler : IQueryHandler<TestQuery27, int> { public ValueTask<int> Handle(TestQuery27 query, CancellationToken cancellationToken) => new ValueTask<int>(27); }
public sealed record TestQuery28() : IQuery<int>; public sealed record TestQuery28Handler : IQueryHandler<TestQuery28, int> { public ValueTask<int> Handle(TestQuery28 query, CancellationToken cancellationToken) => new ValueTask<int>(28); }
public sealed record TestQuery29() : IQuery<int>; public sealed record TestQuery29Handler : IQueryHandler<TestQuery29, int> { public ValueTask<int> Handle(TestQuery29 query, CancellationToken cancellationToken) => new ValueTask<int>(29); }

public sealed record TestCommand0() : ICommand<int>; public sealed record TestCommand0Handler : ICommandHandler<TestCommand0, int> { public ValueTask<int> Handle(TestCommand0 command, CancellationToken cancellationToken) => new ValueTask<int>(0); }
public sealed record TestCommand1() : ICommand<int>; public sealed record TestCommand1Handler : ICommandHandler<TestCommand1, int> { public ValueTask<int> Handle(TestCommand1 command, CancellationToken cancellationToken) => new ValueTask<int>(1); }
public sealed record TestCommand2() : ICommand<int>; public sealed record TestCommand2Handler : ICommandHandler<TestCommand2, int> { public ValueTask<int> Handle(TestCommand2 command, CancellationToken cancellationToken) => new ValueTask<int>(2); }
public sealed record TestCommand3() : ICommand<int>; public sealed record TestCommand3Handler : ICommandHandler<TestCommand3, int> { public ValueTask<int> Handle(TestCommand3 command, CancellationToken cancellationToken) => new ValueTask<int>(3); }
public sealed record TestCommand4() : ICommand<int>; public sealed record TestCommand4Handler : ICommandHandler<TestCommand4, int> { public ValueTask<int> Handle(TestCommand4 command, CancellationToken cancellationToken) => new ValueTask<int>(4); }
public sealed record TestCommand5() : ICommand<int>; public sealed record TestCommand5Handler : ICommandHandler<TestCommand5, int> { public ValueTask<int> Handle(TestCommand5 command, CancellationToken cancellationToken) => new ValueTask<int>(5); }
public sealed record TestCommand6() : ICommand<int>; public sealed record TestCommand6Handler : ICommandHandler<TestCommand6, int> { public ValueTask<int> Handle(TestCommand6 command, CancellationToken cancellationToken) => new ValueTask<int>(6); }
public sealed record TestCommand7() : ICommand<int>; public sealed record TestCommand7Handler : ICommandHandler<TestCommand7, int> { public ValueTask<int> Handle(TestCommand7 command, CancellationToken cancellationToken) => new ValueTask<int>(7); }
public sealed record TestCommand8() : ICommand<int>; public sealed record TestCommand8Handler : ICommandHandler<TestCommand8, int> { public ValueTask<int> Handle(TestCommand8 command, CancellationToken cancellationToken) => new ValueTask<int>(8); }
public sealed record TestCommand9() : ICommand<int>; public sealed record TestCommand9Handler : ICommandHandler<TestCommand9, int> { public ValueTask<int> Handle(TestCommand9 command, CancellationToken cancellationToken) => new ValueTask<int>(9); }
public sealed record TestCommand10() : ICommand<int>; public sealed record TestCommand10Handler : ICommandHandler<TestCommand10, int> { public ValueTask<int> Handle(TestCommand10 command, CancellationToken cancellationToken) => new ValueTask<int>(10); }
public sealed record TestCommand11() : ICommand<int>; public sealed record TestCommand11Handler : ICommandHandler<TestCommand11, int> { public ValueTask<int> Handle(TestCommand11 command, CancellationToken cancellationToken) => new ValueTask<int>(11); }
public sealed record TestCommand12() : ICommand<int>; public sealed record TestCommand12Handler : ICommandHandler<TestCommand12, int> { public ValueTask<int> Handle(TestCommand12 command, CancellationToken cancellationToken) => new ValueTask<int>(12); }
public sealed record TestCommand13() : ICommand<int>; public sealed record TestCommand13Handler : ICommandHandler<TestCommand13, int> { public ValueTask<int> Handle(TestCommand13 command, CancellationToken cancellationToken) => new ValueTask<int>(13); }
public sealed record TestCommand14() : ICommand<int>; public sealed record TestCommand14Handler : ICommandHandler<TestCommand14, int> { public ValueTask<int> Handle(TestCommand14 command, CancellationToken cancellationToken) => new ValueTask<int>(14); }
public sealed record TestCommand15() : ICommand<int>; public sealed record TestCommand15Handler : ICommandHandler<TestCommand15, int> { public ValueTask<int> Handle(TestCommand15 command, CancellationToken cancellationToken) => new ValueTask<int>(15); }
public sealed record TestCommand16() : ICommand<int>; public sealed record TestCommand16Handler : ICommandHandler<TestCommand16, int> { public ValueTask<int> Handle(TestCommand16 command, CancellationToken cancellationToken) => new ValueTask<int>(16); }
public sealed record TestCommand17() : ICommand<int>; public sealed record TestCommand17Handler : ICommandHandler<TestCommand17, int> { public ValueTask<int> Handle(TestCommand17 command, CancellationToken cancellationToken) => new ValueTask<int>(17); }
public sealed record TestCommand18() : ICommand<int>; public sealed record TestCommand18Handler : ICommandHandler<TestCommand18, int> { public ValueTask<int> Handle(TestCommand18 command, CancellationToken cancellationToken) => new ValueTask<int>(18); }
public sealed record TestCommand19() : ICommand<int>; public sealed record TestCommand19Handler : ICommandHandler<TestCommand19, int> { public ValueTask<int> Handle(TestCommand19 command, CancellationToken cancellationToken) => new ValueTask<int>(19); }
public sealed record TestCommand20() : ICommand<int>; public sealed record TestCommand20Handler : ICommandHandler<TestCommand20, int> { public ValueTask<int> Handle(TestCommand20 command, CancellationToken cancellationToken) => new ValueTask<int>(20); }
public sealed record TestCommand21() : ICommand<int>; public sealed record TestCommand21Handler : ICommandHandler<TestCommand21, int> { public ValueTask<int> Handle(TestCommand21 command, CancellationToken cancellationToken) => new ValueTask<int>(21); }
public sealed record TestCommand22() : ICommand<int>; public sealed record TestCommand22Handler : ICommandHandler<TestCommand22, int> { public ValueTask<int> Handle(TestCommand22 command, CancellationToken cancellationToken) => new ValueTask<int>(22); }
public sealed record TestCommand23() : ICommand<int>; public sealed record TestCommand23Handler : ICommandHandler<TestCommand23, int> { public ValueTask<int> Handle(TestCommand23 command, CancellationToken cancellationToken) => new ValueTask<int>(23); }
public sealed record TestCommand24() : ICommand<int>; public sealed record TestCommand24Handler : ICommandHandler<TestCommand24, int> { public ValueTask<int> Handle(TestCommand24 command, CancellationToken cancellationToken) => new ValueTask<int>(24); }
public sealed record TestCommand25() : ICommand<int>; public sealed record TestCommand25Handler : ICommandHandler<TestCommand25, int> { public ValueTask<int> Handle(TestCommand25 command, CancellationToken cancellationToken) => new ValueTask<int>(25); }
public sealed record TestCommand26() : ICommand<int>; public sealed record TestCommand26Handler : ICommandHandler<TestCommand26, int> { public ValueTask<int> Handle(TestCommand26 command, CancellationToken cancellationToken) => new ValueTask<int>(26); }
public sealed record TestCommand27() : ICommand<int>; public sealed record TestCommand27Handler : ICommandHandler<TestCommand27, int> { public ValueTask<int> Handle(TestCommand27 command, CancellationToken cancellationToken) => new ValueTask<int>(27); }
public sealed record TestCommand28() : ICommand<int>; public sealed record TestCommand28Handler : ICommandHandler<TestCommand28, int> { public ValueTask<int> Handle(TestCommand28 command, CancellationToken cancellationToken) => new ValueTask<int>(28); }
public sealed record TestCommand29() : ICommand<int>; public sealed record TestCommand29Handler : ICommandHandler<TestCommand29, int> { public ValueTask<int> Handle(TestCommand29 command, CancellationToken cancellationToken) => new ValueTask<int>(29); }

public sealed record TestNotification0() : INotification; public sealed record TestNotification0Handler : INotificationHandler<TestNotification0> { public ValueTask Handle(TestNotification0 notification, CancellationToken cancellationToken) => default; }
public sealed record TestNotification1() : INotification; public sealed record TestNotification1Handler : INotificationHandler<TestNotification1> { public ValueTask Handle(TestNotification1 notification, CancellationToken cancellationToken) => default; }
public sealed record TestNotification2() : INotification; public sealed record TestNotification2Handler : INotificationHandler<TestNotification2> { public ValueTask Handle(TestNotification2 notification, CancellationToken cancellationToken) => default; }
public sealed record TestNotification3() : INotification; public sealed record TestNotification3Handler : INotificationHandler<TestNotification3> { public ValueTask Handle(TestNotification3 notification, CancellationToken cancellationToken) => default; }
public sealed record TestNotification4() : INotification; public sealed record TestNotification4Handler : INotificationHandler<TestNotification4> { public ValueTask Handle(TestNotification4 notification, CancellationToken cancellationToken) => default; }
public sealed record TestNotification5() : INotification; public sealed record TestNotification5Handler : INotificationHandler<TestNotification5> { public ValueTask Handle(TestNotification5 notification, CancellationToken cancellationToken) => default; }
public sealed record TestNotification6() : INotification; public sealed record TestNotification6Handler : INotificationHandler<TestNotification6> { public ValueTask Handle(TestNotification6 notification, CancellationToken cancellationToken) => default; }
public sealed record TestNotification7() : INotification; public sealed record TestNotification7Handler : INotificationHandler<TestNotification7> { public ValueTask Handle(TestNotification7 notification, CancellationToken cancellationToken) => default; }
public sealed record TestNotification8() : INotification; public sealed record TestNotification8Handler : INotificationHandler<TestNotification8> { public ValueTask Handle(TestNotification8 notification, CancellationToken cancellationToken) => default; }
public sealed record TestNotification9() : INotification; public sealed record TestNotification9Handler : INotificationHandler<TestNotification9> { public ValueTask Handle(TestNotification9 notification, CancellationToken cancellationToken) => default; }
public sealed record TestNotification10() : INotification; public sealed record TestNotification10Handler : INotificationHandler<TestNotification10> { public ValueTask Handle(TestNotification10 notification, CancellationToken cancellationToken) => default; }
public sealed record TestNotification11() : INotification; public sealed record TestNotification11Handler : INotificationHandler<TestNotification11> { public ValueTask Handle(TestNotification11 notification, CancellationToken cancellationToken) => default; }
public sealed record TestNotification12() : INotification; public sealed record TestNotification12Handler : INotificationHandler<TestNotification12> { public ValueTask Handle(TestNotification12 notification, CancellationToken cancellationToken) => default; }
public sealed record TestNotification13() : INotification; public sealed record TestNotification13Handler : INotificationHandler<TestNotification13> { public ValueTask Handle(TestNotification13 notification, CancellationToken cancellationToken) => default; }
public sealed record TestNotification14() : INotification; public sealed record TestNotification14Handler : INotificationHandler<TestNotification14> { public ValueTask Handle(TestNotification14 notification, CancellationToken cancellationToken) => default; }
public sealed record TestNotification15() : INotification; public sealed record TestNotification15Handler : INotificationHandler<TestNotification15> { public ValueTask Handle(TestNotification15 notification, CancellationToken cancellationToken) => default; }
public sealed record TestNotification16() : INotification; public sealed record TestNotification16Handler : INotificationHandler<TestNotification16> { public ValueTask Handle(TestNotification16 notification, CancellationToken cancellationToken) => default; }
public sealed record TestNotification17() : INotification; public sealed record TestNotification17Handler : INotificationHandler<TestNotification17> { public ValueTask Handle(TestNotification17 notification, CancellationToken cancellationToken) => default; }
public sealed record TestNotification18() : INotification; public sealed record TestNotification18Handler : INotificationHandler<TestNotification18> { public ValueTask Handle(TestNotification18 notification, CancellationToken cancellationToken) => default; }
public sealed record TestNotification19() : INotification; public sealed record TestNotification19Handler : INotificationHandler<TestNotification19> { public ValueTask Handle(TestNotification19 notification, CancellationToken cancellationToken) => default; }
public sealed record TestNotification20() : INotification; public sealed record TestNotification20Handler : INotificationHandler<TestNotification20> { public ValueTask Handle(TestNotification20 notification, CancellationToken cancellationToken) => default; }
public sealed record TestNotification21() : INotification; public sealed record TestNotification21Handler : INotificationHandler<TestNotification21> { public ValueTask Handle(TestNotification21 notification, CancellationToken cancellationToken) => default; }
public sealed record TestNotification22() : INotification; public sealed record TestNotification22Handler : INotificationHandler<TestNotification22> { public ValueTask Handle(TestNotification22 notification, CancellationToken cancellationToken) => default; }
public sealed record TestNotification23() : INotification; public sealed record TestNotification23Handler : INotificationHandler<TestNotification23> { public ValueTask Handle(TestNotification23 notification, CancellationToken cancellationToken) => default; }
public sealed record TestNotification24() : INotification; public sealed record TestNotification24Handler : INotificationHandler<TestNotification24> { public ValueTask Handle(TestNotification24 notification, CancellationToken cancellationToken) => default; }
public sealed record TestNotification25() : INotification; public sealed record TestNotification25Handler : INotificationHandler<TestNotification25> { public ValueTask Handle(TestNotification25 notification, CancellationToken cancellationToken) => default; }
public sealed record TestNotification26() : INotification; public sealed record TestNotification26Handler : INotificationHandler<TestNotification26> { public ValueTask Handle(TestNotification26 notification, CancellationToken cancellationToken) => default; }
public sealed record TestNotification27() : INotification; public sealed record TestNotification27Handler : INotificationHandler<TestNotification27> { public ValueTask Handle(TestNotification27 notification, CancellationToken cancellationToken) => default; }
public sealed record TestNotification28() : INotification; public sealed record TestNotification28Handler : INotificationHandler<TestNotification28> { public ValueTask Handle(TestNotification28 notification, CancellationToken cancellationToken) => default; }
public sealed record TestNotification29() : INotification; public sealed record TestNotification29Handler : INotificationHandler<TestNotification29> { public ValueTask Handle(TestNotification29 notification, CancellationToken cancellationToken) => default; }


public sealed record TestStreamRequest0() : IStreamRequest<int>; public sealed record TestStreamRequest0Handler : IStreamRequestHandler<TestStreamRequest0, int> { public async IAsyncEnumerable<int> Handle(TestStreamRequest0 request, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 0; } } }
public sealed record TestStreamRequest1() : IStreamRequest<int>; public sealed record TestStreamRequest1Handler : IStreamRequestHandler<TestStreamRequest1, int> { public async IAsyncEnumerable<int> Handle(TestStreamRequest1 request, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 1; } } }
public sealed record TestStreamRequest2() : IStreamRequest<int>; public sealed record TestStreamRequest2Handler : IStreamRequestHandler<TestStreamRequest2, int> { public async IAsyncEnumerable<int> Handle(TestStreamRequest2 request, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 2; } } }
public sealed record TestStreamRequest3() : IStreamRequest<int>; public sealed record TestStreamRequest3Handler : IStreamRequestHandler<TestStreamRequest3, int> { public async IAsyncEnumerable<int> Handle(TestStreamRequest3 request, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 3; } } }
public sealed record TestStreamRequest4() : IStreamRequest<int>; public sealed record TestStreamRequest4Handler : IStreamRequestHandler<TestStreamRequest4, int> { public async IAsyncEnumerable<int> Handle(TestStreamRequest4 request, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 4; } } }
public sealed record TestStreamRequest5() : IStreamRequest<int>; public sealed record TestStreamRequest5Handler : IStreamRequestHandler<TestStreamRequest5, int> { public async IAsyncEnumerable<int> Handle(TestStreamRequest5 request, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 5; } } }
public sealed record TestStreamRequest6() : IStreamRequest<int>; public sealed record TestStreamRequest6Handler : IStreamRequestHandler<TestStreamRequest6, int> { public async IAsyncEnumerable<int> Handle(TestStreamRequest6 request, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 6; } } }
public sealed record TestStreamRequest7() : IStreamRequest<int>; public sealed record TestStreamRequest7Handler : IStreamRequestHandler<TestStreamRequest7, int> { public async IAsyncEnumerable<int> Handle(TestStreamRequest7 request, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 7; } } }
public sealed record TestStreamRequest8() : IStreamRequest<int>; public sealed record TestStreamRequest8Handler : IStreamRequestHandler<TestStreamRequest8, int> { public async IAsyncEnumerable<int> Handle(TestStreamRequest8 request, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 8; } } }
public sealed record TestStreamRequest9() : IStreamRequest<int>; public sealed record TestStreamRequest9Handler : IStreamRequestHandler<TestStreamRequest9, int> { public async IAsyncEnumerable<int> Handle(TestStreamRequest9 request, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 9; } } }
public sealed record TestStreamRequest10() : IStreamRequest<int>; public sealed record TestStreamRequest10Handler : IStreamRequestHandler<TestStreamRequest10, int> { public async IAsyncEnumerable<int> Handle(TestStreamRequest10 request, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 10; } } }
public sealed record TestStreamRequest11() : IStreamRequest<int>; public sealed record TestStreamRequest11Handler : IStreamRequestHandler<TestStreamRequest11, int> { public async IAsyncEnumerable<int> Handle(TestStreamRequest11 request, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 11; } } }
public sealed record TestStreamRequest12() : IStreamRequest<int>; public sealed record TestStreamRequest12Handler : IStreamRequestHandler<TestStreamRequest12, int> { public async IAsyncEnumerable<int> Handle(TestStreamRequest12 request, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 12; } } }
public sealed record TestStreamRequest13() : IStreamRequest<int>; public sealed record TestStreamRequest13Handler : IStreamRequestHandler<TestStreamRequest13, int> { public async IAsyncEnumerable<int> Handle(TestStreamRequest13 request, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 13; } } }
public sealed record TestStreamRequest14() : IStreamRequest<int>; public sealed record TestStreamRequest14Handler : IStreamRequestHandler<TestStreamRequest14, int> { public async IAsyncEnumerable<int> Handle(TestStreamRequest14 request, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 14; } } }
public sealed record TestStreamRequest15() : IStreamRequest<int>; public sealed record TestStreamRequest15Handler : IStreamRequestHandler<TestStreamRequest15, int> { public async IAsyncEnumerable<int> Handle(TestStreamRequest15 request, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 15; } } }
public sealed record TestStreamRequest16() : IStreamRequest<int>; public sealed record TestStreamRequest16Handler : IStreamRequestHandler<TestStreamRequest16, int> { public async IAsyncEnumerable<int> Handle(TestStreamRequest16 request, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 16; } } }
public sealed record TestStreamRequest17() : IStreamRequest<int>; public sealed record TestStreamRequest17Handler : IStreamRequestHandler<TestStreamRequest17, int> { public async IAsyncEnumerable<int> Handle(TestStreamRequest17 request, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 17; } } }
public sealed record TestStreamRequest18() : IStreamRequest<int>; public sealed record TestStreamRequest18Handler : IStreamRequestHandler<TestStreamRequest18, int> { public async IAsyncEnumerable<int> Handle(TestStreamRequest18 request, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 18; } } }
public sealed record TestStreamRequest19() : IStreamRequest<int>; public sealed record TestStreamRequest19Handler : IStreamRequestHandler<TestStreamRequest19, int> { public async IAsyncEnumerable<int> Handle(TestStreamRequest19 request, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 19; } } }
public sealed record TestStreamRequest20() : IStreamRequest<int>; public sealed record TestStreamRequest20Handler : IStreamRequestHandler<TestStreamRequest20, int> { public async IAsyncEnumerable<int> Handle(TestStreamRequest20 request, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 20; } } }
public sealed record TestStreamRequest21() : IStreamRequest<int>; public sealed record TestStreamRequest21Handler : IStreamRequestHandler<TestStreamRequest21, int> { public async IAsyncEnumerable<int> Handle(TestStreamRequest21 request, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 21; } } }
public sealed record TestStreamRequest22() : IStreamRequest<int>; public sealed record TestStreamRequest22Handler : IStreamRequestHandler<TestStreamRequest22, int> { public async IAsyncEnumerable<int> Handle(TestStreamRequest22 request, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 22; } } }
public sealed record TestStreamRequest23() : IStreamRequest<int>; public sealed record TestStreamRequest23Handler : IStreamRequestHandler<TestStreamRequest23, int> { public async IAsyncEnumerable<int> Handle(TestStreamRequest23 request, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 23; } } }
public sealed record TestStreamRequest24() : IStreamRequest<int>; public sealed record TestStreamRequest24Handler : IStreamRequestHandler<TestStreamRequest24, int> { public async IAsyncEnumerable<int> Handle(TestStreamRequest24 request, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 24; } } }
public sealed record TestStreamRequest25() : IStreamRequest<int>; public sealed record TestStreamRequest25Handler : IStreamRequestHandler<TestStreamRequest25, int> { public async IAsyncEnumerable<int> Handle(TestStreamRequest25 request, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 25; } } }
public sealed record TestStreamRequest26() : IStreamRequest<int>; public sealed record TestStreamRequest26Handler : IStreamRequestHandler<TestStreamRequest26, int> { public async IAsyncEnumerable<int> Handle(TestStreamRequest26 request, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 26; } } }
public sealed record TestStreamRequest27() : IStreamRequest<int>; public sealed record TestStreamRequest27Handler : IStreamRequestHandler<TestStreamRequest27, int> { public async IAsyncEnumerable<int> Handle(TestStreamRequest27 request, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 27; } } }
public sealed record TestStreamRequest28() : IStreamRequest<int>; public sealed record TestStreamRequest28Handler : IStreamRequestHandler<TestStreamRequest28, int> { public async IAsyncEnumerable<int> Handle(TestStreamRequest28 request, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 28; } } }
public sealed record TestStreamRequest29() : IStreamRequest<int>; public sealed record TestStreamRequest29Handler : IStreamRequestHandler<TestStreamRequest29, int> { public async IAsyncEnumerable<int> Handle(TestStreamRequest29 request, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 29; } } }

public sealed record TestStreamQuery0() : IStreamQuery<int>; public sealed record TestStreamQuery0Handler : IStreamQueryHandler<TestStreamQuery0, int> { public async IAsyncEnumerable<int> Handle(TestStreamQuery0 query, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 0; } } }
public sealed record TestStreamQuery1() : IStreamQuery<int>; public sealed record TestStreamQuery1Handler : IStreamQueryHandler<TestStreamQuery1, int> { public async IAsyncEnumerable<int> Handle(TestStreamQuery1 query, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 1; } } }
public sealed record TestStreamQuery2() : IStreamQuery<int>; public sealed record TestStreamQuery2Handler : IStreamQueryHandler<TestStreamQuery2, int> { public async IAsyncEnumerable<int> Handle(TestStreamQuery2 query, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 2; } } }
public sealed record TestStreamQuery3() : IStreamQuery<int>; public sealed record TestStreamQuery3Handler : IStreamQueryHandler<TestStreamQuery3, int> { public async IAsyncEnumerable<int> Handle(TestStreamQuery3 query, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 3; } } }
public sealed record TestStreamQuery4() : IStreamQuery<int>; public sealed record TestStreamQuery4Handler : IStreamQueryHandler<TestStreamQuery4, int> { public async IAsyncEnumerable<int> Handle(TestStreamQuery4 query, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 4; } } }
public sealed record TestStreamQuery5() : IStreamQuery<int>; public sealed record TestStreamQuery5Handler : IStreamQueryHandler<TestStreamQuery5, int> { public async IAsyncEnumerable<int> Handle(TestStreamQuery5 query, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 5; } } }
public sealed record TestStreamQuery6() : IStreamQuery<int>; public sealed record TestStreamQuery6Handler : IStreamQueryHandler<TestStreamQuery6, int> { public async IAsyncEnumerable<int> Handle(TestStreamQuery6 query, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 6; } } }
public sealed record TestStreamQuery7() : IStreamQuery<int>; public sealed record TestStreamQuery7Handler : IStreamQueryHandler<TestStreamQuery7, int> { public async IAsyncEnumerable<int> Handle(TestStreamQuery7 query, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 7; } } }
public sealed record TestStreamQuery8() : IStreamQuery<int>; public sealed record TestStreamQuery8Handler : IStreamQueryHandler<TestStreamQuery8, int> { public async IAsyncEnumerable<int> Handle(TestStreamQuery8 query, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 8; } } }
public sealed record TestStreamQuery9() : IStreamQuery<int>; public sealed record TestStreamQuery9Handler : IStreamQueryHandler<TestStreamQuery9, int> { public async IAsyncEnumerable<int> Handle(TestStreamQuery9 query, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 9; } } }
public sealed record TestStreamQuery10() : IStreamQuery<int>; public sealed record TestStreamQuery10Handler : IStreamQueryHandler<TestStreamQuery10, int> { public async IAsyncEnumerable<int> Handle(TestStreamQuery10 query, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 10; } } }
public sealed record TestStreamQuery11() : IStreamQuery<int>; public sealed record TestStreamQuery11Handler : IStreamQueryHandler<TestStreamQuery11, int> { public async IAsyncEnumerable<int> Handle(TestStreamQuery11 query, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 11; } } }
public sealed record TestStreamQuery12() : IStreamQuery<int>; public sealed record TestStreamQuery12Handler : IStreamQueryHandler<TestStreamQuery12, int> { public async IAsyncEnumerable<int> Handle(TestStreamQuery12 query, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 12; } } }
public sealed record TestStreamQuery13() : IStreamQuery<int>; public sealed record TestStreamQuery13Handler : IStreamQueryHandler<TestStreamQuery13, int> { public async IAsyncEnumerable<int> Handle(TestStreamQuery13 query, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 13; } } }
public sealed record TestStreamQuery14() : IStreamQuery<int>; public sealed record TestStreamQuery14Handler : IStreamQueryHandler<TestStreamQuery14, int> { public async IAsyncEnumerable<int> Handle(TestStreamQuery14 query, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 14; } } }
public sealed record TestStreamQuery15() : IStreamQuery<int>; public sealed record TestStreamQuery15Handler : IStreamQueryHandler<TestStreamQuery15, int> { public async IAsyncEnumerable<int> Handle(TestStreamQuery15 query, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 15; } } }
public sealed record TestStreamQuery16() : IStreamQuery<int>; public sealed record TestStreamQuery16Handler : IStreamQueryHandler<TestStreamQuery16, int> { public async IAsyncEnumerable<int> Handle(TestStreamQuery16 query, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 16; } } }
public sealed record TestStreamQuery17() : IStreamQuery<int>; public sealed record TestStreamQuery17Handler : IStreamQueryHandler<TestStreamQuery17, int> { public async IAsyncEnumerable<int> Handle(TestStreamQuery17 query, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 17; } } }
public sealed record TestStreamQuery18() : IStreamQuery<int>; public sealed record TestStreamQuery18Handler : IStreamQueryHandler<TestStreamQuery18, int> { public async IAsyncEnumerable<int> Handle(TestStreamQuery18 query, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 18; } } }
public sealed record TestStreamQuery19() : IStreamQuery<int>; public sealed record TestStreamQuery19Handler : IStreamQueryHandler<TestStreamQuery19, int> { public async IAsyncEnumerable<int> Handle(TestStreamQuery19 query, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 19; } } }
public sealed record TestStreamQuery20() : IStreamQuery<int>; public sealed record TestStreamQuery20Handler : IStreamQueryHandler<TestStreamQuery20, int> { public async IAsyncEnumerable<int> Handle(TestStreamQuery20 query, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 20; } } }
public sealed record TestStreamQuery21() : IStreamQuery<int>; public sealed record TestStreamQuery21Handler : IStreamQueryHandler<TestStreamQuery21, int> { public async IAsyncEnumerable<int> Handle(TestStreamQuery21 query, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 21; } } }
public sealed record TestStreamQuery22() : IStreamQuery<int>; public sealed record TestStreamQuery22Handler : IStreamQueryHandler<TestStreamQuery22, int> { public async IAsyncEnumerable<int> Handle(TestStreamQuery22 query, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 22; } } }
public sealed record TestStreamQuery23() : IStreamQuery<int>; public sealed record TestStreamQuery23Handler : IStreamQueryHandler<TestStreamQuery23, int> { public async IAsyncEnumerable<int> Handle(TestStreamQuery23 query, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 23; } } }
public sealed record TestStreamQuery24() : IStreamQuery<int>; public sealed record TestStreamQuery24Handler : IStreamQueryHandler<TestStreamQuery24, int> { public async IAsyncEnumerable<int> Handle(TestStreamQuery24 query, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 24; } } }
public sealed record TestStreamQuery25() : IStreamQuery<int>; public sealed record TestStreamQuery25Handler : IStreamQueryHandler<TestStreamQuery25, int> { public async IAsyncEnumerable<int> Handle(TestStreamQuery25 query, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 25; } } }
public sealed record TestStreamQuery26() : IStreamQuery<int>; public sealed record TestStreamQuery26Handler : IStreamQueryHandler<TestStreamQuery26, int> { public async IAsyncEnumerable<int> Handle(TestStreamQuery26 query, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 26; } } }
public sealed record TestStreamQuery27() : IStreamQuery<int>; public sealed record TestStreamQuery27Handler : IStreamQueryHandler<TestStreamQuery27, int> { public async IAsyncEnumerable<int> Handle(TestStreamQuery27 query, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 27; } } }
public sealed record TestStreamQuery28() : IStreamQuery<int>; public sealed record TestStreamQuery28Handler : IStreamQueryHandler<TestStreamQuery28, int> { public async IAsyncEnumerable<int> Handle(TestStreamQuery28 query, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 28; } } }
public sealed record TestStreamQuery29() : IStreamQuery<int>; public sealed record TestStreamQuery29Handler : IStreamQueryHandler<TestStreamQuery29, int> { public async IAsyncEnumerable<int> Handle(TestStreamQuery29 query, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 29; } } }

public sealed record TestStreamCommand0() : IStreamCommand<int>; public sealed record TestStreamCommand0Handler : IStreamCommandHandler<TestStreamCommand0, int> { public async IAsyncEnumerable<int> Handle(TestStreamCommand0 command, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 0; } } }
public sealed record TestStreamCommand1() : IStreamCommand<int>; public sealed record TestStreamCommand1Handler : IStreamCommandHandler<TestStreamCommand1, int> { public async IAsyncEnumerable<int> Handle(TestStreamCommand1 command, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 1; } } }
public sealed record TestStreamCommand2() : IStreamCommand<int>; public sealed record TestStreamCommand2Handler : IStreamCommandHandler<TestStreamCommand2, int> { public async IAsyncEnumerable<int> Handle(TestStreamCommand2 command, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 2; } } }
public sealed record TestStreamCommand3() : IStreamCommand<int>; public sealed record TestStreamCommand3Handler : IStreamCommandHandler<TestStreamCommand3, int> { public async IAsyncEnumerable<int> Handle(TestStreamCommand3 command, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 3; } } }
public sealed record TestStreamCommand4() : IStreamCommand<int>; public sealed record TestStreamCommand4Handler : IStreamCommandHandler<TestStreamCommand4, int> { public async IAsyncEnumerable<int> Handle(TestStreamCommand4 command, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 4; } } }
public sealed record TestStreamCommand5() : IStreamCommand<int>; public sealed record TestStreamCommand5Handler : IStreamCommandHandler<TestStreamCommand5, int> { public async IAsyncEnumerable<int> Handle(TestStreamCommand5 command, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 5; } } }
public sealed record TestStreamCommand6() : IStreamCommand<int>; public sealed record TestStreamCommand6Handler : IStreamCommandHandler<TestStreamCommand6, int> { public async IAsyncEnumerable<int> Handle(TestStreamCommand6 command, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 6; } } }
public sealed record TestStreamCommand7() : IStreamCommand<int>; public sealed record TestStreamCommand7Handler : IStreamCommandHandler<TestStreamCommand7, int> { public async IAsyncEnumerable<int> Handle(TestStreamCommand7 command, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 7; } } }
public sealed record TestStreamCommand8() : IStreamCommand<int>; public sealed record TestStreamCommand8Handler : IStreamCommandHandler<TestStreamCommand8, int> { public async IAsyncEnumerable<int> Handle(TestStreamCommand8 command, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 8; } } }
public sealed record TestStreamCommand9() : IStreamCommand<int>; public sealed record TestStreamCommand9Handler : IStreamCommandHandler<TestStreamCommand9, int> { public async IAsyncEnumerable<int> Handle(TestStreamCommand9 command, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 9; } } }
public sealed record TestStreamCommand10() : IStreamCommand<int>; public sealed record TestStreamCommand10Handler : IStreamCommandHandler<TestStreamCommand10, int> { public async IAsyncEnumerable<int> Handle(TestStreamCommand10 command, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 10; } } }
public sealed record TestStreamCommand11() : IStreamCommand<int>; public sealed record TestStreamCommand11Handler : IStreamCommandHandler<TestStreamCommand11, int> { public async IAsyncEnumerable<int> Handle(TestStreamCommand11 command, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 11; } } }
public sealed record TestStreamCommand12() : IStreamCommand<int>; public sealed record TestStreamCommand12Handler : IStreamCommandHandler<TestStreamCommand12, int> { public async IAsyncEnumerable<int> Handle(TestStreamCommand12 command, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 12; } } }
public sealed record TestStreamCommand13() : IStreamCommand<int>; public sealed record TestStreamCommand13Handler : IStreamCommandHandler<TestStreamCommand13, int> { public async IAsyncEnumerable<int> Handle(TestStreamCommand13 command, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 13; } } }
public sealed record TestStreamCommand14() : IStreamCommand<int>; public sealed record TestStreamCommand14Handler : IStreamCommandHandler<TestStreamCommand14, int> { public async IAsyncEnumerable<int> Handle(TestStreamCommand14 command, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 14; } } }
public sealed record TestStreamCommand15() : IStreamCommand<int>; public sealed record TestStreamCommand15Handler : IStreamCommandHandler<TestStreamCommand15, int> { public async IAsyncEnumerable<int> Handle(TestStreamCommand15 command, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 15; } } }
public sealed record TestStreamCommand16() : IStreamCommand<int>; public sealed record TestStreamCommand16Handler : IStreamCommandHandler<TestStreamCommand16, int> { public async IAsyncEnumerable<int> Handle(TestStreamCommand16 command, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 16; } } }
public sealed record TestStreamCommand17() : IStreamCommand<int>; public sealed record TestStreamCommand17Handler : IStreamCommandHandler<TestStreamCommand17, int> { public async IAsyncEnumerable<int> Handle(TestStreamCommand17 command, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 17; } } }
public sealed record TestStreamCommand18() : IStreamCommand<int>; public sealed record TestStreamCommand18Handler : IStreamCommandHandler<TestStreamCommand18, int> { public async IAsyncEnumerable<int> Handle(TestStreamCommand18 command, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 18; } } }
public sealed record TestStreamCommand19() : IStreamCommand<int>; public sealed record TestStreamCommand19Handler : IStreamCommandHandler<TestStreamCommand19, int> { public async IAsyncEnumerable<int> Handle(TestStreamCommand19 command, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 19; } } }
public sealed record TestStreamCommand20() : IStreamCommand<int>; public sealed record TestStreamCommand20Handler : IStreamCommandHandler<TestStreamCommand20, int> { public async IAsyncEnumerable<int> Handle(TestStreamCommand20 command, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 20; } } }
public sealed record TestStreamCommand21() : IStreamCommand<int>; public sealed record TestStreamCommand21Handler : IStreamCommandHandler<TestStreamCommand21, int> { public async IAsyncEnumerable<int> Handle(TestStreamCommand21 command, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 21; } } }
public sealed record TestStreamCommand22() : IStreamCommand<int>; public sealed record TestStreamCommand22Handler : IStreamCommandHandler<TestStreamCommand22, int> { public async IAsyncEnumerable<int> Handle(TestStreamCommand22 command, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 22; } } }
public sealed record TestStreamCommand23() : IStreamCommand<int>; public sealed record TestStreamCommand23Handler : IStreamCommandHandler<TestStreamCommand23, int> { public async IAsyncEnumerable<int> Handle(TestStreamCommand23 command, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 23; } } }
public sealed record TestStreamCommand24() : IStreamCommand<int>; public sealed record TestStreamCommand24Handler : IStreamCommandHandler<TestStreamCommand24, int> { public async IAsyncEnumerable<int> Handle(TestStreamCommand24 command, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 24; } } }
public sealed record TestStreamCommand25() : IStreamCommand<int>; public sealed record TestStreamCommand25Handler : IStreamCommandHandler<TestStreamCommand25, int> { public async IAsyncEnumerable<int> Handle(TestStreamCommand25 command, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 25; } } }
public sealed record TestStreamCommand26() : IStreamCommand<int>; public sealed record TestStreamCommand26Handler : IStreamCommandHandler<TestStreamCommand26, int> { public async IAsyncEnumerable<int> Handle(TestStreamCommand26 command, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 26; } } }
public sealed record TestStreamCommand27() : IStreamCommand<int>; public sealed record TestStreamCommand27Handler : IStreamCommandHandler<TestStreamCommand27, int> { public async IAsyncEnumerable<int> Handle(TestStreamCommand27 command, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 27; } } }
public sealed record TestStreamCommand28() : IStreamCommand<int>; public sealed record TestStreamCommand28Handler : IStreamCommandHandler<TestStreamCommand28, int> { public async IAsyncEnumerable<int> Handle(TestStreamCommand28 command, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 28; } } }
public sealed record TestStreamCommand29() : IStreamCommand<int>; public sealed record TestStreamCommand29Handler : IStreamCommandHandler<TestStreamCommand29, int> { public async IAsyncEnumerable<int> Handle(TestStreamCommand29 command, [EnumeratorCancellation] CancellationToken cancellationToken) { for (int i = 0; i < 3; i++) { await Task.Yield(); yield return 29; } } }
// csharpier-ignore-end
