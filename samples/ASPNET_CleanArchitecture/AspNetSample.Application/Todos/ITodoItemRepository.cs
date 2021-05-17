using AspNetSample.Domain;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetSample.Application
{
    public interface ITodoItemRepository
    {
        ValueTask<IEnumerable<TodoItem>> GetItems(CancellationToken cancellationToken);

        ValueTask AddItem(TodoItem item, CancellationToken cancellationToken);
    }
}
