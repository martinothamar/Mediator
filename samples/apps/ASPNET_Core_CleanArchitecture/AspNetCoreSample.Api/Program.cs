using AspNetCoreSample.Api;
using AspNetCoreSample.Application;
using AspNetCoreSample.Infrastructure;
using Mediator;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

const string serviceName = "AspNetCoreSample.Api";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure();
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

app.MapTodoApi();

app.Run();
