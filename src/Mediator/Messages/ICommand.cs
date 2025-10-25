namespace Mediator;

public interface ICommand : IBaseCommand { }

public interface ICommand<out TResponse> : IBaseCommand { }
