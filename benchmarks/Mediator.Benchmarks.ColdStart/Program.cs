#pragma warning disable CS0162 // Unreachable code detected

using Mediator;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
#if !Mediator_Baseline
services.AddMediator();
#else
services.AddSingleton<PingHandler>();
#endif

await using var provider = services.BuildServiceProvider();

#if !Mediator_Baseline
#if Mediator_Lifetime_Scoped
await using var scope = provider.CreateAsyncScope();
var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
#else
var mediator = provider.GetRequiredService<IMediator>();
#endif

var request = new Ping();

var n = await mediator.Send(request);
#else
var request = new Ping();
var handler = provider.GetRequiredService<PingHandler>();
var n = await handler.Handle(request, default);
#endif

Console.WriteLine(n);
return 0;

public sealed record Ping() : IRequest<int>;

public sealed class PingHandler : IRequestHandler<Ping, int>
{
    public ValueTask<int> Handle(Ping request, CancellationToken cancellationToken) =>
        new ValueTask<int>(Random.Shared.Next());
}
