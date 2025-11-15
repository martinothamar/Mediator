using System;
using System.Threading.Tasks;
using Mediator.Tests.TestTypes;
using Microsoft.Extensions.DependencyInjection;

namespace Mediator.Tests;

public sealed class ScopedLifetimeTests
{
    [Fact(Skip = Mediator.ServiceLifetime != ServiceLifetime.Scoped ? "Only tested for Scoped lifetime" : null)]
    public void Test_Generated_Code_Lifetime()
    {
        Assert.Equal(ServiceLifetime.Scoped, Mediator.ServiceLifetime);
    }

    [Fact(Skip = Mediator.ServiceLifetime != ServiceLifetime.Scoped ? "Only tested for Scoped lifetime" : null)]
    public void Test_Returns_Same_Instance_In_Scope()
    {
        var (sp, _) = Fixture.GetMediator(createScope: true);

        var handler1 = sp.GetRequiredService<IRequestHandler<SomeRequest, SomeResponse>>();
        var handler2 = sp.GetRequiredService<IRequestHandler<SomeRequest, SomeResponse>>();
        Assert.NotNull(handler1);
        Assert.NotNull(handler2);
        Assert.Equal(handler1, handler2);
    }

    [Fact(Skip = Mediator.ServiceLifetime != ServiceLifetime.Scoped ? "Only tested for Scoped lifetime" : null)]
    public async Task Test_Returns_Different_Instances_From_Different_Scopes()
    {
        var ct = TestContext.Current.CancellationToken;
        var services = new ServiceCollection();

        services.AddMediator();

        await using var sp = services.BuildServiceProvider(
            new ServiceProviderOptions() { ValidateOnBuild = true, ValidateScopes = true }
        );

        IMediator mediator1;
        IMediator mediator2;

        await using (var scope1 = sp.CreateAsyncScope())
        {
            mediator1 = scope1.ServiceProvider.GetRequiredService<IMediator>();
            var handler1 = scope1.ServiceProvider.GetRequiredService<IRequestHandler<SomeRequest, SomeResponse>>();
            var handler2 = scope1.ServiceProvider.GetRequiredService<IRequestHandler<SomeRequest, SomeResponse>>();

            await mediator1.Send(new SomeRequest(Guid.NewGuid()), ct);

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

    [Fact(Skip = Mediator.ServiceLifetime != ServiceLifetime.Scoped ? "Only tested for Scoped lifetime" : null)]
    public async Task Test_Disposes_Handler_Correctly()
    {
        var ct = TestContext.Current.CancellationToken;
        var services = new ServiceCollection();

        services.AddMediator();

        IRequestHandler<SomeRequest, SomeResponse> handler;
        await using (
            var sp = services.BuildServiceProvider(
                new ServiceProviderOptions() { ValidateOnBuild = true, ValidateScopes = true }
            )
        )
        {
            await using (var scope = sp.CreateAsyncScope())
            {
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                handler = scope.ServiceProvider.GetRequiredService<IRequestHandler<SomeRequest, SomeResponse>>();

                await mediator.Send(new SomeRequest(Guid.NewGuid()), ct);

                Assert.NotNull(handler);
                Assert.False(((SomeRequestHandler)handler).Disposed);
            }

            Assert.True(((SomeRequestHandler)handler).Disposed);
        }

        Assert.True(((SomeRequestHandler)handler).Disposed);
    }

    [Fact(Skip = Mediator.ServiceLifetime != ServiceLifetime.Scoped ? "Only tested for Scoped lifetime" : null)]
    public void Test_Throws_When_Trying_To_Get_Mediator_From_Scoped()
    {
        Assert.ThrowsAny<Exception>(() => Fixture.GetMediator(createScope: false));
    }
}
