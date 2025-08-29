using Microsoft.Extensions.Logging;

namespace Sample.Xunit.OpenTelemetry;

public class UnitTests_Others : TestBase
{
    [Fact]
    public async Task Test01()
    {
        // Test `System.Net.Http` logs are recorded.
        using var httpClient = new HttpClient();
        await httpClient.GetAsync("https://example.com", CancellationToken);
    }
}
