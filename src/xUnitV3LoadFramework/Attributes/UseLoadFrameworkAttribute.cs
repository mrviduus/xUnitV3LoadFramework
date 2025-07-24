using System;

namespace xUnitV3LoadFramework.Attributes;

/// <summary>
/// Marks a test class to use the Load Testing Framework.
/// When applied to a class, all methods with [Load] attribute will be executed as load tests.
/// Classes without this attribute will use standard xUnit test execution.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class UseLoadFrameworkAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UseLoadFrameworkAttribute"/> class.
    /// </summary>
    public UseLoadFrameworkAttribute()
    {
    }
}
