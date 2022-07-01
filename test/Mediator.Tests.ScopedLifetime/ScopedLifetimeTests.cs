using Mediator.Tests.TestTypes;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Mediator.Tests.ScopedLifetime;

public sealed class ScopedLifetimeTests
{
    [Fact]
    public void Test_Generated_Code_Lifetime()
    {
        Assert.Equal(ServiceLifetime.Scoped, Mediator.ServiceLifetime);
    }

    [Fact]
    public void Test_Returns_Same_Instance_In_Scope()
    {
        var (sp, _) = Fixture.GetMediator(createScope: true);

        var handler1 = sp.GetRequiredService<SomeRequestHandler>();
        var handler2 = sp.GetRequiredService<SomeRequestHandler>();
        Assert.NotNull(handler1);
        Assert.NotNull(handler2);
        Assert.Equal(handler1, handler2);
    }

    [Fact]
    public void Test_Returns_Different_Instances_From_Different_Scopes()
    {
        var services = new ServiceCollection();

        services.AddMediator();

        IServiceProvider sp = services.BuildServiceProvider(validateScopes: true);

        IMediator mediator1;
        IMediator mediator2;

        using (var scope1 = sp.CreateScope())
        {
            mediator1 = scope1.ServiceProvider.GetRequiredService<IMediator>();
            var handler1 = scope1.ServiceProvider.GetRequiredService<SomeRequestHandler>();
            var handler2 = scope1.ServiceProvider.GetRequiredService<SomeRequestHandler>();
            Assert.NotNull(handler1);
            Assert.NotNull(handler2);
            Assert.Equal(handler1, handler2);
        }
        using (var scope2 = sp.CreateScope())
        {
            mediator2 = scope2.ServiceProvider.GetRequiredService<IMediator>();
        }

        Assert.NotNull(mediator1);
        Assert.NotNull(mediator2);
        Assert.NotEqual(mediator1, mediator2);
    }

    [Fact]
    public void Test_Throws_When_Trying_To_Get_Mediator_From_Scoped()
    {
        Assert.ThrowsAny<Exception>(() => Fixture.GetMediator(createScope: false));
    }
}
