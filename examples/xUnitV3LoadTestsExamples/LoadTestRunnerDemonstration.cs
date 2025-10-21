using xUnitV3LoadFramework.Attributes;
using xUnitV3LoadFramework.Extensions;
using Xunit;

namespace xUnitV3LoadFramework.Examples;

/// <summary>
/// Demonstrates the enhanced LoadTestRunner with fluent API capabilities.
/// Shows both traditional Load attribute usage and the new fluent API approach.
/// All examples use self-contained simulated workloads without external dependencies.
/// </summary>
public class LoadTestRunnerDemonstration
{
    /// <summary>
    /// Traditional approach using Load attribute with LoadTestRunner.ExecuteAsync
    /// Simulates a database operation without actual database connection
    /// </summary>
    [Load(concurrency: 5, duration: 3000, interval: 200)]
    public async Task Traditional_LoadTest_With_LoadTestRunner()
    {
        var result = await LoadTestRunner.ExecuteAsync(async () =>
        {
            // Simulate database query with variable latency
            var latency = Random.Shared.Next(10, 50);
            await Task.Delay(latency);

            // Simulate 95% success rate
            if (Random.Shared.Next(100) < 95)
            {
                return true;
            }

            // Simulate occasional failures
            return false;
        });

        // Assert performance expectations
        Assert.True(result.Success > 0, "Should have successful executions");
        Assert.True(result.RequestsPerSecond > 0, "Should achieve measurable throughput");

        Console.WriteLine($"Traditional LoadTest completed: {result.Success}/{result.Total} success, {result.RequestsPerSecond:F2} req/sec");
    }

    /// <summary>
    /// Simplified approach using LoadTestRunner.RunAsync
    /// Simulates a cache operation
    /// </summary>
    [Load(concurrency: 3, duration: 2000, interval: 300)]
    public async Task Simplified_LoadTest_With_RunAsync()
    {
        var cache = new Dictionary<string, string>();
        var hitCount = 0;

        var result = await LoadTestRunner.RunAsync(async () =>
        {
            var key = $"item_{Random.Shared.Next(1, 10)}";

            // Simulate cache check
            await Task.Delay(5);

            if (cache.TryGetValue(key, out var value))
            {
                Interlocked.Increment(ref hitCount);
            }
            else
            {
                // Simulate cache miss - add to cache
                cache[key] = $"value_{Guid.NewGuid()}";
            }

            // Success is implicit if no exception
        });

        Assert.True(result.Success > 0, "Should have successful executions");
        Console.WriteLine($"Simplified LoadTest completed: {result.Success}/{result.Total} success, Cache hits: {hitCount}");
    }

    /// <summary>
    /// Fluent API approach for dynamic configuration
    /// Simulates message processing workload
    /// </summary>
    [Fact]
    public async Task FluentAPI_LoadTest_Basic()
    {
        var processedMessages = 0;

        var result = await LoadTestRunner.Create()
            .WithConcurrency(4)
            .WithDuration(2500)
            .WithInterval(150)
            .WithName("FluentAPI_Basic_Test")
            .RunAsync(async () =>
            {
                // Simulate message processing
                await Task.Delay(Random.Shared.Next(20, 40));

                // Simulate processing logic
                var messageId = Guid.NewGuid();
                var processingTime = Random.Shared.Next(10, 30);
                await Task.Delay(processingTime);

                Interlocked.Increment(ref processedMessages);
            });

        Assert.True(result.Success > 0, "Fluent API test should succeed");
        Assert.Equal("FluentAPI_Basic_Test", result.Name);
        Console.WriteLine($"Fluent API Test '{result.Name}' completed: {result.RequestsPerSecond:F2} req/sec, Processed: {processedMessages} messages");
    }

    /// <summary>
    /// Fluent API with explicit success indication
    /// Simulates validation workload with success criteria
    /// </summary>
    [Fact]
    public async Task FluentAPI_LoadTest_WithExplicitSuccess()
    {
        var validationsPassed = 0;

        var result = await LoadTestRunner.Create()
            .WithConcurrency(6)
            .WithDuration(3000)
            .WithInterval(100)
            .WithName("FluentAPI_Explicit_Success")
            .RunAsync(async () =>
            {
                // Simulate data validation
                await Task.Delay(Random.Shared.Next(15, 35));

                // Simulate complex validation logic
                var data = GenerateTestData();
                var isValid = await ValidateDataAsync(data);

                if (isValid)
                {
                    Interlocked.Increment(ref validationsPassed);
                    return true;
                }

                return false;
            });

        Assert.True(result.Success > 0, "Should have successful executions");
        Assert.True(result.Success >= result.Total * 0.8, "Should have high success rate");
        Console.WriteLine($"Explicit Success Test completed: {result.Success}/{result.Total} success ({(double)result.Success / result.Total * 100:F1}% success rate)");
    }

