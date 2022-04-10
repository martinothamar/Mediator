using Mediator;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Some.Nested.Types
{
    public static class Program
    {
        private static readonly string MediatorNamespace = "SimpleConsole.Mediator";
        private static readonly ServiceLifetime Lifetime = ServiceLifetime.Transient;

        public static async Task Main()
        {
            var services = new ServiceCollection();

            services.AddMediator(options =>
            {
                options.Namespace = MediatorNamespace;
                options.DefaultServiceLifetime = Lifetime;
            });

            var serviceProvider = services.BuildServiceProvider();

            var mediator = serviceProvider.GetRequiredService<IMediator>();
        }
    }
}
