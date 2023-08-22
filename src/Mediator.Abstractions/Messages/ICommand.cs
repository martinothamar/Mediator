namespace Mediator;

public interface ICommand : ICommand<Unit> { }

public interface ICommand<out TResponse> : IBaseCommand { }
