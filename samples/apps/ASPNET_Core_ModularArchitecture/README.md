## Modular architecture with ASP.NET Core

This sample demonstrates a modular registration style for Mediator:

* The API project calls `AddMediatorCore(...)` once to generate the shared mediator and wrapper infrastructure.
* Each module exposes an extension method that calls `AddMediatorHandlers()` from within the module assembly.
* `ModuleB` includes an `internal` request handler to demonstrate that handler registrations can stay encapsulated inside the module.

### Run

Run the API project in Visual Studio or through the dotnet CLI.
The Swagger UI should be visible at [http://localhost:5000/swagger/index.html](http://localhost:5000/swagger/index.html).

### Endpoints

* `GET /module-a/{name}`
* `GET /module-b/public/{name}`
* `GET /module-b/internal/{name}`
