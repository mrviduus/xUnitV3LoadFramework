using xUnit.OTel.Attributes;
using xUnitV3LoadFramework.Extensions.Framework;
using xUnitV3LoadTests;
[assembly: Trace]
[assembly: AssemblyFixture(typeof(TestSetup))]
[assembly: TestFramework(typeof(LoadTestFramework))]
//[assembly: Trace]
[assembly: CaptureConsole]