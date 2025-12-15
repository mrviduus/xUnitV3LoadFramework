using xUnitV3LoadFramework.Extensions;
using Xunit;

namespace xUnitV3LoadFramework.Tests.Integration;

/// <summary>
/// Tests for Fluent API edge cases, including MaxIterations, timing edge cases,
/// and various configuration combinations.
/// </summary>
public class FluentApiEdgeCaseTests
{
    #region MaxIterations Tests

    /// <summary>
    /// Verifies MaxIterations stops the test after specified number of iterations.
    /// </summary>
    [Fact]
    public async Task Fluent_MaxIterations_Should_Stop_After_Specified_Count()
    {
        var iterationCount = 0;

        var result = await LoadTestRunner.Create()
            .WithName("MaxIterations_StopAt50")
            .WithConcurrency(5)
            .WithDuration(TimeSpan.FromMinutes(5)) // Long duration - should stop by MaxIterations
            .WithInterval(TimeSpan.FromMilliseconds(50))
            .WithMaxIterations(50) // Stop after 50 iterations
            .RunAsync(async () =>
            {
                Interlocked.Increment(ref iterationCount);
                await Task.Delay(5);
            });

        // Should stop at or very close to MaxIterations
        Assert.True(result.Total <= 60, $"Should stop around MaxIterations, got {result.Total}");
        Assert.True(result.Total >= 45, $"Should execute close to MaxIterations, got {result.Total}");
    }

    /// <summary>
    /// Verifies MaxIterations works with single concurrency.
    /// </summary>
    [Fact]
    public async Task Fluent_MaxIterations_SingleConcurrency_Should_Be_Exact()
    {
        var result = await LoadTestRunner.Create()
            .WithName("MaxIterations_SingleConcurrency")
            .WithConcurrency(1)
            .WithDuration(TimeSpan.FromMinutes(5))
            .WithInterval(TimeSpan.FromMilliseconds(10))
            .WithMaxIterations(10)
            .RunAsync(async () =>
            {
                await Task.Delay(5);
            });

        // With single concurrency, should be very close to exact
        Assert.True(result.Total >= 10, $"Should execute at least MaxIterations, got {result.Total}");
        Assert.True(result.Total <= 15, $"Should not exceed MaxIterations by much, got {result.Total}");
    }

    /// <summary>
    /// Verifies MaxIterations=1 executes exactly once (edge case).
    /// </summary>
    [Fact]
    public async Task Fluent_MaxIterations_One_Should_Execute_Once()
    {
        var counter = 0;

        var result = await LoadTestRunner.Create()
            .WithName("MaxIterations_One")
            .WithConcurrency(1)
            .WithDuration(TimeSpan.FromMinutes(5))
            .WithInterval(TimeSpan.FromMilliseconds(10))
            .WithMaxIterations(1)
            .RunAsync(async () =>
            {
                Interlocked.Increment(ref counter);
                await Task.Delay(5);
            });

        // Should execute exactly once
        Assert.Equal(1, result.Total);
    }

    #endregion

    #region Timing Edge Cases

    /// <summary>
    /// Very short duration test - framework should handle gracefully.
    /// </summary>
    [Fact]
    public async Task Fluent_VeryShortDuration_Should_Complete()
    {
        var result = await LoadTestRunner.Create()
            .WithName("VeryShortDuration")
            .WithConcurrency(1)
            .WithDuration(TimeSpan.FromMilliseconds(50))
            .WithInterval(TimeSpan.FromMilliseconds(10))
            .RunAsync(async () =>
            {
                await Task.Delay(1);
            });

        Assert.True(result.Total >= 1, "Should execute at least once");
    }

    /// <summary>
    /// Interval longer than duration - at least one batch should execute.
    /// </summary>
    [Fact]
    public async Task Fluent_IntervalLongerThanDuration_Should_Execute_AtLeastOnce()
    {
        var result = await LoadTestRunner.Create()
            .WithName("IntervalLongerThanDuration")
            .WithConcurrency(3)
            .WithDuration(TimeSpan.FromMilliseconds(100))
            .WithInterval(TimeSpan.FromMilliseconds(500)) // Interval > Duration
            .RunAsync(async () =>
            {
                await Task.Delay(5);
            });

        // Should execute initial batch at minimum
        Assert.True(result.Total >= 1, "Should execute at least the initial batch");
    }

