## ASPNET Core Blazor app

Scaffolded as `dotnet new blazor -int Auto`.

> Uses interactive server-side rendering while downloading the Blazor bundle and activating the Blazor runtime on the client, then uses client-side rendering with WebAssembly.

## Run

```sh
cd AspNetCoreBlazor
dotnet run
```

Now you can open [localhost:5000](http://localhost:5000) and check out the
* Counter page - it uses a Mediator request to bump the counter
* Weather page - it uses a streaming query to update the UI
