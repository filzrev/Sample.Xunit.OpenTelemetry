using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Sample.Xunit.OpenTelemetry;

public class UnitTests_LogLifecycles : TestBase
{
    public UnitTests_LogLifecycles()
    {
        using var activity = ActivitySource.StartActivity("Ctor");
    }

    public override ValueTask InitializeAsync()
    {
        using var activity = ActivitySource.StartActivity("InitializeAsync");
        return base.InitializeAsync();
    }

    public override ValueTask DisposeAsync()
    {
        // TODO: Currently `DisposeAsync` is called inside `Test` activity.
        using var activity = ActivitySource.StartActivity("DisposeAsync");
        return base.DisposeAsync();
    }

    [Fact]
    public void Test01()
    {
        using var activity = ActivitySource.StartActivity("Test01");
        Logger.LogInformation("Test01");
    }

    [Fact]
    public void Test02()
    {
        using var activity = ActivitySource.StartActivity("Test02");
        Logger.LogInformation("Test02");

        // This log is forwarded to Microsoft.Extensions.Logging.ILogger.
        Output.WriteLine("Test02");
    }

    [Fact]
    public void Test03()
    {
        using var activity = ActivitySource.StartActivity("Test03");
        Logger.LogInformation("Test03");

        // Following logs are not forwarded to Microsoft.Extensions.Logging.ILogger.
        TestContext.Current.TestOutputHelper!.WriteLine("Test03");
        Console.WriteLine("Test03");
    }
}
