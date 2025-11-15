using System.Collections.Concurrent;
using System.Reflection;
using App;
using MassTransit;
using Mediator;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddMediator(
    (MediatorOptions options) =>
    {
        options.Assemblies = [typeof(WeatherUpdated)];
    }
);

builder.Services.AddMassTransit(options =>
{
    options.AddConsumers(Assembly.GetEntryAssembly());
    options.UsingInMemory(
        (context, cfg) =>
        {
            cfg.ConfigureEndpoints(context);
        }
    );
});

builder.Services.AddSingleton<Db>();

var app = builder.Build();

app.MapOpenApi();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", "v1");
});

app.MapPost(
    "/weather/update/{city}/{temperature:int}",
    async (IBus bus, string city, int temperature) =>
    {
        await bus.Publish(new WeatherUpdated(city, temperature));
        return Results.Ok($"Weather update published for {city}.");
    }
);

app.Run();

namespace App
{
    public sealed record WeatherUpdated(string City, int Temperature) : INotification;

    public sealed class WeatherUpdatedConsumer(IMediator mediator) : IConsumer<WeatherUpdated>
    {
        private readonly IMediator _mediator = mediator;

        public async Task Consume(ConsumeContext<WeatherUpdated> context) => await _mediator.Publish(context.Message);
    }

    public sealed class WeatherUpdateLogger(ILogger<WeatherUpdateLogger> logger) : INotificationHandler<WeatherUpdated>
    {
        private readonly ILogger<WeatherUpdateLogger> _logger = logger;

        public ValueTask Handle(WeatherUpdated notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Handled update: {City} is {Temperature}°C",
                notification.City,
                notification.Temperature
            );
            return default;
        }
    }

    public sealed class WeatherUpdateStorage(Db db) : INotificationHandler<WeatherUpdated>
    {
        private readonly Db _db = db;

        public ValueTask Handle(WeatherUpdated notification, CancellationToken cancellationToken)
        {
            _db.Store(notification.City, notification.Temperature);
            return default;
        }
    }

    public sealed class Db
    {
        private readonly ConcurrentDictionary<string, int> _storage = new();

        public void Store(string city, int temperature) => _storage[city] = temperature;
    }
}
