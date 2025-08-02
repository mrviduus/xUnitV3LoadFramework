using xUnitV3LoadFramework.Attributes;
using xUnitV3LoadFramework.Extensions;

namespace xUnitV3LoadTests;

/// <summary>
/// Standard xUnit test class - no UseLoadFramework attribute
/// These tests should use standard xUnit framework behavior
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
/// Another standard test class to verify framework selection
/// </summary>
public class AnotherStandardTestClass
{
	[Fact]
	public void AnotherStandardTest_ShouldUseXUnit()
	{
		Assert.True(true);
		Console.WriteLine("Another standard test executed");
	}
}

/// <summary>
/// Stress test class using the new V2 pattern
/// Demonstrates modern stress testing with full xUnit v3 compatibility
/// </summary>
[UseStressFramework]
public class StressTestsExample : IDisposable
{
	private readonly string _testData;

	// Standard xUnit constructor for setup
	public StressTestsExample()
	{
		_testData = "Test data initialized in constructor";
		Console.WriteLine("Setup completed in constructor");
	}

	[Stress(order: 1, concurrency: 5, duration: 2000, interval: 500)]
	public async Task StressTest_ShouldHandleConcurrentLoad()
	{
		// Direct test implementation - no separate setup methods needed
		Assert.NotNull(_testData);
		
		// Simulate async work
		await Task.Delay(Random.Shared.Next(10, 50));
		
		Console.WriteLine($"Stress test executed at {DateTime.Now:HH:mm:ss.fff}");
	}

	[Stress(order: 2, concurrency: 3, duration: 1500, interval: 300)]
	public async Task StressTest_WithComplexLogic()
	{
		// More complex test logic directly in the method
		var startTime = DateTime.UtcNow;
		
		// Simulate database operations
		for (int i = 0; i < 10; i++)
		{
			await Task.Delay(Random.Shared.Next(5, 20));
		}
		
		var duration = DateTime.UtcNow - startTime;
		Assert.True(duration.TotalMilliseconds >= 50);
		
		Console.WriteLine($"Complex stress test - Duration: {duration.TotalMilliseconds:F1}ms");
	}

	// Standard xUnit disposal pattern
	public void Dispose()
	{
		Console.WriteLine("Cleanup completed in Dispose");
	}
}
