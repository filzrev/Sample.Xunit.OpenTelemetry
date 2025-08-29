using Microsoft.Extensions.Logging;

namespace Sample.Xunit.OpenTelemetry;

[TraceBeforeAfterTest]
public class UnitTests_WithBeforeAfterTest : TestBase
{
    public UnitTests_WithBeforeAfterTest()
    {
        using var activity = ActivitySource.StartActivity("Ctor");
    }

    public override async ValueTask InitializeAsync()
    {
        using var activity = ActivitySource.StartActivity("InitializeAsync");
        await base.InitializeAsync();
    }

    public override async ValueTask DisposeAsync()
    {
        // TODO: Currently `DisposeAsync` is called inside `Test` activity. (From `3.0.2-pre.10`)
        using var activity = ActivitySource.StartActivity("DisposeAsync");

        // Following delay should not be included on `Test` activity.
        await Task.Delay(500, CancellationToken);

        await base.DisposeAsync();
    }

    [Fact]
    public async Task Test01()
    {
        await Task.Delay(100, CancellationToken);
    }

    [Fact]
    public async Task Test02()
    {
        await Task.Delay(100, CancellationToken);
    }

}
