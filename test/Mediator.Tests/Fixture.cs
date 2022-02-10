using Microsoft.Extensions.DependencyInjection;

namespace Mediator.Tests;

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

        IServiceProvider sp = services.BuildServiceProvider(validateScopes: true);

        var shouldCreateScope = createScope.HasValue ? createScope.Value : CreateServiceScope;
        if (shouldCreateScope)
            sp = sp.CreateScope().ServiceProvider;

        var mediator = sp.GetRequiredService<IMediator>();
        return (shouldCreateScope ? sp.CreateScope().ServiceProvider : sp, mediator!);
    }
}
