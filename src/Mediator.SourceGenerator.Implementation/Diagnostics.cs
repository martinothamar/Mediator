#pragma warning disable RS2008 // Enable analyzer release tracking

using System.Runtime.CompilerServices;

namespace Mediator.SourceGenerator;

public static class Diagnostics
{
    private static long _counter;
    private static readonly HashSet<string> _ids;

    public static IReadOnlyCollection<string> Ids => _ids;

    private static class MediatorGenerator { }

    static Diagnostics()
    {
        _ids = new();

        GenericError = new DiagnosticDescriptor(
            GetNextId(),
            $"{nameof(MediatorGenerator)} unknown error",
            $"{nameof(MediatorGenerator)} got unknown error while generating mediator implementation, please report this to the issue tracker on github at https://github.com/martinothamar/Mediator/issues/new . Error: "
                + "{0}",
            nameof(MediatorGenerator),
            DiagnosticSeverity.Error,
            isEnabledByDefault: true
        );

        MultipleHandlersError = new DiagnosticDescriptor(
            GetNextId(),
            $"{nameof(MediatorGenerator)} multiple handlers",
            $"{nameof(MediatorGenerator)} found multiple handlers " + "of message type {0}",
            nameof(MediatorGenerator),
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true
        );

        InvalidHandlerTypeError = new DiagnosticDescriptor(
            GetNextId(),
            $"{nameof(MediatorGenerator)} invalid handler",
            $"{nameof(MediatorGenerator)} found invalid handler type " + "{0}",
            nameof(MediatorGenerator),
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true
        );

        OpenGenericRequestHandler = new DiagnosticDescriptor(
            GetNextId(),
            $"{nameof(MediatorGenerator)} invalid handler",
            $"{nameof(MediatorGenerator)} found invalid handler type, request/query/command handlers cannot be generic: "
                + "{0}",
            nameof(MediatorGenerator),
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true
        );

        MessageDerivesFromMultipleMessageInterfaces = new DiagnosticDescriptor(
            GetNextId(),
            $"{nameof(MediatorGenerator)} invalid message",
            $"{nameof(MediatorGenerator)} found message that derives from multiple message interfaces: " + "{0}",
            nameof(MediatorGenerator),
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true
        );

        MessageWithoutHandler = new DiagnosticDescriptor(
            GetNextId(),
            $"{nameof(MediatorGenerator)} message warning",
            $"{nameof(MediatorGenerator)} found message without any registered handler: " + "{0}",
            nameof(MediatorGenerator),
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true
        );

        ConflictingConfiguration = new DiagnosticDescriptor(
            GetNextId(),
            $"{nameof(MediatorGenerator)} configuration error",
            $"{nameof(MediatorGenerator)} found conflicting configuration - both MediatorOptions and MediatorOptionsAttribute configuration are being used.",
            nameof(MediatorGenerator),
            DiagnosticSeverity.Error,
            isEnabledByDefault: true
        );

        InvalidCodeBasedConfiguration = new DiagnosticDescriptor(
            GetNextId(),
            $"{nameof(MediatorGenerator)} configuration error",
            $"{nameof(MediatorGenerator)} cannot parse MediatorOptions-based configuration. Only compile-time constant values can be used in MediatorOptions configuration.",
            nameof(MediatorGenerator),
            DiagnosticSeverity.Error,
            isEnabledByDefault: true
        );

        RequiredSymbolNotFound = new DiagnosticDescriptor(
            GetNextId(),
            $"{nameof(MediatorGenerator)} symbol not found during analysis",
            $"{nameof(MediatorGenerator)} could not find symbol required during analysis: " + "{0}",
            nameof(MediatorGenerator),
            DiagnosticSeverity.Error,
            isEnabledByDefault: true
        );

        static string GetNextId()
        {
            var count = _counter++;
            var id = $"MSG{count.ToString().PadLeft(4, '0')}";
            _ids.Add(id);

            return id;
        }
    }

