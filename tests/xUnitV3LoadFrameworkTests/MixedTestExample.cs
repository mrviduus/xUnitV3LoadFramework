using Microsoft.Extensions.DependencyInjection;
using xUnitV3LoadFramework.Attributes;
using xUnitV3LoadFramework.Extensions;
using Xunit;

namespace xUnitV3LoadTests;

public class MixedTestExample : TestSetup
{
    [Fact]
    public void StandardTestShouldWork()
    {
        // This is a standard xUnit test that should work alongside LoadFact tests
        var httpClientFactory = GetService<IHttpClientFactory>();
        
        Assert.NotNull(httpClientFactory);
        
        var httpClient = httpClientFactory.CreateClient();
        Assert.NotNull(httpClient);
    }

    [LoadFact(order: 1, concurrency: 3, duration: 2000, interval: 200)]
    public async Task LoadTestShouldWork()
    {
        // Execute this test as a load test using the LoadFact parameters
        var result = await LoadTestHelper.ExecuteLoadTestAsync(async () =>
        {
            var httpClient = GetService<IHttpClientFactory>().CreateClient();
            var response = await httpClient.GetAsync("https://httpbin.org/status/200", TestContext.Current.CancellationToken);
            response.EnsureSuccessStatusCode();
            return true; // Return true for successful execution
        });
        
        // Assert that the load test had some successful executions
        Assert.True(result.Success > 0, "Load test should have at least some successful executions");
        Assert.True(result.Total > 0, "Load test should have executed at least once");
        
        // Log the results for user feedback
        Console.WriteLine($"Load test completed with {result.Success}/{result.Total} successful executions");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void TheoryTestShouldWork(int value)
    {
        // This is a standard xUnit theory test that should work alongside LoadFact tests
        Assert.True(value > 0, "Value should be positive");
        Assert.True(value <= 3, "Value should be <= 3");
    }
}
