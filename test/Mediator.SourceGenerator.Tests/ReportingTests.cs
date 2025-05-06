using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace Mediator.SourceGenerator.Tests;

public sealed class ReportingTests
{
    [Fact]
    public async Task Test_Empty_Program()
    {
        var inputCompilation = Fixture.CreateLibrary(
            @"
namespace MyCode
{
    public class Program
    {
        public static void Main(string[] args)
        {
        }
    }
}
"
        );

        await inputCompilation.AssertAndVerify(
            Assertions.CompilesWithoutDiagnostics,
            result =>
            {
                Assert.True(result.OutputCompilation.SyntaxTrees.Count() == 5); // Original + attribute + options + mediator impl
                Assert.True(result.RunResult.GeneratedTrees.Length == 4); // attribute + options + mediator impl
            }
        );
    }

    [Fact]
    public async Task Test_Deep_Namespace_Program()
    {
        var source = await Fixture.SourceFromResourceFile("DeepNamespaceProgram.cs");
        var inputCompilation = Fixture.CreateLibrary(source);

        await inputCompilation.AssertAndVerify(Assertions.CompilesWithoutDiagnostics);
    }

    [Fact]
    public async Task Test_Static_Nested_Handler_Program()
    {
        var source = await Fixture.SourceFromResourceFile("StaticNestedHandlerProgram.cs");
        var inputCompilation = Fixture.CreateLibrary(source);

        await inputCompilation.AssertAndVerify(Assertions.CompilesWithoutDiagnostics);
    }

    [Fact]
    public async Task Test_Abstract_Handler_Program()
    {
        var source = await Fixture.SourceFromResourceFile("AbstractHandlerProgram.cs");
        var inputCompilation = Fixture.CreateLibrary(source);

        await inputCompilation.AssertAndVerify(
            Assertions.CompilesWithoutErrorDiagnostics,
            result =>
            {
                Assert.Contains(
                    result.Diagnostics,
                    d => d.Id == Diagnostics.MessageWithoutHandler.Id && d.Severity == DiagnosticSeverity.Warning
                );
                Assert.Single(result.Diagnostics);
            }
        );
    }

    [Fact]
    public async Task Test_Duplicate_Handlers()
    {
        var source = await Fixture.SourceFromResourceFile("DuplicateHandlersProgram.cs");
        var inputCompilation = Fixture.CreateLibrary(source);

        await inputCompilation.AssertAndVerify(result =>
        {
            Assertions.AssertCommon(result);

            Assert.Contains(result.Diagnostics, d => d.Id == Diagnostics.MultipleHandlersError.Id);
            Assert.Single(result.Diagnostics);
        });
    }

    [Fact]
    public async Task Test_Invalid_Handler_Type()
    {
        var source = await Fixture.SourceFromResourceFile("StructHandlerProgram.cs");
        var inputCompilation = Fixture.CreateLibrary(source);

        await inputCompilation.AssertAndVerify(result =>
        {
            Assertions.AssertCommon(result);

            Assert.Contains(result.Diagnostics, d => d.Id == Diagnostics.InvalidHandlerTypeError.Id);
            Assert.Contains(
                result.Diagnostics,
                d => d.Id == Diagnostics.MessageWithoutHandler.Id && d.Severity == DiagnosticSeverity.Warning
            );
            Assert.True(result.Diagnostics.Length == 2);
        });
    }

    [Fact]
    public async Task Test_Multiple_AddMediator_Calls()
    {
        var source = await Fixture.SourceFromResourceFile("MultipleAddMediatorCalls.cs");
        var inputCompilation = Fixture.CreateLibrary(source);

        await inputCompilation.AssertAndVerify(Assertions.CompilesWithoutDiagnostics);
    }

    [Fact]
    public async Task Test_Multiple_Errors()
    {
        var source = await Fixture.SourceFromResourceFile("MultipleErrorsProgram.cs");
        var inputCompilation = Fixture.CreateLibrary(source);

        await inputCompilation.AssertAndVerify(result =>
        {
            Assertions.AssertCommon(result);

            Assert.Contains(result.Diagnostics, d => d.Id == Diagnostics.MultipleHandlersError.Id);
            Assert.Contains(result.Diagnostics, d => d.Id == Diagnostics.InvalidHandlerTypeError.Id);
            Assert.True(result.Diagnostics.Length == 2);
        });
    }

