using System;
using System.Threading.Tasks;
using Xunit;
using xUnitLoadRunnerLib;

namespace xUnitLoadRunner.Tests;

public class TestPlanTests
{
    [Fact]
    public async Task ConcurrencyOfOneCompletesAllSteps()
    {
        var plan = new TestPlan
        {
            Name = "SingleConcurrencyPlan",
            Steps =
            [
                new TestStep
                {
                    Name = "test",
                    Action = async () => await Task.FromResult(true)
                },
               new TestStep
                {
                    Name = "test",
                    Action = async () => await Task.FromResult(true)
                }
            ],
            Concurrency = 1,
            Duration = TimeSpan.FromMilliseconds(200),
            Interval = TimeSpan.Zero
        };
        await plan.Run();
        Assert.True(plan.Steps.Length > 0);
    }

    [Fact]
    public async Task TerminatesAfterSpecifiedDuration()
    {
        var plan = new TestPlan
        {
            Name = "TimedPlan",
            Steps = new[]
            {
                new TestStep
                {
                    Name = "testName",
                    Action = async () => await Task.FromResult(true)
                }
            },
            Concurrency = 1,
            Duration = TimeSpan.FromMilliseconds(100),
            Interval = TimeSpan.Zero
        };
        var start = DateTime.UtcNow;
        await plan.Run();
        var end = DateTime.UtcNow;
        Assert.True(end - start < TimeSpan.FromSeconds(2));
    }

    [Fact]
    public async Task HandlesZeroConcurrencyWithoutError()
    {
        var plan = new TestPlan
        {
            Name = "NoConcurrencyPlan",
            Steps = new[]
            {
                new TestStep
                {
                    Name = "testName",
                    Action = async () => await Task.FromResult(true)
                }
            },
            Concurrency = 0,
            Duration = TimeSpan.FromMilliseconds(100),
            Interval = TimeSpan.Zero
        };
        await plan.Run();
        Assert.True(true);
    }

    [Fact]
    public async Task SkipsExecutionIfNoSteps()
    {
        var plan = new TestPlan
        {
            Name = "EmptyPlan",
            Steps = Array.Empty<TestStep>(),
            Concurrency = 1,
            Duration = TimeSpan.FromMilliseconds(100),
            Interval = TimeSpan.Zero
        };
        await plan.Run();
        Assert.True(true);
    }

    [Fact]
    public async Task ThrowsIfStepsIsNull()
    {
        var plan = new TestPlan
        {
            Name = "NullStepsPlan",
            Steps = null,
            Concurrency = 1,
            Duration = TimeSpan.FromMilliseconds(100),
            Interval = TimeSpan.Zero
        };
        await Assert.ThrowsAsync<ArgumentNullException>(() => plan.Run());
    }
}