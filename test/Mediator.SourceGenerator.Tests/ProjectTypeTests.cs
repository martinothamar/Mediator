using System;
using System.Text;
using System.Threading.Tasks;

namespace Mediator.SourceGenerator.Tests;

public sealed class ProjectTypeTests
{
    [Theory]
    [InlineData(16)]
    [InlineData(17)]
    public async Task Test_Project_Sizes(int n)
    {
        var code = new StringBuilder(
            """
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

                    services.AddMediator();
                }
            }

            """
        );

        string[] messageTypes = ["Request", "Query", "Command", "Notification"];
        foreach (var messageType in messageTypes)
        {
            for (int i = 0; i < n; i++)
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

        var inputCompilation = Fixture.CreateLibrary(code.ToString());

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
                }
            )
            .UseParameters(n);
    }
}
