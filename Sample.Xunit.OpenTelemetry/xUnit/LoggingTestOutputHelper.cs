using Microsoft.Extensions.Logging;
using System.Globalization;

namespace Sample.Xunit.OpenTelemetry;

/// <summary>
/// Custom ITestOutputHelper implementation that forward output to `Microsoft.Extension.Logging.ILogger`.
/// </summary>
public class LogForwardTestOutputHelper : ITestOutputHelper
{
    private readonly ITestOutputHelper InnerTestOutputHelper;
    private readonly ILogger Logger;

    public LogForwardTestOutputHelper(ILogger logger)
    {
        InnerTestOutputHelper = TestContext.Current.TestOutputHelper!;
        Logger = logger;
    }

    /// <inheritdoc/>
    public string Output => InnerTestOutputHelper.Output;

    /// <inheritdoc/>
    public void Write(string message)
    {
        InnerTestOutputHelper.Write(message);
        Logger.LogInformation(message);
    }

    /// <inheritdoc/>
    public void WriteLine(string message)
    {
        InnerTestOutputHelper.WriteLine(message);
        Logger.LogInformation(message);
    }

    /// <inheritdoc/>
    public void Write(string format, params object[] args)
    {
        var message = string.Format(CultureInfo.CurrentCulture, format, args);
        Write(message);
    }

    /// <inheritdoc/>
    public void WriteLine(string format, params object[] args)
    {
        var message = string.Format(CultureInfo.CurrentCulture, format, args);
        WriteLine(format, args);
    }
}
