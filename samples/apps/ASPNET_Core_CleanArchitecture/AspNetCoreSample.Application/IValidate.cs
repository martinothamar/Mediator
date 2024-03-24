using System.Diagnostics.CodeAnalysis;
using Mediator;

namespace AspNetCoreSample.Application;

public interface IValidate : IMessage
{
    bool IsValid([NotNullWhen(false)] out ValidationError? error);
}
