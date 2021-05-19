using AspNetSample.Application;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetSample.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            return services.AddSingleton<ITodoItemRepository, InMemoryTodoItemRepository>();
        }
    }
}
