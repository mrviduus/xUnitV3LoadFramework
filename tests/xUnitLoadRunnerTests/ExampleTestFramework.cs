using Xunit.Abstractions;
using xUnitLoadFramework;

[assembly:Xunit.TestFramework("xUnitLoadRunnerTests.ExampleTestFramework", "xUnitLoadRunnerTests")]

namespace xUnitLoadRunnerTests;

public class ExampleTestFramework(IMessageSink messageSink) : LoadTestFramework(messageSink);