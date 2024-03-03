using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Mediator.SourceGenerator.Tests;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

public static class CSharpSourceGeneratorVerifier<TSourceGenerator>
    where TSourceGenerator : IIncrementalGenerator, new()
{
    public static Task VerifySolution(string source)
    {
        var test = new Test
        {
            TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck,
            TestState = { Sources = { source }, },
        };
        return test.RunAsync();
    }

    private class Test : CSharpSourceGeneratorTest<EmptySourceGeneratorProvider, DefaultVerifier>
    {
        public Test()
        {
            var refs = Fixture.AssemblyReferencesForCodegen.Select(a => a.Location).ToImmutableArray();
            this.ReferenceAssemblies = ReferenceAssemblies.Net.Net80.WithAssemblies(refs);

            this.SolutionTransforms.Add(
                (solution, projectId) =>
                {
                    var compilationOptions = solution.GetProject(projectId)!.CompilationOptions;
                    compilationOptions = compilationOptions!.WithSpecificDiagnosticOptions(
                        compilationOptions.SpecificDiagnosticOptions.SetItems(GetNullableWarningsFromCompiler())
                    );
                    compilationOptions = ((CSharpCompilationOptions)compilationOptions!).WithUsings(
                        "System",
                        "System.Collections.Generic",
                        "System.IO",
                        "System.Linq",
                        "System.Net.Http",
                        "System.Threading",
                        "System.Threading.Tasks"
                    );
                    solution = solution.AddMetadataReferences(
                        projectId,
                        refs.Select(l => MetadataReference.CreateFromFile(l)).ToImmutableArray()
                    );
                    solution = solution.WithProjectCompilationOptions(projectId, compilationOptions);
                    return solution;
                }
            );
        }

        protected override IEnumerable<Type> GetSourceGenerators()
        {
            yield return new TSourceGenerator().AsSourceGenerator().GetGeneratorType();
        }

        public LanguageVersion LanguageVersion { get; set; } = LanguageVersion.Latest;

        protected override ParseOptions CreateParseOptions()
        {
            return ((CSharpParseOptions)base.CreateParseOptions()).WithLanguageVersion(LanguageVersion);
        }

        static ImmutableDictionary<string, ReportDiagnostic> GetNullableWarningsFromCompiler()
        {
            string[] args = { "/warnaserror:nullable" };
            var commandLineArguments = CSharpCommandLineParser.Default.Parse(
                args,
                baseDirectory: Environment.CurrentDirectory,
                sdkDirectory: Environment.CurrentDirectory
            );
            return commandLineArguments.CompilationOptions.SpecificDiagnosticOptions;
        }
    }
}
