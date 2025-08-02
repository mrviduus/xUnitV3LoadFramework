using Microsoft.Extensions.DependencyInjection;
using xUnitV3LoadFramework.Attributes;
using xUnitV3LoadFramework.Extensions;

namespace xUnitV3LoadTests;

[UseStressFramework]
public class WebTests : IDisposable
{
	private HttpClient? _httpClient;

	public WebTests()
	{
		// Create the TestSetup and initialize HttpClient in constructor
		var setup = new TestSetup();
		setup.InitializeAsync().GetAwaiter().GetResult();
		
		_httpClient = setup.Host.Services.GetRequiredService<IHttpClientFactory>().CreateClient();
	}

	public void Dispose()
	{
		_httpClient?.Dispose();
	}

	[Stress(order: 1, concurrency: 2, duration: 5000, interval: 500)]
	public async Task TestGoogleIsWorking()
	{
		// This will show you:
		// - When the call started
		// - How long it took
		// - If it worked or failed
		var response = await _httpClient!.GetAsync("https://www.google.com", TestContext.Current.CancellationToken);

		Assert.True(response.IsSuccessStatusCode);
	}
}

