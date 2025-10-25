using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Mediator;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

// This extensions method is generated, and is put in the "Mediator" namespace by default.
services.AddMediator(
    (MediatorOptions options) =>
    {
        options.Assemblies = [typeof(StreamQuery)];
    }
);

services.AddSingleton(typeof(IStreamPipelineBehavior<,>), typeof(StreamPreProcessor<,>));
services.AddSingleton(typeof(IStreamPipelineBehavior<,>), typeof(StreamPostProcessor<,>));

var serviceProvider = services.BuildServiceProvider();

var mediator = serviceProvider.GetRequiredService<IMediator>();

Console.WriteLine("=== Stream Message Processing Example ===\n");

// Send stream query
var query = new StreamQuery("test query");

int count = 0;
await foreach (var result in mediator.CreateStream(query))
{
    count++;
    Console.WriteLine($"[Main] Received result: {result.Data}");
}

Console.WriteLine($"\nTotal received {count} results");
Console.WriteLine("\nNote:");
Console.WriteLine("- Pre-processor executes once before stream starts");
Console.WriteLine("- Post-processor executes once after stream completes, with all items");

return 0;

//
// Here are the types used
//

// Define a stream query
public sealed record StreamQuery(string Query) : IStreamQuery<StreamResult>;

public sealed record StreamResult(int Index, string Data);

// Stream query handler
public sealed class StreamQueryHandler : IStreamQueryHandler<StreamQuery, StreamResult>
{
    public async IAsyncEnumerable<StreamResult> Handle(
        StreamQuery query,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        for (int i = 0; i < 5; i++)
        {
            await Task.Delay(100, cancellationToken);
            yield return new StreamResult(i, $"Processing '{query.Query}' - Result {i}");
        }
    }
}

// Stream message pre-processor - runs before handler execution
public sealed class StreamPreProcessor<TMessage, TResponse> : StreamMessagePreProcessor<TMessage, TResponse>
    where TMessage : notnull, IStreamMessage
{
    protected override ValueTask Handle(TMessage message, CancellationToken cancellationToken)
    {
        Console.WriteLine($"[PreProcess] Starting stream message processing: {typeof(TMessage).Name}");
        return default;
    }
}

// Stream message post-processor - runs once after stream completes
public sealed class StreamPostProcessor<TMessage, TResponse> : StreamMessagePostProcessor<TMessage, TResponse>
    where TMessage : notnull, IStreamMessage
{
    protected override ValueTask Handle(
        TMessage message,
        IReadOnlyList<TResponse> responses,
        CancellationToken cancellationToken
    )
    {
        Console.WriteLine($"[PostProcess] Stream completed with {responses.Count()} items");
        return default;
    }
}
