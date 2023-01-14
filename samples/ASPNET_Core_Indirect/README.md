## Scenario with the indirect reference of the Mediator.Abstraction

This example has been created to show the problem in the Source Generator that ignores project
if they are not directly using any of the types from `Mediator.Abstraction` package,
but rather include it indirectly through other projects.

`AspNetCoreIndirect.BaseClasses` contains the base classes that are used by the `AspNetCoreIndirect.Implementations`
project, that provides the implementations of controllers and handlers. `AspNetCoreIndirect.Application` is the entry point
and references Source Generator.

### Run

Run the application in Visual Studio or through the dotnet CLI.
The Swagger UI should be visible at [http://localhost:5000/swagger/index.html](http://localhost:5000/swagger/index.html).

#### REST client

Use the `get-weather-forecast.http` file and the "REST Client" VSCode extension to test it out.

You

```http
GET http://localhost:5000/WeatherForecast

HTTP/1.1 500 Internal Server Error
Content-Type: application/json; charset=utf-8
Date: Sat, 14 Jan 2023 15:16:08 GMT
Server: Kestrel
Transfer-Encoding: chunked

{
  "message": "No handler registered for message type: AspNetCoreIndirect.Application.GetWeatherForecast"
}
```

