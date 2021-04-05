using AspNetSample.Domain;
using FluentValidation;
using Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetSample.Application
{
    public sealed record AddTodoItem(string Title, string Text) : ICommand;

    public class AddTodoItemValidator : AbstractValidator<AddTodoItem>
    {
        public AddTodoItemValidator()
        {
            RuleFor(x => x.Title).Length(1, 40);
            RuleFor(x => x.Text).Length(1, 150);
        }
    }

    public sealed class TodoItemCommandHandler : ICommandHandler<AddTodoItem>
    {
        private readonly ITodoItemRepository _repository;

        public TodoItemCommandHandler(ITodoItemRepository repository)
        {
            _repository = repository;
        }

        public ValueTask Handle(AddTodoItem command, CancellationToken cancellationToken)
        {
            var item = new TodoItem(Guid.NewGuid(), command.Title, command.Text, false);

            return _repository.AddItem(item, cancellationToken);
        }
    }

    public sealed record GetTodoItems() : IQuery<IEnumerable<TodoItemDto>>;

    public sealed record TodoItemDto(string Title, string Text, bool Done);

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
