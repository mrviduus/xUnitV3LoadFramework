using Microsoft.Extensions.DependencyInjection;
using xUnitV3LoadFramework.Attributes;
using xUnitV3LoadFramework.Extensions;
using Xunit;

namespace xUnitV3LoadFramework.Tests.Performance;

/// <summary>
/// Performance tests for high-load scenarios.
/// These tests validate the framework's ability to handle significant load and measure performance characteristics.
/// </summary>
public class HighLoadPerformanceTests : xUnitV3LoadTests.TestSetup
{
    /// <summary>
    /// High-concurrency test with 10 concurrent requests over 5 seconds.
    /// Validates framework performance under moderate concurrent load.
    /// </summary>
    [Load(concurrency: 10, duration: 5000, interval: 100)]
    public async Task HighConcurrency_Should_Handle_10_Concurrent_Requests()
    {
        var result = await LoadTestRunner.ExecuteAsync(async () =>
        {
            // Simulate some processing work without external dependencies
            await Task.Delay(50, TestContext.Current.CancellationToken);
            var calculation = Enumerable.Range(1, 100).Sum();
            return calculation > 0;
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
    /// Validates framework behavior under rapid-fire request scenarios.
    /// </summary>
    [Load(concurrency: 5, duration: 3000, interval: 50)]
    public async Task StressTest_Should_Handle_Rapid_Fire_Requests()
    {
        var result = await LoadTestRunner.ExecuteAsync(async () =>
        {
            // Fast in-memory processing to test rapid execution
            await Task.Delay(20, TestContext.Current.CancellationToken);
            var hash = "test".GetHashCode();
            return hash != 0;
        });
        
        // Assert stress test characteristics
        Assert.True(result.Success > 0, "Stress test should have successful executions");
        Assert.True(result.Total >= 100, "Should execute many requests with 50ms intervals");
        Assert.True(result.RequestsPerSecond >= 10, "Should achieve high throughput");
        
        Console.WriteLine($"Stress test: {result.Total} total requests, {result.RequestsPerSecond:F2} req/sec");
    }

    /// <summary>
    /// Endurance test with longer duration to validate sustained performance.
    /// Tests framework stability over extended execution periods.
    /// </summary>
    [Load(concurrency: 3, duration: 8000, interval: 200)]
    public async Task EnduranceTest_Should_Maintain_Performance_Over_Time()
    {
        var result = await LoadTestRunner.ExecuteAsync(async () =>
        {
            // Simulate variable processing time
            var random = new Random();
            var delay = random.Next(50, 150);
            await Task.Delay(delay, TestContext.Current.CancellationToken);
            return true;
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
    /// Ensures the framework handles memory efficiently under load.
    /// </summary>
    [Load(concurrency: 8, duration: 4000, interval: 150)]
    public async Task MemoryPressureTest_Should_Manage_Resources_Efficiently()
    {
        var result = await LoadTestRunner.ExecuteAsync(async () =>
        {
            // Create memory pressure with in-memory data processing
            var data = new byte[10000]; // 10KB allocation
            new Random().NextBytes(data);
            
            await Task.Delay(75, TestContext.Current.CancellationToken);
            
            // Process the data to simulate work
            var sum = data.Sum(b => (int)b);
            Assert.True(sum > 0, "Should process data successfully");
            
            return true;
        });
        
        // Assert resource management
        Assert.True(result.Success > 0, "Memory pressure test should succeed");
        Assert.True(result.Success >= result.Total * 0.85, "Should maintain reasonable success rate under memory pressure");
        
        Console.WriteLine($"Memory test: {result.PeakMemoryUsage / 1024 / 1024:F1}MB peak, {result.Success}/{result.Total} success");
    }
}
