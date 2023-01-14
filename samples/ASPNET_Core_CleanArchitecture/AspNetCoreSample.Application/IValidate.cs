using Mediator;
using System.Diagnostics.CodeAnalysis;

namespace AspNetCoreSample.Application;

public interface IValidate : IMessage
{
    bool IsValid([NotNullWhen(false)] out ValidationError? error);
}
