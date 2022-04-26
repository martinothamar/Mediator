using Microsoft.Extensions.DependencyInjection;

namespace Mediator.Tests.TestTypes;

public static class Test
{
    public static void Some()
    {
        var services = new ServiceCollection();
        services.AddSingleton(typeof(IRequestHandler<>).MakeGenericType(typeof(SomeGenericRequest<>)), typeof(SomeGenericRequestHandler<>));
    }
}

public sealed record SomeGenericRequest<T>(T Value) : IRequest;

public sealed class SomeGenericRequestHandler<T> : IRequestHandler<SomeGenericRequest<T>>
{
    public ValueTask<Unit> Handle(SomeGenericRequest<T> request, CancellationToken cancellationToken)
    {
        return default;
    }
}
