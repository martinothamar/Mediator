namespace Mediator.SourceGenerator;

internal sealed class SyntaxReceiver : ISyntaxReceiver
{
    public List<InvocationExpressionSyntax> AddMediatorCalls { get; } = new List<InvocationExpressionSyntax>();

    public void OnVisitSyntaxNode(SyntaxNode context)
    {
        if (context is not InvocationExpressionSyntax invocationSyntax)
            return;
        if (invocationSyntax.Expression is not MemberAccessExpressionSyntax identifier)
            return;
        if (identifier.Name.Identifier.ValueText != "AddMediator")
            return;

        AddMediatorCalls.Add(invocationSyntax);
    }
}
