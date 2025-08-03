using System;
using System.Threading.Tasks;
using xUnitV3LoadFramework.Extensions;

namespace TestFluentAPI
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Testing LoadTestRunner Fluent API...");
            
            try
            {
                // Test 1: Basic fluent API test
                var result = await LoadTestRunner.Create()
                    .WithName("FluentAPI_Test")
                    .WithConcurrency(2)
                    .WithDuration(TimeSpan.FromSeconds(5))
                    .WithInterval(TimeSpan.FromMilliseconds(500))
                    .RunAsync(async () =>
                    {
                        await Task.Delay(50);
                        return true;
                    });

                Console.WriteLine($"Test 1 Results:");
                Console.WriteLine($"- Name: {result.Name}");
                Console.WriteLine($"- ScenarioName: {result.ScenarioName}");
                Console.WriteLine($"- Total: {result.Total}");
                Console.WriteLine($"- Success: {result.Success}");
                Console.WriteLine($"- Failure: {result.Failure}");
                Console.WriteLine($"- Time: {result.Time:F2}s");
                
                // Test 2: Verify Name property works correctly
                if (result.Name == "FluentAPI_Test")
                {
                    Console.WriteLine("Name property working correctly!");
                }
                else
                {
                    Console.WriteLine($"Name property failed. Expected: FluentAPI_Test, Got: {result.Name}");
                }
                
                // Test 3: Verify ScenarioName equals Name
                if (result.ScenarioName == result.Name)
                {
                    Console.WriteLine("ScenarioName equals Name property!");
                }
                else
                {
                    Console.WriteLine($"ScenarioName doesn't match Name. ScenarioName: {result.ScenarioName}, Name: {result.Name}");
                }
                
                Console.WriteLine("\n🎉 LoadTestRunner Fluent API test completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Test failed with error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }
}
