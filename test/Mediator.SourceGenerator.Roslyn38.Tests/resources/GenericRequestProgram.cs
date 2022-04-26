using Mediator;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

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
            var request = new SomeGenericRequest<Guid>(id);

            await mediator.Send(request);
        }
    }

    //
    // Types
    //

    public sealed record SomeGenericRequest<T>(T Value) : IRequest;

    public sealed class SomeGenericRequestHandler<T> : IRequestHandler<SomeGenericRequest<T>>
    {
        public ValueTask<Unit> Handle(SomeGenericRequest<T> request, CancellationToken cancellationToken)
        {
            return default;
        }
    }
}
