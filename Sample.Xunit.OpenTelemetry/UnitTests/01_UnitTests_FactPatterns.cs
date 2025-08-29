using Microsoft.Extensions.Logging;

namespace Sample.Xunit.OpenTelemetry;

public class UnitTests_FactPatterns : TestBase
{
    [Fact]
    public async Task Test01()
    {
        await Task.Delay(100, CancellationToken);
    }

    [Fact(DisplayName = "CustomDisplayName")]
    public async Task Test02()
    {
        await Task.Delay(100, CancellationToken);
    }

    [Fact(Skip = "Skipped")]
    public async Task Test03()
    {
        await Task.Delay(100, CancellationToken);
    }

    public static bool IsSkipped => true;

    [Fact(Skip = "ConditionalSkipped", SkipWhen = nameof(IsSkipped))]
    public async Task Test04()
    {
        await Task.Delay(100, CancellationToken);
    }

    [Fact(Explicit = true)]
    public async Task Test05()
    {
        await Task.Delay(100, CancellationToken);
    }

    [Fact(Timeout = 50)]
    public async Task Test06()
    {
        await Task.Delay(5000, CancellationToken);
    }
}
