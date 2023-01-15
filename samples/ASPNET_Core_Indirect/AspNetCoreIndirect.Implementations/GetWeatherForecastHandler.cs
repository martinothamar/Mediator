using AspNetCoreIndirect.BaseClasses;

namespace AspNetCoreIndirect.Application.Controllers;

public class GetWeatherForecastHandler : ApplicationHandler<GetWeatherForecast, IReadOnlyList<WeatherForecast>>
{
    private static readonly string[] _summaries =
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
        "Scorching"
    };

    protected override ValueTask<IReadOnlyList<WeatherForecast>> ProcessRequest(
        GetWeatherForecast request,
        CancellationToken cancellationToken
    )
    {
        IReadOnlyList<WeatherForecast> result = Enumerable
            .Range(1, request.Count)
            .Select(
                index =>
                    new WeatherForecast
                    {
                        Date = DateTime.Now.AddDays(index),
                        TemperatureC = Random.Shared.Next(-20, 55),
                        Summary = _summaries[Random.Shared.Next(_summaries.Length)]
                    }
            )
            .ToArray();

        return ValueTask.FromResult(result);
    }
}
