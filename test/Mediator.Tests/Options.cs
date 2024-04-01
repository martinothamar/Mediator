using Mediator;
using Microsoft.Extensions.DependencyInjection;

#if TASKWHENALLPUBLISHER
[assembly: MediatorOptions(
    ServiceLifetime = ServiceLifetime.Singleton,
    NotificationPublisherType = typeof(TaskWhenAllPublisher)
)]
#else
[assembly: MediatorOptions(ServiceLifetime = ServiceLifetime.Singleton)]
#endif
