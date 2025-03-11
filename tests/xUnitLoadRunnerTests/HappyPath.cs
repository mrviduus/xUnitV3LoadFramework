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
    }
}