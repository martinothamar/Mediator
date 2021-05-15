using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace Mediator.SourceGenerator
{
    internal sealed class AnalysisReporter
    {
        private static readonly DiagnosticDescriptor _errorDescriptor = new DiagnosticDescriptor(
#pragma warning disable RS2008 // Enable analyzer release tracking
            "MG0001",
#pragma warning restore RS2008 // Enable analyzer release tracking
            $"Error in the {nameof(MediatorGenerator)} generator",
            $"Error in the {nameof(MediatorGenerator)} generator: " + "{0}",
            $"{nameof(MediatorGenerator)}",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public bool Report(in GeneratorExecutionContext context, CompilationAnalyzer analyzer)
        {
            bool error = false;

            var handledMessages = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);

            foreach (var handler in analyzer.ConcreteHandlerSymbolMap)
            {
                if (handler.Key.TypeKind == TypeKind.Struct)
                    Report(context, $"handler types cannot be structs: {handler.Key.Name}", ref error);

                foreach (var handlerInterface in handler.Value)
                {
                    var requestSymbol = handlerInterface.TypeArguments[0] as INamedTypeSymbol;
                    if (requestSymbol is null)
                        continue;

                    if (DerivedFromNotification(requestSymbol))
                        continue;

                    if (!handledMessages.Add(requestSymbol))
                        Report(context, $"found multiple handlers of: {requestSymbol.Name}", ref error);
                }
            }

            return error;

            static bool DerivedFromNotification(INamedTypeSymbol requestSymbol)
            {
                return requestSymbol.AllInterfaces.Any(i => (i.ContainingNamespace?.Name == Constants.MediatorLib && i.Name == "INotification") || DerivedFromNotification(i));
            }

            static void Report(in GeneratorExecutionContext context, string message, ref bool error)
            {
                error = true;
                context.ReportDiagnostic(Diagnostic.Create(_errorDescriptor, Location.None, message));
            }
        }
    }
}
