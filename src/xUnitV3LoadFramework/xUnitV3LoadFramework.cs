// using System.Globalization;
// using System.Reflection;
// using Xunit.Internal;
// using Xunit.Sdk;
// using Xunit.v3;
//
// [assembly: TestFramework(typeof(xUnitV3LoadFramework.LoadTestFramework))]
//
// namespace xUnitV3LoadFramework
// {
//     public class LoadTestFramework : ITestFramework
//     {
//         readonly string? configFileName;
//
//         public LoadTestFramework()
//         {
//         }
//
//         public LoadTestFramework(string? configFileName)
//         {
//             this.configFileName = configFileName;
//         }
//
//         internal static string DisplayName { get; } =
//             string.Format(CultureInfo.InvariantCulture, "🔧 LoadTest Framework (xUnit v3 base {0})", ThisAssembly.AssemblyInformationalVersion);
//
//         public override string TestFrameworkDisplayName =>
//             DisplayName;
//
//         protected override ITestFrameworkDiscoverer CreateDiscoverer(Assembly assembly)
//         {
//             var testAssembly = new XunitTestAssembly(
//                 Guard.ArgumentNotNull(assembly),
//                 configFileName,
//                 assembly.GetName().Version
//             );
//
//             return new XunitTestFrameworkDiscoverer(testAssembly);
//         }
//
//         protected override ITestFrameworkExecutor CreateExecutor(Assembly assembly)
//         {
//             var testAssembly = new XunitTestAssembly(
//                 Guard.ArgumentNotNull(assembly),
//                 configFileName,
//                 assembly.GetName().Version
//             );
//
//             return new LoadTestFrameworkExecutor(testAssembly);
//         }
//
//
//     public class LoadTestFrameworkExecutor(IXunitTestAssembly testAssembly)
//         : TestFrameworkExecutor<IXunitTestCase>(testAssembly)
//     {
//         readonly Lazy<XunitTestFrameworkDiscoverer> discoverer = new(() => new(testAssembly));
//
//         protected new IXunitTestAssembly TestAssembly { get; } = Guard.ArgumentNotNull(testAssembly);
//
//         protected override ITestFrameworkDiscoverer CreateDiscoverer() =>
//             discoverer.Value;
//
//         public override async ValueTask RunTestCases(
//             IReadOnlyCollection<IXunitTestCase> testCases,
//             IMessageSink executionMessageSink,
//             ITestFrameworkExecutionOptions executionOptions,
//             CancellationToken cancellationToken)
//         {
//             // 💡 Custom behavior: use our own AssemblyRunner
//             var assemblyRunner = new LoadTestAssemblyRunner();
//
//             await assemblyRunner.Run(TestAssembly, testCases, executionMessageSink, executionOptions, cancellationToken);
//         }
//
//         static void SetEnvironment(string environmentVariableName, int? value)
//         {
//             if (value.HasValue)
//                 Environment.SetEnvironmentVariable(environmentVariableName, value.Value.ToString(CultureInfo.InvariantCulture));
//         }
//     }
//
//     public class LoadTestAssemblyRunner : XunitTestAssemblyRunnerBase<XunitTestAssemblyRunnerContext, IXunitTestAssembly, IXunitTestCollection, IXunitTestCase>
//     {
//         public async ValueTask<RunSummary> Run(
//             IXunitTestAssembly testAssembly,
//             IReadOnlyCollection<IXunitTestCase> testCases,
//             IMessageSink executionMessageSink,
//             ITestFrameworkExecutionOptions executionOptions,
//             CancellationToken cancellationToken)
//         {
//             Guard.ArgumentNotNull(testAssembly);
//             Guard.ArgumentNotNull(testCases);
//             Guard.ArgumentNotNull(executionMessageSink);
//             Guard.ArgumentNotNull(executionOptions);
//
//             await using var context = new XunitTestAssemblyRunnerContext(testAssembly, testCases, executionMessageSink, executionOptions, cancellationToken);
//
//             await context.InitializeAsync();
//
//             // Add any custom logic here before running
//             executionMessageSink.OnMessage(new DiagnosticMessage($"[LoadRunner] Running {testCases.Count} test case(s) in assembly {testAssembly.Assembly.AssemblyPath}"));
//
//             return await Run(context);
//         }
//     }
//
//     public class LoadTestCollectionRunner : XunitTestCollectionRunner
//     {
//         public LoadTestCollectionRunner(
//             ITestCollection testCollection,
//             IReadOnlyCollection<IXunitTestCase> testCases,
//             IXunitTestAssembly testAssembly,
//             IMessageSink diagnosticMessageSink,
//             IMessageSink executionMessageSink,
//             ITestFrameworkExecutionOptions executionOptions)
//             : base(testCollection, testCases, testAssembly, diagnosticMessageSink, executionMessageSink, executionOptions)
//         {
//         }
//
//         protected override XunitTestClassRunner CreateClassRunner(ITestClass testClass, IReadOnlyCollection<IXunitTestCase> testCases)
//         {
//             DiagnosticMessageSink.OnMessage(new DiagnosticMessage($"🏷️ Running Class: {testClass.Class.Name}"));
//             return new LoadTestClassRunner(testClass, testCases, TestAssembly, TestCollection, DiagnosticMessageSink, ExecutionMessageSink, ExecutionOptions);
//         }
//     }
//
//     public class LoadTestClassRunner : XunitTestClassRunner
//     {
//         public LoadTestClassRunner(
//             ITestClass testClass,
//             IReadOnlyCollection<IXunitTestCase> testCases,
//             IXunitTestAssembly testAssembly,
//             ITestCollection testCollection,
//             IMessageSink diagnosticMessageSink,
//             IMessageSink executionMessageSink,
//             ITestFrameworkExecutionOptions executionOptions)
//             : base(testClass, testCases, testAssembly, testCollection, diagnosticMessageSink, executionMessageSink, executionOptions)
//         {
//         }
//
//         protected override XunitTestMethodRunner CreateTestMethodRunner(ITestMethod testMethod, IReadOnlyCollection<IXunitTestCase> testCases)
//         {
//             DiagnosticMessageSink.OnMessage(new DiagnosticMessage($"🧪 Running Method: {testMethod.Method.Name}"));
//             return new LoadTestMethodRunner(testMethod, testCases, TestClass, TestCollection, TestAssembly, DiagnosticMessageSink, ExecutionMessageSink, ExecutionOptions);
//         }
//     }
//
//     /// <summary>
//     /// Custom method runner for xUnit v3.
//     /// </summary>
//     public class LoadTestMethodRunner : XunitTestMethodRunnerBase<XunitTestMethodRunnerContext, IXunitTestMethod, IXunitTestCase>
//     {
//         public async ValueTask<RunSummary> Run(
//             IXunitTestMethod testMethod,
//             IReadOnlyCollection<IXunitTestCase> testCases,
//             ExplicitOption explicitOption,
//             IMessageBus messageBus,
//             ExceptionAggregator aggregator,
//             CancellationTokenSource cancellationTokenSource,
//             object?[] constructorArguments)
//         {
//             Guard.ArgumentNotNull(testCases);
//             Guard.ArgumentNotNull(messageBus);
//             Guard.ArgumentNotNull(constructorArguments);
//
//             await using var context = new XunitTestMethodRunnerContext(
//                 testMethod,
//                 testCases,
//                 explicitOption,
//                 messageBus,
//                 aggregator,
//                 cancellationTokenSource,
//                 constructorArguments
//             );
//
//             await context.InitializeAsync();
//
//             // 💡 Optional custom logic goes here
//             messageBus.QueueMessage(new DiagnosticMessage($"[LoadRunner] Running method: {testMethod.Method.Name}"));
//
//             return await Run(context);
//         }
//     }
// }