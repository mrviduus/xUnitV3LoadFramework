using xUnitV3LoadFramework.Attributes;

namespace xUnitV3LoadFramework.Tests.Integration;

/// <summary>
/// Core integration tests for Load attribute functionality.
/// Tests the essential end-to-end execution of load tests using native approach.
/// The test method body becomes the action - no manual LoadTestRunner.ExecuteAsync() call needed.
/// </summary>
public class LoadIntegrationTests
{
    /// <summary>
    /// Tests basic Load functionality with simulated async operations.
    /// Native approach: method body runs under load automatically.
    /// </summary>
    [Load(concurrency: 2, duration: 2000, interval: 500)]
    public async Task Load_Should_Execute_Async_Operations_Successfully()
    {
        // Method body IS the action - runs N times under load automatically
        var latency = Random.Shared.Next(50, 150);
        await Task.Delay(latency);

        // Pass if no exception, fail if exception thrown
    }

    /// <summary>
    /// Tests Load with data processing simulation.
    /// Native approach: method body runs under load automatically.
    /// </summary>
    [Load(concurrency: 2, duration: 1500, interval: 400)]
    public async Task Load_Should_Process_Data_Successfully()
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

        // Validation - throws if invalid (native approach: exception = failure)
        if (data.userId <= 0 || string.IsNullOrEmpty(data.title))
        {
            throw new InvalidOperationException("Data validation failed");
        }
    }
}