using Xunit.Abstractions;
using Xunit.Sdk;

namespace xUnitLoadFramework;

public class LoadTestCase : IXunitTestCase
{
    public IMethodInfo Method { get; private set; }
    public Exception InitializationException { get; private set; }
    public int Timeout { get; private set; }
    public string DisplayName { get; private set; }
    public string SkipReason { get; private set; }
    public ISourceInformation SourceInformation { get; set; }
    public ITestMethod TestMethod { get; private set; }
    public object[] TestMethodArguments { get; private set; }
    public Dictionary<string, List<string>> Traits { get; private set; } = new();
    public string UniqueID { get; }

    // Default constructor for deserialization
    public LoadTestCase(Exception initializationException, string skipReason, ISourceInformation sourceInformation, string uniqueId, IMethodInfo method, string displayName, ITestMethod testMethod, object[] testMethodArguments)
    {
        InitializationException = initializationException;
        SkipReason = skipReason;
        SourceInformation = sourceInformation;
        UniqueID = uniqueId;
        Method = method;
        DisplayName = displayName;
        TestMethod = testMethod;
        TestMethodArguments = testMethodArguments;
    }

    public LoadTestCase(ITestMethod testMethod, string displayName, Exception initializationException, string skipReason, ISourceInformation sourceInformation, string uniqueId, IMethodInfo method, object[] testMethodArguments = null)
    {
        TestMethod = testMethod;
        DisplayName = displayName;
        InitializationException = initializationException;
        SkipReason = skipReason;
        SourceInformation = sourceInformation;
        UniqueID = uniqueId;
        TestMethodArguments = testMethodArguments ?? Array.Empty<object>();
        Method = testMethod.Method;
    }

    public async Task<RunSummary> RunAsync(IMessageSink diagnosticMessageSink,
        IMessageBus messageBus,
        object[] constructorArguments,
        ExceptionAggregator aggregator,
        CancellationTokenSource cancellationTokenSource)
    {
        var runner = new LoadTestCaseRunner<IXunitTestCase>(
            testCase: this,
            diagnosticMessageSink: diagnosticMessageSink,
            messageBus: messageBus,
            aggregator: aggregator,
            cancellationTokenSource: cancellationTokenSource,
            constructorArguments: constructorArguments);

        return await runner.RunAsync();
    }

    public void Serialize(IXunitSerializationInfo info)
    {
        info.AddValue("DisplayName", DisplayName);
        info.AddValue("SkipReason", SkipReason);
    }

    public void Deserialize(IXunitSerializationInfo info)
    {
        DisplayName = info.GetValue<string>("DisplayName");
        SkipReason = info.GetValue<string>("SkipReason");
    }
}