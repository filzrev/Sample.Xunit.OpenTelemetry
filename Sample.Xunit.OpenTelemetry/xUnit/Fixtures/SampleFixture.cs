using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Reflection;
using Xunit.v3;

namespace Sample.Xunit.OpenTelemetry;

public class SampleFixture : IDisposable
{
    private readonly ILogger Logger;

    public SampleFixture()
    {
        Logger = XUnitStartup.LoggerFactory.CreateLogger<SampleFixture>();
        Logger.LogInformation("SampleFixture::ctor");

        // Activity.Current is expected to be TestCollection activity.
        // Note: It can't set Activity.Current here. Because this constructor is called by `FixtureMappingManager.InitializeAsync` async context.
        Activity.Current?.AddEvent(new ActivityEvent("SampleFixture::ctor"));
    }

    public void Dispose()
    {
        Logger.LogInformation("SampleFixture::ctor");

        Activity.Current?.AddEvent(new ActivityEvent("SampleFixture::Dispose"));
    }
}
