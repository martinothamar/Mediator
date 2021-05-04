namespace Mediator.SourceGenerator
{
    public sealed record HandlerType(string Name, bool HasResponse)
    {
        public bool IsNotificationType => Name.Contains("Notification");
    }
}
