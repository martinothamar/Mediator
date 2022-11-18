using data;
using Mediator;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddMediator(options => options.ServiceLifetime = ServiceLifetime.Scoped);

var serviceProvider = services.BuildServiceProvider();
await using var scope = serviceProvider.CreateAsyncScope();

var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

await mediator.Send(new Ping());
Console.WriteLine("Pong");
