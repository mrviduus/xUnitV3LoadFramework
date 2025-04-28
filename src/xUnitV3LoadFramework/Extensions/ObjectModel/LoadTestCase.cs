using System.ComponentModel;
using System.Diagnostics;
using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;

namespace xUnitV3LoadFramework.Extensions.ObjectModel;

[DebuggerDisplay(@"\{ class = {TestMethod.TestClass.Class.Name}, method = {TestMethod.Method.Name}, display = {TestCaseDisplayName} \}")]
public class LoadTestCase : ITestCase, IXunitSerializable
{
	LoadTestMethod? testMethod;

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
	public LoadTestCase()
	{ }

	/// <summary>
	/// Initializes a new instance of the <see cref="XunitTestCase"/> class.
	/// </summary>
	/// <param name="testMethod">The test method this test case belongs to.</param>
	/// <param name="order">The value from <see cref="LoadAttribute.Order"/>.</param>
	public LoadTestCase(
		LoadTestMethod testMethod,
		int order)
	{
		this.testMethod = Guard.ArgumentNotNull(testMethod);
		Order = order;
	}

	public LoadTestCase(
		LoadTestMethod testMethod,
		int concurrency,
		int duration,
		int interval)
	{
		testMethod = Guard.ArgumentNotNull(testMethod);
		Concurrency = concurrency;
		Duration = duration;
		Interval = interval;
	}

	bool ITestCaseMetadata.Explicit =>
		false;

	public int Order { get; private set; }
	public int Concurrency { get; set; }
	public int Duration { get; set; }
	public int Interval { get; set; }

	string? ITestCaseMetadata.SkipReason =>
		null;

	string? ITestCaseMetadata.SourceFilePath =>
		null;

	int? ITestCaseMetadata.SourceLineNumber =>
		null;

	public string TestCaseDisplayName =>
		$"{TestClass.DisplayName}, it {TestMethod.DisplayName}";

	public LoadTestClass TestClass =>
		TestMethod.TestClass;

	ITestClass? ITestCase.TestClass =>
		TestClass;

	int? ITestCaseMetadata.TestClassMetadataToken =>
		TestClass.Class.MetadataToken;

	string? ITestCaseMetadata.TestClassName =>
		TestClass.TestClassName;

	string? ITestCaseMetadata.TestClassNamespace =>
		TestClass.TestClassNamespace;

	string? ITestCaseMetadata.TestClassSimpleName =>
		TestClass.TestClassSimpleName;

	public LoadCollection TestCollection =>
		TestMethod.TestClass.TestCollection;

	ITestCollection ITestCase.TestCollection =>
		TestCollection;

	public LoadTestMethod TestMethod =>
		testMethod ?? throw new InvalidOperationException($"Attempted to retrieve an uninitialized {nameof(LoadTestCase)}.{nameof(TestMethod)}");

	ITestMethod? ITestCase.TestMethod =>
		TestMethod;

	int? ITestCaseMetadata.TestMethodMetadataToken =>
		TestMethod.Method.MetadataToken;

	string? ITestCaseMetadata.TestMethodName =>
		TestMethod.MethodName;

	string[]? ITestCaseMetadata.TestMethodParameterTypesVSTest =>
		null;

	string? ITestCaseMetadata.TestMethodReturnTypeVSTest =>
		null;

	public IReadOnlyDictionary<string, IReadOnlyCollection<string>> Traits =>
		TestMethod.Traits;

	public string UniqueID =>
		UniqueIDGenerator.ForTestCase(TestMethod.UniqueID, testMethodGenericTypes: null, testMethodArguments: null);

	public void Deserialize(IXunitSerializationInfo info)
	{
		testMethod = Guard.NotNull("Could not retrieve TestMethod from serialization", info.GetValue<LoadTestMethod>("tm"));
		Order = info.GetValue<int>("order");
		Concurrency = info.GetValue<int>("concurrency");
		Duration = info.GetValue<int>("duration");
		Interval = info.GetValue<int>("interval");
	}

	public void Serialize(IXunitSerializationInfo info)
	{
		info.AddValue("tm", TestMethod);
		info.AddValue("order", Order);
		info.AddValue("concurrency", Concurrency);
		info.AddValue("duration", Duration);
		info.AddValue("interval", Interval);
	}
}
