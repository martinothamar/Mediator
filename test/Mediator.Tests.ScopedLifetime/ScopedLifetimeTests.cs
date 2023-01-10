using Mediator.Tests.TestTypes;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

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
    public async Task Test_Returns_Different_Instances_From_Different_Scopes()
    {
        var services = new ServiceCollection();

        services.AddMediator();

        await using var sp = services.BuildServiceProvider(validateScopes: true);

        IMediator mediator1;
        IMediator mediator2;

        await using (var scope1 = sp.CreateAsyncScope())
        {
            mediator1 = scope1.ServiceProvider.GetRequiredService<IMediator>();
            var handler1 = scope1.ServiceProvider.GetRequiredService<SomeRequestHandler>();
            var handler2 = scope1.ServiceProvider.GetRequiredService<SomeRequestHandler>();

            await mediator1.Send(new SomeRequest(Guid.NewGuid()));

            Assert.NotNull(handler1);
            Assert.NotNull(handler2);
            Assert.Equal(handler1, handler2);
        }
        await using (var scope2 = sp.CreateAsyncScope())
        {
            mediator2 = scope2.ServiceProvider.GetRequiredService<IMediator>();
        }

        Assert.NotNull(mediator1);
        Assert.NotNull(mediator2);
        Assert.NotEqual(mediator1, mediator2);
    }

    [Fact]
    public async Task Test_Disposes_Handler_Correctly()
    {
        var services = new ServiceCollection();

        services.AddMediator();

        SomeRequestHandler handler;
        await using (var sp = services.BuildServiceProvider(validateScopes: true))
        {
            await using (var scope = sp.CreateAsyncScope())
            {
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                handler = scope.ServiceProvider.GetRequiredService<SomeRequestHandler>();

                await mediator.Send(new SomeRequest(Guid.NewGuid()));

                Assert.NotNull(handler);
                Assert.False(handler.Disposed);
            }

            Assert.True(handler.Disposed);
        }

        Assert.True(handler.Disposed);
    }

    [Fact]
    public void Test_Throws_When_Trying_To_Get_Mediator_From_Scoped()
    {
        Assert.ThrowsAny<Exception>(() => Fixture.GetMediator(createScope: false));
    }
}
