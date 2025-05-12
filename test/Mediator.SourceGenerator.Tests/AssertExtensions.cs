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
        return AssertAndVerify(
            inputCompilation,
            r => r.HintName.Contains("MediatorOptions") || r.HintName.Contains("AssemblyReference"),
            assertionDelegates
        );
    }

    public static SettingsTask AssertAndVerify(
        this Compilation inputCompilation,
        Func<GeneratedSourceResult, bool>? ignoreGeneratedResult,
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

        var verifier = Verifier.Verify(driver);
        if (ignoreGeneratedResult is not null)
            verifier = verifier.IgnoreGeneratedResult(r => ignoreGeneratedResult(r));
        return verifier;
    }
}