    [Fact]
    public async Task Test_No_Messages_Program()
    {
        var source = await Fixture.SourceFromResourceFile("NoMessagesProgram.cs");
        var inputCompilation = Fixture.CreateLibrary(source);

        await inputCompilation.AssertAndVerify(Assertions.CompilesWithoutDiagnostics);
    }

    [Fact]
    public async Task Test_Null_Namespace_Variable()
    {
        var source = await Fixture.SourceFromResourceFile("NullNamespaceVariable.cs");
        var inputCompilation = Fixture.CreateLibrary(source);

        await inputCompilation.AssertAndVerify(Assertions.CompilesWithoutDiagnostics);
    }

    [Fact]
    public async Task Test_Byte_Array_Response_Program()
    {
        var source = await Fixture.SourceFromResourceFile("ByteArrayResponseProgram.cs");
        var inputCompilation = Fixture.CreateLibrary(source);

        await inputCompilation.AssertAndVerify(Assertions.CompilesWithoutDiagnostics);
    }

    [Fact]
    public async Task Test_Cast_Lifetime_Config()
    {
        var source = await Fixture.SourceFromResourceFile("IntCastLifetime.cs");
        var inputCompilation = Fixture.CreateLibrary(source);

        await inputCompilation.AssertAndVerify(result =>
        {
            Assertions.AssertCommon(result);

            Assert.Contains(result.Diagnostics, d => d.Id == Diagnostics.InvalidCodeBasedConfiguration.Id);
            Assert.True(result.Diagnostics.Length == 1);
        });
    }

    [Fact]
    public async Task Test_Configuratoin_Conflict()
    {
        var source = await Fixture.SourceFromResourceFile("ConfigurationConflictProgram.cs");
        var inputCompilation = Fixture.CreateLibrary(source);

        await inputCompilation.AssertAndVerify(result =>
        {
            Assertions.AssertCommon(result);

            Assert.Contains(result.Diagnostics, d => d.Id == Diagnostics.ConflictingConfiguration.Id);
            Assert.True(result.Diagnostics.Length == 1);
        });
    }

    [Fact]
    public async Task Test_Const_Variable_In_Config()
    {
        var source = await Fixture.SourceFromResourceFile("ConstVariablesConfig.cs");
        var inputCompilation = Fixture.CreateLibrary(source);

        await inputCompilation.AssertAndVerify(Assertions.CompilesWithoutDiagnostics);
    }

    [Fact]
    public async Task Test_Local_Literal_Variable_In_Config()
    {
        var source = await Fixture.SourceFromResourceFile("LocalLiteralVariableConfig.cs");
        var inputCompilation = Fixture.CreateLibrary(source);

        await inputCompilation.AssertAndVerify(
            Assertions.CompilesWithoutDiagnostics,
            result =>
            {
                var analyzer = result.Generator.CompilationAnalyzer;
                Assert.True(analyzer?.ServiceLifetimeIsScoped);
                Assert.Equal("SomeNamespace", analyzer?.MediatorNamespace);
            }
        );
    }

    [Fact]
    public async Task Test_Local_Variables_Referencing_Consts_Config()
    {
        var source = await Fixture.SourceFromResourceFile("LocalVariablesReferencingConstsConfig.cs");
        var inputCompilation = Fixture.CreateLibrary(source);

        await inputCompilation.AssertAndVerify(
            Assertions.CompilesWithoutDiagnostics,
            result =>
            {
                var analyzer = result.Generator.CompilationAnalyzer;
                Assert.True(analyzer?.ServiceLifetimeIsScoped);
                Assert.Equal("SomeNamespace", analyzer?.MediatorNamespace);
            }
        );
    }

