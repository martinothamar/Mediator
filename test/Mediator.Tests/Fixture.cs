using Microsoft.Extensions.DependencyInjection;
using System;

namespace Mediator.Tests
{
    public static class Fixture
    {
        public static (IServiceProvider sp, IMediator mediator) GetMediator(Action<IServiceCollection>? configureServices = null)
        {
            var services = new ServiceCollection();

            services.AddMediator();

            configureServices?.Invoke(services);

            var sp = services.BuildServiceProvider();

            var mediator = sp.GetRequiredService<IMediator>();

            return (sp, mediator!);
        }
    }
}
