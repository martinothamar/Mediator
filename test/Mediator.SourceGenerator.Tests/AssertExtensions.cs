using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using VerifyTests;
using VerifyXunit;

namespace Mediator.SourceGenerator.Tests;

public static class AssertExtensions
{
    public static SettingsTask AssertAndVerify(
        this Compilation inputCompilation,
        params Action<GeneratorResult>[] assertionDelegates
    )
    {
        var generator = new IncrementalMediatorGenerator();

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

        return Verifier.Verify(driver).IgnoreGeneratedResult(r => r.HintName.Contains("MediatorOptions"));
    }
}
