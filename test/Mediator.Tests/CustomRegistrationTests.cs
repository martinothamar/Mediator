using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Mediator.Tests
{
    public class CustomRegistrationTests
    {
        [Fact]
        public void WithoutCustomization_ShouldBeSingleton()
        {
            var (sp, _) = Fixture.GetMediator();

            var handlers = sp.GetServices<INotificationHandler<CustomNotification>>();
            Assert.NotNull(handlers);
            var handlersArray = handlers.Where(h => h is CustomNotificationHandler).ToArray();
            Assert.NotNull(handlersArray);
            Assert.Single(handlersArray);

            var handlers2 = sp.GetServices<INotificationHandler<CustomNotification>>();
            Assert.NotNull(handlers2);
            var handlers2Array = handlers2.Where(h => h is CustomNotificationHandler).ToArray();
            Assert.NotNull(handlers2Array);
            Assert.Single(handlers2Array);

            Assert.Same(handlersArray[0], handlers2Array[0]); // must be same instance
        }

        [Fact]
        public void WhenHandlerIsAlreadyRegistered_AsSingleton_DoesNotRegisterDuplicate()
        {
            var services = new ServiceCollection();
            services.AddSingleton<INotificationHandler<CustomNotification>, CustomNotificationHandler>();

            var (sp, _) = Fixture.GetMediator(services);

            var handlers = sp.GetServices<INotificationHandler<CustomNotification>>();
            Assert.NotNull(handlers);
            var handlersArray = handlers.Where(h => h is CustomNotificationHandler).ToArray();
            Assert.NotNull(handlersArray);
            Assert.Single(handlersArray);

            var handlers2 = sp.GetServices<INotificationHandler<CustomNotification>>();
            Assert.NotNull(handlers2);
            var handlers2Array = handlers2.Where(h => h is CustomNotificationHandler).ToArray();
            Assert.NotNull(handlers2Array);
            Assert.Single(handlers2Array);

            Assert.Same(handlersArray[0], handlers2Array[0]); // must be same instance
        }

        [Fact]
        public void WhenHandlerIsAlreadyRegistered_AsType_DoesNotRegisterDuplicate()
        {
            var services = new ServiceCollection();
            services.AddTransient<INotificationHandler<CustomNotification>, CustomNotificationHandler>();

            var (sp, _) = Fixture.GetMediator(services);

            var handlers = sp.GetServices<INotificationHandler<CustomNotification>>();
            Assert.NotNull(handlers);
            var handlersArray = handlers.Where(h => h is CustomNotificationHandler).ToArray();
            Assert.NotNull(handlersArray);
            Assert.Single(handlersArray);

            var handlers2 = sp.GetServices<INotificationHandler<CustomNotification>>();
            Assert.NotNull(handlers2);
            var handlers2Array = handlers2.Where(h => h is CustomNotificationHandler).ToArray();
            Assert.NotNull(handlers2Array);
            Assert.Single(handlers2Array);

            Assert.NotSame(handlersArray[0], handlers2Array[0]); // must be same instance
        }

        [Fact]
        public void WhenHandlerIsAlreadyRegistered_AsInstance_DoesNotRegisterDuplicate()
        {
            var services = new ServiceCollection();
            services.AddSingleton<INotificationHandler<CustomNotification>>(new CustomNotificationHandler());

            var (sp, _) = Fixture.GetMediator(services);

            var handlers = sp.GetServices<INotificationHandler<CustomNotification>>();
            Assert.NotNull(handlers);
            var handlersArray = handlers.Where(h => h is CustomNotificationHandler).ToArray();
            Assert.NotNull(handlersArray);
            Assert.Single(handlersArray);

            var handlers2 = sp.GetServices<INotificationHandler<CustomNotification>>();
            Assert.NotNull(handlers2);
            var handlers2Array = handlers2.Where(h => h is CustomNotificationHandler).ToArray();
            Assert.NotNull(handlers2Array);
            Assert.Single(handlers2Array);

            Assert.Same(handlersArray[0], handlers2Array[0]); // must be same instance
        }
    }

    public record CustomNotification : INotification { }

    public class CustomNotificationHandler : INotificationHandler<CustomNotification>
    {
        public ValueTask Handle(CustomNotification notification, CancellationToken cancellationToken)
        {
            return new();
        }
    }
}