    [Fact]
    public async Task Test_Invalid_Variable_In_Config()
    {
        var source = await Fixture.SourceFromResourceFile("InvalidVariablesConfig.cs");
        var inputCompilation = Fixture.CreateLibrary(source);

        await inputCompilation.AssertAndVerify(result =>
        {
            Assertions.AssertCommon(result);

            Assert.Contains(result.Diagnostics, d => d.Id == Diagnostics.InvalidCodeBasedConfiguration.Id);
            Assert.True(result.Diagnostics.Length == 1);
        });
    }

    [Fact]
    public async Task Test_Unassigned_Variables_In_Config()
    {
        var source = await Fixture.SourceFromResourceFile("UnassignedVariablesConfig.cs");
        var inputCompilation = Fixture.CreateLibrary(source);

        await inputCompilation.AssertAndVerify(result =>
        {
            Assertions.AssertCommon(result);

            Assert.Contains(result.Diagnostics, d => d.Id == Diagnostics.InvalidCodeBasedConfiguration.Id);
        });
    }

    [Fact]
    public async Task Test_Unassigned_Namespace_Variable_In_Config()
    {
        var source = await Fixture.SourceFromResourceFile("UnassignedNamespaceVariableConfig.cs");
        var inputCompilation = Fixture.CreateLibrary(source);

        await inputCompilation.AssertAndVerify(result =>
        {
            Assertions.AssertCommon(result);

            Assert.Contains(result.Diagnostics, d => d.Id == Diagnostics.InvalidCodeBasedConfiguration.Id);
        });
    }

    [Fact]
    public async Task Test_Unassigned_Lifetime_Variable_In_Config()
    {
        var source = await Fixture.SourceFromResourceFile("UnassignedLifetimeVariableConfig.cs");
        var inputCompilation = Fixture.CreateLibrary(source);

        await inputCompilation.AssertAndVerify(result =>
        {
            Assertions.AssertCommon(result);

            Assert.Contains(result.Diagnostics, d => d.Id == Diagnostics.InvalidCodeBasedConfiguration.Id);
            Assert.True(result.Diagnostics.Length == 1);
        });
    }

    [Fact]
    public async Task Test_Request_Without_Handler_Warning()
    {
        var source = await Fixture.SourceFromResourceFile("RequestWithoutHandlerProgram.cs");
        var inputCompilation = Fixture.CreateLibrary(source);

        await inputCompilation.AssertAndVerify(
            Assertions.CompilesWithoutErrorDiagnostics,
            result =>
            {
                Assert.Contains(
                    result.Diagnostics,
                    d => d.Id == Diagnostics.MessageWithoutHandler.Id && d.Severity == DiagnosticSeverity.Warning
                );
            }
        );
    }

    [Fact]
    public async Task Test_Request_Without_Handler_In_Referenced_Library()
    {
        var referencedLibrary1 = Fixture
            .CreateLibrary(
                """
                using System;
                using System.Threading;
                using System.Threading.Tasks;
                using Mediator;

                namespace TestCode.Library1;

                public readonly record struct Request1(Guid Id) : IRequest<Response1>;
                public readonly record struct Response1(Guid Id);
                """
            )
            .WithAssemblyName("TestCode.Library1");

        var mainLibrary0 = Fixture
            .CreateLibrary(
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
            )
            .WithAssemblyName("TestCode");
        mainLibrary0 = mainLibrary0.AddReferences(referencedLibrary1.ToMetadataReference());

        await mainLibrary0.AssertAndVerify(
            Assertions.CompilesWithoutErrorDiagnostics,
            result =>
            {
                Assert.Contains(
                    result.Diagnostics,
                    d => d.Id == Diagnostics.MessageWithoutHandler.Id && d.Severity == DiagnosticSeverity.Warning
                );
            }
        );
    }

    [Fact]
    public async Task Test_Notification_Without_Any_Handlers()
    {
        var source = await Fixture.SourceFromResourceFile("NotificationWithoutHandlerProgram.cs");
        var inputCompilation = Fixture.CreateLibrary(source);

        await inputCompilation.AssertAndVerify(
            Assertions.CompilesWithoutErrorDiagnostics,
            result =>
            {
                Assert.Contains(
                    result.Diagnostics,
                    d => d.Id == Diagnostics.MessageWithoutHandler.Id && d.Severity == DiagnosticSeverity.Warning
                );
            }
        );
    }
}
