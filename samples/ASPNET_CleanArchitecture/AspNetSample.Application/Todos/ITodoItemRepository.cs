using AspNetSample.Domain;

namespace AspNetSample.Application;

public interface ITodoItemRepository
{
    ValueTask<IEnumerable<TodoItem>> GetItems(CancellationToken cancellationToken);

    ValueTask AddItem(TodoItem item, CancellationToken cancellationToken);
}
