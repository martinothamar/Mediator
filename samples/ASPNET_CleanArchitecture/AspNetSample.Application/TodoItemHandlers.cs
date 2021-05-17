using AspNetSample.Domain;
using FluentValidation;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using OneOf;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetSample.Application
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            //IPipelineBehavior<AddTodoItem, OneOf<TodoItemDto, ValidationError>> h = new MessageValidatorBehaviour<AddTodoItem, TodoItemDto>();

            return services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(MessageValidatorBehaviour<,>));
        }
    }

    public interface IValidate : IMessage
    {
        bool IsValid([NotNullWhen(false)] out ValidationError? error);
    }

    public sealed record ValidationError(IEnumerable<string> Errors);

    public sealed record AddTodoItem(string Title, string Text) : ICommand<OneOf<TodoItemDto, ValidationError>>, IValidate
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

    public class AddTodoItemValidator : AbstractValidator<AddTodoItem>
    {
        public AddTodoItemValidator()
        {
            RuleFor(x => x.Title).Length(1, 40);
            RuleFor(x => x.Text).Length(1, 150);
        }
    }

    public sealed class MessageValidatorBehaviour<TMessage, TResponse> : IPipelineBehavior<TMessage, TResponse>
        where TMessage : IMessage, IValidate
        where TResponse : IOneOf
    {
        public ValueTask<TResponse> Handle(TMessage message, CancellationToken cancellationToken, MessageHandlerDelegate<TMessage, TResponse> next)
        {
            if (!message.IsValid(out var validationError))
                return new ValueTask<TResponse>(default(TResponse)!);

            return next(message, cancellationToken);
        }
    }

    public sealed class TodoItemCommandHandler : ICommandHandler<AddTodoItem, OneOf<TodoItemDto, ValidationError>>
    {
        private readonly ITodoItemRepository _repository;

        public TodoItemCommandHandler(ITodoItemRepository repository)
        {
            _repository = repository;
        }

        public async ValueTask<OneOf<TodoItemDto, ValidationError>> Handle(AddTodoItem command, CancellationToken cancellationToken)
        {
            var item = new TodoItem(Guid.NewGuid(), command.Title, command.Text, false);

            await _repository.AddItem(item, cancellationToken);

            return new TodoItemDto(item.Title, item.Text, item.Done);
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
