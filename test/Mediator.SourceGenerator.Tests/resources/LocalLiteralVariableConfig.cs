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

            var ns = "SomeNamespace";
            var lifetime = ServiceLifetime.Scoped;

            services.AddMediator(
                options =>
                {
                    options.Namespace = ns;
                    options.ServiceLifetime = lifetime;
                }
            );

            var serviceProvider = services.BuildServiceProvider();

            var mediator = serviceProvider.GetRequiredService<IMediator>();
        }
    }
}
