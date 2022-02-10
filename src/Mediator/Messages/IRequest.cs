namespace Mediator
{
    public interface IBaseRequest : IMessage { }
    public interface IRequest : IRequest<Unit> { }
    public interface IRequest<out TResponse> : IBaseRequest { }
    public interface IStreamRequest<out TResponse> : IStreamMessage { }
}
