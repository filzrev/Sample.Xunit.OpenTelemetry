using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Text;
using System.Text.RegularExpressions;
using Xunit.Sdk;
using static System.Net.Mime.MediaTypeNames;

namespace Sample.Xunit.OpenTelemetry;

/// <summary>
// Event listener for `xUnit.TestEventSource`
// https://github.com/xunit/xunit/blob/main/src/xunit.v3.core/EventSources/TestEventSource.cs
//
// This EventListener listen xUnit events and create Activity for OpenTelemetry.
/// </summary>
internal class XunitEventListener : EventListener
{
    // Single to instance for xUnit.net Activity
    private static ActivitySource ActivitySource => XUnitStartup.XUnitActivitySource;

    protected override void OnEventSourceCreated(EventSource eventSource)
    {
        const string XUnitEventSourceName = "xUnit.TestEventSource";
        if (eventSource.Name != XUnitEventSourceName)
            return;

        // No need to enable events if listener is not exists.
        // (e.g. OpenTelemety is disabled by environment variable)
        if (!ActivitySource.HasListeners())
            return;

        EnableEvents(eventSource, EventLevel.Informational);
    }

    protected override void OnEventWritten(EventWrittenEventArgs eventData)
    {
        //if (eventData.EventName != "EventCounters")
        //    return;

        switch (eventData.Opcode)
        {
            case EventOpcode.Start:
                HandleStartEvent(eventData);
                return;
            case EventOpcode.Stop:
                HandleStopEvent(eventData);
                return;
            default:
                return;
        }
    }

    private void HandleStartEvent(EventWrittenEventArgs eventData)
    {
        var context = TestContext.Current;

        switch (eventData.EventId)
        {
            case EventIds.TestStart:
                {
                    var test = context!.Test!;
                    var testDisplayName = test.TestDisplayName; // TODO: Gets simple display name without namespace.
                    var activityName = $"Test({testDisplayName})";

                    Activity.Current = ActivitySource.StartActivity(activityName, ActivityKind.Internal, parentId: default, tags: new ActivityTagsCollection
                    {
                        ["test.case.name"] = test.TestDisplayName, // Use fully qualified display name.
                        ["xunit.test.id"] = test.UniqueID,
                        ["xunit.test.name"] = testDisplayName,
                        // ["xunit.test.traits"] = test.Traits.ToArray(),
                    });
                    return;
                }
            case EventIds.TestCaseStart:
                {
                    var testCase = context!.TestCase!;
                    var testCaseDisplayName = testCase.TestCaseDisplayName; // TODO: Gets simple display name without namespace.
                    var activityName = $"TestCase({testCaseDisplayName})";
                    Activity.Current = ActivitySource.StartActivity(activityName, ActivityKind.Internal, parentId: default, tags: new ActivityTagsCollection
                    {
                        ["xunit.testcase.id"] = testCase.UniqueID,
                        ["xunit.testcase.name"] = testCaseDisplayName,
                    });

                    return;
                }
            case EventIds.TestMethodStart:
                {
                    var testMethod = context!.TestMethod!;
                    var activityName = $"TestMethod({testMethod.MethodName})";
                    Activity.Current = ActivitySource.StartActivity(activityName, ActivityKind.Internal, parentId: default, tags: new ActivityTagsCollection
                    {
                        ["xunit.testmethod.id"] = testMethod.UniqueID,
                        ["xunit.testmethod.name"] = testMethod.MethodName,
                    });
                    return;
                }

            case EventIds.TestCollectionStart:
                {
                    var testCollection = context!.TestCollection!;
                    var testCollectionSimpleName = testCollection.TestCollectionDisplayName;

                    // Try to extract type name from auto generated display name.
                    Match match = Regex.Match(testCollectionSimpleName, @"^Test collection for .+\.([A-Za-z0-9_]+) \(id:");
                    if (match.Success)
                        testCollectionSimpleName = match.Groups[1].Value;

                    var activityName = $"TestCollection({testCollectionSimpleName})";
                    Activity.Current = ActivitySource.StartActivity(activityName, ActivityKind.Internal, parentId: default, tags: new ActivityTagsCollection
                    {
                        ["xunit.testcollection.id"] = testCollection.UniqueID,
                        ["xunit.testcollection.classname"] = testCollection.TestCollectionClassName,
                        ["xunit.testcollection.displayname"] = testCollection.TestCollectionDisplayName, // Use full display name.
                    });
                    return;
                }

            case EventIds.TestClassStart:
                {
                    var testClass = context!.TestClass!;
                    var activityName = $"TestClass({testClass.TestClassSimpleName})";
                    // var testClassSimpleName = context!.TestClass!.TestClassSimpleName;
                    Activity.Current = ActivitySource.StartActivity(activityName, ActivityKind.Internal, parentId: default, tags: new ActivityTagsCollection
                    {
                        ["xunit.testclass.id"] = testClass.UniqueID,
                        ["xunit.testclass.name"] = testClass.TestClassSimpleName,
                        ["xunit.testclass.namespace"] = testClass.TestClassNamespace,
                    });
                    return;
                }

            case EventIds.TestAssemblyStart:
                {
                    var testAssembly = context!.TestAssembly!;
                    var activityName = $"TestAssembly({testAssembly.SimpleAssemblyName()})";
                    Activity.Current = ActivitySource.StartActivity(activityName, ActivityKind.Internal, parentId: default, tags: new ActivityTagsCollection
                    {
                        ["xunit.testassembly.id"] = testAssembly.UniqueID,
                        ["xunit.testassembly.name"] = testAssembly.SimpleAssemblyName(),
                    });
                    return;
                }
            default:
                break;
        }
    }

