using AspNetCoreSample.Domain;
using Mediator;

namespace AspNetCoreSample.Application;

public sealed record GetTodoItems() : IQuery<IEnumerable<TodoItem>>;

public sealed class TodoItemQueryHandler : IQueryHandler<GetTodoItems, IEnumerable<TodoItem>>
{
    private readonly ITodoItemRepository _repository;

    public TodoItemQueryHandler(ITodoItemRepository repository) => _repository = repository;

    public ValueTask<IEnumerable<TodoItem>> Handle(GetTodoItems query, CancellationToken cancellationToken) =>
        _repository.GetItems(cancellationToken);
}
