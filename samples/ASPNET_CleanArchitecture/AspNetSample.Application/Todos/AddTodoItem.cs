using Mediator;
using System.Diagnostics.CodeAnalysis;

namespace AspNetSample.Application;

public sealed record AddTodoItem(string Title, string Text) : ICommand<TodoItemDto>, IValidate
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