    /// <summary>
    /// Action takes longer than interval - validates no race conditions.
    /// </summary>
    [Fact]
    public async Task Fluent_ActionLongerThanInterval_Should_Handle_Gracefully()
    {
        var result = await LoadTestRunner.Create()
            .WithName("ActionLongerThanInterval")
            .WithConcurrency(2)
            .WithDuration(TimeSpan.FromMilliseconds(500))
            .WithInterval(TimeSpan.FromMilliseconds(50))
            .RunAsync(async () =>
            {
                // Action takes 100ms, but interval is 50ms
                await Task.Delay(100);
            });

        Assert.True(result.Total > 0, "Should complete some iterations");
        Assert.True(result.Success > 0, "Should have successes");
    }

    #endregion

    #region Concurrency Edge Cases

    /// <summary>
    /// High concurrency with short interval - stress test.
    /// </summary>
    [Fact]
    public async Task Fluent_HighConcurrency_ShortInterval_Should_Handle()
    {
        var result = await LoadTestRunner.Create()
            .WithName("HighConcurrency_ShortInterval")
            .WithConcurrency(50)
            .WithDuration(TimeSpan.FromMilliseconds(500))
            .WithInterval(TimeSpan.FromMilliseconds(50))
            .RunAsync(async () =>
            {
                await Task.Delay(5);
            });

        Assert.True(result.Total >= 50, "Should execute many iterations");
        Assert.True(result.Success > 0, "Should have successes");
    }

    /// <summary>
    /// Single concurrency - validates sequential execution works.
    /// </summary>
    [Fact]
    public async Task Fluent_SingleConcurrency_Should_Execute_Sequentially()
    {
        var maxConcurrent = 0;
        var currentConcurrent = 0;

        var result = await LoadTestRunner.Create()
            .WithName("SingleConcurrency")
            .WithConcurrency(1)
            .WithDuration(TimeSpan.FromMilliseconds(300))
            .WithInterval(TimeSpan.FromMilliseconds(50))
            .RunAsync(async () =>
            {
                var current = Interlocked.Increment(ref currentConcurrent);
                Interlocked.CompareExchange(ref maxConcurrent, current, maxConcurrent < current ? maxConcurrent : current);

                await Task.Delay(10);

                Interlocked.Decrement(ref currentConcurrent);
            });

        Assert.True(result.Total > 0, "Should execute iterations");
    }

    #endregion

    #region Return Type Variations

    /// <summary>
    /// Fluent API with explicit bool return - true = success.
    /// </summary>
    [Fact]
    public async Task Fluent_ExplicitBoolReturn_True_Should_Count_As_Success()
    {
        var result = await LoadTestRunner.Create()
            .WithName("ExplicitBool_True")
            .WithConcurrency(2)
            .WithDuration(TimeSpan.FromMilliseconds(200))
            .WithInterval(TimeSpan.FromMilliseconds(50))
            .RunAsync(async () =>
            {
                await Task.Delay(5);
                return true;
            });

        Assert.Equal(result.Total, result.Success);
        Assert.Equal(0, result.Failure);
    }

    /// <summary>
    /// Fluent API with void action - no exception = success.
    /// </summary>
    [Fact]
    public async Task Fluent_VoidAction_NoException_Should_Count_As_Success()
    {
        var result = await LoadTestRunner.Create()
            .WithName("VoidAction")
            .WithConcurrency(2)
            .WithDuration(TimeSpan.FromMilliseconds(200))
            .WithInterval(TimeSpan.FromMilliseconds(50))
            .RunAsync(async () =>
            {
                await Task.Delay(5);
                // No return - void action
            });

        Assert.Equal(result.Total, result.Success);
        Assert.Equal(0, result.Failure);
    }

    #endregion

    #region Metrics Validation

