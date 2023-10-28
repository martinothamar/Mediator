using Mediator;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Some.Nested.Types
{
    public static class Program
    {
        private static string MediatorNamespace { get; }

        public static async Task Main()
        {
            var services = new ServiceCollection();

            services.AddMediator(options =>
            {
                options.Namespace = MediatorNamespace;
            });

            var serviceProvider = services.BuildServiceProvider();

            var mediator = serviceProvider.GetRequiredService<IMediator>();
        }
    }
}
