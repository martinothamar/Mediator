using FluentAssertions;
using Microsoft.CodeAnalysis;
using System.Linq;

namespace Mediator.SourceGenerator.IncrementalGenerator.Tests;

internal static class IncrementalGeneratorHelper
{
    public static void AssertRunReasons(
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
        var actualStepReason = runResult.TrackedSteps[stepName]
            .SelectMany(x => x.Outputs)
            .ElementAt(outputIndex)
            .Reason;
        actualStepReason.Should().Be(expectedStepReason, $"step {stepName} of mediator at index {outputIndex}");
    }
}
