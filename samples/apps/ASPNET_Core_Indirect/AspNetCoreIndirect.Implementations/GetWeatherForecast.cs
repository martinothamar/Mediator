using AspNetCoreIndirect.BaseClasses;

namespace AspNetCoreIndirect.Application;

public record GetWeatherForecast(int Count = 5) : ApplicationRequest<IReadOnlyList<WeatherForecast>>;
