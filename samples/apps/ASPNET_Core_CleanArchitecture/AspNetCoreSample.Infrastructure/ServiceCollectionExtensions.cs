using AspNetCoreSample.Application;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCoreSample.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        return services.AddSingleton<ITodoItemRepository, InMemoryTodoItemRepository>();
    }
}
