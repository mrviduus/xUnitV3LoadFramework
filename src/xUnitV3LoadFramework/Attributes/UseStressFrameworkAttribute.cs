// Import System namespace for attribute functionality
using System;

namespace xUnitV3LoadFramework.Attributes;

/// <summary>
/// Marks a test class to use the Stress Testing Framework instead of standard xUnit execution.
/// When applied to a class, all methods with [Stress] attribute will be executed as stress tests
/// using the custom StressTestFramework with performance monitoring and result aggregation.
/// Classes without this attribute will use standard xUnit test execution behavior.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class UseStressFrameworkAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the UseStressFrameworkAttribute class.
    /// This parameterless constructor enables simple attribute application to test classes.
    /// </summary>
    public UseStressFrameworkAttribute()
    {
        // No initialization required - attribute serves as a marker
        // for framework selection during test discovery and execution
    }
}
