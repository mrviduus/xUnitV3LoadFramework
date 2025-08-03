using Microsoft.Extensions.DependencyInjection;
using xUnitV3LoadFramework.Attributes;
using xUnitV3LoadFramework.Extensions;

namespace xUnitV3LoadTests;

public class WebTests : Specification
{
	private HttpClient? _httpClient;

	protected override void EstablishContext()
	{
		// Create the TestSetup and initialize HttpClient in EstablishContext
		var setup = new TestSetup();
		setup.InitializeAsync().GetAwaiter().GetResult();
		
		_httpClient = setup.Host.Services.GetRequiredService<IHttpClientFactory>().CreateClient();
	}

	protected override void DestroyContext()
	{
		_httpClient?.Dispose();
	}

	[Load(order: 1, concurrency: 2, duration: 5000, interval: 500)]
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

