namespace Mediator.SourceGenerator
{
    public sealed record MessageType(
        string FullName,
        string iMessageType,
        string SyncMethodName,
        string AsyncMethodName,
        string SyncReturnType,
        string AsyncReturnType
    );
}
