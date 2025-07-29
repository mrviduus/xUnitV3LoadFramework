// Import xUnit SDK for test case ordering infrastructure
using Xunit.Sdk;
// Import xUnit v3 core interfaces for test management
using Xunit.v3;
// Import custom object model for load testing components
using xUnitV3LoadFramework.Extensions.ObjectModel;

namespace xUnitV3LoadFramework.Extensions.Framework;

/// <summary>
/// Custom test case orderer that sorts load tests by their execution order.
/// Ensures load tests run in the sequence specified by the Order property
/// on the LoadAttribute, enabling controlled test execution flow.
/// </summary>
public class LoadTestCaseOrderer : ITestCaseOrderer
{
    /// <summary>
    /// Gets the singleton instance of the LoadTestCaseOrderer.
    /// Uses singleton pattern to avoid creating multiple orderer instances.
    /// </summary>
    public static LoadTestCaseOrderer Instance { get; } = new();

    /// <summary>
    /// Orders a collection of test cases based on their execution order.
    /// LoadTestCase objects are sorted by their Order property, while other
    /// test case types default to order 0 for consistent sequencing.
    /// </summary>
    /// <typeparam name="TTestCase">The type of test case being ordered</typeparam>
    /// <param name="testCases">Collection of test cases to order</param>
    /// <returns>Ordered collection with LoadTestCase objects sorted by Order property</returns>
    public IReadOnlyCollection<TTestCase> OrderTestCases<TTestCase>(IReadOnlyCollection<TTestCase> testCases)
        where TTestCase : notnull, ITestCase =>
            // Sort test cases by order, with LoadTestCase using Order property
            // and other test cases defaulting to order 0
            [.. testCases.OrderBy(tc => tc is LoadTestCase otc ? otc.Order : 0)];
}
