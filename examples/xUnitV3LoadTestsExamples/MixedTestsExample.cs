using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using xUnitV3LoadFramework.Attributes;

namespace xUnitV3LoadTests;

/// <summary>
/// Example demonstrating mixed testing scenarios where you can use both standard xUnit [Fact] 
/// attributes alongside [Load] attributes in the same test class.
/// This shows how the framework supports both functional testing and load testing patterns.
/// </summary>
public class MixedTestsExample : IDisposable
{
	private readonly HttpClient _httpClient;
	private readonly IHost _testHost;

	public MixedTestsExample()
	{
		// Setup test infrastructure using standard xUnit constructor pattern
		_testHost = Host.CreateDefaultBuilder()
			.ConfigureServices(services =>
			{
				services.AddHttpClient();
			})
			.Build();

		_httpClient = _testHost.Services.GetRequiredService<HttpClient>();

		Console.WriteLine(">> Test class initialized");
	}

	public void Dispose()
	{
		// Standard xUnit cleanup using IDisposable pattern
		_httpClient?.Dispose();
		_testHost?.Dispose();
		Console.WriteLine(">> Test class disposed");
	}

	// Standard xUnit functional test
	[Fact]
	public void Should_Initialize_HttpClient_Successfully()
	{
		// Arrange & Act & Assert
		Assert.NotNull(_httpClient);
		Assert.True(_httpClient.BaseAddress == null); // Default base address should be null

		Console.WriteLine("✓ Functional test: HttpClient initialized correctly");
	}

	// Another standard xUnit test using Theory
	[Theory]
	[InlineData("https://httpbin.org/get")]
	[InlineData("https://jsonplaceholder.typicode.com/posts/1")]
	public async Task Should_Handle_Various_Endpoints_In_Functional_Test(string url)
	{
		// Arrange & Act
		var response = await _httpClient.GetAsync(url);

		// Assert
		Assert.True(response.IsSuccessStatusCode, $"Failed to get response from {url}");

		Console.WriteLine($"✓ Functional test: Successfully called {url}");
	}

	// Load test - executes multiple times concurrently
	[Load(order: 1, concurrency: 5, duration: 3000, interval: 100)]
	public async Task Should_Handle_Concurrent_HTTP_Requests()
	{
		// This method will be executed 5 times concurrently for 3 seconds
		var response = await _httpClient.GetAsync("https://httpbin.org/delay/1");

		Assert.True(response.IsSuccessStatusCode, "HTTP request should succeed under load");

		Console.WriteLine($"✓ Load test: HTTP request completed at {DateTime.Now:HH:mm:ss.fff}");
	}

	// Another load test with different parameters
	[Load(order: 2, concurrency: 3, duration: 2000, interval: 200)]
	public async Task Should_Process_JSON_Data_Under_Load()
	{
		// This method will be executed 3 times concurrently for 2 seconds
		var response = await _httpClient.GetAsync("https://jsonplaceholder.typicode.com/posts/1");
		var content = await response.Content.ReadAsStringAsync();

		Assert.True(response.IsSuccessStatusCode, "JSON API request should succeed");
		Assert.Contains("userId", content, StringComparison.OrdinalIgnoreCase);

		Console.WriteLine($"✓ Load test: JSON processing completed at {DateTime.Now:HH:mm:ss.fff}");
	}

	// Load test focusing on error conditions
	[Load(order: 3, concurrency: 2, duration: 1500, interval: 300)]
	public async Task Should_Handle_Error_Conditions_Under_Load()
	{
		try
		{
			// Test with a non-existent endpoint
			var response = await _httpClient.GetAsync("https://httpbin.org/status/404");

			Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
			Console.WriteLine($"✓ Load test: Error handling verified at {DateTime.Now:HH:mm:ss.fff}");
		}
		catch (Exception ex)
		{
			Console.WriteLine($"⚠ Load test: Unexpected exception - {ex.Message}");
			throw; // Re-throw to fail the test
		}
	}

	// Standard fact test that can run independently
	[Fact]
	public void Should_Validate_Test_Infrastructure_Still_Works()
	{
		// This standard test runs independently of the load tests
		Assert.NotNull(_httpClient);
		Assert.NotNull(_testHost);

		Console.WriteLine("✓ Functional test: Test infrastructure validated");
	}
}

/// <summary>
/// Standard xUnit test class - only uses [Fact] and [Theory] attributes
/// These tests run with regular xUnit behavior without load testing
/// </summary>
public class StandardXUnitTests
{
	[Fact]
	public void StandardFact_ShouldExecuteWithXUnit()
	{
		// This test runs with standard xUnit framework
		var result = 2 + 2;
		Assert.Equal(4, result);
		Console.WriteLine("Standard [Fact] test executed via xUnit");
	}

	[Theory]
	[InlineData(1, 2, 3)]
	[InlineData(5, 5, 10)]
	[InlineData(10, -5, 5)]
	public void StandardTheory_ShouldExecuteWithXUnit(int a, int b, int expected)
	{
		// This test runs with standard xUnit framework
		var result = a + b;
		Assert.Equal(expected, result);
		Console.WriteLine($"Standard [Theory] test executed: {a} + {b} = {result}");
	}
}

/// <summary>
/// Load-only test class - only uses [Load] attributes
/// These tests run with the LoadTestFramework for performance testing
/// </summary>
public class LoadOnlyTests : IDisposable
{
	private string? _testData;

	public LoadOnlyTests()
	{
		_testData = "Test data initialized";
		Console.WriteLine("Constructor: Load test setup completed");
	}

	public void Dispose()
	{
		Console.WriteLine("Dispose: Load test cleanup completed");
	}

	[Load(order: 1, concurrency: 5, duration: 2000, interval: 500)]
	public void LoadTest_ShouldExecuteWithLoadFramework()
	{
		Assert.NotNull(_testData);
		Console.WriteLine($"Load test executed at {DateTime.Now:HH:mm:ss.fff}");
	}

	[Load(order: 2, concurrency: 3, duration: 1500, interval: 300)]
	public void AnotherLoadTest_ShouldExecuteWithLowerLoad()
	{
		Console.WriteLine($"Another load test executed at {DateTime.Now:HH:mm:ss.fff}");
	}
}