    /// <summary>
    /// Parameterized fluent API test demonstrating different load levels
    /// Simulates varying computational workloads
    /// </summary>
    [Theory]
    [InlineData("Light", 2, 1000)]
    [InlineData("Moderate", 5, 2000)]
    [InlineData("Heavy", 8, 3000)]
    public async Task FluentAPI_ParameterizedLoadTest(string loadLevel, int concurrency, int duration)
    {
        var operationsCompleted = 0;

        var result = await LoadTestRunner.Create()
            .WithName($"Parameterized_{loadLevel}_Load")
            .WithConcurrency(concurrency)
            .WithDuration(duration)
            .WithInterval(200)
            .RunAsync(async () =>
            {
                // Simulate varying computational load
                var complexity = loadLevel switch
                {
                    "Heavy" => 50,
                    "Moderate" => 25,
                    _ => 10
                };

                // Simulate CPU-bound operation
                await Task.Run(() => SimulateCpuWork(complexity));

                // Simulate I/O operation
                await Task.Delay(complexity / 2);

                Interlocked.Increment(ref operationsCompleted);
            });

        // Different expectations based on load level
        var expectedMinOps = loadLevel switch
        {
            "Light" => 5,
            "Moderate" => 8,
            "Heavy" => 10,
            _ => 1
        };

        Assert.True(result.Success > 0, $"{loadLevel} load should have successful executions");
        Assert.True(operationsCompleted >= expectedMinOps, $"Should complete at least {expectedMinOps} operations");
        Console.WriteLine($"{loadLevel} Load Test: {result.RequestsPerSecond:F2} req/sec, Completed: {operationsCompleted} operations");
    }

    /// <summary>
    /// Demonstrates error handling with fluent API
    /// Simulates a workload with controlled failure rate
    /// </summary>
    [Fact]
    public async Task FluentAPI_ErrorHandling_Test()
    {
        var successfulOps = 0;
        var failedOps = 0;
        var counter = 0;

        var result = await LoadTestRunner.Create()
            .WithConcurrency(3)
            .WithDuration(1500)
            .WithInterval(300)
            .WithName("Error_Handling_Test")
            .RunAsync(async () =>
            {
                // Simulate operation with deterministic failure rate
                await Task.Delay(Random.Shared.Next(10, 30));

                // Use counter to ensure we get both successes and failures
                var currentCount = Interlocked.Increment(ref counter);

                // Every 3rd operation fails to ensure we have failures
                if (currentCount % 3 != 0)
                {
                    // Success path
                    Interlocked.Increment(ref successfulOps);
                    return true;
                }
                else
                {
                    // Failure path
                    Interlocked.Increment(ref failedOps);
                    throw new InvalidOperationException("Simulated failure");
                }
            });

        // Should have some failures but also some successes
        Assert.True(result.Total > 0, "Should have total executions");
        Assert.True(result.Success > 0, "Should have some successful operations");
        // Only assert failures if we had enough executions
        if (result.Total >= 3)
        {
            Assert.True(result.Failure > 0, "Should have some failures due to simulated errors");
        }
        Console.WriteLine($"Error Handling Test: {successfulOps} success, {failedOps} failures out of {result.Total} total");
    }

    #region Helper Methods

    private static object GenerateTestData()
    {
        return new
        {
            Id = Guid.NewGuid(),
            Value = Random.Shared.Next(1, 100),
            Timestamp = DateTime.UtcNow
        };
    }

    private static async Task<bool> ValidateDataAsync(object data)
    {
        // Simulate async validation
        await Task.Delay(5);

        // 90% validation success rate
        return Random.Shared.Next(100) < 90;
    }

    private static void SimulateCpuWork(int complexity)
    {
        // Simulate CPU-intensive work
        var result = 0;
        for (int i = 0; i < complexity * 1000; i++)
        {
            result += i % 7;
        }
    }

    #endregion
}