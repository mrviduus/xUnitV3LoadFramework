using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit.Sdk;
using Xunit.v3;
using xUnitV3LoadFramework.Attributes;

namespace xUnitV3LoadFramework.Discovery;

/// <summary>
/// Discovers test cases for methods decorated with <see cref="LoadAttribute"/>.
/// Creates <see cref="LoadTestCase"/> instances that execute the test method as a load test.
/// </summary>
public class LoadTestCaseDiscoverer : IXunitTestCaseDiscoverer
{
    /// <summary>
    /// Discovers test cases from a test method decorated with <see cref="LoadAttribute"/>.
    /// </summary>
    /// <param name="discoveryOptions">The discovery options to be used.</param>
    /// <param name="testMethod">The test method the test cases belong to.</param>
    /// <param name="factAttribute">The Load attribute attached to the test method.</param>
    /// <returns>A single <see cref="LoadTestCase"/> for the test method.</returns>
    public ValueTask<IReadOnlyCollection<IXunitTestCase>> Discover(
        ITestFrameworkDiscoveryOptions discoveryOptions,
        IXunitTestMethod testMethod,
        IFactAttribute factAttribute)
    {
        var loadAttribute = (LoadAttribute)factAttribute;

        var testCase = new LoadTestCase(
            testMethod,
            loadAttribute.Concurrency,
            loadAttribute.Duration,
            loadAttribute.Interval,
            loadAttribute.DisplayName,
            loadAttribute.Skip,
            loadAttribute.Explicit,
            loadAttribute.Timeout);

        IReadOnlyCollection<IXunitTestCase> result = new IXunitTestCase[] { testCase };
        return new ValueTask<IReadOnlyCollection<IXunitTestCase>>(result);
    }
}
