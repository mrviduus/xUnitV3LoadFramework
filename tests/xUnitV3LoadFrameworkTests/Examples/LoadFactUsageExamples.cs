using Microsoft.Extensions.DependencyInjection;
using xUnitV3LoadFramework.Attributes;
using xUnitV3LoadFramework.Extensions;
using Xunit;

namespace xUnitV3LoadFramework.Tests.Examples;

/// <summary>
/// Example tests demonstrating LoadFact usage patterns.
/// Shows how to write effective load tests using the LoadFact attribute.
/// </summary>
public class LoadFactUsageExamples : xUnitV3LoadTests.TestSetup
{
    /// <summary>
    /// Example: Basic HTTP load test with Google endpoint.
    /// Demonstrates minimal LoadFact configuration for web service testing.
    /// </summary>
    [LoadFact(order: 1, concurrency: 2, duration: 5000, interval: 500)]
    public async Task Example_Basic_HTTP_Load_Test()
    {
        var result = await LoadTestHelper.ExecuteLoadTestAsync(async () =>
        {
            var httpClient = GetService<IHttpClientFactory>().CreateClient();
            var response = await httpClient.GetAsync("https://www.google.com", TestContext.Current.CancellationToken);
            response.EnsureSuccessStatusCode();
            return true;
        });
        
        Assert.True(result.Success > 0, "Load test should have successful executions");
        Assert.True(result.Total > 0, "Load test should have executed at least once");
    }

    /// <summary>
    /// Example: API endpoint testing with JSON response validation.
    /// Shows how to test REST APIs under load with response validation.
    /// </summary>
    [LoadFact(order: 2, concurrency: 3, duration: 3000, interval: 200)]
    public async Task Example_JSON_API_Load_Test()
    {
        var result = await LoadTestHelper.ExecuteLoadTestAsync(async () =>
        {
            var httpClient = GetService<IHttpClientFactory>().CreateClient();
            var response = await httpClient.GetAsync("https://jsonplaceholder.typicode.com/posts/1", TestContext.Current.CancellationToken);
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("userId", content, StringComparison.OrdinalIgnoreCase);
            return true;
        });
        
        Assert.True(result.Success > 0, "JSON API load test should succeed");
        Console.WriteLine($"Load test completed: {result.Success}/{result.Total} successful executions");
    }

    /// <summary>
    /// Example: Testing service endpoint with controlled delay.
    /// Demonstrates load testing with endpoints that have predictable response times.
    /// </summary>
    [LoadFact(order: 3, concurrency: 4, duration: 2500, interval: 300)]
    public async Task Example_Delayed_Service_Load_Test()
    {
        var result = await LoadTestHelper.ExecuteLoadTestAsync(async () =>
        {
            var httpClient = GetService<IHttpClientFactory>().CreateClient();
            var response = await httpClient.GetAsync("https://httpbin.org/delay/1", TestContext.Current.CancellationToken);
            response.EnsureSuccessStatusCode();
            return true;
        });
        
        Assert.True(result.Success > 0, "Delayed service load test should succeed");
        Assert.True(result.AverageLatency >= 1000, "Average latency should reflect the 1-second delay");
    }

    /// <summary>
    /// Standard xUnit test to demonstrate mixed testing scenarios.
    /// Shows that LoadFact tests can coexist with regular unit tests.
    /// </summary>
    [Fact]
    public async Task Example_Standard_Unit_Test()
    {
        var httpClient = GetService<IHttpClientFactory>().CreateClient();
        var response = await httpClient.GetAsync("https://httpbin.org/status/200", TestContext.Current.CancellationToken);
        
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>
    /// Example: Parameterized test using Theory attribute.
    /// Demonstrates that Theory tests work alongside LoadFact tests.
    /// </summary>
    [Theory]
    [InlineData(200)]
    [InlineData(201)]
    [InlineData(202)]
    public async Task Example_Theory_Test_With_Status_Codes(int statusCode)
    {
        var httpClient = GetService<IHttpClientFactory>().CreateClient();
        var response = await httpClient.GetAsync($"https://httpbin.org/status/{statusCode}", TestContext.Current.CancellationToken);
        
        Assert.Equal(statusCode, (int)response.StatusCode);
    }
}