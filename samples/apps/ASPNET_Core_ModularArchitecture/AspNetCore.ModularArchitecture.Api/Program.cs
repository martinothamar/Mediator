using AspNetCore.ModularArchitecture.ModuleA;
using AspNetCore.ModularArchitecture.ModuleB;
using Mediator;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

// AddMediatorCore() is allowed, but we use Assemblies to make modular boundary obvious
builder.Services.AddMediatorCore(options =>
{
    options.Assemblies = [typeof(ModuleAAssemblyMarker), typeof(ModuleBAssemblyMarker)];
});
builder.Services.AddModuleAHandlers();
builder.Services.AddModuleBHandlers();

var app = builder.Build();

app.MapOpenApi();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", "v1");
});

app.MapGet(
        "/module-a/{name}",
        async (string name, ISender sender, CancellationToken cancellationToken) =>
        {
            var response = await sender.Send(new GetModuleAGreeting(name), cancellationToken);
            return Results.Ok(response);
        }
    )
    .WithName("GetModuleAGreeting");

app.MapGet(
        "/module-b/public/{name}",
        async (string name, ISender sender, CancellationToken cancellationToken) =>
        {
            var response = await sender.Send(new GetModuleBPublicGreeting(name), cancellationToken);
            return Results.Ok(response);
        }
    )
    .WithName("GetModuleBPublicGreeting");

app.MapGet(
        "/module-b/internal/{name}",
        async (string name, ISender sender, CancellationToken cancellationToken) =>
        {
            var response = await sender.Send(new GetModuleBInternalGreeting(name), cancellationToken);
            return Results.Ok(response);
        }
    )
    .WithName("GetModuleBInternalGreeting");

app.Run();
