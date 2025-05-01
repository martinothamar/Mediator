using Mediator;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

// This extensions method is generated, and is put in the "Mediator" namespace by default.
// We override the namespace in the "MediatorOptions" object.
services.AddMediator(options =>
{
    options.Namespace = "SimpleConsoleAOT";
    options.PipelineBehaviors =
    [
        // Standard handlers are added by default, but we need to add pipeline steps manually.
        // Here are two examples.
        typeof(GenericLoggerHandler<,>), // This will run 1st
        typeof(PingValidator), // This will run 2nd
    ];
});

var serviceProvider = services.BuildServiceProvider();

var mediator = serviceProvider.GetRequiredService<IMediator>();

var id = Guid.NewGuid();
var request = new Ping(id);

var response = await mediator.Send(request);

Console.WriteLine("-----------------------------------");
Console.WriteLine("ID: " + id);
Console.WriteLine(request);
Console.WriteLine(response);

return response.Id == id ? 0 : 1;

//
// Here are the types used
//

public readonly record struct Ping(Guid Id) : IRequest<Pong>;

public readonly record struct Pong(Guid Id);

public sealed class GenericLoggerHandler<TMessage, TResponse> : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken
    )
    {
        Console.WriteLine("1) Running logger handler");
        try
        {
            var response = await next(message, cancellationToken);
            Console.WriteLine("5) No error!");
            return response;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
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
        Console.WriteLine("2) Running ping validator");
        if (request.Id == default)
            throw new ArgumentException("Invalid input");
        else
            Console.WriteLine("3) Valid input!");

        return next(request, cancellationToken);
    }
}

public sealed class PingHandler : IRequestHandler<Ping, Pong>
{
    public ValueTask<Pong> Handle(Ping request, CancellationToken cancellationToken)
    {
        Console.WriteLine("4) Returning pong!");
        return new ValueTask<Pong>(new Pong(request.Id));
    }
}
