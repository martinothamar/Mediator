using System.Runtime.CompilerServices;
using Mediator;

namespace AspNetCoreBlazor.Components.Pages;

internal sealed record GetWeatherForecasts(int Count) : IStreamQuery<WeatherForecast>;

internal sealed class WeatherForecast
{
    public DateOnly Date { get; set; }
    public int TemperatureC { get; set; }
    public string? Summary { get; set; }
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

internal sealed class GetWeatherForecastHandler : IStreamQueryHandler<GetWeatherForecasts, WeatherForecast>
{
    public async IAsyncEnumerable<WeatherForecast> Handle(
        GetWeatherForecasts query,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        var startDate = DateOnly.FromDateTime(DateTime.Now);
        var summaries = new[]
        {
            "Freezing",
            "Bracing",
            "Chilly",
            "Cool",
            "Mild",
            "Warm",
            "Balmy",
            "Hot",
            "Sweltering",
            "Scorching",
        };

        for (var i = 0; i < query.Count; i++)
        {
            await Task.Delay(100, cancellationToken);

            yield return new WeatherForecast
            {
                Date = startDate.AddDays(i),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = summaries[Random.Shared.Next(summaries.Length)],
            };
        }
    }
}
