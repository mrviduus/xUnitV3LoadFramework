// Import System namespace for attribute functionality
using System;

namespace xUnitV3LoadFramework.Attributes;

/// <summary>
/// Marks a test class to use the Load Testing Framework instead of standard xUnit execution.
/// When applied to a class, all methods with [Load] attribute will be executed as load tests
/// using the custom LoadTestFramework with performance monitoring and result aggregation.
/// Classes without this attribute will use standard xUnit test execution behavior.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class UseLoadFrameworkAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the UseLoadFrameworkAttribute class.
    /// This parameterless constructor enables simple attribute application to test classes.
    /// </summary>
    public UseLoadFrameworkAttribute()
    {
        // No initialization required - attribute serves as a marker
        // for framework selection during test discovery and execution
    }
}
