using System.Threading.Tasks;

namespace Mediator.SourceGenerator.Tests;

public sealed class ConfigurationTests
{
    [Fact]
    public async Task Test_ForEachAwaitPublisher_Default()
    {
        var inputCompilation = Fixture.CreateLibrary(
            """
            using System;
            using System.Threading;
            using System.Threading.Tasks;
            using Mediator;
            using Microsoft.Extensions.DependencyInjection;

            namespace TestCode;

            public class Program
            {
                public static void Main()
                {
                    var services = new ServiceCollection();

                    services.AddMediator();
                }
            }
            """
        );

        await inputCompilation.AssertAndVerify(
            Assertions.CompilesWithoutDiagnostics,
            result =>
            {
                var model = result.Generator.CompilationModel;
                Assert.NotNull(model);
                Assert.Equal("global::Mediator.ForeachAwaitPublisher", model.NotificationPublisherType.FullName);
            }
        );
    }

    [Fact]
    public async Task Test_TaskWhenAllPublisher_For_Notifications_AddMediator()
    {
        var inputCompilation = Fixture.CreateLibrary(
            """
            using System;
            using System.Threading;
            using System.Threading.Tasks;
            using Mediator;
            using Microsoft.Extensions.DependencyInjection;

            namespace TestCode;

            public class Program
            {
                public static void Main()
                {
                    var services = new ServiceCollection();

                    services.AddMediator(options =>
                    {
                        options.NotificationPublisherType = typeof(TaskWhenAllPublisher);
                    });
                }
            }
            """
        );

        await inputCompilation.AssertAndVerify(
            Assertions.CompilesWithoutDiagnostics,
            result =>
            {
                var model = result.Generator.CompilationModel;
                Assert.NotNull(model);
                Assert.Equal("global::Mediator.TaskWhenAllPublisher", model.NotificationPublisherType.FullName);
            }
        );
    }

    [Fact]
    public async Task Test_TaskWhenAllPublisher_For_Notifications_Attribute()
    {
        var inputCompilation = Fixture.CreateLibrary(
            """
            using System;
            using System.Threading;
            using System.Threading.Tasks;
            using Mediator;
            using Microsoft.Extensions.DependencyInjection;

            [assembly: MediatorOptions(NotificationPublisherType = typeof(TaskWhenAllPublisher))]

            namespace TestCode;

            public class Program
            {
                public static void Main()
                {
                    var services = new ServiceCollection();

                    services.AddMediator();
                }
            }
            """
        );

        await inputCompilation.AssertAndVerify(
            Assertions.CompilesWithoutDiagnostics,
            result =>
            {
                var model = result.Generator.CompilationModel;
                Assert.NotNull(model);
                Assert.Equal("global::Mediator.TaskWhenAllPublisher", model.NotificationPublisherType.FullName);
            }
        );
    }
}
