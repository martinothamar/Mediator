using System;

namespace Mediator.Telemetry.Tests;

internal static class TestConfiguration
{
#if Mediator_Telemetry_EnableMetrics
    public static bool EnableMetrics { get; } = true;
#else
    public static bool EnableMetrics { get; } = false;
#endif

#if Mediator_Telemetry_MeterName_Tests
    public static string MeterName { get; } = "Mediator.Telemetry.Tests";
#else
    public static string MeterName { get; } = "Mediator";
#endif

#if Mediator_Lifetime_Scoped
    public static bool CreateServiceScope { get; } = true;
#else
    public static bool CreateServiceScope { get; } = false;
#endif

#if Mediator_Publisher_TaskWhenAll
    public static Type NotificationPublisherType => typeof(TaskWhenAllPublisher);
#elif Mediator_Publisher_ForeachAwait
    public static Type NotificationPublisherType => typeof(ForeachAwaitPublisher);
#else
    public static Type NotificationPublisherType => typeof(ForeachAwaitPublisher);
#endif
}
