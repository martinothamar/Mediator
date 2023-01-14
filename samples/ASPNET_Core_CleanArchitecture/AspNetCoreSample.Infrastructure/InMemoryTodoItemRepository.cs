using AspNetCoreSample.Application;
using AspNetCoreSample.Domain;

namespace AspNetCoreSample.Infrastructure;

internal sealed class InMemoryTodoItemRepository : ITodoItemRepository
{
    private readonly object _lock = new { };
    private readonly List<TodoItem> _db = new();

    public ValueTask AddItem(TodoItem item, CancellationToken cancellationToken)
    {
        lock (_lock)
        {
            if (_db.Any(i => i.Id == item.Id))
                throw new Exception("Item already exists");

            _db.Add(item);
        }

        return default;
    }

    public ValueTask<IEnumerable<TodoItem>> GetItems(CancellationToken cancellationToken)
    {
        lock (_lock)
        {
            return new ValueTask<IEnumerable<TodoItem>>(_db.ToArray());
        }
    }
}
