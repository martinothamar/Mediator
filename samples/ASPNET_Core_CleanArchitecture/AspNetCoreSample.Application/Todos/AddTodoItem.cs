using AspNetCoreSample.Domain;
using FluentValidation;
using Mediator;
using System.Diagnostics.CodeAnalysis;

namespace AspNetCoreSample.Application;

public class AddTodoItemValidator : AbstractValidator<AddTodoItem>
{
    public AddTodoItemValidator()
    {
        RuleFor(x => x.Title).Length(1, 40);
        RuleFor(x => x.Text).Length(1, 150);
    }
}

public sealed record AddTodoItem(string Title, string Text) : ICommand<TodoItem>, IValidate
{
    public bool IsValid([NotNullWhen(false)] out ValidationError? error)
    {
        var validator = new AddTodoItemValidator();
        var result = validator.Validate(this);
        if (result.IsValid)
            error = null;
        else
            error = new ValidationError(result.Errors.Select(e => e.ErrorMessage).ToArray());

        return result.IsValid;
    }
}

public sealed class TodoItemCommandHandler : ICommandHandler<AddTodoItem, TodoItem>
{
    private readonly ITodoItemRepository _repository;

    public TodoItemCommandHandler(ITodoItemRepository repository) => _repository = repository;

    public async ValueTask<TodoItem> Handle(AddTodoItem command, CancellationToken cancellationToken)
    {
        var item = new TodoItem(Guid.NewGuid(), command.Title, command.Text, false);

        await _repository.AddItem(item, cancellationToken);

        return item;
    }
}
