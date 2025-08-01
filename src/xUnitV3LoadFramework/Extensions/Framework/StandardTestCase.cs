using System.Reflection;
using Xunit;
using Xunit.v3;
using xUnitV3LoadFramework.Extensions.ObjectModel;

namespace xUnitV3LoadFramework.Extensions.Framework;

/// <summary>
/// Test case for standard [Fact] and [Theory] tests in mixed framework scenario
/// This allows standard xUnit tests to be executed within the LoadTestFramework
/// </summary>
public class StandardTestCase : LoadTestCase
{
    private readonly FactAttribute factAttribute;

    public StandardTestCase(
        LoadTestMethod testMethod, 
        FactAttribute factAttribute,
        string sourceFilePath = "",
        int sourceLineNumber = 0) 
        : base(testMethod, 0, sourceFilePath, sourceLineNumber) // Use order 0 for standard tests
    {
        this.factAttribute = factAttribute;
        // Set standard test properties
        this.Concurrency = 1; // Standard tests run once
        this.Duration = 0; // No duration for standard tests
        this.Interval = 0; // No interval for standard tests
        this.SkipReason = factAttribute.Skip;
    }

    /// <summary>
    /// Indicates this is a standard xUnit test, not a load test
    /// </summary>
    public bool IsStandardTest => true;

    /// <summary>
    /// Gets the underlying [Fact] or [Theory] attribute
    /// </summary>
    public FactAttribute FactAttribute => factAttribute;
}
