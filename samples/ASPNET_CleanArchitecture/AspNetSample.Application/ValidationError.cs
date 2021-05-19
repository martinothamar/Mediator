using System.Collections.Generic;

namespace AspNetSample.Application
{
    public sealed record ValidationError(IEnumerable<string> Errors);
}
