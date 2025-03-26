using Xunit.Abstractions;
using xUnitLoadFramework;
using xUnitLoadFramework.Attributes;

namespace xUnitLoadRunnerTests
{
    public class HappyPath
    {
        // [Fact]
        // public void VerySlowTest()
        // {
        //     Thread.Sleep(TimeSpan.FromMinutes(3));
        // }

        [Fact]
        [LoadTestSettings(concurrency: 3, DurationInSeconds = 1, IntervalInSeconds = 1)]
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
        
        // [Fact]
        // [LoadTestSettings(concurrency: 1000, DurationInSeconds = 60, IntervalInSeconds = 1)]
        // public async Task VeryFastTest3()
        // {
        //     Console.WriteLine("This is a very fast test3");
        //     await Task.Delay(TimeSpan.FromSeconds(1));
        // }
    }
}