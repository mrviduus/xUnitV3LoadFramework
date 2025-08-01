using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;

namespace xUnitV3LoadFramework.Extensions.ObjectModel;

[DebuggerDisplay(@"\{ class = {TestClass.TestClassName}, method = {MethodName} \}")]
public class LoadTestMethod : ITestMethod, IXunitSerializable
{
	MethodInfo? method;
	LoadTestClass? testClass;
	readonly Lazy<IReadOnlyDictionary<string, IReadOnlyCollection<string>>> traits;
	readonly Lazy<string> uniqueID;

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
	public LoadTestMethod()
	{
		traits = new(() => ExtensibilityPointFactory.GetMethodTraits(Method, TestClass.Traits));
		uniqueID = new(() => UniqueIDGenerator.ForTestMethod(TestClass.UniqueID, MethodName));
	}

#pragma warning disable CS0618
	public LoadTestMethod(
		LoadTestClass testClass,
		MethodInfo method) :
			this()
#pragma warning restore CS0618
	{
		this.testClass = Guard.ArgumentNotNull(testClass);
		this.method = Guard.ArgumentNotNull(method);
	}

	public string DisplayName =>
		Method.Name.Replace('_', ' ');

	public MethodInfo Method =>
		method ?? throw new InvalidOperationException($"Attempted to retrieve an uninitialized {nameof(LoadTestMethod)}.{nameof(Method)}");

	public string MethodName =>
		Method.Name;

	public LoadTestClass TestClass =>
		testClass ?? throw new InvalidOperationException($"Attempted to retrieve an uninitialized {nameof(LoadTestMethod)}.{nameof(TestClass)}");

	ITestClass ITestMethod.TestClass =>
		TestClass;

	public IReadOnlyDictionary<string, IReadOnlyCollection<string>> Traits =>
		traits.Value;

	public string UniqueID =>
		uniqueID.Value;

	public int? MethodArity => throw new NotImplementedException();

	public void Deserialize(IXunitSerializationInfo info)
	{
		testClass = Guard.NotNull("Could not retrieve TestClass from serialization", info.GetValue<LoadTestClass>("c"));

		var reflectedType = Guard.NotNull("Could not retrieve the class name of the test method", info.GetValue<string>("t"));
		var @class = Guard.NotNull(() => $"Could not look up type {reflectedType}", TypeHelper.GetType(reflectedType));
		var methodName = Guard.NotNull("Could not retrieve MethodName from serialization", info.GetValue<string>("n"));
		method = Guard.NotNull(() => $"Could not find test method {methodName} on test class {testClass.TestClassName}", @class.GetMethod(methodName, LoadTestClass.MethodBindingFlags));
	}

	public void Serialize(IXunitSerializationInfo info)
	{
		Guard.NotNull("Method does not appear to come from a reflected type", Method.ReflectedType);
		Guard.NotNull("Method's reflected type does not have an assembly qualified name", Method.ReflectedType.AssemblyQualifiedName);

		info.AddValue("t", Method.ReflectedType.AssemblyQualifiedName);
		info.AddValue("n", Method.Name);
		info.AddValue("c", TestClass);
	}
}
