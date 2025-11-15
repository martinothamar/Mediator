using Autofac.Extensions.DependencyInjection;
using Mediator;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

builder.Services.AddMediator(
    (MediatorOptions options) =>
    {
        options.Namespace = "Foo.Generated";
        options.Assemblies = [typeof(Ping)];
        options.ServiceLifetime = ServiceLifetime.Scoped;
    }
);

builder.Services.AddOpenApi();

var app = builder.Build();

app.MapOpenApi();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", "v1");
});

app.MapGet(
        "/ping",
        async ([FromServices] Foo.Generated.Mediator mediator) =>
        {
            var id = Guid.NewGuid();
            var request = new Ping(id);
            var response = await mediator.Send(request);
            return Results.Ok(response);
        }
    )
    .WithName("Ping");

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
