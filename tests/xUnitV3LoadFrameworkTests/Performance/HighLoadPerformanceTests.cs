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
    [LoadFact(order: 1, concurrency: 10, duration: 5000, interval: 100)]
    public async Task HighConcurrency_Should_Handle_10_Concurrent_Requests()
    {
        var result = await LoadTestHelper.ExecuteLoadTestAsync(async () =>
        {
            var httpClient = GetService<IHttpClientFactory>().CreateClient();
            var response = await httpClient.GetAsync("https://httpbin.org/status/200", TestContext.Current.CancellationToken);
            response.EnsureSuccessStatusCode();
            return true;
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
    [LoadFact(order: 2, concurrency: 5, duration: 3000, interval: 50)]
    public async Task StressTest_Should_Handle_Rapid_Fire_Requests()
    {
        var result = await LoadTestHelper.ExecuteLoadTestAsync(async () =>
        {
            var httpClient = GetService<IHttpClientFactory>().CreateClient();
            var response = await httpClient.GetAsync("https://httpbin.org/status/200", TestContext.Current.CancellationToken);
            response.EnsureSuccessStatusCode();
            return true;
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
    [LoadFact(order: 3, concurrency: 3, duration: 8000, interval: 200)]
    public async Task EnduranceTest_Should_Maintain_Performance_Over_Time()
    {
        var result = await LoadTestHelper.ExecuteLoadTestAsync(async () =>
        {
            var httpClient = GetService<IHttpClientFactory>().CreateClient();
            var response = await httpClient.GetAsync("https://httpbin.org/delay/0.1", TestContext.Current.CancellationToken);
            response.EnsureSuccessStatusCode();
            return true;
        });
        
        // Assert endurance characteristics
        Assert.True(result.Success > 0, "Endurance test should complete successfully");
        Assert.True(result.Time >= 7.5, "Should run for approximately 8 seconds");
        Assert.True(result.Success >= result.Total * 0.90, "Should maintain 90% success rate over time");
        Assert.InRange(result.AverageLatency, 100, 2000); // 0.1s delay + network overhead
        
        Console.WriteLine($"Endurance test: {result.Time:F1}s duration, {result.AverageLatency:F0}ms avg latency");
    }

    /// <summary>
    /// Memory pressure test to validate resource management.
    /// Ensures the framework handles memory efficiently under load.
    /// </summary>
    [LoadFact(order: 4, concurrency: 8, duration: 4000, interval: 150)]
    public async Task MemoryPressureTest_Should_Manage_Resources_Efficiently()
    {
        var result = await LoadTestHelper.ExecuteLoadTestAsync(async () =>
        {
            var httpClient = GetService<IHttpClientFactory>().CreateClient();
            
            // Create some memory pressure with larger responses
            var response = await httpClient.GetAsync("https://jsonplaceholder.typicode.com/posts", TestContext.Current.CancellationToken);
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync();
            Assert.True(content.Length > 1000, "Should receive substantial response data");
            
            return true;
        });
        
        // Assert resource management
        Assert.True(result.Success > 0, "Memory pressure test should succeed");
        Assert.True(result.PeakMemoryUsage > 0, "Should record memory usage");
        Assert.True(result.Success >= result.Total * 0.85, "Should maintain reasonable success rate under memory pressure");
        
        Console.WriteLine($"Memory test: {result.PeakMemoryUsage / 1024 / 1024:F1}MB peak, {result.Success}/{result.Total} success");
    }
}