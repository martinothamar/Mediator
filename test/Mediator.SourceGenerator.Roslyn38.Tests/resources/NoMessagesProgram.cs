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
        }
    }
}
