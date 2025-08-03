using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using xUnitV3LoadFramework.Attributes;
using xUnitV3LoadFramework.Extensions;

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

		Console.WriteLine("Functional test: HttpClient initialized correctly");
	}

	// Another standard xUnit test using Theory
	[Theory]
	[InlineData("https://httpbin.org/get")]
	[InlineData("https://jsonplaceholder.typicode.com/posts/1")]
	public async Task Should_Handle_Various_Endpoints_In_Functional_Test(string url)
	{
		// Arrange & Act
		var response = await _httpClient.GetAsync(url, TestContext.Current.CancellationToken);

		// Assert
		Assert.True(response.IsSuccessStatusCode, $"Failed to get response from {url}");

		Console.WriteLine($"Functional test: Successfully called {url}");
	}

	// Load test - executes multiple times concurrently using Load
	[Load(order: 1, concurrency: 5, duration: 3000, interval: 100)]
	public async Task Should_Handle_Concurrent_HTTP_Requests()
	{
		// Use LoadTestRunner to properly execute this as a load test
		var result = await LoadTestRunner.ExecuteAsync(async () =>
		{
			// This lambda will be executed 5 times concurrently for 3 seconds
			var response = await _httpClient.GetAsync("https://httpbin.org/delay/1");
			
			if (!response.IsSuccessStatusCode)
			{
				Console.WriteLine($"HTTP request failed with status: {response.StatusCode}");
				return false;
			}

			Console.WriteLine($"Load test: HTTP request completed at {DateTime.Now:HH:mm:ss.fff}");
			return true;
		});

		// Assert that the load test was successful overall
		Assert.True(result.Success > 0, $"Load test should have at least some successful executions. Success: {result.Success}, Failure: {result.Failure}");
		Assert.True(result.Success > result.Failure, $"Load test should have more successes than failures. Success: {result.Success}, Failure: {result.Failure}");
		
		Console.WriteLine($"Load test completed - Success: {result.Success}, Failure: {result.Failure}, Success Rate: {(double)result.Success / result.Total * 100:F2}%");
	}

	// Another load test with different parameters using Load
	[Load(order: 2, concurrency: 3, duration: 2000, interval: 200)]
	public async Task Should_Process_JSON_Data_Under_Load()
	{
		// Use LoadTestRunner to properly execute this as a load test
		var result = await LoadTestRunner.ExecuteAsync(async () =>
		{
			// This lambda will be executed 3 times concurrently for 2 seconds
			var response = await _httpClient.GetAsync("https://jsonplaceholder.typicode.com/posts/1");
			var content = await response.Content.ReadAsStringAsync();

			if (!response.IsSuccessStatusCode)
			{
				Console.WriteLine($"JSON API request failed with status: {response.StatusCode}");
				return false;
			}

			if (!content.Contains("userId", StringComparison.OrdinalIgnoreCase))
			{
				Console.WriteLine($"JSON response doesn't contain expected 'userId' field");
				return false;
			}

			Console.WriteLine($"Load test: JSON processing completed at {DateTime.Now:HH:mm:ss.fff}");
			return true;
		});

		// Assert that the load test was successful overall
		Assert.True(result.Success > 0, $"Load test should have at least some successful executions. Success: {result.Success}, Failure: {result.Failure}");
		Assert.True(result.Success >= result.Failure, $"Load test should have equal or more successes than failures. Success: {result.Success}, Failure: {result.Failure}");
		
		Console.WriteLine($"JSON Load test completed - Success: {result.Success}, Failure: {result.Failure}, Success Rate: {(double)result.Success / result.Total * 100:F2}%");
	}

	// Load test focusing on error conditions using Load
	[Load(order: 3, concurrency: 2, duration: 1500, interval: 300)]
	public async Task Should_Handle_Error_Conditions_Under_Load()
	{
		// Use LoadTestRunner to properly execute this as a load test
		var result = await LoadTestRunner.ExecuteAsync(async () =>
		{
			try
			{
				// Test with a non-existent endpoint - this should return 404
				var response = await _httpClient.GetAsync("https://httpbin.org/status/404");

				if (response.StatusCode != System.Net.HttpStatusCode.NotFound)
				{
					Console.WriteLine($"Expected 404 but got: {response.StatusCode}");
					return false;
				}

				Console.WriteLine($"Load test: Error handling verified at {DateTime.Now:HH:mm:ss.fff}");
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Load test: Unexpected exception - {ex.Message}");
				return false;
			}
		});

		// Assert that the load test was successful overall
		Assert.True(result.Success > 0, $"Load test should have at least some successful executions. Success: {result.Success}, Failure: {result.Failure}");
		Assert.True(result.Success >= result.Failure, $"Load test should handle error conditions properly. Success: {result.Success}, Failure: {result.Failure}");
		
		Console.WriteLine($"Error handling Load test completed - Success: {result.Success}, Failure: {result.Failure}, Success Rate: {(double)result.Success / result.Total * 100:F2}%");
	}

	// Standard fact test that can run independently
	[Fact]
	public void Should_Validate_Test_Infrastructure_Still_Works()
	{
		// This standard test runs independently of the load tests
		Assert.NotNull(_httpClient);
		Assert.NotNull(_testHost);

		Console.WriteLine("Functional test: Test infrastructure validated");
	}

	// Advanced load test that validates performance characteristics
	[Load(order: 4, concurrency: 10, duration: 5000, interval: 50)]
	public async Task Should_Validate_Load_Performance_Metrics()
	{
		var executionTimes = new List<double>();
		var requestCount = 0;

		var result = await LoadTestRunner.ExecuteAsync(async () =>
		{
			var stopwatch = System.Diagnostics.Stopwatch.StartNew();
			
			try
			{
				// Use a faster endpoint for performance testing
				var response = await _httpClient.GetAsync("https://httpbin.org/get");
				stopwatch.Stop();
				
				lock (executionTimes)
				{
					executionTimes.Add(stopwatch.Elapsed.TotalMilliseconds);
					requestCount++;
				}

				if (!response.IsSuccessStatusCode)
				{
					Console.WriteLine($"Performance test request failed: {response.StatusCode}");
					return false;
				}

				Console.WriteLine($"Performance test: Request #{requestCount} completed in {stopwatch.Elapsed.TotalMilliseconds:F2}ms");
				return true;
			}
			catch (Exception ex)
			{
				stopwatch.Stop();
				Console.WriteLine($"Performance test exception: {ex.Message}");
				return false;
			}
		});

		// Validate load test execution
		Assert.True(result.Success > 0, $"Load test should have successful executions. Success: {result.Success}, Failure: {result.Failure}");
		Assert.True(result.Total >= 10, $"Load test should execute multiple times. Total: {result.Total}");
		
		// Validate performance characteristics
		if (executionTimes.Count > 0)
		{
			var avgResponseTime = executionTimes.Average();
			var maxResponseTime = executionTimes.Max();
			var minResponseTime = executionTimes.Min();
			
			Console.WriteLine($"Performance Metrics:");
			Console.WriteLine($"  Total Requests: {executionTimes.Count}");
			Console.WriteLine($"  Avg Response Time: {avgResponseTime:F2}ms");
			Console.WriteLine($"  Min Response Time: {minResponseTime:F2}ms");
			Console.WriteLine($"  Max Response Time: {maxResponseTime:F2}ms");
			
			// Basic performance assertions
			Assert.True(avgResponseTime < 5000, $"Average response time should be under 5 seconds. Actual: {avgResponseTime:F2}ms");
			Assert.True(executionTimes.Count >= 5, $"Should execute at least 5 requests under load. Actual: {executionTimes.Count}");
		}

		Console.WriteLine($"Performance Load test completed - Success: {result.Success}, Failure: {result.Failure}, Success Rate: {(double)result.Success / result.Total * 100:F2}%");
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
