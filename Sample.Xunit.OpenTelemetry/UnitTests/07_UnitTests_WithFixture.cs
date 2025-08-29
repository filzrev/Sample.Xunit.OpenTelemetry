using Microsoft.Extensions.Logging;

namespace Sample.Xunit.OpenTelemetry;

public class UnitTests_WithFixture : TestBase, IClassFixture<SampleFixture>
{
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
