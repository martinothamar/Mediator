using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;

#pragma warning disable RS2008 // Enable analyzer release tracking

namespace Mediator.SourceGenerator
{
    public static class Diagnostics
    {
        private static long _counter;
        private static readonly HashSet<string> _ids;

        public static IReadOnlyCollection<string> Ids => _ids;

        static Diagnostics()
        {
            _ids = new();

            GenericError = new DiagnosticDescriptor(
                GetNextId(),
                $"{nameof(MediatorGenerator)} unknown error",
                $"{nameof(MediatorGenerator)} got unknown error: " + "{0}",
                nameof(MediatorGenerator),
                DiagnosticSeverity.Error,
                isEnabledByDefault: true);

            MultipleHandlersError = new DiagnosticDescriptor(
                GetNextId(),
                $"{nameof(MediatorGenerator)} multiple handlers",
                $"{nameof(MediatorGenerator)} found multiple handlers " + "of message type {0}",
                nameof(MediatorGenerator),
                DiagnosticSeverity.Error,
                isEnabledByDefault: true);

            InvalidHandlerTypeError = new DiagnosticDescriptor(
                GetNextId(),
                $"{nameof(MediatorGenerator)} invalid handler",
                $"{nameof(MediatorGenerator)} found invalid handler type " + "{0}",
                nameof(MediatorGenerator),
                DiagnosticSeverity.Error,
                isEnabledByDefault: true);

            OpenGenericRequestHandler = new DiagnosticDescriptor(
                GetNextId(),
                $"{nameof(MediatorGenerator)} invalid handler",
                $"{nameof(MediatorGenerator)} found invalid handler type, request/query/command handlers cannot be generic: " + "{0}",
                nameof(MediatorGenerator),
                DiagnosticSeverity.Error,
                isEnabledByDefault: true);

            MessageDerivesFromMultipleMessageInterfaces = new DiagnosticDescriptor(
                GetNextId(),
                $"{nameof(MediatorGenerator)} invalid message",
                $"{nameof(MediatorGenerator)} found message that derives from multiple message interfaces: " + "{0}",
                nameof(MediatorGenerator),
                DiagnosticSeverity.Error,
                isEnabledByDefault: true);

            static string GetNextId()
            {
                var count = _counter++;
                var id = $"MSG{count.ToString().PadLeft(4, '0')}";
                _ids.Add(id);

                return id;
            }
        }

        public static readonly DiagnosticDescriptor GenericError;
        internal static void ReportGenericError(this GeneratorExecutionContext context, Exception exception) =>
            context.ReportDiagnostic(Diagnostic.Create(GenericError, Location.None, exception));

        public static readonly DiagnosticDescriptor MultipleHandlersError;
        internal static void ReportMultipleHandlers(this GeneratorExecutionContext context, INamedTypeSymbol messageType) =>
            context.ReportDiagnostic(Diagnostic.Create(MultipleHandlersError, Location.None, messageType.Name));

        public static readonly DiagnosticDescriptor InvalidHandlerTypeError;
        internal static void ReportInvalidHandlerType(this GeneratorExecutionContext context, INamedTypeSymbol handlerType) =>
            context.ReportDiagnostic(Diagnostic.Create(InvalidHandlerTypeError, Location.None, handlerType.Name));

        public static readonly DiagnosticDescriptor OpenGenericRequestHandler;
        internal static void ReportOpenGenericRequestHandler(this GeneratorExecutionContext context, INamedTypeSymbol handlerType) =>
            context.ReportDiagnostic(Diagnostic.Create(OpenGenericRequestHandler, Location.None, handlerType.Name));

        public static readonly DiagnosticDescriptor MessageDerivesFromMultipleMessageInterfaces;
        internal static void ReportMessageDerivesFromMultipleMessageInterfaces(this GeneratorExecutionContext context, INamedTypeSymbol messageType) =>
            context.ReportDiagnostic(Diagnostic.Create(MessageDerivesFromMultipleMessageInterfaces, Location.None, messageType.Name));
    }
}

#pragma warning restore RS2008 // Enable analyzer release tracking
