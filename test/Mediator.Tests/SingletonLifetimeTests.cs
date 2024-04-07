using Microsoft.Extensions.DependencyInjection;

namespace Mediator.Tests;

public class SingletonLifetimeTests
{
    [Fact(Skip = Mediator.ServiceLifetime != ServiceLifetime.Singleton ? "Only tested for Singleton lifetime" : null)]
    public void Test_Generated_Code_Lifetime()
    {
        var (_, mediator) = Fixture.GetMediator();
        Assert.NotNull(mediator);

        Assert.Equal(ServiceLifetime.Singleton, Mediator.ServiceLifetime);
    }
}
