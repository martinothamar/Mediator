using Mediator.Tests.TestTypes;
using Microsoft.Extensions.DependencyInjection;

namespace Mediator.Tests.TransientLifetime;

public sealed class TransientLifetimeTests
{
    [Fact]
    public void Test_Generated_Code_Lifetime()
    {
        Assert.Equal(ServiceLifetime.Transient, Mediator.ServiceLifetime);
    }

    [Fact]
    public void Test_Returns_Different_Instance_Every_Time()
    {
        var (sp, _) = Fixture.GetMediator();

        var handler1 = sp.GetRequiredService<SomeRequestHandler>();
        var handler2 = sp.GetRequiredService<SomeRequestHandler>();

        Assert.NotNull(handler1);
        Assert.NotNull(handler2);
        Assert.NotEqual(handler1, handler2);
    }
}
