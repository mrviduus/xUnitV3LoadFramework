using System.Collections.Generic;
using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;

namespace xUnitV3LoadFramework.Attributes;

/// <summary>
/// Test case discoverer for LoadFact attributes in xUnit v3.
/// Creates specialized test cases that handle load testing scenarios.
/// </summary>
public class LoadFactDiscoverer : IXunitTestCaseDiscoverer
{
    /// <summary>
    /// Discovers test cases for methods marked with LoadFactAttribute.
    /// </summary>
    /// <param name="discoveryOptions">Discovery options from the test framework</param>
    /// <param name="testMethod">The test method being examined</param>
    /// <param name="factAttribute">The LoadFact attribute instance</param>
    /// <returns>Collection of test cases for execution</returns>
    public ValueTask<IReadOnlyCollection<IXunitTestCase>> DiscoverTestCases(
        _ITestFrameworkDiscoveryOptions discoveryOptions,
        _ITestMethod testMethod,
        _IAttributeInfo factAttribute)
    {
        Guard.ArgumentNotNull(discoveryOptions);
        Guard.ArgumentNotNull(testMethod);
        Guard.ArgumentNotNull(factAttribute);

        // Extract load testing parameters from the attribute
        var order = factAttribute.GetNamedArgument<int>(nameof(LoadFactAttribute.Order));
        var concurrency = factAttribute.GetNamedArgument<int>(nameof(LoadFactAttribute.Concurrency));
        var duration = factAttribute.GetNamedArgument<int>(nameof(LoadFactAttribute.Duration));
        var interval = factAttribute.GetNamedArgument<int>(nameof(LoadFactAttribute.Interval));

        // Create a load test case with the specified parameters
        var testCase = new LoadFactTestCase(
            testMethod,
            order,
            concurrency,
            duration,
            interval);

        var testCases = new List<IXunitTestCase> { testCase };
        return new ValueTask<IReadOnlyCollection<IXunitTestCase>>(testCases);
    }
}
