namespace AspNetCoreSample.Application;

public sealed record ValidationError(IEnumerable<string> Errors);
