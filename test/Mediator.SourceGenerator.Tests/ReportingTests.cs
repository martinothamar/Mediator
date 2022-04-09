using Microsoft.CodeAnalysis;

namespace Mediator.SourceGenerator.Tests;

public sealed class ReportingTests
{
    [Fact]
    public void Test_Empty_Program()
    {
        var inputCompilation = Fixture.CreateCompilation(@"
namespace MyCode
{
    public class Program
    {
        public static void Main(string[] args)
        {
        }
    }
}
");

        inputCompilation.AssertGen(
            Assertions.CompilesWithoutDiagnostics,
            result =>
            {
                Assert.True(result.OutputCompilation.SyntaxTrees.Count() == 4); // Original + attribute + options + mediator impl
                    Assert.True(result.RunResult.GeneratedTrees.Length == 3); // attribute + options + mediator impl
                }
        );
    }

    [Fact]
    public async Task Test_Deep_Namespace_Program()
    {
        var source = await Fixture.SourceFromResourceFile("DeepNamespaceProgram.cs");
        var inputCompilation = Fixture.CreateCompilation(source);

        inputCompilation.AssertGen(
            Assertions.CompilesWithoutDiagnostics
        );
    }

    [Fact]
    public async Task Test_Static_Nested_Handler_Program()
    {
        var source = await Fixture.SourceFromResourceFile("StaticNestedHandlerProgram.cs");
        var inputCompilation = Fixture.CreateCompilation(source);

        inputCompilation.AssertGen(
            Assertions.CompilesWithoutDiagnostics
        );
    }

    [Fact]
    public async Task Test_Abstract_Handler_Program()
    {
        var source = await Fixture.SourceFromResourceFile("AbstractHandlerProgram.cs");
        var inputCompilation = Fixture.CreateCompilation(source);

        inputCompilation.AssertGen(
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
    public async Task Test_Streaming_Program()
    {
        var source = await Fixture.SourceFromResourceFile("StreamingProgram.cs");
        var inputCompilation = Fixture.CreateCompilation(source);

        inputCompilation.AssertGen(Assertions.CompilesWithoutDiagnostics);
    }

    [Fact]
    public async Task Test_Duplicate_Handlers()
    {
        var source = await Fixture.SourceFromResourceFile("DuplicateHandlersProgram.cs");
        var inputCompilation = Fixture.CreateCompilation(source);

        inputCompilation.AssertGen(
            result =>
            {
                Assert.Contains(result.Diagnostics, d => d.Id == Diagnostics.MultipleHandlersError.Id);
                Assert.Single(result.Diagnostics);
            }
        );
    }

    [Fact]
    public async Task Test_Invalid_Handler_Type()
    {
        var source = await Fixture.SourceFromResourceFile("StructHandlerProgram.cs");
        var inputCompilation = Fixture.CreateCompilation(source);

        inputCompilation.AssertGen(
            result =>
            {
                Assert.Contains(result.Diagnostics, d => d.Id == Diagnostics.InvalidHandlerTypeError.Id);
                Assert.Contains(
                    result.Diagnostics,
                    d => d.Id == Diagnostics.MessageWithoutHandler.Id && d.Severity == DiagnosticSeverity.Warning
                );
                Assert.True(result.Diagnostics.Length == 2);
            }
        );
    }

    [Fact]
    public async Task Test_Multiple_Errors()
    {
        var source = await Fixture.SourceFromResourceFile("MultipleErrorsProgram.cs");
        var inputCompilation = Fixture.CreateCompilation(source);

        inputCompilation.AssertGen(
            result =>
            {
                Assert.Contains(result.Diagnostics, d => d.Id == Diagnostics.MultipleHandlersError.Id);
                Assert.Contains(result.Diagnostics, d => d.Id == Diagnostics.InvalidHandlerTypeError.Id);
                Assert.True(result.Diagnostics.Length == 2);
            }
        );
    }

    [Fact]
    public async Task Test_Request_Without_Handler_Warning()
    {
        var source = await Fixture.SourceFromResourceFile("RequestWithoutHandlerProgram.cs");
        var inputCompilation = Fixture.CreateCompilation(source);

        inputCompilation.AssertGen(
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

    [Fact(Skip = "We can't statically prove a notification isn't handled yet, see TODO in CompilationAnalyzer")]
    public async Task Test_Notification_Without_Any_Handlers()
    {
        var source = await Fixture.SourceFromResourceFile("NotificationWithoutHandlerProgram.cs");
        var inputCompilation = Fixture.CreateCompilation(source);

        inputCompilation.AssertGen(
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
