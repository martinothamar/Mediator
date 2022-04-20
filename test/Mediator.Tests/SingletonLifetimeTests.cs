using Microsoft.Extensions.DependencyInjection;

namespace Mediator.Tests;

public class SingletonLifetimeTests
{
    [Fact]
    public void Test_Generated_Code_Lifetime()
    {
        var (_, mediator) = Fixture.GetMediator();
        Assert.NotNull(mediator);

        Assert.Equal(ServiceLifetime.Singleton, Mediator.ServiceLifetime);
    }
}
