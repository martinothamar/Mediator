using MassTransit;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMediator(options =>
{
    options.Namespace = "SimpleConsole.Mediator";
    options.ServiceLifetime = ServiceLifetime.Transient;
});

builder.Services.AddMassTransit();

var app = builder.Build();

// Example endpoint to publish an event
app.MapPost("/weather/update", async (IPublishEndpoint publisher, string city, int temperature) =>
{
    await publisher.Publish(new WeatherUpdated(city, temperature));
    return Results.Ok($"Weather update published for {city}.");
});
app.Run();

public class WeatherUpdatedConsumer : IConsumer<WeatherUpdated>
{
    public Task Consume(ConsumeContext<WeatherUpdated> context)
    {
        Console.WriteLine($"Received update: {context.Message.City} is {context.Message.Temperature}°C");
        return Task.CompletedTask;
    }
}
public record WeatherUpdated(string City, int Temperature);
