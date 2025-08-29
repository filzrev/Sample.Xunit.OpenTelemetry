using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Sample.Xunit.OpenTelemetry;


// CollectionTests01/CollectionTests02 should be executed in parallel.
[Collection("CollectionA")]
public class CollectionTests01 : TestBase
{
    [Fact]
    public async Task Test01()
    {
        await Task.Delay(100, CancellationToken);
    }
}

[Collection("CollectionB")]
public class CollectionTests02 : TestBase
{
    [Fact]
    public async Task Test01()
    {
        await Task.Delay(100, CancellationToken);
    }
}

// Note:
// Don't specify name of CollecionDefinition when specify collection by type.
// When name is specified. DisableParallelization seems not works as expected.
[CollectionDefinition(DisableParallelization = true)]
public class NonParallelCollection // : ICollectionFixture<SampleFixture>
{
}

// CollectionTests03/CollectionTests04 should be executed separately.
[Collection(typeof(NonParallelCollection))]
public class CollectionTests03 : TestBase
{
    [Fact]
    public async Task Test01()
    {
        await Task.Delay(100, CancellationToken);
    }
}

[Collection(typeof(NonParallelCollection))]
public class CollectionTests04 : TestBase
{
    [Fact]
    public async Task Test01()
    {
        await Task.Delay(100, CancellationToken);
    }
}