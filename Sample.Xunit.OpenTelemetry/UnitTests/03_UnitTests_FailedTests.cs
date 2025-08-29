using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Sample.Xunit.OpenTelemetry;

public class UnitTests_FailedTests : TestBase
{
    [Fact]
    public void Test01()
    {
        using var activity = ActivitySource.StartActivity("Test01");
        Logger.LogInformation("Test01");

        Assert.False(true); // Assertion failed.
    }

    [Fact]
    public void Test02()
    {
        using var activity = ActivitySource.StartActivity("Test02");
        Logger.LogInformation("Test02");

        // Throw exception
        throw new Exception("Exception", new Exception("InnerException"));
    }
}
