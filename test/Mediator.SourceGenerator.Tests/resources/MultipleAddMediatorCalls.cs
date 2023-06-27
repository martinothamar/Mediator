using Mediator;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Some.Nested.Types
{
    public static class Program
    {
        public static void Main()
        {
            var services = new ServiceCollection();

            services.AddMediator((Mediator.MediatorOptions opts) => opts.ServiceLifetime = ServiceLifetime.Scoped);

            var serviceProvider = services.BuildServiceProvider();

            var mediator = serviceProvider.GetRequiredService<IMediator>();
        }
    }
}

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SomeDIExtension
    {
        public static IServiceCollection AddMediator(this IServiceCollection services, Action<MediatorOptions> options)
        {
            return services;
        }
    }

    public sealed class MediatorOptions
    {
        public string Namespace { get; set; } = "Mediator";

        public ServiceLifetime ServiceLifetime { get; set; } = ServiceLifetime.Singleton;
    }
}
