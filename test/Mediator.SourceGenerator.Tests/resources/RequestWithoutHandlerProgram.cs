using System;
using System.Threading;
using System.Threading.Tasks;
using Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace Some.Nested.Types
{
    public static class Program
    {
        public static async Task Main()
        {
            var services = new ServiceCollection();

            services.AddMediator();

            var serviceProvider = services.BuildServiceProvider();

            var mediator = serviceProvider.GetRequiredService<IMediator>();

            var id = Guid.NewGuid();
            var request = new Ping(id);

            _ = await mediator.Send(request);
        }

        //
        // Types
        //

        public sealed record Ping(Guid Id) : IRequest<Pong>;

        public sealed record Pong(Guid Id);
    }
}
