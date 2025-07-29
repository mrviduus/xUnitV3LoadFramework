// Import xUnit SDK for test infrastructure and ID generation
using Xunit.Sdk;

namespace xUnitV3LoadFramework.Extensions.ObjectModel;

/// <summary>
/// Represents a single load test instance that can be executed.
/// Wraps a LoadTestCase to provide the ITest interface required by xUnit framework.
/// Each LoadTest corresponds to one execution instance of a load test method.
/// </summary>
public class LoadTest(LoadTestCase testCase) :
    ITest
{
    /// <summary>
    /// Gets the LoadTestCase that defines this load test's configuration and behavior.
    /// Provides strongly-typed access to load-specific test case properties.
    /// </summary>
    public LoadTestCase TestCase { get; } = testCase;

    /// <summary>
    /// Gets the test case as the base ITestCase interface.
    /// Required for ITest implementation and framework integration.
    /// </summary>
    ITestCase ITest.TestCase => TestCase;

    /// <summary>
    /// Gets the display name for this test as shown in test runners and reports.
    /// Uses the test case's display name for consistent identification.
    /// </summary>
    public string TestDisplayName { get; } = testCase.TestCaseDisplayName;

    /// <summary>
    /// Gets the traits associated with this test for categorization and filtering.
    /// Inherits traits from the underlying test case for metadata consistency.
    /// </summary>
    public IReadOnlyDictionary<string, IReadOnlyCollection<string>> Traits =>
        TestCase.Traits;

    /// <summary>
    /// Gets the unique identifier for this test instance.
    /// Generated using xUnit's standard algorithm with test case ID and instance index.
    /// </summary>
    public string UniqueID =>
        // Generate unique ID for this test instance using test case ID
        // Index 0 indicates this is the primary instance of the test case
        UniqueIDGenerator.ForTest(TestCase.UniqueID, 0);
}