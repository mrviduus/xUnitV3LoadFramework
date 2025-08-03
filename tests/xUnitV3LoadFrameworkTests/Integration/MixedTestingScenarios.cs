using Microsoft.Extensions.DependencyInjection;
using xUnitV3LoadFramework.Attributes;
using xUnitV3LoadFramework.Extensions;
using Xunit;

namespace xUnitV3LoadFramework.Tests.Integration;

/// <summary>
/// Integration tests demonstrating mixed testing scenarios.
/// Shows how LoadFact, Fact, and Theory attributes can be used together in the same test class.
/// </summary>
public class MixedTestingScenarios : xUnitV3LoadTests.TestSetup
{
    /// <summary>
    /// Standard unit test that validates the test setup infrastructure.
    /// Ensures that dependency injection and HTTP client factory are working correctly.
    /// </summary>
    [Fact]
    public void Standard_Test_Should_Validate_Infrastructure()
    {
        // Arrange & Act
        var httpClientFactory = GetService<IHttpClientFactory>();
        var httpClient = httpClientFactory.CreateClient();
        
        // Assert
        Assert.NotNull(httpClientFactory);
        Assert.NotNull(httpClient);
    }

    /// <summary>
    /// Load test that executes HTTP requests under concurrent load.
    /// Validates that the system can handle multiple simultaneous requests.
    /// </summary>
    [LoadFact(order: 1, concurrency: 3, duration: 2000, interval: 200)]
    public async Task LoadFact_Should_Execute_Concurrent_HTTP_Requests()
    {
        var result = await LoadTestRunner.ExecuteAsync(async () =>
        {
            var httpClient = GetService<IHttpClientFactory>().CreateClient();
            var response = await httpClient.GetAsync("https://httpbin.org/status/200", TestContext.Current.CancellationToken);
            response.EnsureSuccessStatusCode();
            return true;
        });
        
        // Assert load test results
        Assert.True(result.Success > 0, "Load test should have at least some successful executions");
        Assert.True(result.Total > 0, "Load test should have executed at least once");
        Assert.True(result.RequestsPerSecond > 0, "Should calculate requests per second");
        
        // Log results for verification
        Console.WriteLine($"Load test completed with {result.Success}/{result.Total} successful executions");
        Console.WriteLine($"Requests per second: {result.RequestsPerSecond:F2}");
    }

    /// <summary>
    /// Parameterized test using Theory attribute to test multiple HTTP status codes.
    /// Demonstrates that Theory tests work correctly alongside LoadFact tests.
    /// </summary>
    [Theory]
    [InlineData(200, "OK")]
    [InlineData(201, "Created")]
    [InlineData(202, "Accepted")]
    public async Task Theory_Test_Should_Handle_Various_HTTP_Status_Codes(int statusCode, string description)
    {
        // Arrange
        var httpClient = GetService<IHttpClientFactory>().CreateClient();
        
        // Act
        var response = await httpClient.GetAsync($"https://httpbin.org/status/{statusCode}", TestContext.Current.CancellationToken);
        
        // Assert
        Assert.Equal(statusCode, (int)response.StatusCode);
        Assert.NotNull(description); // Verify the description parameter
        
        Console.WriteLine($"? Theory test: {statusCode} ({description}) handled correctly");
    }

    /// <summary>
    /// Another load test with different parameters to test sequential execution.
    /// Validates that multiple LoadFact tests can run in sequence with different configurations.
    /// </summary>
    [LoadFact(order: 2, concurrency: 2, duration: 1500, interval: 400)]
    public async Task LoadFact_Should_Execute_Sequential_JSON_Processing()
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
        Assert.True(result.AverageLatency > 0, "Should record meaningful latency metrics");
    }

    /// <summary>
    /// Standard fact test that can run independently of load tests.
    /// Ensures that the test infrastructure remains stable across different test types.
    /// </summary>
    [Fact]
    public void Standard_Fact_Should_Execute_Independently()
    {
        // Arrange & Act
        var testValue = 2 + 2;
        var currentTime = DateTime.UtcNow;
        
        // Assert
        Assert.Equal(4, testValue);
        Assert.True(currentTime <= DateTime.UtcNow);
        
        Console.WriteLine("? Standard fact test executed independently");
    }
}