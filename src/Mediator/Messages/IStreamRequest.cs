namespace Mediator;

public interface IBaseStreamRequest : IStreamMessage { }

public interface IStreamRequest<out TResponse> : IBaseStreamRequest { }
