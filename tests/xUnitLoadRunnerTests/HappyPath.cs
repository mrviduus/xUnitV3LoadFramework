using Xunit.Abstractions;
using xUnitLoadFramework;

namespace xUnitLoadRunnerTests
{
    public class HappyPath(ITestOutputHelper testOutputHelper)
    {
        // [Fact]
        // public void VerySlowTest()
        // {
        //     Thread.Sleep(TimeSpan.FromMinutes(3));
        // }

        [Fact]
        [LoadTestSettings(concurrency: 10, DurationInSeconds = 1, IntervalInSeconds = 1)]
        public void VeryFastTest()
        {
            Console.WriteLine("This is a very fast test");
        }
        
        [Fact]
        // [LoadTestSettings(concurrency: 4, DurationInSeconds = 1, IntervalInSeconds = 1)]
        public void VeryFastTest2()
        {
            Console.WriteLine("This is a very fast test2");
        }
        
        [Fact]
        [LoadTestSettings(concurrency: 1000, DurationInSeconds = 60, IntervalInSeconds = 1)]
        public async Task VeryFastTest3()
        {
            HttpClient client = new HttpClient();
            var response = await client.GetAsync("https://www.example.com");
            response.EnsureSuccessStatusCode();
        }
    }
}