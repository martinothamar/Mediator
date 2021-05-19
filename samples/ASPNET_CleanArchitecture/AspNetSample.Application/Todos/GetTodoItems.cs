using Mediator;
using System.Collections.Generic;

namespace AspNetSample.Application
{
    public sealed record GetTodoItems() : IQuery<IEnumerable<TodoItemDto>>;
}
