using Autofac.Extensions.DependencyInjection;
using Mediator;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

builder.Services.AddMediator(
    config =>
    {
        config.Namespace = "Foo.Generated";
        config.ServiceLifetime = ServiceLifetime.Scoped;
    }
);

var app = builder.Build();

app.MapGet(
    "/",
    async ([FromServices] Foo.Generated.Mediator mediator) =>
    {
        var id = Guid.NewGuid();
        var request = new Ping(id);
        var response = await mediator.Send(request);
        return Results.Ok(response);
    }
);

app.Run();

public sealed record Ping(Guid Id) : IRequest<Pong>;

public sealed record Pong(Guid Id);

public sealed class PingHandler : IRequestHandler<Ping, Pong>
{
    public ValueTask<Pong> Handle(Ping request, CancellationToken cancellationToken)
    {
        return new ValueTask<Pong>(new Pong(request.Id));
    }
}
