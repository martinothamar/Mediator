using Xunit.Abstractions;
using Xunit.Sdk;

[assembly: TestFramework("Mediator.Tests.ScopedLifetime.Framework", "Mediator.Tests.ScopedLifetime")]

namespace Mediator.Tests.ScopedLifetime;

public class Framework : XunitTestFramework
{
    public Framework(IMessageSink messageSink) : base(messageSink)
    {
        Fixture.CreateServiceScope = true;
    }

    public new void Dispose()
    {
        // Place tear down code here
        base.Dispose();
    }
}
