using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace xUnitLoadRunner;

public class LoadTestFramework : XunitTestFramework
{
    public LoadTestFramework(IMessageSink messageSink) : base(messageSink)
    {
    }

    protected override ITestFrameworkExecutor CreateExecutor(AssemblyName assemblyName)
        => new LoadTestFrameworkExecutor(assemblyName, SourceInformationProvider, DiagnosticMessageSink);
}
