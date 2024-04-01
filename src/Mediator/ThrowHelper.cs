using System.Diagnostics.CodeAnalysis;

namespace Mediator;

internal static class ThrowHelper
{
    [DoesNotReturn]
    internal static void ThrowInvalidOperationException(string message) => throw new InvalidOperationException(message);

    [DoesNotReturn]
    internal static void ThrowAggregateException(List<Exception> exceptions) =>
        throw new AggregateException(exceptions);
}
