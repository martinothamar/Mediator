using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;

namespace Mediator.SourceGenerator.Tests;

public static class AssertExtensions
{
    public static void AssertGen(this Compilation inputCompilation, params Action<GeneratorResult>[] assertionDelegates)
    {
        var generator = new MediatorGenerator();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        driver = driver.RunGeneratorsAndUpdateCompilation(
            inputCompilation,
            out var outputCompilation,
            out var diagnostics
        );

        var runResult = driver.GetRunResult();

        var result = new GeneratorResult(generator, diagnostics, runResult, outputCompilation);

        foreach (var assertions in assertionDelegates)
            assertions(result);
    }
}
