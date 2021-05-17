using Mediator;
using System.Diagnostics.CodeAnalysis;

namespace AspNetSample.Application
{
    public interface IValidate : IMessage
    {
        bool IsValid([NotNullWhen(false)] out ValidationError? error);
    }
}
