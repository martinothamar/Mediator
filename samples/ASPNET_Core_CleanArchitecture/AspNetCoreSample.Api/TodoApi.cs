using AspNetCoreSample.Application;
using AspNetCoreSample.Domain;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Riok.Mapperly.Abstractions;

namespace AspNetCoreSample.Api;

public static class TodoApi
{
    public static WebApplication MapTodoApi(this WebApplication app)
    {
        app.MapGet(
                "/api/todos",
                async (IMediator mediator, CancellationToken cancellationToken) =>
                {
                    var todos = await mediator.Send(new GetTodoItems(), cancellationToken);
                    return Results.Ok(todos.Select(t => TodoItemMapper.MapTodoItem(t)));
                }
            )
            .WithName("Get todos");

        app.MapPost(
                "/api/todos",
                async ([FromBody] AddTodoItemDto todo, IMediator mediator, CancellationToken cancellationToken) =>
                {
                    try
                    {
                        var addTodo = TodoItemMapper.MapAddTodoItem(todo);
                        var response = await mediator.Send(addTodo, cancellationToken);
                        return Results.Ok(TodoItemMapper.MapTodoItem(response));
                    }
                    catch (ValidationException ex)
                    {
                        return Results.BadRequest(ex.ValidationError);
                    }
                }
            )
            .WithName("Add todo");

        return app;
    }
}

public sealed record TodoItemDto(string Title, string Text, bool Done);

public sealed record AddTodoItemDto(string Title, string Text);

[Mapper]
public static partial class TodoItemMapper
{
    public static partial TodoItemDto MapTodoItem(this TodoItem car);

    public static partial AddTodoItem MapAddTodoItem(this AddTodoItemDto car);
}
