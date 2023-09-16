using Microsoft.CodeAnalysis.CSharp;
using System.Linq;
using System.Threading.Tasks;
using static Mediator.SourceGenerator.IncrementalGenerator.Tests.IncrementalGeneratorHelper;

namespace Mediator.SourceGenerator.IncrementalGenerator.Tests;

public class IncrementalGeneratorTest
{
    private const string DefaultProgram = "SimpleConsoleProgram.cs";
    private const string StructHandlerProgram = "StructHandlerProgram.cs";
    private const string LocalVariableProgram = "LocalLiteralVariableConfig.cs";

    [Fact]
    public async Task Test_Add_Unrelated_Type_Doesnt_Regenerate()
    {
        var source = await TestHelper.SourceFromResourceFile(DefaultProgram);

        var syntaxTree = CSharpSyntaxTree.ParseText(source, CSharpParseOptions.Default);
        var compilation1 = TestHelper.CreateLibrary(syntaxTree);

        var driver1 = TestHelper.GenerateTracked(compilation1);
        AssertRunReasons(driver1, IncrementalGeneratorRunReasons.New);

        var compilation2 = compilation1.AddSyntaxTrees(CSharpSyntaxTree.ParseText("struct MyValue {}"));
        var driver2 = driver1.RunGenerators(compilation2);
        AssertRunReasons(driver2, IncrementalGeneratorRunReasons.Cached);
    }

    [Fact]
    public async Task Test_Appending_Unrelated_Type_Doesnt_Regenerate()
    {
        var source = await TestHelper.SourceFromResourceFile(DefaultProgram);

        var syntaxTree = CSharpSyntaxTree.ParseText(source, CSharpParseOptions.Default);
        var compilation1 = TestHelper.CreateLibrary(syntaxTree);

        var driver1 = TestHelper.GenerateTracked(compilation1);
        var newTree = syntaxTree.WithRootAndOptions(
            syntaxTree.GetCompilationUnitRoot().AddMembers(SyntaxFactory.ParseMemberDeclaration("struct MyValue {}")!),
            syntaxTree.Options
        );

        var compilation2 = compilation1.ReplaceSyntaxTree(compilation1.SyntaxTrees.First(), newTree);
        var driver2 = driver1.RunGenerators(compilation2);
        AssertRunReasons(driver2, IncrementalGeneratorRunReasons.Cached);
    }

    [Fact]
    public async Task Test_Small_Change_To_Request_Doesnt_Regenerate()
    {
        var source = await TestHelper.SourceFromResourceFile(DefaultProgram);

        var syntaxTree = CSharpSyntaxTree.ParseText(source, CSharpParseOptions.Default);
        var compilation1 = TestHelper.CreateLibrary(syntaxTree);

        var driver1 = TestHelper.GenerateTracked(compilation1);
        AssertRunReasons(driver1, IncrementalGeneratorRunReasons.New);

        // only change body, don't change the name or request type
        var compilation2 = TestHelper.ReplaceMemberDeclaration(
            compilation1,
            "Ping",
            "public sealed record Ping(Guid Id) : IRequest<Pong> { }"
        );
        var driver2 = driver1.RunGenerators(compilation2);
        AssertRunReasons(driver2, IncrementalGeneratorRunReasons.Cached);
    }

    [Fact]
    public async Task Test_Modify_Handler_Does_Regenerate()
    {
        var source = await TestHelper.SourceFromResourceFile(DefaultProgram);

        var syntaxTree = CSharpSyntaxTree.ParseText(source, CSharpParseOptions.Default);
        var compilation1 = TestHelper.CreateLibrary(syntaxTree);

        var driver1 = TestHelper.GenerateTracked(compilation1);
        AssertRunReasons(driver1, IncrementalGeneratorRunReasons.New);

        // change handler name
        var newHandler =
            @"public sealed class PingPongHandler : IRequestHandler<Ping, Pong>
{
    public ValueTask<Pong> Handle(Ping request, CancellationToken cancellationToken)
    {
        return new ValueTask<Pong>(new Pong(request.Id));
    }
}";
        var compilation2 = TestHelper.ReplaceMemberDeclaration(compilation1, "PingHandler", newHandler);

        var driver2 = driver1.RunGenerators(compilation2);
        AssertRunReasons(driver2, IncrementalGeneratorRunReasons.ModifiedSource);
    }

    [Fact]
    public async Task Test_Fixing_Handler_Does_Regenerate()
    {
        var source = await TestHelper.SourceFromResourceFile(StructHandlerProgram);

        var syntaxTree = CSharpSyntaxTree.ParseText(source, CSharpParseOptions.Default);
        var compilation1 = TestHelper.CreateLibrary(syntaxTree);

        var driver1 = TestHelper.GenerateTracked(compilation1);
        AssertRunReasons(driver1, IncrementalGeneratorRunReasons.New);

        // change handler type from struct to class
        var newHandler =
            @"public sealed class PingHandler : IRequestHandler<Ping, Pong>
{
    public ValueTask<Pong> Handle(Ping request, CancellationToken cancellationToken)
    {
        return new ValueTask<Pong>(new Pong(request.Id));
    }
}";
        var compilation2 = TestHelper.ReplaceMemberDeclaration(compilation1, "PingHandler", newHandler);

        var driver2 = driver1.RunGenerators(compilation2);
        AssertRunReasons(driver2, IncrementalGeneratorRunReasons.Modified);
    }

    [Fact]
    public async Task Test_Changing_Lifetime_Does_Regenerate()
    {
        var source = await TestHelper.SourceFromResourceFile(LocalVariableProgram);

        var syntaxTree = CSharpSyntaxTree.ParseText(source, CSharpParseOptions.Default);
        var compilation1 = TestHelper.CreateLibrary(syntaxTree);

        var driver1 = TestHelper.GenerateTracked(compilation1);
        AssertRunReasons(driver1, IncrementalGeneratorRunReasons.New);

        // change lifetime via a variable
        var newHandler = "var lifetime = ServiceLifetime.Singleton;";
        var compilation2 = TestHelper.ReplaceLocalDeclaration(compilation1, "lifetime", newHandler);

        var driver2 = driver1.RunGenerators(compilation2);
        AssertRunReasons(driver2, IncrementalGeneratorRunReasons.ModifiedSource);
    }

    [Fact]
    public async Task Test_Changing_Namespace_Does_Regenerate()
    {
        var source = await TestHelper.SourceFromResourceFile(LocalVariableProgram);

        var syntaxTree = CSharpSyntaxTree.ParseText(source, CSharpParseOptions.Default);
        var compilation1 = TestHelper.CreateLibrary(syntaxTree);

        var driver1 = TestHelper.GenerateTracked(compilation1);
        AssertRunReasons(driver1, IncrementalGeneratorRunReasons.New);

        // change namespace via a variable
        var newHandler = "var ns = \"SomeOtherNamespace\";";
        var compilation2 = TestHelper.ReplaceLocalDeclaration(compilation1, "ns", newHandler);

        var driver2 = driver1.RunGenerators(compilation2);
        AssertRunReasons(driver2, IncrementalGeneratorRunReasons.ModifiedSource);
    }
}
