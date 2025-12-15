using xUnitV3LoadFramework.Extensions;
using Xunit;

namespace xUnitV3LoadFramework.Tests.Performance;

/// <summary>
/// Performance tests for high-load scenarios using Fluent API.
/// These tests require metric assertions, so they use [Fact] + LoadTestRunner.Create() fluent API
/// instead of native [Load] attribute.
/// </summary>
public class HighLoadPerformanceTests
{
    /// <summary>
    /// High-concurrency test with 10 concurrent requests over 5 seconds.
    /// Uses Fluent API to access LoadResult metrics for assertions.
    /// </summary>
    [Fact]
    public async Task HighConcurrency_Should_Handle_10_Concurrent_Requests()
    {
        var result = await LoadTestRunner.Create()
            .WithName("HighConcurrency_Test")
            .WithConcurrency(10)
            .WithDuration(TimeSpan.FromMilliseconds(5000))
            .WithInterval(TimeSpan.FromMilliseconds(100))
            .RunAsync(async () =>
            {
                // Simulate some processing work without external dependencies
                await Task.Delay(50);
                var calculation = Enumerable.Range(1, 100).Sum();
                if (calculation <= 0) throw new InvalidOperationException("Calculation failed");
            });

        // Assert performance characteristics
        Assert.True(result.Success > 0, "High concurrency test should have successful executions");
        Assert.True(result.Total >= 50, "Should execute at least 50 requests over 5 seconds");
        Assert.True(result.Success >= result.Total * 0.95, "At least 95% success rate expected");
        Assert.True(result.RequestsPerSecond >= 5, "Should achieve at least 5 requests per second");

        Console.WriteLine($"High concurrency test: {result.RequestsPerSecond:F2} req/sec, {result.Success}/{result.Total} success");
    }

    /// <summary>
    /// Stress test with very short intervals to test rapid execution.
    /// Uses Fluent API to access LoadResult metrics for assertions.
    /// </summary>
    [Fact]
    public async Task StressTest_Should_Handle_Rapid_Fire_Requests()
    {
        var result = await LoadTestRunner.Create()
            .WithName("StressTest_RapidFire")
            .WithConcurrency(5)
            .WithDuration(TimeSpan.FromMilliseconds(3000))
            .WithInterval(TimeSpan.FromMilliseconds(50))
            .RunAsync(async () =>
            {
                // Fast in-memory processing to test rapid execution
                await Task.Delay(20);
                var hash = "test".GetHashCode();
                if (hash == 0) throw new InvalidOperationException("Hash failed");
            });

        // Assert stress test characteristics
        Assert.True(result.Success > 0, "Stress test should have successful executions");
        Assert.True(result.Total >= 100, "Should execute many requests with 50ms intervals");
        Assert.True(result.RequestsPerSecond >= 10, "Should achieve high throughput");

        Console.WriteLine($"Stress test: {result.Total} total requests, {result.RequestsPerSecond:F2} req/sec");
    }

    /// <summary>
    /// Endurance test with longer duration to validate sustained performance.
    /// Uses Fluent API to access LoadResult metrics for assertions.
    /// </summary>
    [Fact]
    public async Task EnduranceTest_Should_Maintain_Performance_Over_Time()
    {
        var result = await LoadTestRunner.Create()
            .WithName("EnduranceTest")
            .WithConcurrency(3)
            .WithDuration(TimeSpan.FromMilliseconds(8000))
            .WithInterval(TimeSpan.FromMilliseconds(200))
            .RunAsync(async () =>
            {
                // Simulate variable processing time
                var delay = Random.Shared.Next(50, 150);
                await Task.Delay(delay);
            });

        // Assert endurance characteristics
        Assert.True(result.Success > 0, "Endurance test should complete successfully");
        Assert.True(result.Time >= 7.5, "Should run for approximately 8 seconds");
        Assert.True(result.Success >= result.Total * 0.90, "Should maintain 90% success rate over time");
        Assert.InRange(result.AverageLatency, 50, 300); // Variable delay + processing overhead

        Console.WriteLine($"Endurance test: {result.Time:F1}s duration, {result.AverageLatency:F0}ms avg latency");
    }

    /// <summary>
    /// Memory pressure test to validate resource management.
    /// Uses Fluent API to access LoadResult metrics for assertions.
    /// </summary>
    [Fact]
    public async Task MemoryPressureTest_Should_Manage_Resources_Efficiently()
    {
        var result = await LoadTestRunner.Create()
            .WithName("MemoryPressureTest")
            .WithConcurrency(8)
            .WithDuration(TimeSpan.FromMilliseconds(4000))
            .WithInterval(TimeSpan.FromMilliseconds(150))
            .RunAsync(async () =>
            {
                // Create memory pressure with in-memory data processing
                var data = new byte[10000]; // 10KB allocation
                Random.Shared.NextBytes(data);

                await Task.Delay(75);

                // Process the data to simulate work
                var sum = data.Sum(b => (int)b);
                if (sum <= 0) throw new InvalidOperationException("Data processing failed");
            });

        // Assert resource management
        Assert.True(result.Success > 0, "Memory pressure test should succeed");
        Assert.True(result.Success >= result.Total * 0.85, "Should maintain reasonable success rate under memory pressure");

        Console.WriteLine($"Memory test: {result.PeakMemoryUsage / 1024 / 1024:F1}MB peak, {result.Success}/{result.Total} success");
    }
}
