using Sample.Xunit.OpenTelemetry;
using Xunit.v3;

[assembly: CaptureConsole]
[assembly: TestPipelineStartup(typeof(XUnitStartup))]
