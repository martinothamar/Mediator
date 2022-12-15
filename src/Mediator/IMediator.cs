namespace Mediator;

/// <summary>
/// Mediator instance for sending requests, commands, queries and their streaming counterparts (<see cref="IAsyncEnumerable{T}"/>).
/// Also for publushing notifications.
/// Use the concrete Mediator implementation for the highest performance (monomorphized method overloads per T available).
/// Can use the <see cref="ISender"/> and <see cref="IPublisher"/> for requests/commands/queries and notifications respectively.
/// </summary>
public interface IMediator : ISender, IPublisher { }
