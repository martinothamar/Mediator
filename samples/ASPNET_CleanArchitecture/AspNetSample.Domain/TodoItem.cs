using System;

namespace AspNetSample.Domain
{
    public sealed record TodoItem(Guid id, string Title, string Text, bool Done);
}
