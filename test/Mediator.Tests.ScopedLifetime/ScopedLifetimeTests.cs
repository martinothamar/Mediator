using Mediator.Tests.TestTypes;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Mediator.Tests.ScopedLifetime
{
    public sealed class ScopedLifetimeTests
    {
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
            var (sp, _) = Fixture.GetMediator(createScope: false);

            SomeRequestHandler handler1;
            SomeRequestHandler handler2;
            using (var scope1 = sp.CreateScope())
                handler1 = scope1.ServiceProvider.GetRequiredService<SomeRequestHandler>();

            using (var scope2 = sp.CreateScope())
                handler2 = scope2.ServiceProvider.GetRequiredService<SomeRequestHandler>();

            Assert.NotNull(handler1);
            Assert.NotNull(handler2);
            Assert.NotEqual(handler1, handler2);
        }

        [Fact]
        public void Test_Returns_Same_Instances_Without_Explicit_Scopes()
        {
            var (sp, _) = Fixture.GetMediator(createScope: false);

            SomeRequestHandler handler1;
            SomeRequestHandler handler2;
            handler1 = sp.GetRequiredService<SomeRequestHandler>();
            handler2 = sp.GetRequiredService<SomeRequestHandler>();

            Assert.NotNull(handler1);
            Assert.NotNull(handler2);
            Assert.Equal(handler1, handler2);
        }
    }
}
