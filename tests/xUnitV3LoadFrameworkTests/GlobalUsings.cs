using xUnit.OTel.Attributes;
using xUnitV3LoadTests;
[assembly: Trace]
[assembly: AssemblyFixture(typeof(TestSetup))]
// Custom test framework removed - using standard xUnit v3
//[assembly: TestFramework(typeof(LoadTestFramework))]
//[assembly: Trace]
[assembly: CaptureConsole]