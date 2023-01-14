using Mediator;

namespace ASPNETCore.WeatherForecasts
{
    public sealed record GetWeatherForecasts : IQuery<IEnumerable<WeatherForecast>>;

    public sealed record GetWeatherForecastsHandler : IQueryHandler<GetWeatherForecasts, IEnumerable<WeatherForecast>>
    {
        private static readonly string[] Summaries = new[]
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

        public ValueTask<IEnumerable<WeatherForecast>> Handle(
            GetWeatherForecasts query,
            CancellationToken cancellationToken
        )
        {
            var result = Enumerable
                .Range(1, 5)
                .Select(
                    index =>
                        new WeatherForecast
                        {
                            Date = DateTime.Now.AddDays(index),
                            TemperatureC = Random.Shared.Next(-20, 55),
                            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
                        }
                )
                .ToArray();

            return new ValueTask<IEnumerable<WeatherForecast>>(result);
        }
    }
}
