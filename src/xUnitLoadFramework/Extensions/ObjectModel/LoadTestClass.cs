using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;

namespace xUnitLoadFramework.Extensions.ObjectModel;

[DebuggerDisplay(@"\{ class = {TestClassName} \}")]
public class LoadTestClass : ITestClass, IXunitSerializable
{
    internal static BindingFlags MethodBindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;

    Type? @class;
    LoadTestAssembly? testAssembly;
    readonly Lazy<LoadTestCollection> testCollection;
    readonly Lazy<IReadOnlyDictionary<string, IReadOnlyCollection<string>>> traits;
    readonly Lazy<string> uniqueID;

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
    public LoadTestClass()
    {
        testCollection = new(() => new LoadTestCollection(TestAssembly, Class.FullName ?? Class.Name));
        traits = new(() => ExtensibilityPointFactory.GetClassTraits(Class, TestCollection.Traits));
        uniqueID = new(() => UniqueIDGenerator.ForTestClass(TestCollection.UniqueID, TestClassName));
    }

#pragma warning disable CS0618
    public LoadTestClass(
        LoadTestAssembly testAssembly,
        Type @class) :
            this()
#pragma warning restore CS0618
    {
        this.@class = Guard.ArgumentNotNull(@class);
        this.testAssembly = Guard.ArgumentNotNull(testAssembly);
    }

    public Type Class =>
        @class ?? throw new InvalidOperationException($"Attempted to retrieve an uninitialized {nameof(LoadTestClass)}.{nameof(Class)}");

    public string DisplayName =>
        Class.Name.Replace('_', ' ');

    public MethodInfo[] Methods =>
        Class.GetMethods(MethodBindingFlags);

    LoadTestAssembly TestAssembly =>
        testAssembly ?? throw new InvalidOperationException($"Attempted to retrieve an uninitialized {nameof(LoadTestClass)}.{nameof(TestAssembly)}");

    public string TestClassName =>
        Class.FullName ?? throw new InvalidOperationException("Test class must have a full name");

    public string? TestClassNamespace =>
        Class.Namespace;

    public string TestClassSimpleName =>
        Class.ToSimpleName();

    public LoadTestCollection TestCollection =>
        testCollection.Value;

    ITestCollection ITestClass.TestCollection =>
        testCollection.Value;

    public IReadOnlyDictionary<string, IReadOnlyCollection<string>> Traits =>
        traits.Value;

    public string UniqueID =>
        uniqueID.Value;

    public void Deserialize(IXunitSerializationInfo info)
    {
        testAssembly = Guard.NotNull("Could not retrieve TestAssembly from serialization", info.GetValue<LoadTestAssembly>("a"));
        var typeName = Guard.NotNull("Could not retrieve TestClassName from serialization", info.GetValue<string>("c"));
        @class = Guard.NotNull(() => $"Failed to deserialize type '{typeName}' in assembly '{testAssembly.AssemblyName}'", TypeHelper.GetType(testAssembly.AssemblyName, typeName));
    }

    public void Serialize(IXunitSerializationInfo info)
    {
        info.AddValue("a", TestAssembly);
        info.AddValue("c", TestClassName);
    }
}
