namespace Mediator
{
    public interface IBaseQuery : IMessage { }
    public interface IQuery<out TResponse> : IBaseQuery { }
}
