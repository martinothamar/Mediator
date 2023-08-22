using Mediator;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Some.Nested.Types
{
    public static class Program
    {
        private const string NS = "SomeNamespace";
        private const ServiceLifetime Lifetime = ServiceLifetime.Scoped;

        private static readonly string NSStaticReadonly = NS;
        private static readonly ServiceLifetime LifetimeStaticReadOnly = Lifetime;

        private static string NSStaticProp { get; } = NSStaticReadonly;
        private static ServiceLifetime LifetimeStaticProp { get; } = LifetimeStaticReadOnly;

        public static async Task Main()
        {
            var services = new ServiceCollection();

            var ns = NSStaticProp;
            var lifetime = LifetimeStaticProp;

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
