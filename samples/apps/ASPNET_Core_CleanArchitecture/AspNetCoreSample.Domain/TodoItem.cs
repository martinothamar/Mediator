namespace AspNetCoreSample.Domain;

public sealed record TodoItem(Guid Id, string Title, string Text, bool Done);
