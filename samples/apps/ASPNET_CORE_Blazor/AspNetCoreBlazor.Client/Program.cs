using AspNetCoreBlazor.Client.Pages;
using Mediator;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddMediator(
    (MediatorOptions options) =>
    {
        options.Assemblies = [typeof(IncrementCounter)];
        options.GenerateTypesAsInternal = true;
    }
);

await builder.Build().RunAsync();
