namespace Mediator
{
    public interface IMessage { }

    public interface IStreamMessage { }

    public interface IStreamMessage<out TResponse> : IStreamMessage { }
}
