using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
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

				services.AddOTelDiagnostics(
					configureMeterProviderBuilder: m => m.AddOtlpExporter(),
					configureTracerProviderBuilder: t => t.AddOtlpExporter(),
					configureLoggingBuilder: options => options.AddOpenTelemetry(o => o.AddOtlpExporter())
					);
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

	/// <summary>
	/// Gets a service from the DI container
	/// </summary>
	/// <typeparam name="T">The service type to retrieve</typeparam>
	/// <returns>The service instance</returns>
	public T GetService<T>() where T : notnull
	{
		return Host.Services.GetRequiredService<T>();
	}
}
