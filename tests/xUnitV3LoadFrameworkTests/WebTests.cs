using Microsoft.Extensions.DependencyInjection;
using xUnitV3LoadFramework.Attributes;
using xUnitV3LoadFramework.Extensions;
using Xunit;

namespace xUnitV3LoadTests;

public class WebTests : TestSetup
{
	[LoadFact(order: 1, concurrency: 2, duration: 5000, interval: 500)]
	public async Task TestGoogleIsWorking()
	{
		// Execute this test as a load test using the LoadFact parameters
		var result = await LoadTestHelper.ExecuteLoadTestAsync(async () =>
		{
			var httpClient = GetService<IHttpClientFactory>().CreateClient();
			var response = await httpClient.GetAsync("https://www.google.com", TestContext.Current.CancellationToken);
			response.EnsureSuccessStatusCode();
			return true; // Return true for successful execution
		});
		
		// Assert that the load test had some successful executions
		Assert.True(result.Success > 0, "Load test should have at least some successful executions");
		Assert.True(result.Total > 0, "Load test should have executed at least once");
	}
}

