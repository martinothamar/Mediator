using Mediator;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

var services = new ServiceCollection();

// This extensions method is generated, and is put in the "Mediator" namespace by default.
services.AddMediator();

var serviceProvider = services.BuildServiceProvider();

var mediator = serviceProvider.GetRequiredService<IMediator>();

var id = Guid.NewGuid();
var ping = new StreamPing(id);

await foreach (var pong in mediator.CreateStream(ping))
{
    Console.WriteLine("-----------------------------------");
    Console.WriteLine("ID: " + id);
    Console.WriteLine(ping);
    Console.WriteLine(pong);
}

return 0;

//
// Here are the types used
//

public sealed record StreamPing(Guid Id) : IStreamRequest<Pong>;

public sealed record Pong(Guid Id);

public sealed class PingHandler : IStreamRequestHandler<StreamPing, Pong>
{
    public async IAsyncEnumerable<Pong> Handle(
        StreamPing request,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        for (int i = 0; i < 5; i++)
        {
            await Task.Delay(1000, cancellationToken);
            yield return new Pong(request.Id);
        }
    }
}
