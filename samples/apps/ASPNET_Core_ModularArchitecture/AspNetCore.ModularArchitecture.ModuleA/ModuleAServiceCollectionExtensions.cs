using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.ModularArchitecture.ModuleA;

public static class ModuleAServiceCollectionExtensions
{
    public static IServiceCollection AddModuleAHandlers(this IServiceCollection services)
    {
        services.AddMediatorHandlers(options =>
        {
            options.GenerateTypesAsInternal = true;
        });
        return services;
    }
}
