using InternalMessages.Application;
using InternalMessages.Domain;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

const string serviceName = "InternalMessages.Api";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMediator(
    (MediatorOptions options) =>
    {
        options.Assemblies = [typeof(Ping), typeof(PingHandler)];
        options.Telemetry.EnableMetrics = true;
        options.Telemetry.EnableTracing = true;
    }
);
builder
    .Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(serviceName))
    .WithMetrics(metrics =>
        metrics
            .AddAspNetCoreInstrumentation()
            .AddMeter(Mediator.Mediator.MeterName)
            .AddOtlpExporter(
                (_, metricReaderOptions) =>
                    metricReaderOptions.PeriodicExportingMetricReaderOptions.ExportIntervalMilliseconds = 10_000
            )
    )
    .WithTracing(tracing =>
        tracing
            .SetSampler(new AlwaysOnSampler())
            .AddAspNetCoreInstrumentation()
            .AddSource(Mediator.Mediator.ActivitySourceName)
            .AddOtlpExporter()
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
