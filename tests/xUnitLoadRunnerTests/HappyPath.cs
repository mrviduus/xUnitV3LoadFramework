using Xunit.Abstractions;
using xUnitLoadFramework;
using xUnitLoadFramework.Attributes;

namespace xUnitLoadRunnerTests
{
    public class HappyPath
    {

        [Fact]
        [LoadTestSettings(concurrency: 3, durationInMilliseconds: 5000, intervalInMilliseconds: 1000)]
        public void VeryFastTest()
        {
            Console.WriteLine("This is a very fast test");
        }
        
        [Fact]
        public void VeryFastTest2()
        {
            Console.WriteLine("This is a very fast test2");
        }
        
        [Fact(Skip = "This test is skipped")]
        [LoadTestSettings(concurrency: 3, durationInMilliseconds: 5000, intervalInMilliseconds: 1000)]
        public void VeryFastTest3()
        {
            Console.WriteLine("This is a very fast test");
        }
        
        
        
    }
}