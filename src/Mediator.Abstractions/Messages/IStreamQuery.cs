namespace Mediator;

public interface IBaseStreamQuery : IStreamMessage { }

public interface IStreamQuery<out TResponse> : IBaseStreamQuery { }
