using Mediator;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Some.Nested.Types
{
    public static class Program
    {
        private const string MediatorNamespace = "SimpleConsole.Mediator";
        private const ServiceLifetime Lifetime = ServiceLifetime.Transient;

        public static async Task Main()
        {
            var services = new ServiceCollection();

            services.AddMediator(
                options =>
                {
                    options.Namespace = MediatorNamespace;
                    options.ServiceLifetime = Lifetime;
                }
            );

            var serviceProvider = services.BuildServiceProvider();

            var mediator = serviceProvider.GetRequiredService<IMediator>();
        }
    }
}
