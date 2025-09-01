using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Sdk;

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

        public static string GetSimpleTestCaseDisplayName(this ITestCase testCase)
        {
            // TODO: DisplayName may be customized by Fact/Theory property. And might contains dot on auto generated Theory display name.
            return testCase.TestCaseDisplayName.Split('.').Last();
        }

        public static string GetSimpleTestDisplayName(this ITest test)
        {
            // TODO: DisplayName may be customized by Fact/Theory property. And might contains dot on auto generated Theory display name.
            return test.TestDisplayName.Split('.').Last();
        }
    }
}
