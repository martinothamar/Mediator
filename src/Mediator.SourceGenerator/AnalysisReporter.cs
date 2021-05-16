using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mediator.SourceGenerator
{
    internal sealed class AnalysisReporter
    {
        public bool Report(in GeneratorExecutionContext context, CompilationAnalyzer analyzer)
        {
            var isError = false;

            var handledMessages = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);

            foreach (var handler in analyzer.ConcreteHandlerSymbolMap)
            {
                if (handler.Key.TypeKind == TypeKind.Struct)
                    Report(ref isError, context, c => c.ReportInvalidHandlerType(handler.Key));

                foreach (var handlerInterface in handler.Value)
                {
                    var requestSymbol = handlerInterface.TypeArguments[0] as INamedTypeSymbol;
                    if (requestSymbol is null)
                        continue;

                    if (DerivedFromNotification(requestSymbol))
                        continue;

                    if (!handledMessages.Add(requestSymbol))
                        Report(ref isError, context, c => c.ReportMultipleHandlers(requestSymbol));
                }
            }

            return isError;

            static void Report(ref bool isError, in GeneratorExecutionContext context, Action<GeneratorExecutionContext> del)
            {
                isError = true;
                del(context);
            }

            static bool DerivedFromNotification(INamedTypeSymbol requestSymbol) =>
                requestSymbol
                    .AllInterfaces
                    .Any(i => (i.ContainingNamespace?.Name == Constants.MediatorLib && i.Name == "INotification") || DerivedFromNotification(i));
        }
    }
}
