using Microsoft.Extensions.DependencyInjection;
using xUnitV3LoadFramework.Attributes;
using xUnitV3LoadFramework.Extensions;
using Xunit;

namespace xUnitV3LoadFramework.Tests.Integration;

/// <summary>
/// Core integration tests for Load attribute functionality.
/// Tests the essential end-to-end execution of load tests.
/// </summary>
public class LoadIntegrationTests : xUnitV3LoadTests.TestSetup
{
    /// <summary>
    /// Tests basic Load functionality with simulated async operations.
    /// </summary>
    [Load(concurrency: 2, duration: 2000, interval: 500)]
    public async Task Load_Should_Execute_Async_Operations_Successfully()
    {
        var result = await LoadTestRunner.ExecuteAsync(async () =>
        {
            // Simulate async operation with variable latency (similar to HTTP request)
            var latency = Random.Shared.Next(50, 150);
            await Task.Delay(latency);

            // Simulate success rate similar to real HTTP calls
            if (Random.Shared.Next(100) < 98) // 98% success rate
            {
                return true;
            }

            // Simulate occasional failures
            throw new InvalidOperationException("Simulated operation failure");
        });

        // Assert core functionality
        Assert.True(result.Success > 0, "Load test should have successful executions");
        Assert.True(result.Total > 0, "Load test should have executed at least once");
        Assert.True(result.Time > 0, "Should record execution time");
    }

    /// <summary>
    /// Tests Load with data processing simulation.
    /// </summary>
    [Load(concurrency: 2, duration: 1500, interval: 400)]
    public async Task Load_Should_Process_Data_Successfully()
    {
        var processedCount = 0;

        var result = await LoadTestRunner.ExecuteAsync(async () =>
        {
            // Simulate data processing with async operations
            await Task.Delay(Random.Shared.Next(20, 80));

            // Simulate data generation and validation
            var data = new
            {
                userId = Random.Shared.Next(1, 10),
                id = Guid.NewGuid(),
                title = $"Title_{Random.Shared.Next(1000)}",
                body = $"Body content {DateTime.UtcNow.Ticks}"
            };

            // Simulate data validation
            await Task.Delay(10);

            if (data.userId > 0 && !string.IsNullOrEmpty(data.title))
            {
                Interlocked.Increment(ref processedCount);
                return true;
            }

            return false;
        });

        Assert.True(result.Success > 0, "Data processing should succeed");
        Assert.True(result.AverageLatency > 0, "Should record latency metrics");
        Assert.True(processedCount > 0, "Should have processed data items");
    }
}