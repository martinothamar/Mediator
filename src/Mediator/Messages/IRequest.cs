namespace Mediator;

public interface IRequest : IBaseRequest { }

public interface IRequest<out TResponse> : IBaseRequest { }
