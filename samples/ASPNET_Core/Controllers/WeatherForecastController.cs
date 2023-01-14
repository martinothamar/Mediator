using ASPNETCore.WeatherForecasts;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace ASPNETCore.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IMediator _mediator;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public ValueTask<IEnumerable<WeatherForecast>> Get()
        {
            return _mediator.Send(new GetWeatherForecasts());
        }
    }
}
