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

#if Mediator_Large_Project
var request = new TestRequest0();
#else
var request = new Ping(Guid.NewGuid());
#endif

await mediator.Send(request);
#else
var request = new Ping(Guid.NewGuid());
var handler = provider.GetRequiredService<PingHandler>();
await handler.Handle(request, default);
#endif

return 0;

#if !Mediator_Large_Project || Mediator_Baseline
public sealed record Ping(Guid Id) : IRequest<Pong>;

public sealed record Pong(Guid Id);

public sealed class PingHandler : IRequestHandler<Ping, Pong>
{
    public ValueTask<Pong> Handle(Ping request, CancellationToken cancellationToken) =>
        new ValueTask<Pong>(new Pong(request.Id));
}
#endif
