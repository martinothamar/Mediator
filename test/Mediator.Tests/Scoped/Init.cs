#if Mediator_Lifetime_Scoped

using System.Runtime.CompilerServices;

namespace Mediator.Tests;

internal static class ScopedLifetimeInitializer
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        Fixture.CreateServiceScope = true;
    }
}

#endif
