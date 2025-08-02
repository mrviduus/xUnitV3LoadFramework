using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using xUnit.OTel.Diagnostics;

// Trace everything!

namespace xUnitV3LoadTests;

public class TestSetup : IAsyncLifetime
{
	public IHost Host { get; private set; }

	public async ValueTask InitializeAsync()
	{
		// Create a mini-application for testing
		Host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
			.ConfigureServices(services =>
			{
				// Add the magic tracking ✨
				services.AddOTelDiagnostics();
				// Add ability to make web calls
				services.AddHttpClient();
			})
			.Build();

		await Host.StartAsync();
	}

	public async ValueTask DisposeAsync()
	{
		// Clean up when done
		await Host.StopAsync();
		Host.Dispose();
	}
}
