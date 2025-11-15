using Mediator;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMediator(
    (MediatorOptions options) =>
    {
        options.ServiceLifetime = ServiceLifetime.Singleton;
        options.PipelineBehaviors = [typeof(TelemetryBehavior<,>)];
    }
);
builder.Services.AddHttpContextAccessor();

builder.Services.AddOpenApi();

var app = builder.Build();

app.MapOpenApi();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", "v1");
});

app.UseHttpsRedirection();

app.MapGet(
        "/weatherforecast",
        async Task<Ok<IReadOnlyList<WeatherForecast>>> (
            HttpContext httpContext,
            [FromServices] IMediator mediator,
            CancellationToken cancellationToken
        ) =>
        {
            var context = new ApplicationRequestContext();
            var forecast = await mediator.Send(new GetWeatherForecasts(5, context));

            httpContext.Response.Headers.TryAdd("X-App-CacheHit", context.CacheHit.ToString().ToLowerInvariant());

            return TypedResults.Ok(forecast);
        }
    )
    .WithName("GetWeatherForecast");

app.Run();

public sealed class ApplicationRequestContext
{
    public bool CacheHit { get; set; }
}

public interface IApplicationQuery : IQuery<IReadOnlyList<WeatherForecast>>
{
    public ApplicationRequestContext Context { get; }
}

public sealed record GetWeatherForecasts(int Limit, ApplicationRequestContext Context) : IApplicationQuery;

public sealed record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

internal sealed class TelemetryBehavior<TMessage, TResponse>(IHttpContextAccessor _httpContextAccessor)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : notnull, IMessage
{
    public ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken
    )
    {
        var context = message switch
        {
            IApplicationQuery query => query.Context,
            _ => null,
        };

        if (context is null)
            return next(message, cancellationToken);

        return HandleAsync(context, message, next, cancellationToken);

        async ValueTask<TResponse> HandleAsync(
            ApplicationRequestContext context,
            TMessage message,
            MessageHandlerDelegate<TMessage, TResponse> next,
            CancellationToken cancellationToken
        )
        {
            try
            {
                return await next(message, cancellationToken);
            }
            finally
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext is not null)
                {
                    var activity = httpContext.Features.Get<IHttpActivityFeature>()?.Activity;
                    activity?.SetTag("cache.hit", context.CacheHit.ToString().ToLowerInvariant());
                }
            }
        }
    }
}

internal sealed class GetWeatherForecastsHandler : IQueryHandler<GetWeatherForecasts, IReadOnlyList<WeatherForecast>>
{
    private static readonly string[] _summaries =
    [
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
    ];

    public ValueTask<IReadOnlyList<WeatherForecast>> Handle(
        GetWeatherForecasts query,
        CancellationToken cancellationToken
    )
    {
        var hitCache = Random.Shared.NextSingle() < 0.5f;

        if (hitCache)
        {
            query.Context.CacheHit = true;
            var forecast = GenerateForecast(query.Limit);

            return new ValueTask<IReadOnlyList<WeatherForecast>>(forecast);
        }

        return HandleAsync(query, cancellationToken);

        async ValueTask<IReadOnlyList<WeatherForecast>> HandleAsync(
            GetWeatherForecasts query,
            CancellationToken cancellationToken
        )
        {
            query.Context.CacheHit = false;

            await Task.Delay(100, cancellationToken); // Let's pretend we're doing something

            var forecast = GenerateForecast(query.Limit);

            return forecast;
        }
    }

    private WeatherForecast[] GenerateForecast(int limit)
    {
        var forecast = Enumerable
            .Range(1, 5)
            .Select(index => new WeatherForecast(
                DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                Random.Shared.Next(-20, 55),
                _summaries[Random.Shared.Next(_summaries.Length)]
            ))
            .ToArray();
        return forecast;
    }
}
