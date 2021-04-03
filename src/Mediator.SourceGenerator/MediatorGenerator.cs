using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Text;

namespace Mediator.SourceGenerator
{
    [Generator]
    public sealed partial class MediatorGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            var debugOptionExists = context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.Mediator_AttachDebugger", out _);

            if (debugOptionExists)
                System.Diagnostics.Debugger.Launch();

            try
            {
                ExecuteInternal(in context);
            }
            catch (Exception exception)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        new DiagnosticDescriptor(
                            "MEDIATOR00001",
                            "An exception was thrown by the Mediator source generator",
                            "An exception was thrown by the Mediator source generator: '{0}'",
                            "Mediator",
                            DiagnosticSeverity.Error,
                            isEnabledByDefault: true
                        ),
                        Location.None,
                        exception.ToString()
                    )
                );

                throw;
            }
        }

        private void ExecuteInternal(in GeneratorExecutionContext context)
        {
            GenerateOptionsAttribute(in context);

            var compilation = context.Compilation;

            var compilationAnalyzer = new CompilationAnalyzer(compilation);
            compilationAnalyzer.Analyze(context.CancellationToken);

            var mediatorContext = new MediatorGenerationContext(
                compilationAnalyzer.HandlerMap,
                compilationAnalyzer.HandlerTypes,
                compilationAnalyzer.MediatorNamespace
            );
            var mediatorImplementationGenerator = new MediatorImplementationGenerator();
            mediatorImplementationGenerator.Generate(in context, in mediatorContext);
        }

        private void GenerateOptionsAttribute(in GeneratorExecutionContext context)
        {
            var attributeSource = EmbeddedResource.GetContent(@"resources/MediatorOptionsAttribute.cs");
            context.AddSource("MediatorOptionsAttribute.g.cs", SourceText.From(attributeSource, Encoding.UTF8));
        }

        public void Initialize(GeneratorInitializationContext context)
        {
        }
    }
}
