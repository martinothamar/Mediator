using Mediator;
using Microsoft.Extensions.DependencyInjection;

#if TASKWHENALLPUBLISHER
[assembly: MediatorOptions(
    ServiceLifetime = ServiceLifetime.Transient,
    NotificationPublisherType = typeof(TaskWhenAllPublisher)
)]
#else
[assembly: MediatorOptions(ServiceLifetime = ServiceLifetime.Transient)]
#endif
