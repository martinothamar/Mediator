#if Mediator_Lifetime_Scoped

using Xunit.Abstractions;
using Xunit.Sdk;

[assembly: TestFramework("Mediator.Tests.Framework", "Mediator.Tests")]

namespace Mediator.Tests;

public class Framework : XunitTestFramework
{
    public Framework(IMessageSink messageSink)
        : base(messageSink)
    {
        Fixture.CreateServiceScope = true;
    }

    public new void Dispose()
    {
        // Place tear down code here
        base.Dispose();
    }
}

#endif
