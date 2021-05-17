namespace Mediator
{
    public readonly struct Unit { }

    public interface IMessage { }

    public interface IBaseRequest : IMessage { }
    public interface IRequest : IRequest<Unit> { }
    public interface IRequest<out TResponse> : IBaseRequest { }


    public interface IBaseCommand : IMessage { }
    public interface ICommand : ICommand<Unit> { }
    public interface ICommand<out TResponse> : IBaseCommand { }

    public interface IBaseQuery : IMessage { }
    public interface IQuery<out TResponse> : IBaseQuery { }


    public interface INotification : IMessage { }
}
