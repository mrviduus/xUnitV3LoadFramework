using xUnitLoadFramework;
using xUnitLoadFramework.Attributes;

namespace xUnitLoadDemo;

public class UnitTest1
{
    [Fact]
    [LoadTestSettings(concurrency: 3, durationInMilliseconds: 1, intervalInMilliseconds: 1)]
    public void Test1()
    {
        Console.WriteLine("This is a very fast test");
    }
}
