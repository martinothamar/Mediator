# IncludeTypesForGeneration - Modular Monolith Support

The `IncludeTypesForGeneration` option enables filtering of request types during source generation based on marker interfaces. This is particularly useful for modular monolith architectures where multiple APIs/modules share the same codebase but should only generate mediator code for their respective requests.

## How It Works

When you specify one or more marker interface types in `IncludeTypesForGeneration`, only request types that implement at least one of those marker interfaces will be included in code generation. If the collection is empty or not configured, **all discovered request types will be included** (no filtering applied).

## Usage Example

### 1. Define Marker Interfaces

```csharp
// Marker interface for API1 requests
public interface IApi1Request { }

// Marker interface for API2 requests
public interface IApi2Request { }
```

### 2. Implement Request Types with Marker Interfaces

```csharp
// Shared between both APIs
public record SharedRequest(Guid Id) 
    : IRequest<SharedResponse>, IApi1Request, IApi2Request;

// API1 only
public record Api1OnlyRequest(Guid Id) 
    : IRequest<Api1Response>, IApi1Request;

// API2 only
public record Api2OnlyRequest(Guid Id) 
    : IRequest<Api2Response>, IApi2Request;

// Not included in either API (no marker interface)
public record InternalRequest(Guid Id) 
    : IRequest<InternalResponse>;
```

### 3. Configure Each API's Mediator

```csharp
// In Api1's startup/configuration
builder.Services.AddMediator(options =>
{
    options.Namespace = "Api1";
    options.ServiceLifetime = ServiceLifetime.Transient;
    options.GenerateTypesAsInternal = true;
    options.IncludeTypesForGeneration = [typeof(IApi1Request)];
});

// In Api2's startup/configuration
builder.Services.AddMediator(options =>
{
    options.Namespace = "Api2";
    options.ServiceLifetime = ServiceLifetime.Transient;
    options.GenerateTypesAsInternal = true;
    options.IncludeTypesForGeneration = [typeof(IApi2Request)];
});
```

### Result

- **Api1** will generate code for: `SharedRequest`, `Api1OnlyRequest`
- **Api2** will generate code for: `SharedRequest`, `Api2OnlyRequest`
- Neither will generate code for: `InternalRequest` (no marker interface)

## Benefits

1. **No Service Validation Issues**: Only handlers for included request types are registered in DI, so you won't need to disable validation or deal with missing dependencies for handlers you don't use.

2. **Clean Separation**: Each API/module only contains mediator code for requests it actually handles.

3. **Shared Requests**: Requests can be easily shared across multiple APIs by implementing multiple marker interfaces.

4. **Type Safety**: Compiler ensures requests implement the correct marker interfaces at build time.

## Aspire Support

This feature works seamlessly with .NET Aspire applications where you have multiple API projects in the same solution:

```csharp
// AppHost project
var builder = DistributedApplication.CreateBuilder(args);

var api1 = builder.AddProject<Projects.Api1>("api1");
var api2 = builder.AddProject<Projects.Api2>("api2");
var worker = builder.AddProject<Projects.Worker>("worker");

builder.Build().Run();
```

Each project can configure its own mediator with `IncludeTypesForGeneration` to only generate code for its relevant requests.

## Alternative: Exact Type Filtering

While the primary use case is marker interfaces, you can also specify exact request types:

```csharp
options.IncludeTypesForGeneration = [
    typeof(Request1),
    typeof(Request2)
];
```

However, this requires listing every request type explicitly and doesn't scale well for modular monoliths. The marker interface approach is recommended.
