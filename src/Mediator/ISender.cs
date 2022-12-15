namespace Mediator;

/// <summary>
/// Mediator instance for sending requests, queries, commands,
/// as well as their streaming counterparts (<see cref="IAsyncEnumerable{T}"/>).
/// </summary>
public interface ISender
{
    /// <summary>
    /// Send request.
    /// Throws <see cref="ArgumentNullException"/> if message is null.
    /// Throws <see cref="InvalidMessageException"/> if request does not implement <see cref="IRequest{TResponse}"/>.
    /// Throws <see cref="MissingMessageHandlerException"/> if no handler is registered.
    /// </summary>
    /// <param name="request">Incoming request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Awaitable task</returns>
    ValueTask<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send command.
    /// Throws <see cref="ArgumentNullException"/> if message is null.
    /// Throws <see cref="InvalidMessageException"/> if command does not implement <see cref="ICommand{TResponse}"/>.
    /// Throws <see cref="MissingMessageHandlerException"/> if no handler is registered.
    /// </summary>
    /// <param name="command">Incoming command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Awaitable task</returns>
    ValueTask<TResponse> Send<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send query.
    /// Throws <see cref="ArgumentNullException"/> if message is null.
    /// Throws <see cref="InvalidMessageException"/> if query does not implement <see cref="IQuery{TResponse}"/>.
    /// Throws <see cref="MissingMessageHandlerException"/> if no handler is registered.
    /// </summary>
    /// <param name="query">Incoming query</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Awaitable task</returns>
    ValueTask<TResponse> Send<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send message.
    /// Throws <see cref="ArgumentNullException"/> if message is null.
    /// Throws <see cref="InvalidMessageException"/> if message does not implement <see cref="IMessage"/>.
    /// Throws <see cref="MissingMessageHandlerException"/> if no handler is registered.
    /// </summary>
    /// <param name="message">Incoming message</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Awaitable task</returns>
    ValueTask<object?> Send(object message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create stream for query.
    /// Throws <see cref="ArgumentNullException"/> if message is null.
    /// Throws <see cref="InvalidMessageException"/> if query does not implement <see cref="IStreamQuery{TResponse}"/>.
    /// Throws <see cref="MissingMessageHandlerException"/> if no handler is registered.
    /// </summary>
    /// <param name="query">Incoming message</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Async enumerable</returns>
    IAsyncEnumerable<TResponse> CreateStream<TResponse>(
        IStreamQuery<TResponse> query,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Create stream for request.
    /// Throws <see cref="ArgumentNullException"/> if message is null.
    /// Throws <see cref="InvalidMessageException"/> if request does not implement <see cref="IStreamRequest{TResponse}"/>.
    /// Throws <see cref="MissingMessageHandlerException"/> if no handler is registered.
    /// </summary>
    /// <param name="request">Incoming message</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Async enumerable</returns>
    IAsyncEnumerable<TResponse> CreateStream<TResponse>(
        IStreamRequest<TResponse> request,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Create stream for command.
    /// Throws <see cref="ArgumentNullException"/> if message is null.
    /// Throws <see cref="InvalidMessageException"/> if command does not implement <see cref="IStreamCommand{TResponse}"/>.
    /// Throws <see cref="MissingMessageHandlerException"/> if no handler is registered.
    /// </summary>
    /// <param name="command">Incoming message</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Async enumerable</returns>
    IAsyncEnumerable<TResponse> CreateStream<TResponse>(
        IStreamCommand<TResponse> command,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Create stream.
    /// Throws <see cref="ArgumentNullException"/> if message is null.
    /// Throws <see cref="InvalidMessageException"/> if message does not implement <see cref="IStreamMessage"/>.
    /// Throws <see cref="MissingMessageHandlerException"/> if no handler is registered.
    /// </summary>
    /// <param name="message">Incoming message</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Async enumerable</returns>
    IAsyncEnumerable<object?> CreateStream(object message, CancellationToken cancellationToken = default);
}
