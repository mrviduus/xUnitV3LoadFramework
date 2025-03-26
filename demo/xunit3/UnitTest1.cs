using xUnitLoadFramework;
using xUnitLoadFramework.Attributes;

namespace xunit3;

public class UnitTest1
{
    [Fact]
    [LoadTestSettings(concurrency: 3, DurationInSeconds = 1, IntervalInSeconds = 1)]
    public void Test1()
    {
        Console.WriteLine("This is a very fast test");
    }
}
