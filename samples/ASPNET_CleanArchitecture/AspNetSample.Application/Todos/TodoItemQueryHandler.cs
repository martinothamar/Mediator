using Mediator;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetSample.Application
{

    public sealed class TodoItemQueryHandler : IQueryHandler<GetTodoItems, IEnumerable<TodoItemDto>>
    {
        private readonly ITodoItemRepository _repository;

        public TodoItemQueryHandler(ITodoItemRepository repository)
        {
            _repository = repository;
        }

        public async ValueTask<IEnumerable<TodoItemDto>> Handle(GetTodoItems query, CancellationToken cancellationToken)
        {
            var items = await _repository.GetItems(cancellationToken);

            return items
                .Select(i => new TodoItemDto(i.Title, i.Text, i.Done))
                .ToArray();
        }
    }
}
