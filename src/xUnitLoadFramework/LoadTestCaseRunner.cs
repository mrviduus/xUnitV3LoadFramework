// using System.Diagnostics;
// using Xunit.Abstractions;
// using Xunit.Sdk;

// namespace xUnitLoadFramework
// {
//     public class LoadTestCaseRunner<TTestCase> : TestCaseRunner<TTestCase>
//         where TTestCase : IXunitTestCase
//     {
//         private readonly object[] _constructorArguments;
//         private readonly IMessageSink _diagnosticMessageSink;

//         public LoadTestCaseRunner(TTestCase testCase,
//                                   IMessageSink diagnosticMessageSink,
//                                   IMessageBus messageBus,
//                                   ExceptionAggregator aggregator,
//                                   CancellationTokenSource cancellationTokenSource,
//                                   object[] constructorArguments)
//             : base(testCase, messageBus, aggregator, cancellationTokenSource)
//         {
//             _diagnosticMessageSink = diagnosticMessageSink;
//             _constructorArguments = constructorArguments;
//         }

//         protected override async Task<RunSummary> RunTestAsync()
//         {
//             var runSummary = new RunSummary();

//             try
//             {
//                 var stopwatch = Stopwatch.StartNew();

//                 var summary = await TestCase.RunAsync(
//                     diagnosticMessageSink: _diagnosticMessageSink,
//                     messageBus: MessageBus,
//                     constructorArguments: _constructorArguments,
//                     aggregator: Aggregator,
//                     cancellationTokenSource: CancellationTokenSource);

//                 stopwatch.Stop();

//                 runSummary.Total = summary.Total;
//                 runSummary.Failed = summary.Failed;
//                 runSummary.Skipped = summary.Skipped;

//                 _diagnosticMessageSink.OnMessage(new DiagnosticMessage(
//                     $"Test Case '{TestCase.DisplayName}' completed in {runSummary.Time:F2}s - Total: {runSummary.Total}, Failed: {runSummary.Failed}, Skipped: {runSummary.Skipped}"));
//             }
//             catch (Exception ex)
//             {
//                 Aggregator.Add(ex);
//                 _diagnosticMessageSink.OnMessage(new DiagnosticMessage(
//                     $"Error executing Test Case '{TestCase.DisplayName}': {ex.Message}"));
//             }

//             return runSummary;
//         }

//         protected override Task AfterTestCaseStartingAsync()
//         {
//             _diagnosticMessageSink.OnMessage(new DiagnosticMessage($"Starting test case: {TestCase.DisplayName}"));
//             return Task.CompletedTask;
//         }

//         protected override Task BeforeTestCaseFinishedAsync()
//         {
//             _diagnosticMessageSink.OnMessage(new DiagnosticMessage($"Finishing test case: {TestCase.DisplayName}"));
//             return Task.CompletedTask;
//         }
//     }
// }
