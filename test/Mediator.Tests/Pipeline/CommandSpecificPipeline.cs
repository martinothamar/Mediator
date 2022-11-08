using System.Threading;
using System.Threading.Tasks;

namespace Mediator.Tests.Pipeline;

public sealed class CommandSpecificPipeline<TCommand, TResponse> : IPipelineBehavior<TCommand, TResponse>
    where TCommand : IBaseCommand
{
    public static int CallCount { get; private set; }

    public ValueTask<TResponse> Handle(
        TCommand message,
        MessageHandlerDelegate<TCommand, TResponse> next,
        CancellationToken cancellationToken
    )
    {
        CallCount++;
        return next(message, cancellationToken);
    }
}
