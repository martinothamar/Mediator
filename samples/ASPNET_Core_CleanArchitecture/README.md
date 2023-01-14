## Clean architecture with ASP.NET Core

This sample shows a more complete example on how Mediator can be used, following
an onion/clean architecture style.

### Run

Run the application in Visual Studio or through the dotnet CLI.
The Swagger UI should be visible at [http://localhost:5000/swagger/index.html](http://localhost:5000/swagger/index.html).

#### REST client

Use the `post-todo.http` file and the "REST Client" VSCode extension to test it out:

```http
POST http://localhost:5000/api/todos HTTP/1.1
content-type: application/json

{
  "title": "",
  "text": "This is a todo without a title, we should get an error..."
}

HTTP/1.1 400 Bad Request
Connection: close
Date: Mon, 17 May 2021 10:51:45 GMT
Content-Type: application/json; charset=utf-8
Server: Kestrel
Transfer-Encoding: chunked

{
  "errors": [
    "'Title' must be between 1 and 40 characters. You entered 0 characters."
  ]
}
```

### Code smells

#### Exceptions for control flow

Exceptions are not good for control flow, but in this sample a `ValidationException`
is used to "return early" from a generic pipeline behaviour.
The `MessageValidatorBehaviour` will process any message sent through mediator which implements `IValidate`.

Ideally, the return value for the `AddTodoItem` was some sort of `Result<TResponse, ValidationError>` or `OneOf<TResponse, ValidationError>` type,
in which case we could just return an instance of `ValidationError` in the pipeline behaviour before invoking the handler.
Then the caller would have to handle both success and failure cases, `TResponse` or `ValidationError` and return statuscode accordingly.
