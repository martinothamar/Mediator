using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Mediator.SourceGenerator.Tests;

public sealed class ProjectTypeTests
{
    private static string GenerateMessagesAndHandlers(
        int n,
        ServiceLifetime? serviceLifetime = null,
        string? cachingMode = null,
        string manyOfMessageType = ""
    )
    {
        var code = new StringBuilder(
            $$"""
            using System;
            using System.Collections.Generic;
            using System.Threading;
            using System.Threading.Tasks;
            using Mediator;
            using Microsoft.Extensions.DependencyInjection;
            using System.Runtime.CompilerServices;

            namespace TestCode;

            public class Program
            {
                public static void Main()
                {
                    var services = new ServiceCollection();

                    services.AddMediator(options =>
                    {
            """
        );

        if (serviceLifetime is not null)
        {
            code.AppendLine(
                $$"""
                        options.ServiceLifetime = ServiceLifetime.{{serviceLifetime}};
                """
            );
        }

        if (cachingMode is not null)
        {
            code.AppendLine(
                $$"""
                        options.CachingMode = CachingMode.{{cachingMode}};
                """
            );
        }

        code.AppendLine(
            """
                        });
                    }
                }

            """
        );

        string[] messageTypes = ["Request", "Query", "Command", "Notification"];
        foreach (var messageType in messageTypes)
        {
            var toGenerate = messageType == manyOfMessageType ? 17 : n;
            for (int i = 0; i < toGenerate; i++)
            {
                if (messageType == "Notification")
                {
                    code.AppendLine(
                        $"public sealed record Test{messageType}{i}() : I{messageType}; "
                            + $"public sealed record Test{messageType}{i}Handler : I{messageType}Handler<Test{messageType}{i}> {{ "
                            + $"public ValueTask Handle(Test{messageType}{i} {messageType.ToLowerInvariant()}, CancellationToken cancellationToken) => default; "
                            + $"}}"
                    );
                }
                else
                {
                    code.AppendLine(
                        $"public sealed record Test{messageType}{i}() : I{messageType}<int>; "
                            + $"public sealed record Test{messageType}{i}Handler : I{messageType}Handler<Test{messageType}{i}, int> {{ "
                            + $"public ValueTask<int> Handle(Test{messageType}{i} {messageType.ToLowerInvariant()}, CancellationToken cancellationToken) => new ValueTask<int>({i}); "
                            + $"}}"
                    );
                }
            }
            code.AppendLine();
        }

        foreach (var messageType in messageTypes)
        {
            if (messageType == "Notification")
                continue;

            var toGenerate = $"Stream{messageType}" == manyOfMessageType ? 17 : n;
            code.AppendLine();
            for (int i = 0; i < n; i++)
            {
                code.AppendLine(
                    $"public sealed record TestStream{messageType}{i}() : IStream{messageType}<int>; "
                        + $"public sealed record TestStream{messageType}{i}Handler : IStream{messageType}Handler<TestStream{messageType}{i}, int> {{ "
                        + $"public async IAsyncEnumerable<int> Handle(TestStream{messageType}{i} {messageType.ToLowerInvariant()}, [EnumeratorCancellation] CancellationToken cancellationToken) {{ for (int i = 0; i < 3; i++) {{ await Task.Yield(); yield return {i}; }} }} "
                        + $"}}"
                );
            }
        }

        return code.ToString();
    }

    [Theory, CombinatorialData]
    public async Task Test_Project_Sizes(
        [CombinatorialValues(16, 17)] int n,
        ServiceLifetime serviceLifetime,
        [CombinatorialValues("Eager", "Lazy")] string cachingMode
    )
    {
        var code = GenerateMessagesAndHandlers(n, serviceLifetime, cachingMode: cachingMode);

        var inputCompilation = Fixture.CreateLibrary(code);

        await inputCompilation
            .AssertAndVerify(
                Assertions.CompilesWithoutDiagnostics,
                result =>
                {
                    var model = result.Generator.CompilationModel;
                    Assert.NotNull(model);
                    Action<bool> assertMany = n > 16 ? Assert.True : Assert.False;
                    assertMany(model.HasManyRequests);
                    assertMany(model.HasManyQueries);
                    assertMany(model.HasManyCommands);
                    assertMany(model.HasManyStreamRequests);
                    assertMany(model.HasManyStreamQueries);
                    assertMany(model.HasManyStreamCommands);
                    assertMany(model.HasManyNotifications);

                    Assert.True(model.HasRequests);
                    Assert.True(model.HasQueries);
                    Assert.True(model.HasCommands);
                    Assert.True(model.HasStreamRequests);
                    Assert.True(model.HasStreamQueries);
                    Assert.True(model.HasStreamCommands);
                    Assert.True(model.HasNotifications);

                    if (cachingMode == "Eager")
                    {
                        Assert.True(model.CachingModeIsEager);
                        Assert.False(model.CachingModeIsLazy);
                    }
                    else
                    {
                        Assert.False(model.CachingModeIsEager);
                        Assert.True(model.CachingModeIsLazy);
                    }
                }
            )
            .UseParameters(n, serviceLifetime, cachingMode);
    }

    [Theory, CombinatorialData]
    public async Task Test_Project_Size_Uneven(
        [CombinatorialValues(
            "Request",
            "Query",
            "Command",
            "Notification",
            "StreamRequest",
            "StreamQuery",
            "StreamCommand"
        )]
            string manyOfMessageType,
        ServiceLifetime serviceLifetime,
        [CombinatorialValues("Eager", "Lazy")] string cachingMode
    )
    {
        var code = GenerateMessagesAndHandlers(
            1,
            serviceLifetime,
            cachingMode: cachingMode,
            manyOfMessageType: manyOfMessageType
        );

        var inputCompilation = Fixture.CreateLibrary(code);

        await inputCompilation
            .AssertAndVerify(
                Assertions.CompilesWithoutDiagnostics,
                result =>
                {
                    var model = result.Generator.CompilationModel;
                    Assert.NotNull(model);

                    if (cachingMode == "Eager")
                    {
                        Assert.True(model.CachingModeIsEager);
                        Assert.False(model.CachingModeIsLazy);
                    }
                    else
                    {
                        Assert.False(model.CachingModeIsEager);
                        Assert.True(model.CachingModeIsLazy);
                    }
                }
            )
            .UseParameters(manyOfMessageType, serviceLifetime, cachingMode);
    }
}
