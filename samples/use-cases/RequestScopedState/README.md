## Request-scoped state

Sample implementation of having per-request-state despite using `Singleton` service lifetime.
The implementation uses a `ApplicationRequestContext` to model the scoped state per logical request.
The same context instance could be passed to multiple queries/commands as needed.
The context contains a simple `CacheHit` boolean.
This is very artificial example, but hopefully you can imagine how it can be used/extended based on your scenario.

* `CacheHit` is set in the query handler - we simulate cache hits about 50% of the time
* `IPipelineBehavior` annotates `Activity`/trace with a `cache.hit` tag based on the `CacheHit` property on the context
* HTTP endpoint appends custom `X-App-CacheHit` header based on the `CacheHit` property on the context

The point of this sample is to illustrate that the structuring of the logic/application/domain can (and perhaps should) be
independent of the state-keeping requirements of a given application.

### Build and run

```console
$ dotnet run
Building...
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5236
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Microsoft.Hosting.Lifetime[0]
      Hosting environment: Development
info: Microsoft.Hosting.Lifetime[0]
      Content root path: /home/martin/code/private/Mediator/samples/use-cases/RequestScopedState
```

Call the endpoint, observe the `X-App-CacheHit` header:

```console
$ curl -X 'GET' 'http://localhost:5236/weatherforecast' -H 'accept: application/json' -i
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8
Date: Sun, 28 Sep 2025 12:44:21 GMT
Server: Kestrel
Transfer-Encoding: chunked
X-App-CacheHit: false

[{"date":"2025-09-29","temperatureC":7,"summary":"Balmy","temperatureF":44},{"date":"2025-09-30","temperatureC":54,"summary":"Mild","temperatureF":129},{"date":"2025-10-01","temperatureC":2,"summary":"Cool","temperatureF":35},{"date":"2025-10-02","temperatureC":48,"summary":"Sweltering","temperatureF":118},{"date":"2025-10-03","temperatureC":47,"summary":"Warm","temperatureF":116}]
```
