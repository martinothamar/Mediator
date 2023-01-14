using AspNetCoreSample.Domain;

namespace AspNetCoreSample.Application;

public interface ITodoItemRepository
{
    ValueTask<IEnumerable<TodoItem>> GetItems(CancellationToken cancellationToken);

    ValueTask AddItem(TodoItem item, CancellationToken cancellationToken);
}
