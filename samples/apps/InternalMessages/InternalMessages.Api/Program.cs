using InternalMessages.Application;
using InternalMessages.Domain;
using Mediator;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMediator(
    (MediatorOptions options) =>
    {
        options.Assemblies = [typeof(Ping), typeof(PingHandler)];
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
        async ([FromServices] Mediator.Mediator mediator) =>
        {
            var request = new Ping(Guid.NewGuid());

            var response = await mediator.Send(request);

            return Results.Ok(response.Id);
        }
    )
    .WithName("Ping");

app.Run();
