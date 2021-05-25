using Microsoft.Extensions.DependencyInjection;
using System;

namespace Mediator.Tests
{
    public static class Fixture
    {
        public static bool CreateServiceScope;

        public static (IServiceProvider sp, IMediator mediator) GetMediator(
            Action<IServiceCollection>? configureServices = null,
            bool? createScope = null
        )
        {
            var services = new ServiceCollection();

            services.AddMediator();

            configureServices?.Invoke(services);

            var sp = services.BuildServiceProvider();

            var mediator = sp.GetRequiredService<IMediator>();

            var shouldCreateScope = createScope.HasValue ? (createScope.Value) : CreateServiceScope;
            return (shouldCreateScope ? sp.CreateScope().ServiceProvider : sp, mediator!);
        }
    }
}
