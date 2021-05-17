## Mediator

This is a high performance .NET implementation of the Mediator pattern using the [source generators](https://devblogs.microsoft.com/dotnet/introducing-c-source-generators/) feature introduced in .NET 5.
The API and usage is mostly based on the great [MediatR](https://github.com/jbogard/MediatR) library.

> **NOTE**
> This is very new, and API etc might change.
> Will soon publish pre-release NuGet packages.


Using source generators instead of relying on reflection has multiple benefits
* AOT friendly
  * Faster startup - build time reflection instead of runtime/startup reflection
* Build time errors instead of runtime errors
* Better performance
  * Runtime performance can be the same for both runtime reflection and source generator based approaches, but it's easier to optimize. High performance runtime reflection based implementation would rely on emitting IL which is hard to deal with in many ways. The source generator emitted code can be easily inspected and analyzed as any other C# code.
* More flexibility
  * Through the use of options (for example `[assembly: MediatorOptions("SimpleConsole.Mediator")]`) we can achieve more flexibility.

In particular, source generators in this library is used to
* Generate code for DI registration
  * Includes polymorphic dispatch and constrained generics in pipeline steps and notification handlers.
* Generate code for `IMediator` interface and implementation
  * Request/Command/Query `Send` methods are monomorphized (1 method per T), the generic `ISender.Send` methods rely on these

- [Mediator](#mediator)
- [2. Usage](#2-usage)
  - [2.1. Message types](#21-message-types)
  - [2.2. Handler types](#22-handler-types)
  - [2.3. Pipeline types](#23-pipeline-types)
  - [2.4. Simple end-to-end example](#24-simple-end-to-end-example)
- [3. Benchmarks](#3-benchmarks)


## 2. Usage

There are two NuGet packages needed to use this library
* Mediator.SourceGenerator
  * To generate the `IMediator` interface, implementation and dependency injection setup.
* Mediator
  * Message types (`IRequest<,>`, `INotification`), handler types (`IRequestHandler<,>`, `INotificationHandler<>`), pipeline types (`IPipelineBehavior`)

You install the source generator package into your edge/outermost project (i.e. ASP.NET Core application, Background worker project),
and then use the `Mediator` package wherever you defined message types and handlers.
Standard message handlers are automatically picked up and added to the DI container in the generated `AddMediator` method.
Pipeline behaviors need to be added manually.

For example implementations, see the samples folder.

### 2.1. Message types

* `IMessage` - marker interface
* `IRequest` - a request message, no return value (`ValueTask`)
* `IRequest<out TResponse>` - a request message with a response (`ValueTask<TResponse>`)
* `ICommand` - a command message, no return value (`ValueTask`)
* `ICommand<out TResponse>` - a command message with a response (`ValueTask<TResponse>`)
* `IQuery<out TResponse>` - a query message with a response (`ValueTask<TResponse>`)
* `INotification` - a notification message, no return value (`ValueTask`)

As you can see, you can achieve the exact same thing with requests, commands and queries. But I find the distinction in naming useful if you for example use the CQRS pattern or for some reason have a preference on naming in your application. In the future this could even be configurable as the source generator could generate anything.

### 2.2. Handler types

* `IRequestHandler<in TRequest>`
* `IRequestHandler<in TRequest, TResponse>`
* `ICommandHandler<in TCommand>`
* `ICommandHandler<in TCommand, TResponse>`
* `IQueryHandler<in TQuery, TResponse>`
* `INotificationHandler<in TNotification>`

These types are used in correlation with the message types above.

### 2.3. Pipeline types

* `IPipelineBehavior<TMessage>`
* `IPipelineBehavior<TMessage, TResponse>`

This means that if you want a generic message handler (for all message, with or without responses),
you need to implement both interfaces. Like so:

```csharp
public sealed class GenericHandler<TMessage, TResponse> : IPipelineBehavior<TMessage>, IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
    public ValueTask Handle(TMessage message, CancellationToken cancellationToken, MessageHandlerDelegate<TMessage> next)
    {
        ...
        return next(message, cancellationToken);
    }

    public ValueTask<TResponse> Handle(TMessage message, CancellationToken cancellationToken, MessageHandlerDelegate<TMessage, TResponse> next)
    {
        ...
        return next(message, cancellationToken);
    }
}
```

### 2.4. Simple end-to-end example

Add package

```pwsh
dotnet add package Mediator.SourceGenerator --version notyetpublished
dotnet add package Mediator --version notyetpublished
```
or
```xml
<PackageReference Include="Mediator.SourceGenerator" Version="notyetpublished">
  <PrivateAssets>all</PrivateAssets>
  <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
</PackageReference>
<PackageReference Include="Mediator" Version="notyetpublished" />
```

In `ConfigureServices` or equivalent, call `AddMediator` (unless `MediatorOptions` is configured, default namespace is `Mediator`).
This registers your handler below.

```csharp
using Mediator;

services.AddMediator();
```

Then somewhere in your application, define your messages, handlers and pipelines

```csharp
public sealed record Ping(Guid Id) : IRequest<Pong>;

public sealed record Pong(Guid Id);

public sealed class PingHandler : IRequestHandler<Ping, Pong>
{
    public ValueTask<Pong> Handle(Ping request, CancellationToken cancellationToken)
    {
        return new ValueTask<Pong>(new Pong(request.Id));
    }
}
```

Say for instance that you want to validate the ping, then you can add a validator to the pipeline

```csharp
public sealed class PingValidator : IPipelineBehavior<Ping, Pong>
{
    public ValueTask<Pong> Handle(Ping request, CancellationToken cancellationToken, MessageHandlerDelegate<Ping, Pong> next)
    {
        if (request is null || request.Id == default)
            throw new ArgumentException("Invalid input");

        return next(request, cancellationToken);
    }
}
```

## 3. Benchmarks

This benchmark exposes the perf overhead of the libraries.
Baseline is a simple method invocation.
Mediator (this library) and MediatR methods show the overhead of the respective mediator implementations.

See [benchmarks code](/benchmarks/Mediator.Benchmarks/Request/RequestBenchmarks.cs) for more information.

``` ini

BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19042
Intel Core i7-7700HQ CPU 2.80GHz (Kaby Lake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=5.0.300-preview.21228.15
  [Host]     : .NET Core 5.0.6 (CoreCLR 5.0.621.22011, CoreFX 5.0.621.22011), X64 RyuJIT
  DefaultJob : .NET Core 5.0.6 (CoreCLR 5.0.621.22011, CoreFX 5.0.621.22011), X64 RyuJIT


```
|                  Method |        Mean |    Error |   StdDev | Ratio |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|------------------------ |------------:|---------:|---------:|------:|-------:|------:|------:|----------:|
|     SendRequest_MediatR | 1,051.89 ns | 3.154 ns | 2.951 ns |  1.00 | 0.4349 |     - |     - |    1368 B |
|    SendRequest_Mediator |    86.14 ns | 0.145 ns | 0.121 ns |  0.08 |      - |     - |     - |         - |
| SendRequest_MessagePipe |    14.21 ns | 0.061 ns | 0.054 ns |  0.01 |      - |     - |     - |         - |
|    SendRequest_Baseline |    11.67 ns | 0.040 ns | 0.033 ns |  0.01 |      - |     - |     - |         - |
