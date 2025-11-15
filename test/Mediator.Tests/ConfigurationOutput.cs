using Microsoft.Extensions.DependencyInjection;

namespace Mediator.Tests;

public class ConfigurationOutput
{
    private readonly ITestOutputHelper output;

    public ConfigurationOutput(ITestOutputHelper output)
    {
        this.output = output;
    }

    [Fact]
    public void Test()
    {
        var (sp, _) = Fixture.GetMediator();

        var publisher = sp.GetRequiredService<INotificationPublisher>();

        output.WriteLine("");
        output.WriteLine("");
        output.WriteLine("");
        output.WriteLine("------------------------------------------------------------------------------");
        output.WriteLine("Mediator configuration:");
        output.WriteLine($" ServiceLifetime: {Mediator.ServiceLifetime}");
        output.WriteLine($" CachingMode: {Mediator.CachingMode}");
        output.WriteLine(
            $" NotificationPublisherType: {publisher.GetType().FullName} ({Mediator.NotificationPublisherName})"
        );
        output.WriteLine($" Message count: {Mediator.TotalMessages}");
        output.WriteLine($" CrossedManyMessagesThreshold: {Mediator.CrossedManyMessagesThreshold}");
        output.WriteLine("------------------------------------------------------------------------------");
        output.WriteLine("");
    }
}
