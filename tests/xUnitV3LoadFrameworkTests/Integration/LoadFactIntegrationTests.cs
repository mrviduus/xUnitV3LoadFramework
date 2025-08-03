using Microsoft.Extensions.DependencyInjection;
using xUnitV3LoadFramework.Attributes;
using xUnitV3LoadFramework.Extensions;
using xUnitV3LoadFramework.LoadRunnerCore.Models;
using xUnitV3LoadFramework.Tests;
using Xunit;

namespace xUnitV3LoadFramework.Tests.Integration;

/// <summary>
/// Integration tests for LoadFact attribute functionality.
/// Tests the complete end-to-end execution of load tests using the LoadFact attribute.
/// </summary>
public class LoadFactIntegrationTests : xUnitV3LoadTests.TestSetup
{
    /// <summary>
    /// Tests basic LoadFact functionality with HTTP client requests.
    /// Verifies that load tests execute with correct concurrency and duration.
    /// </summary>
    [LoadFact(order: 1, concurrency: 2, duration: 3000, interval: 500)]
    public async Task LoadFact_Should_Execute_HTTP_Requests_Under_Load()
    {
        // Arrange & Act
        var result = await LoadTestRunner.ExecuteAsync(async () =>
        {
            var httpClient = GetService<IHttpClientFactory>().CreateClient();
            var response = await httpClient.GetAsync("https://httpbin.org/status/200", TestContext.Current.CancellationToken);
            response.EnsureSuccessStatusCode();
            return true;
        });
        
        // Assert
        Assert.True(result.Success > 0, "Load test should have at least some successful executions");
        Assert.True(result.Total > 0, "Load test should have executed at least once");
        Assert.True(result.Success >= result.Total * 0.8, "At least 80% of requests should succeed");
        
        // Verify load test characteristics
        Assert.True(result.Time >= 2.5, "Load test should run for approximately the configured duration");
        Assert.InRange(result.RequestsPerSecond, 0.1, 100); // Reasonable range for test
    }

    /// <summary>
    /// Tests LoadFact with higher concurrency to verify parallel execution.
    /// </summary>
    [LoadFact(order: 2, concurrency: 5, duration: 2000, interval: 200)]
    public async Task LoadFact_Should_Handle_Higher_Concurrency()
    {
        var result = await LoadTestRunner.ExecuteAsync(async () =>
        {
            var httpClient = GetService<IHttpClientFactory>().CreateClient();
            var response = await httpClient.GetAsync("https://httpbin.org/delay/0.1", TestContext.Current.CancellationToken);
            response.EnsureSuccessStatusCode();
            return true;
        });
        
        Assert.True(result.Success > 0, "High concurrency load test should succeed");
        Assert.True(result.Total >= 5, "Should execute multiple requests with concurrency 5");
    }

    /// <summary>
    /// Tests LoadFact error handling and failure scenarios.
    /// </summary>
    [LoadFact(order: 3, concurrency: 2, duration: 1500, interval: 300)]
    public async Task LoadFact_Should_Handle_Failures_Gracefully()
    {
        var result = await LoadTestRunner.ExecuteAsync(async () =>
        {
            var httpClient = GetService<IHttpClientFactory>().CreateClient();
            var response = await httpClient.GetAsync("https://httpbin.org/status/404", TestContext.Current.CancellationToken);
            
            // Expect 404, which should be handled gracefully
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
            return true;
        });
        
        Assert.True(result.Total > 0, "Load test should execute even with expected failures");
        // Note: All should succeed since we're asserting the 404 status code
        Assert.True(result.Success > 0, "Should handle expected failure scenarios correctly");
    }

    /// <summary>
    /// Tests LoadFact with JSON API processing under load.
    /// </summary>
    [LoadFact(order: 4, concurrency: 3, duration: 2500, interval: 400)]
    public async Task LoadFact_Should_Process_JSON_Data_Under_Load()
    {
        var result = await LoadTestRunner.ExecuteAsync(async () =>
        {
            var httpClient = GetService<IHttpClientFactory>().CreateClient();
            var response = await httpClient.GetAsync("https://jsonplaceholder.typicode.com/posts/1", TestContext.Current.CancellationToken);
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("userId", content, StringComparison.OrdinalIgnoreCase);
            return true;
        });
        
        Assert.True(result.Success > 0, "JSON processing load test should succeed");
        Assert.True(result.AverageLatency > 0, "Should record latency metrics");
    }
}