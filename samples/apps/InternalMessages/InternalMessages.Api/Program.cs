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

var app = builder.Build();

app.MapGet(
    "/",
    async ([FromServices] Mediator.Mediator mediator) =>
    {
        var request = new Ping(Guid.NewGuid());

        var response = await mediator.Send(request);

        return Results.Ok(response.Id);
    }
);

app.Run();
