using System.Reflection;
using Xunit.v3;

namespace ObservationExample;

public class LoadTestFramework : TestFramework
{
    public override string TestFrameworkDisplayName =>
        "Observation Framework";

    protected override ITestFrameworkDiscoverer CreateDiscoverer(Assembly assembly) =>
        new LoadDiscoverer(new LoadTestAssembly(assembly));

    protected override ITestFrameworkExecutor CreateExecutor(Assembly assembly) =>
        new LoadExecutor(new LoadTestAssembly(assembly));
}
