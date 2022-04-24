using Microsoft.Extensions.DependencyInjection;

namespace Mediator.Benchmarks;

internal static class MediatorConfig
{
    public const ServiceLifetime Lifetime = ServiceLifetime.Singleton;
}
