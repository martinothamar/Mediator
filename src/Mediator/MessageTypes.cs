namespace Mediator
{
    public readonly struct Unit { }

    public interface IMessage { }

    public interface IRequest : IMessage { }

    public interface IRequest<out TResponse> : IRequest { }

    public interface ICommand : IMessage { }

    public interface ICommand<out TResponse> : ICommand { }

    public interface IQuery<out TResponse> : IMessage { }

    public interface INotification : IMessage { }
}
