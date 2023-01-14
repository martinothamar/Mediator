## Scenario with the indirect reference of the Mediator.Abstraction

This example has been created to demonstrate the indirect `Mediator.Abstraction` usage approach and
to perform a regression testing after the source generator analyzer fix.
Source generator was ignoring projects if they are not directly using any of the types from `Mediator.Abstraction` package,
but rather include them indirectly through other projects.

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

HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8
Date: Sat, 14 Jan 2023 16:10:38 GMT
Server: Kestrel
Transfer-Encoding: chunked

[
  {
    "date": "2023-01-15T19:10:38.959973+03:00",
    "temperatureC": 33,
    "temperatureF": 91,
    "summary": "Warm"
  },
  {
    "date": "2023-01-16T19:10:38.978643+03:00",
    "temperatureC": 7,
    "temperatureF": 44,
    "summary": "Freezing"
  },
  {
    "date": "2023-01-17T19:10:38.978647+03:00",
    "temperatureC": 11,
    "temperatureF": 51,
    "summary": "Sweltering"
  },
  {
    "date": "2023-01-18T19:10:38.978647+03:00",
    "temperatureC": 25,
    "temperatureF": 76,
    "summary": "Balmy"
  },
  {
    "date": "2023-01-19T19:10:38.978647+03:00",
    "temperatureC": -18,
    "temperatureF": 0,
    "summary": "Balmy"
  }
]
```

