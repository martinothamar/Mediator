using System;

namespace AspNetSample.Domain
{
    public sealed record TodoItem(Guid Id, string Title, string Text, bool Done);
}
