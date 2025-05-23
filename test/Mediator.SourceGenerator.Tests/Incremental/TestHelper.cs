﻿using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Mediator.SourceGenerator.Tests.Incremental;

internal static class TestHelper
{
    private static readonly GeneratorDriverOptions EnableIncrementalTrackingDriverOptions = new(
        IncrementalGeneratorOutputKind.None,
        trackIncrementalGeneratorSteps: true
    );

    internal static GeneratorDriver GenerateTracked(Compilation compilation)
    {
        var generator = new IncrementalMediatorGenerator();

        var driver = CSharpGeneratorDriver.Create(
            new[] { generator.AsSourceGenerator() },
            driverOptions: EnableIncrementalTrackingDriverOptions
        );
        return driver.RunGenerators(compilation);
    }

    internal static CSharpCompilation ReplaceMemberDeclaration(
        CSharpCompilation compilation,
        string memberName,
        string newMember
    )
    {
        var syntaxTree = compilation.SyntaxTrees.Single();
        var memberDeclaration = syntaxTree
            .GetCompilationUnitRoot()
            .DescendantNodes()
            .OfType<TypeDeclarationSyntax>()
            .Single(x => x.Identifier.Text == memberName);
        var updatedMemberDeclaration = SyntaxFactory.ParseMemberDeclaration(newMember)!;

        var newRoot = syntaxTree.GetCompilationUnitRoot().ReplaceNode(memberDeclaration, updatedMemberDeclaration);
        var newTree = syntaxTree.WithRootAndOptions(newRoot, syntaxTree.Options);

        return compilation.ReplaceSyntaxTree(compilation.SyntaxTrees.First(), newTree);
    }

    internal static CSharpCompilation ReplaceLocalDeclaration(
        CSharpCompilation compilation,
        string variableName,
        string newDeclaration
    )
    {
        var syntaxTree = compilation.SyntaxTrees.Single();

        var memberDeclaration = syntaxTree
            .GetCompilationUnitRoot()
            .DescendantNodes()
            .OfType<LocalDeclarationStatementSyntax>()
            .Single(x => x.Declaration.Variables.Any(x => x.Identifier.ToString() == variableName));
        var updatedMemberDeclaration = SyntaxFactory.ParseStatement(newDeclaration)!;

        var newRoot = syntaxTree.GetCompilationUnitRoot().ReplaceNode(memberDeclaration, updatedMemberDeclaration);
        var newTree = syntaxTree.WithRootAndOptions(newRoot, syntaxTree.Options);

        return compilation.ReplaceSyntaxTree(compilation.SyntaxTrees.First(), newTree);
    }

    internal static void AssertRunReasons(
        GeneratorDriver driver,
        IncrementalGeneratorRunReasons reasons,
        int outputIndex = 0
    )
    {
        var runResult = driver.GetRunResult().Results[0];

        AssertRunReason(
            runResult,
            MediatorGeneratorStepName.ReportDiagnostics,
            reasons.ReportDiagnosticsStep,
            outputIndex
        );
        AssertRunReason(runResult, MediatorGeneratorStepName.BuildMediator, reasons.BuildMediatorStep, outputIndex);
    }

    private static void AssertRunReason(
        GeneratorRunResult runResult,
        string stepName,
        IncrementalStepRunReason expectedStepReason,
        int outputIndex
    )
    {
        var actualStepReason = runResult
            .TrackedSteps[stepName]
            .SelectMany(x => x.Outputs)
            .ElementAt(outputIndex)
            .Reason;
        actualStepReason.Should().Be(expectedStepReason, $"step {stepName} of mediator at index {outputIndex}");
    }
}
