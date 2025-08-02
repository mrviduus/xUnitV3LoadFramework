// Import System namespace for attribute functionality
using System;

namespace xUnitV3LoadFramework.Attributes;

/// <summary>
/// Marks a test class to use the Load Testing Framework instead of standard xUnit execution.
/// When applied to a class, all methods with [Load] attribute will be executed as load tests
/// using the custom LoadTestFramework with performance monitoring and result aggregation.
/// Classes without this attribute will use standard xUnit test execution behavior.
/// 
/// DEPRECATED: Use UseStressFrameworkAttribute instead. This will be removed in v3.0.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
[Obsolete("Use UseStressFrameworkAttribute instead. UseLoadFrameworkAttribute will be removed in v3.0.", false)]
public sealed class UseLoadFrameworkAttribute : UseStressFrameworkAttribute
{
    /// <summary>
    /// Initializes a new instance of the UseLoadFrameworkAttribute class.
    /// This parameterless constructor enables simple attribute application to test classes.
    /// </summary>
    public UseLoadFrameworkAttribute() : base()
    {
        // All functionality is now handled by the UseStressFrameworkAttribute base class
    }
}
