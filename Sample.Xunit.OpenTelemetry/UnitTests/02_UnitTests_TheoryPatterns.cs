namespace Sample.Xunit.OpenTelemetry;

public class UnitTests_TheoryPatterns : TestBase
{
    [Theory(DisableDiscoveryEnumeration = false)]
    [InlineData("1")]
    [InlineData("2")]
    [InlineData("3")]
    public async Task Test01(string _)
    {
        await Task.Delay(100, CancellationToken);
    }

    [Theory(DisableDiscoveryEnumeration = true)]
    [InlineData("1")]
    [InlineData("2")]
    [InlineData("3")]
    public async Task Test02(string _)
    {
        await Task.Delay(100, CancellationToken);
    }

    [Theory(DisplayName = "CustomDisplayName")]
    [InlineData("1")]
    [InlineData("2")]
    [InlineData("3")]
    public async Task Test03(string _)
    {
        await Task.Delay(100, CancellationToken);
    }

}