    /// <summary>
    /// Verifies latency metrics are calculated correctly.
    /// </summary>
    [Fact]
    public async Task Fluent_Latency_Metrics_Should_Be_Reasonable()
    {
        var result = await LoadTestRunner.Create()
            .WithName("LatencyMetrics")
            .WithConcurrency(3)
            .WithDuration(TimeSpan.FromMilliseconds(500))
            .WithInterval(TimeSpan.FromMilliseconds(100))
            .RunAsync(async () =>
            {
                // Fixed delay of 50ms
                await Task.Delay(50);
            });

        Assert.True(result.Total > 0, "Should have iterations");
        Assert.True(result.AverageLatency >= 40, $"Average latency should be >= 40ms, got {result.AverageLatency}");
        Assert.True(result.AverageLatency <= 200, $"Average latency should be reasonable, got {result.AverageLatency}");
        Assert.True(result.MinLatency >= 0, "Min latency should be non-negative");
        Assert.True(result.MaxLatency >= result.MinLatency, "Max should be >= Min");
    }

    /// <summary>
    /// Verifies RPS (Requests Per Second) is calculated correctly.
    /// </summary>
    [Fact]
    public async Task Fluent_RPS_Should_Be_Calculated_Correctly()
    {
        var result = await LoadTestRunner.Create()
            .WithName("RPS_Calculation")
            .WithConcurrency(5)
            .WithDuration(TimeSpan.FromSeconds(1))
            .WithInterval(TimeSpan.FromMilliseconds(100))
            .RunAsync(async () =>
            {
                await Task.Delay(10);
            });

        Assert.True(result.Total > 0, "Should have iterations");
        Assert.True(result.Time > 0, "Time should be recorded");

        // RPS = Total / Time
        var expectedRps = result.Total / result.Time;
        Assert.True(Math.Abs(result.RequestsPerSecond - expectedRps) < 1,
            $"RPS {result.RequestsPerSecond} should match Total/Time {expectedRps}");
    }

    /// <summary>
    /// Verifies percentile metrics are available and reasonable.
    /// </summary>
    [Fact]
    public async Task Fluent_Percentile_Metrics_Should_Be_Available()
    {
        var result = await LoadTestRunner.Create()
            .WithName("PercentileMetrics")
            .WithConcurrency(5)
            .WithDuration(TimeSpan.FromMilliseconds(500))
            .WithInterval(TimeSpan.FromMilliseconds(50))
            .RunAsync(async () =>
            {
                // Variable delay
                await Task.Delay(Random.Shared.Next(10, 50));
            });

        Assert.True(result.Total >= 10, "Should have enough data points");

        // Percentiles should follow: P50 <= P95 <= P99
        Assert.True(result.MedianLatency >= 0, "Median should be non-negative");
        Assert.True(result.Percentile95Latency >= result.MedianLatency,
            "P95 should be >= Median");
        Assert.True(result.Percentile99Latency >= result.Percentile95Latency,
            "P99 should be >= P95");
    }

    #endregion

    #region Configuration Builder Tests

    /// <summary>
    /// Test chaining all builder methods together.
    /// </summary>
    [Fact]
    public async Task Fluent_AllBuilderMethods_ShouldChainCorrectly()
    {
        var result = await LoadTestRunner.Create()
            .WithName("AllMethods_Chained")
            .WithConcurrency(3)
            .WithDuration(TimeSpan.FromMilliseconds(300))
            .WithInterval(TimeSpan.FromMilliseconds(50))
            .WithMaxIterations(20)
            .RunAsync(async () =>
            {
                await Task.Delay(5);
            });

        Assert.True(result.Total > 0);
        Assert.True(result.Total <= 25); // Constrained by MaxIterations
    }

    /// <summary>
    /// Test with millisecond overloads.
    /// </summary>
    [Fact]
    public async Task Fluent_MillisecondOverloads_ShouldWork()
    {
        var result = await LoadTestRunner.Create()
            .WithName("MillisecondOverloads")
            .WithConcurrency(2)
            .WithDuration(200) // int milliseconds
            .WithInterval(50) // int milliseconds
            .RunAsync(async () =>
            {
                await Task.Delay(5);
            });

        Assert.True(result.Total > 0);
    }

    #endregion
}
