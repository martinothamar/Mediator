using Mediator;

namespace AspNetSample.Application;

public sealed record GetTodoItems() : IQuery<IEnumerable<TodoItemDto>>;
