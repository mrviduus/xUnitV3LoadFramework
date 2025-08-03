using Microsoft.Extensions.DependencyInjection;
using xUnitV3LoadFramework.Attributes;
using xUnitV3LoadFramework.Extensions;
using Xunit;

namespace xUnitV3LoadFramework.Examples;

/// <summary>
/// Demonstrates the enhanced LoadTestRunner with fluent API capabilities.
/// Shows both traditional LoadFact attribute usage and the new fluent API approach.
/// </summary>
public class LoadTestRunnerDemonstration : IDisposable
{
    private readonly HttpClient _httpClient;

    public LoadTestRunnerDemonstration()
    {
        _httpClient = new HttpClient();
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }

    /// <summary>
    /// Traditional approach using LoadFact attribute with LoadTestRunner.ExecuteAsync
    /// </summary>
    [LoadFact(order: 1, concurrency: 5, duration: 3000, interval: 200)]
    public async Task Traditional_LoadTest_With_LoadTestRunner()
    {
        var result = await LoadTestRunner.ExecuteAsync(async () =>
        {
            var response = await _httpClient.GetAsync("https://httpbin.org/get");
            response.EnsureSuccessStatusCode();
            return true;
        });

        // Assert performance expectations
        Assert.True(result.Success > 0, "Should have successful executions");
        Assert.True(result.RequestsPerSecond > 0, "Should achieve measurable throughput");
        
        Console.WriteLine($" Traditional LoadTest completed: {result.Success}/{result.Total} success, {result.RequestsPerSecond:F2} req/sec");
    }

    /// <summary>
    /// Simplified approach using LoadTestRunner.RunAsync
    /// </summary>
    [LoadFact(order: 2, concurrency: 3, duration: 2000, interval: 300)]
    public async Task Simplified_LoadTest_With_RunAsync()
    {
        var result = await LoadTestRunner.RunAsync(async () =>
        {
            var response = await _httpClient.GetAsync("https://httpbin.org/status/200");
            response.EnsureSuccessStatusCode();
            // No need to return true - success is implicit if no exception
        });

        Assert.True(result.Success > 0, "Should have successful executions");
        Console.WriteLine($" Simplified LoadTest completed: {result.Success}/{result.Total} success");
    }

    /// <summary>
    /// Fluent API approach for dynamic configuration
    /// </summary>
    [Fact]
    public async Task FluentAPI_LoadTest_Basic()
    {
        var result = await LoadTestRunner.Create()
            .WithConcurrency(4)
            .WithDuration(2500)
            .WithInterval(150)
            .WithName("FluentAPI_Basic_Test")
            .RunAsync(async () =>
            {
                var response = await _httpClient.GetAsync("https://httpbin.org/uuid");
                response.EnsureSuccessStatusCode();
            });

        Assert.True(result.Success > 0, "Fluent API test should succeed");
        Assert.Equal("FluentAPI_Basic_Test", result.Name);
        Console.WriteLine($" Fluent API Test '{result.Name}' completed: {result.RequestsPerSecond:F2} req/sec");
    }

    /// <summary>
    /// Fluent API with explicit success indication
    /// </summary>
    [Fact]
    public async Task FluentAPI_LoadTest_WithExplicitSuccess()
    {
        var result = await LoadTestRunner.Create()
            .WithConcurrency(6)
            .WithDuration(3000)
            .WithInterval(100)
            .WithName("FluentAPI_Explicit_Success")
            .RunAsync(async () =>
            {
                var response = await _httpClient.GetAsync("https://httpbin.org/json");
                if (!response.IsSuccessStatusCode)
                    return false;

                var content = await response.Content.ReadAsStringAsync();
                return !string.IsNullOrEmpty(content);
            });

        Assert.True(result.Success > 0, "Should have successful executions");
        Assert.True(result.Success >= result.Total * 0.8, "Should have high success rate");
        Console.WriteLine($" Explicit Success Test completed: {result.Success}/{result.Total} success ({(double)result.Success / result.Total * 100:F1}% success rate)");
    }

    /// <summary>
    /// Parameterized fluent API test demonstrating different load levels
    /// </summary>
    [Theory]
    [InlineData("Light", 2, 1000)]
    [InlineData("Moderate", 5, 2000)]
    [InlineData("Heavy", 8, 3000)]
    public async Task FluentAPI_ParameterizedLoadTest(string loadLevel, int concurrency, int duration)
    {
        var result = await LoadTestRunner.Create()
            .WithName($"Parameterized_{loadLevel}_Load")
            .WithConcurrency(concurrency)
            .WithDuration(duration)
            .WithInterval(200)
            .RunAsync(async () =>
            {
                // Simulate varying load by adding delay for heavier tests
                var delay = loadLevel switch
                {
                    "Heavy" => 50,
                    "Moderate" => 25,
                    _ => 10
                };
                
                await Task.Delay(delay);
                var response = await _httpClient.GetAsync("https://httpbin.org/delay/0");
                response.EnsureSuccessStatusCode();
            });

        // Different expectations based on load level
        var expectedMinRps = loadLevel switch
        {
            "Light" => 5,
            "Moderate" => 8,
            "Heavy" => 10,
            _ => 1
        };

        Assert.True(result.Success > 0, $"{loadLevel} load should have successful executions");
        Console.WriteLine($" {loadLevel} Load Test: {result.RequestsPerSecond:F2} req/sec (expected min: {expectedMinRps})");
    }

    /// <summary>
    /// Demonstrates error handling with fluent API
    /// </summary>
    [Fact]
    public async Task FluentAPI_ErrorHandling_Test()
    {
        var result = await LoadTestRunner.Create()
            .WithConcurrency(3)
            .WithDuration(1500)
            .WithInterval(300)
            .WithName("Error_Handling_Test")
            .RunAsync(async () =>
            {
                // Simulate 50% failure rate
                if (Random.Shared.Next(0, 2) == 0)
                {
                    throw new InvalidOperationException("Simulated failure");
                }

                var response = await _httpClient.GetAsync("https://httpbin.org/status/200");
                response.EnsureSuccessStatusCode();
                return true;
            });

        // Should have some failures but also some successes
        Assert.True(result.Total > 0, "Should have total executions");
        Assert.True(result.Failure > 0, "Should have some failures due to simulated errors");
        Console.WriteLine($" Error Handling Test: {result.Success} success, {result.Failure} failures out of {result.Total} total");
    }
}
