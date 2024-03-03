using System;
using System.Threading;
using System.Threading.Tasks;
using Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace Some.Nested.Types
{
    public static class Program
    {
        private static ServiceLifetime Lifetime { get; }

        public static async Task Main()
        {
            var services = new ServiceCollection();

            services.AddMediator(options =>
            {
                options.ServiceLifetime = Lifetime;
            });

            var serviceProvider = services.BuildServiceProvider();

            var mediator = serviceProvider.GetRequiredService<IMediator>();
        }
    }
}
