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
            // Simulate async operation similar to HTTP request
            await Task.Delay(Random.Shared.Next(30, 100), TestContext.Current.CancellationToken);

            // Simulate successful operation with occasional failures
            if (Random.Shared.Next(100) < 95) // 95% success rate
            {
                return true;
            }

            throw new InvalidOperationException("Simulated operation failure");
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
        // Simulate HTTP status code handling without external dependencies
        await Task.Delay(100, TestContext.Current.CancellationToken);
        
        // Test that the status code is what we expect and description is not null
        Assert.True(statusCode >= 200 && statusCode < 300, "Status code should be success range");
        Assert.NotNull(description);
        Assert.NotEmpty(description);
    }
}
