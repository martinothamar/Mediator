using Mediator.SourceGenerator.Tests;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using System;
using System.Collections.Immutable;
using System.Linq;

public static class CSharpSourceGeneratorVerifier<TSourceGenerator> where TSourceGenerator : ISourceGenerator, new()
{
    public class Test : CSharpSourceGeneratorTest<TSourceGenerator, XUnitVerifier>
    {
        public Test()
        {
            var refs = Fixture.AssemblyReferencesForCodegen.Select(a => a.Location).ToImmutableArray();

            var metadataRefs = refs.Select(l => MetadataReference.CreateFromFile(l)).ToImmutableArray();

            this.ReferenceAssemblies = Microsoft.CodeAnalysis.Testing.ReferenceAssemblies.Net.Net60.WithAssemblies(
                refs
            );

            this.SolutionTransforms.Add(
                (sln, projectId) =>
                {
                    var compilationOptions = ((CSharpCompilationOptions)sln.GetProject(projectId)!.CompilationOptions!)
                        .WithOutputKind(OutputKind.ConsoleApplication)
                        .WithUsings(
                            "System",
                            "System.Collections.Generic",
                            "System.IO",
                            "System.Linq",
                            "System.Net.Http",
                            "System.Threading",
                            "System.Threading.Tasks"
                        );

                    sln = sln.AddMetadataReferences(projectId, metadataRefs)
                        .WithProjectCompilationOptions(projectId, compilationOptions);
                    return sln;
                }
            );
        }

        public TSourceGenerator SourceGenerator => (TSourceGenerator)GetSourceGenerators().Single();

        protected override CompilationOptions CreateCompilationOptions()
        {
            var compilationOptions = base.CreateCompilationOptions();
            return compilationOptions
                .WithSpecificDiagnosticOptions(
                    compilationOptions.SpecificDiagnosticOptions.SetItems(GetNullableWarningsFromCompiler())
                )
                .WithOutputKind(OutputKind.ConsoleApplication);
        }

        public LanguageVersion LanguageVersion { get; set; } = LanguageVersion.Latest;

        private static ImmutableDictionary<string, ReportDiagnostic> GetNullableWarningsFromCompiler()
        {
            string[] args = { "/warnaserror:nullable" };
            var commandLineArguments = CSharpCommandLineParser.Default.Parse(
                args,
                baseDirectory: Environment.CurrentDirectory,
                sdkDirectory: Environment.CurrentDirectory
            );
            var nullableWarnings = commandLineArguments.CompilationOptions.SpecificDiagnosticOptions;

            return nullableWarnings;
        }

        protected override ParseOptions CreateParseOptions()
        {
            return ((CSharpParseOptions)base.CreateParseOptions()).WithLanguageVersion(LanguageVersion);
        }
    }
}
