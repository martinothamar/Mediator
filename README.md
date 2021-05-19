## Mediator

This is a high performance .NET implementation of the Mediator pattern using the [source generators](https://devblogs.microsoft.com/dotnet/introducing-c-source-generators/) feature introduced in .NET 5.
The API and usage is mostly based on the great [MediatR](https://github.com/jbogard/MediatR) library.

> **NOTE**
> This is very new, and API etc might change.
> Will soon publish pre-release NuGet packages.

Using source generators instead of relying on reflection has multiple benefits
* AOT friendly
  * MediatR will not work in AOT, as it uses parts of reflection API that generates code during runtime
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
- [2. Benchmarks](#2-benchmarks)
- [3. Usage](#3-usage)
  - [3.1. Message types](#31-message-types)
  - [3.2. Handler types](#32-handler-types)
  - [3.3. Pipeline types](#33-pipeline-types)
  - [3.4. Simple end-to-end example](#34-simple-end-to-end-example)
    - [3.4.1. Add package](#341-add-package)
    - [3.4.2. Add Mediator to DI container](#342-add-mediator-to-di-container)
    - [3.4.3. Add your messages and handlers](#343-add-your-messages-and-handlers)
    - [3.4.4. Use pipeline behaviors for middleware](#344-use-pipeline-behaviors-for-middleware)
    - [3.4.5. Use pipeline behaviors for cross cutting concerns using open generics](#345-use-pipeline-behaviors-for-cross-cutting-concerns-using-open-generics)
    - [3.4.6. Use notifications](#346-use-notifications)
    - [3.4.7. Polymorphic dispatch with notification handlers](#347-polymorphic-dispatch-with-notification-handlers)

## 2. Benchmarks

This benchmark exposes the perf overhead of the libraries.
Baseline is a simple method invocation.
Mediator (this library) and MediatR methods show the overhead of the respective mediator implementations.

See [benchmarks code](/benchmarks/Mediator.Benchmarks/Request/RequestBenchmarks.cs) for more information.

## 3. Usage

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

### 3.1. Message types

* `IMessage` - marker interface
* `IRequest` - a request message, no return value (`ValueTask`)
* `IRequest<out TResponse>` - a request message with a response (`ValueTask<TResponse>`)
* `ICommand` - a command message, no return value (`ValueTask`)
* `ICommand<out TResponse>` - a command message with a response (`ValueTask<TResponse>`)
* `IQuery<out TResponse>` - a query message with a response (`ValueTask<TResponse>`)
* `INotification` - a notification message, no return value (`ValueTask`)

As you can see, you can achieve the exact same thing with requests, commands and queries. But I find the distinction in naming useful if you for example use the CQRS pattern or for some reason have a preference on naming in your application. In the future this could even be configurable as the source generator could generate anything.

### 3.2. Handler types

* `IRequestHandler<in TRequest>`
* `IRequestHandler<in TRequest, TResponse>`
* `ICommandHandler<in TCommand>`
* `ICommandHandler<in TCommand, TResponse>`
* `IQueryHandler<in TQuery, TResponse>`
* `INotificationHandler<in TNotification>`

These types are used in correlation with the message types above.

### 3.3. Pipeline types

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

### 3.4. Simple end-to-end example

#### 3.4.1. Add package

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

#### 3.4.2. Add Mediator to DI container

In `ConfigureServices` or equivalent, call `AddMediator` (unless `MediatorOptions` is configured, default namespace is `Mediator`).
This registers your handler below.

```csharp
using Mediator;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection(); // Most likely IServiceCollection comes from IHostBuilder/Generic host abstraction in Microsoft.Extensions.Hosting

services.AddMediator();
```

#### 3.4.3. Add your messages and handlers

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

#### 3.4.4. Use pipeline behaviors for middleware

The pipeline behavior below validates all incoming `Ping` messages.

```csharp
services.AddMediator();
services.AddSingleton<IPipelineBehavior<Ping, Pong>, PingValidator>();

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

#### 3.4.5. Use pipeline behaviors for cross cutting concerns using open generics

Add open generic handler to process all or a subset of messages passing through Mediator.
This handler will log any error that is thrown from message handlers (`IRequest`, `ICommand`, `IQuery`).
It also publishes a notification allowing notification handlers to react to errors.

```csharp
services.AddMediator();
services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(ErrorLoggerHandler<,>));

public sealed record ErrorMessage(Exception Exception) : INotification;
public sealed record SuccessfulMessage() : INotification;

public sealed class ErrorLoggerHandler<TMessage, TResponse> : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage // Constrained to IMessage, or constrain to IBaseCommand or any custom interface you've implemented
{
    private readonly ILogger<ErrorLoggerHandler<TMessage, TResponse>> _logger;
    private readonly IMediator _mediator;

    public ErrorLoggerHandler(ILogger<ErrorLoggerHandler<TMessage, TResponse>> logger, IMediator mediator) 
    {
        _logger = logger;
        _mediator = mediator;
    }

    public async ValueTask<TResponse> Handle(TMessage message, CancellationToken cancellationToken, MessageHandlerDelegate<TMessage, TResponse> next)
    {
        try
        {
            var response = await next(message, cancellationToken);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling message");
            await _mediator.Publish(new ErrorMessage(ex));
            throw;
        }
    }
}
```

#### 3.4.6. Use notifications

We can define a notification handler to catch errors from the above pipeline behavior.

```csharp
// Notification handlers are automatically added to DI container

public sealed class ErrorNotificationHandler : INotificationHandler<ErrorMessage>
{
    public ValueTask Handle(ErrorMessage error, CancellationToken cancellationToken)
    {
        // Could log to application insights or something...
        return default;
    }
}
```

#### 3.4.7. Polymorphic dispatch with notification handlers

We can also define a notification handler that receives all notifications.

```csharp

public sealed class StatsNotificationHandler : INotificationHandler<INotification> // or any other interface deriving from INotification
{
    private long _messageCount;
    private long _messageErrorCount;

    public (long MessageCount, long MessageErrorCount) Stats => (_messageCount, _messageErrorCount);

    public ValueTask Handle(INotification notification, CancellationToken cancellationToken)
    {
        if (notification is SuccessfulMessage)
            Interlocked.Increment(ref _messageCount);
        if (notification is ErrorMessage)
            Interlocked.Increment(ref _messageErrorCount);
        return default;
    }
}
```
