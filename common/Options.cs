using Mediator;
using Microsoft.Extensions.DependencyInjection;

[assembly: MediatorOptions(
#if Mediator_Lifetime_Singleton
    ServiceLifetime = ServiceLifetime.Singleton
#elif Mediator_Lifetime_Transient
    ServiceLifetime = ServiceLifetime.Transient
#elif Mediator_Lifetime_Scoped
    ServiceLifetime = ServiceLifetime.Scoped
#else
    ServiceLifetime = ServiceLifetime.Singleton
#endif
#if Mediator_Publisher_TaskWhenAll
    ,
    NotificationPublisherType = typeof(TaskWhenAllPublisher)
#elif Mediator_Publisher_ForeachAwait
    ,
    NotificationPublisherType = typeof(ForeachAwaitPublisher)
#else
    ,
    NotificationPublisherType = typeof(ForeachAwaitPublisher)
#endif
#if Mediator_CachingMode_Lazy
    ,
    CachingMode = CachingMode.Lazy
#elif Mediator_CachingMode_Eager
    ,
    CachingMode = CachingMode.Eager
#else
    ,
    CachingMode = CachingMode.Eager
#endif
)]
