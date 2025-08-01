using System.ComponentModel;
using System.Diagnostics;
using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;
using xUnitV3LoadFramework.Attributes;

namespace xUnitV3LoadFramework.Extensions.ObjectModel;

[DebuggerDisplay(@"\{ class = {TestMethod.TestClass.Class.Name}, method = {TestMethod.Method.Name}, display = {TestCaseDisplayName} \}")]
public class LoadTestCase : ITestCase, IXunitSerializable
{
	LoadTestMethod? testMethod;

	public int Order { get; private set; }
	public int Concurrency { get; set; }
	public int Duration { get; set; }
	public int Interval { get; set; }
	public string? SkipReason { get; set; }
	public string? SourceFilePath { get; set; }
	public int? SourceLineNumber { get; set; }

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
	public LoadTestCase()
	{ }

	/// <summary>
	/// Initializes a new instance of the <see cref="LoadTestCase"/> class.
	/// </summary>
	/// <param name="testMethod">The test method this test case belongs to.</param>
	/// <param name="order">The value from <see cref="LoadAttribute.Order"/>.</param>
	/// <param name="sourceFilePath">Source file path from the LoadAttribute</param>
	/// <param name="sourceLineNumber">Source line number from the LoadAttribute</param>
	public LoadTestCase(
		LoadTestMethod testMethod,
		int order,
		string? sourceFilePath = null,
		int? sourceLineNumber = null)
	{
		this.testMethod = Guard.ArgumentNotNull(testMethod);
		Order = order;
		SourceFilePath = sourceFilePath;
		SourceLineNumber = sourceLineNumber;
	}

	bool ITestCaseMetadata.Explicit =>
		false;

	string? ITestCaseMetadata.SkipReason =>
		SkipReason;

	string? ITestCaseMetadata.SourceFilePath =>
		SourceFilePath;

	int? ITestCaseMetadata.SourceLineNumber =>
		SourceLineNumber;

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

	public int? TestMethodArity => TestMethod.Method.IsGenericMethodDefinition ? TestMethod.Method.GetGenericArguments().Length : 0;

	public void Deserialize(IXunitSerializationInfo info)
	{
		testMethod = Guard.NotNull("Could not retrieve TestMethod from serialization", info.GetValue<LoadTestMethod>("tm"));
		Order = info.GetValue<int>("order");
		Concurrency = info.GetValue<int>("concurrency");
		Duration = info.GetValue<int>("duration");
		Interval = info.GetValue<int>("interval");
		SkipReason = info.GetValue<string?>("skipReason");
		SourceFilePath = info.GetValue<string?>("sourceFilePath");
		SourceLineNumber = info.GetValue<int?>("sourceLineNumber");
	}

	public void Serialize(IXunitSerializationInfo info)
	{
		info.AddValue("tm", TestMethod);
		info.AddValue("order", Order);
		info.AddValue("concurrency", Concurrency);
		info.AddValue("duration", Duration);
		info.AddValue("interval", Interval);
		info.AddValue("skipReason", SkipReason);
		info.AddValue("sourceFilePath", SourceFilePath);
		info.AddValue("sourceLineNumber", SourceLineNumber);
	}
}
