using AspNetSample.Domain;
using Mediator;

namespace AspNetSample.Application;

public sealed class TodoItemCommandHandler : ICommandHandler<AddTodoItem, TodoItemDto>
{
    private readonly ITodoItemRepository _repository;

    public TodoItemCommandHandler(ITodoItemRepository repository)
    {
        _repository = repository;
    }

    public async ValueTask<TodoItemDto> Handle(AddTodoItem command, CancellationToken cancellationToken)
    {
        var item = new TodoItem(Guid.NewGuid(), command.Title, command.Text, false);

        await _repository.AddItem(item, cancellationToken);

        return new TodoItemDto(item.Title, item.Text, item.Done);
    }
}
