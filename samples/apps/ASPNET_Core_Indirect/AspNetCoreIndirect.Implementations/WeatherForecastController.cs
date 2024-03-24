using AspNetCoreIndirect.BaseClasses;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCoreIndirect.Application.Controllers;

[Route("[controller]")]
public class WeatherForecastController : ApplicationController
{
    [HttpGet]
    public Task<IActionResult> Get([FromQuery] GetWeatherForecast request) =>
        ProcessAsync<GetWeatherForecast, IReadOnlyList<WeatherForecast>>(request);
}
