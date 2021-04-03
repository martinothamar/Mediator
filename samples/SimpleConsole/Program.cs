using Mediator;
using Microsoft.Extensions.DependencyInjection;
using SimpleConsole.Mediator;
using System;
using System.Threading;
using System.Threading.Tasks;

[assembly: MediatorOptions("SimpleConsole.Mediator")]

namespace SimpleConsole
{
    class Program
    {
        static async Task<int> Main()
        {
            var services = new ServiceCollection();

            services.AddMediator();

            services.AddSingleton<IPipelineBehavior<SomeRequest, SomeResponse>, Validator>();

            var serviceProvider = services.BuildServiceProvider();

            var mediator = serviceProvider.GetRequiredService<IMediator>();

            var id = Guid.NewGuid();
            var request = new SomeRequest(id);

            var response = await mediator.Send(request);

            Console.WriteLine("ID: " + id);
            Console.WriteLine(request);
            Console.WriteLine(response);

            return response.Id == id ? 0 : 1;
        }
    }

    public sealed class Validator : IPipelineBehavior<SomeRequest, SomeResponse>
    {
        public ValueTask<SomeResponse> Handle(SomeRequest request, CancellationToken cancellationToken, MessageHandlerDelegate<SomeRequest, SomeResponse> next)
        {
            if (request is null || request.Id == default)
                throw new ArgumentException("Invalid input");
            else
                Console.WriteLine("Validator says it's valid!");

            return next(request!, cancellationToken);
        }
    }

    public sealed record SomeRequest(Guid Id) : IRequest<SomeResponse>;

    public sealed record SomeResponse(Guid Id);

    public sealed class SomeClass : IRequestHandler<SomeRequest, SomeResponse>
    {
        public ValueTask<SomeResponse> Handle(SomeRequest request, CancellationToken cancellationToken)
        {
            return new ValueTask<SomeResponse>(new SomeResponse(request.Id));
        }
    }
}