    private void HandleStopEvent(EventWrittenEventArgs eventData)
    {
        using var activity = Activity.Current;

        if (activity == null)
            return;

        switch (eventData.EventId)
        {
            case EventIds.TestStop:
                {
                    var context = TestContext.Current;
                    var testState = context.TestState;
                    activity.AddTag(AttributeNames.TestCaseResultStatus, testState?.Result.ToTestCaseResultStatus());

                    switch (testState?.Result)
                    {
                        case TestResult.Failed:
                            var errorMessage = ExtractErrorMessage(context.TestState!);
                            activity.SetStatus(ActivityStatusCode.Error, description: errorMessage);
                            break;
                        default:
                            break;
                    }

                    return;
                }
            case EventIds.TestCaseStop:
                {
                    // TODO: Need to handle Skipped/NotRun result (Currently it's not set on TestCaseStop event and Test event is not raised)
                    var context = TestContext.Current;
                    var testState = context.TestState;

                    //if (testState?.Result == TestResult.Skipped)
                    //{
                    //      activity.AddEvent(new ActivityEvent("Skipped") { });
                    //}
                    return;
                }

            case EventIds.TestMethodStop:
            case EventIds.TestClassStop:
            case EventIds.TestCollectionStop:
            case EventIds.TestAssemblyStop:
            default:
                break;
        }
    }

    private string? ExtractErrorMessage(TestResultState testResultState)
    {
        if (testResultState.Result != TestResult.Failed)
            return null;

        StringBuilder sb = new();
        sb.AppendLine($"FailureCause: {testResultState.FailureCause}");

        // TODO: Handle inner exceptions.
        int index = 0;
        sb.AppendLine($"{testResultState.ExceptionTypes![index]}: {testResultState.ExceptionMessages![index]}");
        sb.AppendLine("Stack Trace:");
        sb.AppendLine(testResultState.ExceptionStackTraces![index]);

        return sb.ToString();
    }



    public override void Dispose()
    {
        base.Dispose();
    }

    private static class AttributeNames
    {
        // Use Semantic conventions for Test (https://opentelemetry.io/docs/specs/semconv/registry/attributes/test/)

        /// <summary>
        /// The fully qualified human readable name of the test case.
        /// </summary>
        public const string TestCaseName = "test.case.name";

        /// <summary>
        /// The status of the actual test case result from test execution.
        /// </summary>
        public const string TestCaseResultStatus = "test.case.result.status";

        /// <summary>
        /// The human readable name of a test suite.
        /// </summary>
        public const string TestSuiteName = "test.suite.name";

        /// <summary>
        /// The status of the test suite run.
        /// </summary>
        public const string TestSuiteRunStatus = "test.suite.run.status";
    }

    ////private static class Tasks
    ////{
    ////    public const EventTask Test = (EventTask)1;
    ////    public const EventTask TestCase = (EventTask)2;
    ////    public const EventTask TestMethod = (EventTask)3;
    ////    public const EventTask TestClass = (EventTask)4;
    ////    public const EventTask TestCollection = (EventTask)5;
    ////    public const EventTask TestAssembly = (EventTask)6;
    ////}

    private static class EventIds
    {
        public const int TestStart = 1;
        public const int TestStop = 2;

        public const int TestCaseStart = 11;
        public const int TestCaseStop = 12;

        public const int TestMethodStart = 21;
        public const int TestMethodStop = 22;

        public const int TestClassStart = 31;
        public const int TestClassStop = 32;

        public const int TestCollectionStart = 41;
        public const int TestCollectionStop = 42;

        public const int TestAssemblyStart = 51;
        public const int TestAssemblyStop = 52;
    }

}
