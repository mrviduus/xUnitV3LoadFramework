using xUnitV3LoadFramework.Extensions.Framework;
using xUnitV3LoadTest;

[assembly:Xunit.TestFramework(typeof(ExampleTestFramework))]

namespace xUnitV3LoadTest;


public class ExampleTestFramework: LoadTestFramework
{
    
}