    private static Diagnostic Report<T>(
        this CompilationAnalyzerContext context,
        DiagnosticDescriptor diagnosticDescriptor,
        T arg
    ) where T : class
    {
        Diagnostic diagnostic;
        if (arg is ISymbol symbolArg)
        {
            var location = symbolArg.Locations.FirstOrDefault(l => l.IsInSource);
            var symbolName = symbolArg.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);
            diagnostic = Diagnostic.Create(diagnosticDescriptor, location ?? Location.None, symbolName);
        }
        else
        {
            diagnostic = Diagnostic.Create(diagnosticDescriptor, Location.None, arg);
        }
        context.ReportDiagnostic(diagnostic);
        return diagnostic;
    }

    private static Diagnostic Report(this CompilationAnalyzerContext context, DiagnosticDescriptor diagnosticDescriptor)
    {
        var diagnostic = Diagnostic.Create(diagnosticDescriptor, Location.None);
        context.ReportDiagnostic(diagnostic);
        return diagnostic;
    }

    public static readonly DiagnosticDescriptor GenericError;

    internal static Diagnostic ReportGenericError(this CompilationAnalyzerContext context, Exception exception)
    {
        var error =
            $"{exception.Message}: {exception.StackTrace}{(exception.InnerException is not null ? $"\nInner: {exception.InnerException}" : "")}";
        var diagnostic = Diagnostic.Create(GenericError, Location.None, error);
        context.ReportDiagnostic(diagnostic);
        return diagnostic;
    }

    public static readonly DiagnosticDescriptor MultipleHandlersError;

    internal static Diagnostic ReportMultipleHandlers(
        this CompilationAnalyzerContext context,
        INamedTypeSymbol messageType
    ) => context.Report(MultipleHandlersError, messageType);

    public static readonly DiagnosticDescriptor InvalidHandlerTypeError;

    internal static Diagnostic ReportInvalidHandlerType(
        this CompilationAnalyzerContext context,
        INamedTypeSymbol handlerType
    ) => context.Report(InvalidHandlerTypeError, handlerType);

    public static readonly DiagnosticDescriptor OpenGenericRequestHandler;

    internal static Diagnostic ReportOpenGenericRequestHandler(
        this CompilationAnalyzerContext context,
        INamedTypeSymbol handlerType
    ) => context.Report(OpenGenericRequestHandler, handlerType);

    public static readonly DiagnosticDescriptor MessageDerivesFromMultipleMessageInterfaces;

    internal static Diagnostic ReportMessageDerivesFromMultipleMessageInterfaces(
        this CompilationAnalyzerContext context,
        INamedTypeSymbol messageType
    ) => context.Report(MessageDerivesFromMultipleMessageInterfaces, messageType);

    public static readonly DiagnosticDescriptor MessageWithoutHandler;

    internal static Diagnostic ReportMessageWithoutHandler(
        this CompilationAnalyzerContext context,
        INamedTypeSymbol messageType
    ) => context.Report(MessageWithoutHandler, messageType);

    public static readonly DiagnosticDescriptor ConflictingConfiguration;

    internal static Diagnostic ReportConflictingConfiguration(this CompilationAnalyzerContext context) =>
        context.Report(ConflictingConfiguration);

    public static readonly DiagnosticDescriptor InvalidCodeBasedConfiguration;

    internal static Diagnostic ReportInvalidCodeBasedConfiguration(this CompilationAnalyzerContext context) =>
        context.Report(InvalidCodeBasedConfiguration);

    public static readonly DiagnosticDescriptor RequiredSymbolNotFound;

    internal static Diagnostic ReportRequiredSymbolNotFound(this CompilationAnalyzerContext context, string name) =>
        context.Report(RequiredSymbolNotFound, name);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void InjectError()
    {
        if (true)
        {
            throw new Exception("Injected error");
        }
    }
}

#pragma warning restore RS2008 // Enable analyzer release tracking
