using System.Reflection;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;
using xUnitV3LoadFramework.Extensions.ObjectModel;

namespace xUnitV3LoadFramework.Extensions.Framework;

/// <summary>
/// Test case for standard [Fact] tests in mixed framework scenario
/// </summary>
public class StandardFactTestCase : ITestCase
{
    private readonly LoadTestMethod testMethod;

    public StandardFactTestCase(LoadTestMethod testMethod)
    {
        this.testMethod = testMethod;
    }

    public string DisplayName => testMethod.DisplayName;

    public string? SkipReason => testMethod.Method.GetCustomAttribute<FactAttribute>()?.Skip;

    public Exception? SkipUnless => null;

    public Exception? SkipWhen => null;

    public Type?[] SkipWhenTypes => Array.Empty<Type>();

    public ITestClass TestClass => testMethod.TestClass;

    public ITestCollection TestCollection => testMethod.TestClass.TestCollection;

    public ITestMethod TestMethod => testMethod;

    public string? Timeout => null;

    public IReadOnlyDictionary<string, IReadOnlyCollection<string>> Traits => testMethod.Traits;

    public string UniqueID => testMethod.UniqueID;

    public void Deserialize(IXunitSerializationInfo info)
    {
        // Implementation for deserialization if needed
    }

    public void Serialize(IXunitSerializationInfo info)
    {
        // Implementation for serialization if needed
    }
}
