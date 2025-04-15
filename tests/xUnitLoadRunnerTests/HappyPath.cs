using Xunit.Abstractions;
using xUnitLoadFramework;
using xUnitLoadFramework.Attributes;

namespace xUnitLoadRunnerTests
{
    public class HappyPath
    {

        [Fact]
        [LoadTestSettings(concurrency: 3, durationInMilliseconds: 10000, intervalInMilliseconds: 1000)]
        public void VeryFastTest()
        {
            Console.WriteLine("This is a very fast test");
        }
        
        [Fact]
        public void VeryFastTest2()
        {
            Console.WriteLine("This is a very fast test2");
        }
        
    }
}