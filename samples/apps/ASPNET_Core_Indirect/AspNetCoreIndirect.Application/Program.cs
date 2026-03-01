using AspNetCoreIndirect.Application;
using Mediator;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

const string serviceName = "AspNetCoreIndirect.Application";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddMediator(
    (MediatorOptions options) =>
    {
        options.Assemblies = [typeof(GetWeatherForecast)];
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

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapOpenApi();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", "v1");
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
