namespace Mediator
{
    public interface IMessage { }

    public interface IRequest : IMessage { }

    public interface IRequest<out TResponse> : IMessage { }

    public interface ICommand : IMessage { }

    public interface ICommand<out TResponse> : IMessage { }

    public interface IQuery<out TResponse> : IMessage { }

    public interface INotification : IMessage { }
}
