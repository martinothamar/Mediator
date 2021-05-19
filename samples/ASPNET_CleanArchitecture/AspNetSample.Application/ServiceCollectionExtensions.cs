using Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetSample.Application
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            return services
                .AddSingleton(typeof(IPipelineBehavior<,>), typeof(ErrorLoggingBehaviour<,>))
                .AddSingleton(typeof(IPipelineBehavior<,>), typeof(MessageValidatorBehaviour<,>));
        }
    }
}
