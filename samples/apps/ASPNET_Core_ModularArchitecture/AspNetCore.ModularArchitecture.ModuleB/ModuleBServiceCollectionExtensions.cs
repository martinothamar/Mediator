using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.ModularArchitecture.ModuleB;

public static class ModuleBServiceCollectionExtensions
{
    public static IServiceCollection AddModuleBHandlers(this IServiceCollection services)
    {
        services.AddMediatorHandlers(options =>
        {
            options.GenerateTypesAsInternal = true;
        });
        return services;
    }
}
