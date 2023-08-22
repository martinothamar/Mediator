using Mediator;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

[assembly: MediatorOptions(Namespace = "SimpleConsole.Mediator", ServiceLifetime = ServiceLifetime.Transient)]

namespace Some.Nested.Types
{
    public static class Program
    {
        public static async Task Main()
        {
            var services = new ServiceCollection();

            services.AddMediator(
                options =>
                {
                    options.Namespace = "SimpleConsole.Mediator";
                    options.ServiceLifetime = ServiceLifetime.Transient;
                }
            );

            var serviceProvider = services.BuildServiceProvider();

            var mediator = serviceProvider.GetRequiredService<IMediator>();
        }
    }
}
