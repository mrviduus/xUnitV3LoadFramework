using Microsoft.Extensions.DependencyInjection;
using xUnitV3LoadFramework.Attributes;
using xUnitV3LoadFramework.Extensions;
using Xunit;

namespace xUnitV3LoadFramework.Tests.Integration;

/// <summary>
/// Core mixed testing scenarios - demonstrates Load, Fact, and Theory working together.
/// </summary>
public class MixedTestingScenarios : xUnitV3LoadTests.TestSetup
{
    /// <summary>
    /// Standard unit test that validates infrastructure.
    /// </summary>
    [Fact]
    public void Standard_Test_Should_Validate_Infrastructure()
    {
        var httpClientFactory = GetService<IHttpClientFactory>();
        var httpClient = httpClientFactory.CreateClient();
        
        Assert.NotNull(httpClientFactory);
        Assert.NotNull(httpClient);
    }

    /// <summary>
    /// Load test for mixed testing demonstration.
    /// </summary>
    [Load(concurrency: 2, duration: 1500, interval: 300)]
    public async Task Load_Should_Execute_With_Other_Tests()
    {
        var result = await LoadTestRunner.ExecuteAsync(async () =>
        {
            var httpClient = GetService<IHttpClientFactory>().CreateClient();
            var response = await httpClient.GetAsync("https://httpbin.org/status/200", TestContext.Current.CancellationToken);
            response.EnsureSuccessStatusCode();
            return true;
        });
        
        Assert.True(result.Success > 0, "Mixed load test should succeed");
        Assert.True(result.Total > 0, "Should execute at least once");
    }

    /// <summary>
    /// Theory test for multiple scenarios.
    /// </summary>
    [Theory]
    [InlineData(200, "OK")]
    [InlineData(201, "Created")]
    public async Task Theory_Test_Should_Handle_HTTP_Status_Codes(int statusCode, string description)
    {
        var httpClient = GetService<IHttpClientFactory>().CreateClient();
        var response = await httpClient.GetAsync($"https://httpbin.org/status/{statusCode}", TestContext.Current.CancellationToken);
        
        Assert.Equal(statusCode, (int)response.StatusCode);
        Assert.NotNull(description);
    }
}
