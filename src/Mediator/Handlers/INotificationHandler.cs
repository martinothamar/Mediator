using System.Threading;
using System.Threading.Tasks;

namespace Mediator
{
    public interface INotificationHandler<in TNotification>
        where TNotification : INotification
    {
        ValueTask Handle(TNotification notification, CancellationToken cancellationToken);
    }
}
