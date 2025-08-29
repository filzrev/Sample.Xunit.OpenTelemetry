using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sample.Xunit.OpenTelemetry;
using System.Diagnostics;

namespace Sample.Xunit.OpenTelemetry;

public abstract class TestBase : IAsyncLifetime
{
    protected static ActivitySource ActivitySource => new(Constants.TestActivitySourceName);

    private readonly AsyncServiceScope ServiceScope;
    protected readonly IServiceProvider ServiceProvider;
    protected readonly ILogger Logger;

    protected readonly ITestOutputHelper Output;
    protected CancellationToken CancellationToken = TestContext.Current.CancellationToken;

    protected TestBase()
    {
        ServiceScope = XUnitStartup.ServiceProvider.CreateAsyncScope();
        ServiceProvider = ServiceScope.ServiceProvider;
        Logger = XUnitStartup.LoggerFactory.CreateLogger(GetType());

        // Set custom TestOutputHelper to forward logs.
        Output = new LogForwardTestOutputHelper(Logger);
    }

    /// <inheritdoc/>
    public virtual async ValueTask InitializeAsync()
    {
        await Task.CompletedTask;
    }

    /// <inheritdoc/>
    public virtual async ValueTask DisposeAsync()
    {
        await ServiceScope.DisposeAsync();
        ActivitySource.Dispose();
        await Task.CompletedTask;
    }
}
