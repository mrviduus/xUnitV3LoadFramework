using xUnitLoadFramework;

namespace xUnitDemo;

public class DemoScenario
{
    [Fact]
    [LoadTestSettings(concurrency:10, DurationInSeconds = 5, IntervalInSeconds = 1)]
    public void Test1()
    {
        Console.WriteLine("This is a very fast test");
    }
}
