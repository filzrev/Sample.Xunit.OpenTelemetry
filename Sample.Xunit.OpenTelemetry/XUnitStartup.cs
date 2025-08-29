using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Reflection;
using Xunit.Sdk;
using Xunit.v3;

namespace Sample.Xunit.OpenTelemetry;

/// <summary>
/// Define code that executed on test pipeline startup and shutdown.
/// </summary>
/// <remarks>
/// This startup code is called on both MTP test discovery/execution.
/// </remarks>
public class XUnitStartup : ITestPipelineStartup
{
    public static readonly ActivitySource XUnitActivitySource = new(Constants.XUnitActivitySourceName);

    public IConfigurationRoot? Configuration { get; private set; }

    // Following fields are set by StartAsync. and disposed on StopAsync.
    internal static ServiceProvider ServiceProvider = default!;
    internal static ILoggerFactory LoggerFactory = default!;

    private XunitEventListener? _eventSourceListener = new();
    private TracerProvider? _tracerProvider = null;
    // private MeterProvider? _meterProvider = null;

    public XUnitStartup()
    {
    }

    /// <inheritdoc/>
    public ValueTask StartAsync(IMessageSink diagnosticMessageSink)
    {
        var services = new ServiceCollection();

        services.AddOptions();
        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.AddOpenTelemetry(options =>
            {
                options.IncludeFormattedMessage = true;
                options.IncludeScopes = true;           // Include scope information
                options.ParseStateValues = true;        // Enable structured log parsing

                var resourceBuilder = ResourceBuilder.CreateDefault();
                ConfigureResourceBuilder(resourceBuilder);
                options.SetResourceBuilder(resourceBuilder);
                options.AddOtlpExporter(otlp =>
                {
                });
            });
        });

        // Build ServiceProvider
        ServiceProvider = services.BuildServiceProvider();

        Configuration = InitializeConfiguration();

        _tracerProvider = InitializeTraceProvider();
        // _meterProvider = InitializeMeterProvider();
        LoggerFactory = InitializeLoggerFactory();

        return ValueTask.CompletedTask;
    }

    /// <inheritdoc/>
    public ValueTask StopAsync()
    {
        _eventSourceListener?.Dispose();

        _tracerProvider?.Dispose();

        // TODO: Meter's ForceFlush operation takes about 2000[ms] when using default timeout setting. It's too long for unit tests
        // _meterProvider?.Dispose(); 

        LoggerFactory?.Dispose();
        ServiceProvider?.Dispose();
        XUnitActivitySource.Dispose();

        return ValueTask.CompletedTask;
    }

    private static IConfigurationRoot InitializeConfiguration()
    {
        var builder = new ConfigurationBuilder();
        builder.SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appSettings.json", optional: false, reloadOnChange: true)
               .AddEnvironmentVariables();

        var env = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "";
        if (env != "")
        {
            builder.AddJsonFile($"appSettings.{env}.json", optional: true, reloadOnChange: true);
        }

        return builder.Build();
    }

    private static TracerProvider InitializeTraceProvider()
    {
        var builder = Sdk.CreateTracerProviderBuilder();
        builder.ConfigureServices(services =>
        {
        });

        builder.AddOtlpExporter(otlpOptions =>
        {
        });

        builder
            .ConfigureResource(ConfigureResourceBuilder)
            //.AddSource(Constants.XUnitActivitySourceName)
            //.AddSource(Constants.TestActivitySourceName)
            .AddSource("*");

        return builder.Build();
    }

    private static MeterProvider InitializeMeterProvider()
    {
        var builder = Sdk.CreateMeterProviderBuilder();

        //builder.AddRuntimeInstrumentation(options =>
        //{
        //});

        builder.AddOtlpExporter((otlpExporterOptions, metricsReaderOptions) =>
        {
        });
        builder.ConfigureResource(ConfigureResourceBuilder);

        return builder.Build();
    }

    private static ILoggerFactory InitializeLoggerFactory()
    {
        return Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Trace);

            var resourceBuilder = ResourceBuilder.CreateDefault();
            ConfigureResourceBuilder(resourceBuilder);

            builder.AddOpenTelemetry(options =>
            {
                options.SetResourceBuilder(resourceBuilder);
                options.AddOtlpExporter((otlpExporterOptions, logExportProcessorOptions) =>
                {
                });
            });
        });
    }

    private static void ConfigureResourceBuilder(ResourceBuilder builder)
    {
        builder.AddService("UnitTests", autoGenerateServiceInstanceId: true) // Assembly.GetExecutingAssembly().GetName().Name!
               .AddAttributes(new Dictionary<string, object>
               {
                   ["test.framework.name"] = "xunit",
                   ["test.framework.version"] = typeof(global::Xunit.TestContext).Assembly.GetName().Version?.ToString() ?? "unknown",
               });
    }
}
