using System.Diagnostics;
using System.Reflection;
using Xunit.v3;

namespace Sample.Xunit.OpenTelemetry;

/// <summary>
/// Custom BeforeAfterTestAttribute that record activity before/after test.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class TraceBeforeAfterTestAttribute : BeforeAfterTestAttribute
{
    public override void Before(MethodInfo methodUnderTest, IXunitTest test)
    {
        Activity.Current?.AddEvent(new ActivityEvent("BeforeAfterTestAttribute::Before"));
    }

    public override void After(MethodInfo methodUnderTest, IXunitTest test)
    {
        Activity.Current?.AddEvent(new ActivityEvent("BeforeAfterTestAttribute::After"));
    }
}
