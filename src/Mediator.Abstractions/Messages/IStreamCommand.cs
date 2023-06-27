namespace Mediator;

public interface IBaseStreamCommand : IStreamMessage { }

public interface IStreamCommand<out TResponse> : IBaseStreamCommand { }
