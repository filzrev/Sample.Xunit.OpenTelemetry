using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample.Xunit.OpenTelemetry
{
    internal static class ExtensionMethods
    {
        // Map TestResult enum to `test.case.result.status` code.
        // https://opentelemetry.io/docs/specs/semconv/registry/attributes/test/#test-case-result-status
        public static string? ToTestCaseResultStatus(this TestResult result)
        {
            return result switch
            {
                TestResult.Passed => "pass",
                TestResult.Failed => "fail",
                // Use custom values for Skipped/NotRun
                TestResult.Skipped => "skip",
                TestResult.NotRun => "notrun",
                _ => null,
            };
        }
    }
}
