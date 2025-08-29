using System.Runtime.CompilerServices;

namespace Sample.Xunit.OpenTelemetry;

internal static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Initializer()
    {
        Environment.SetEnvironmentVariable("DOTNET_CLI_TELEMETRY_OPTOUT", "true");
        Environment.SetEnvironmentVariable("TESTINGPLATFORM_TELEMETRY_OPTOUT", "true");
    }
}
