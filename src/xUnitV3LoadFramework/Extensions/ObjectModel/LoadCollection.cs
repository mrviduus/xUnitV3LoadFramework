// Import xUnit internal utilities for argument validation
using Xunit.Internal;
// Import xUnit SDK for test collection infrastructure
using Xunit.Sdk;
// Import xUnit v3 core interfaces for test management
using Xunit.v3;

namespace xUnitV3LoadFramework.Extensions.ObjectModel;

/// <summary>
/// Represents a collection of load tests within a test assembly.
/// Groups related load tests together for organized execution and reporting.
/// Implements ITestCollection to integrate with xUnit's test collection system.
/// </summary>
public class LoadCollection(
    LoadTestAssembly testAssembly,
    string displayName) :
        ITestCollection
{
    /// <summary>
    /// Gets the LoadTestAssembly that contains this test collection.
    /// Provides strongly-typed access to the assembly with load testing metadata.
    /// </summary>
    public LoadTestAssembly TestAssembly { get; } =
        Guard.ArgumentNotNull(testAssembly);

    /// <summary>
    /// Gets the test assembly as the base ITestAssembly interface.
    /// Required for ITestCollection implementation and framework integration.
    /// </summary>
    ITestAssembly ITestCollection.TestAssembly =>
        TestAssembly;

    /// <summary>
    /// Gets the name of the test collection definition class.
    /// Returns null as load test collections don't use collection definition classes.
    /// </summary>
    public string? TestCollectionClassName =>
        null;

    /// <summary>
    /// Gets the display name for this test collection.
    /// Used in test runners and reports to identify the collection.
    /// </summary>
    public string TestCollectionDisplayName =>
        Guard.ArgumentNotNull(displayName);

    /// <summary>
    /// Gets the traits associated with this test collection.
    /// Includes metadata for test categorization and filtering.
    /// </summary>
    public IReadOnlyDictionary<string, IReadOnlyCollection<string>> Traits { get; } =
        // Extract collection traits using xUnit's extensibility factory
        // No collection definition class, so pass null and use assembly traits
        ExtensibilityPointFactory.GetCollectionTraits(testCollectionDefinition: null, testAssembly.Traits);

    /// <summary>
    /// Gets the unique identifier for this test collection.
    /// Generated using xUnit's standard algorithm for collection identification.
    /// </summary>
    public string UniqueID { get; } =
        // Generate unique ID using assembly ID, display name, and no collection definition
        UniqueIDGenerator.ForTestCollection(testAssembly.UniqueID, displayName, collectionDefinitionClassName: null);
}
