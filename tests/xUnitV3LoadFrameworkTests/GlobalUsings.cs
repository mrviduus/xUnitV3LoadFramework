using xUnitOTel.Attributes;
using xUnitV3LoadTests;

[assembly: Trace]
[assembly: AssemblyFixture(typeof(TestSetup))]
[assembly: CaptureConsole]