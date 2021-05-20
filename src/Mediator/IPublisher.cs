using System.Threading;
using System.Threading.Tasks;

namespace Mediator
{
    public interface IPublisher
    {
        ValueTask Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotification;
    }
}
