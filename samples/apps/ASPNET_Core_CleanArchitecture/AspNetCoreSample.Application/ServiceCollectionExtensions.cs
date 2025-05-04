using Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCoreSample.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediator(
            (MediatorOptions options) =>
            {
                options.Assemblies = [typeof(AddTodoItem)];
                options.ServiceLifetime = ServiceLifetime.Scoped;
            }
        );
        return services
            .AddSingleton(typeof(IPipelineBehavior<,>), typeof(ErrorLoggingBehaviour<,>))
            .AddSingleton(typeof(IPipelineBehavior<,>), typeof(MessageValidatorBehaviour<,>));
    }
}
