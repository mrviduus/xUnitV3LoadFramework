#pragma warning disable IDE1006
using xUnitV3LoadFramework.Attributes;

namespace xUnitV3LoadTests;

//==================================//
// EXAMPLE 1: STANDARD STRESS WORKFLOW //
//==================================// 
[UseStressFramework]
public class When_running_standard_stress_scenarios : IDisposable
{
	private readonly string _context;

	public When_running_standard_stress_scenarios()
	{
		_context = "Context established in constructor";
		Console.WriteLine(">> Setup completed in constructor");
	}

	[Stress(order: 1, concurrency: 2, duration: 5000, interval: 500)]
	public async Task should_run_first_scenario()
	{
		Assert.NotNull(_context);
		await Task.Delay(Random.Shared.Next(10, 50));
		Console.WriteLine(">> Executing first stress scenario");
	}

	[Stress(order: 2, concurrency: 3, duration: 7000, interval: 300)]
	public async Task should_run_second_scenario()
	{
		Assert.NotNull(_context);
		await Task.Delay(Random.Shared.Next(20, 100));
		Console.WriteLine(">> Executing second stress scenario");
	}

	[Stress(order: 3, concurrency: 1, duration: 3000, interval: 1000, Skip = "testing skip")]
	public async Task should_skip_scenario()
	{
		await Task.Delay(10);
		Console.WriteLine(">> This test should be skipped");
	}

	public void Dispose()
	{
		Console.WriteLine(">> Cleanup completed in Dispose");
	}
}

//=========================================================//
// EXAMPLE 2: VERIFYING LIFECYCLE EXECUTION ORDER        //
//=========================================================//

[UseStressFramework]
public class When_testing_lifecycle_hooks : IDisposable
{
	private readonly List<string> _executionOrder;

	public When_testing_lifecycle_hooks()
	{
		_executionOrder = new List<string>();
		_executionOrder.Add("Constructor");
		Console.WriteLine(">> [Lifecycle] Constructor invoked");
	}

	[Stress(order: 1, concurrency: 1, duration: 2000, interval: 1000)]
	public async Task should_run_and_log_full_lifecycle()
	{
		_executionOrder.Add("Test Method");
		Console.WriteLine(">> Running lifecycle test");
		
		Assert.Contains("Constructor", _executionOrder);
		await Task.Delay(10);
	}

	public void Dispose()
	{
		_executionOrder.Add("Dispose");
		Console.WriteLine(">> [Lifecycle] Dispose invoked");
	}
}

//=========================================================//
// EXAMPLE 3: EXCEPTION HANDLING IN STRESS TESTS         //
//=========================================================//

[UseStressFramework]
public class When_handling_exceptions_in_stress_tests : IDisposable
{
	private readonly bool _shouldThrow;

	public When_handling_exceptions_in_stress_tests()
	{
		_shouldThrow = false; // Set to true to test exception handling
		Console.WriteLine(">> Exception handling test initialized");
	}

	[Stress(order: 1, concurrency: 2, duration: 3000, interval: 500)]
	public async Task should_handle_exceptions_gracefully()
	{
		if (_shouldThrow)
		{
			throw new InvalidOperationException("Simulated test exception");
		}

		await Task.Delay(Random.Shared.Next(10, 100));
		Console.WriteLine(">> Exception handling test completed successfully");
	}

	public void Dispose()
	{
		Console.WriteLine(">> Exception handling test cleanup");
	}
}

//=========================================================//
// EXAMPLE 4: MIXED TESTING WITH FACTS AND STRESS        //
//=========================================================//

[UseStressFramework]
public class When_combining_different_test_types
{
	[Fact]
	public void should_run_standard_unit_test()
	{
		var result = 2 + 2;
		Assert.Equal(4, result);
		Console.WriteLine(">> Standard unit test executed");
	}

	[Theory]
	[InlineData(1, 1, 2)]
	[InlineData(3, 4, 7)]
	[InlineData(10, 5, 15)]
	public void should_run_parameterized_test(int a, int b, int expected)
	{
		var result = a + b;
		Assert.Equal(expected, result);
		Console.WriteLine($">> Theory test: {a} + {b} = {result}");
	}

	[Stress(concurrency: 5, duration: 3000, order: 1, interval: 100)]
	public async Task should_run_stress_test_in_mixed_class()
	{
		var iterations = 50;
		var sum = 0;
		
		for (int i = 0; i < iterations; i++)
		{
			sum += i;
			if (i % 10 == 0)
				await Task.Delay(1);
		}
		
		Assert.True(sum > 0);
		Console.WriteLine($">> Mixed stress test completed - Sum: {sum}");
	}
}