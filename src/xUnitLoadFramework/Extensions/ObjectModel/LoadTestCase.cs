using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;

namespace ObservationExample;

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

    bool ITestCaseMetadata.Explicit =>
        false;

    public int Order { get; private set; }

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

    public LoadTestCollection TestCollection =>
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
        Order = info.GetValue<int>("o");
    }

    public void Serialize(IXunitSerializationInfo info)
    {
        info.AddValue("o", Order);
        info.AddValue("tm", TestMethod);
    }
}
