namespace Mediator
{
    public interface IBaseQuery : IMessage { }
    public interface IQuery<out TResponse> : IBaseQuery { }
    public interface IStreamQuery<out TResponse> : IStreamMessage { }
}
