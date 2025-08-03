using Microsoft.Extensions.DependencyInjection;
using xUnitV3LoadFramework.Attributes;
using xUnitV3LoadFramework.Extensions;
using Xunit;

namespace xUnitV3LoadFramework.Tests.Integration;

/// <summary>
/// Core integration tests for LoadFact attribute functionality.
/// Tests the essential end-to-end execution of load tests.
/// </summary>
public class LoadFactIntegrationTests : xUnitV3LoadTests.TestSetup
{
    /// <summary>
    /// Tests basic LoadFact functionality with HTTP client requests.
    /// </summary>
    [LoadFact(order: 1, concurrency: 2, duration: 2000, interval: 500)]
    public async Task LoadFact_Should_Execute_HTTP_Requests_Successfully()
    {
        var result = await LoadTestRunner.ExecuteAsync(async () =>
        {
            var httpClient = GetService<IHttpClientFactory>().CreateClient();
            var response = await httpClient.GetAsync("https://httpbin.org/status/200", TestContext.Current.CancellationToken);
            response.EnsureSuccessStatusCode();
            return true;
        });
        
        // Assert core functionality
        Assert.True(result.Success > 0, "Load test should have successful executions");
        Assert.True(result.Total > 0, "Load test should have executed at least once");
        Assert.True(result.Time > 0, "Should record execution time");
    }

    /// <summary>
    /// Tests LoadFact with JSON API processing.
    /// </summary>
    [LoadFact(order: 2, concurrency: 2, duration: 1500, interval: 400)]
    public async Task LoadFact_Should_Process_JSON_Data()
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
        
        Assert.True(result.Success > 0, "JSON processing should succeed");
        Assert.True(result.AverageLatency > 0, "Should record latency metrics");
    }
}