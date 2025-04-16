using LoadRunnerCore.Models;
using LoadRunnerCore.Runner;
using Xunit.Abstractions;
using Xunit.Sdk;
using xUnitLoadFramework.Attributes;

namespace xUnitLoadFramework
{
    public class LoadTestMethodRunner : XunitTestMethodRunner
    {
        private readonly IMessageSink _diagnosticMessageSink;
        private readonly object[] _constructorArguments;

        public LoadTestMethodRunner(
            ITestMethod testMethod,
            IReflectionTypeInfo @class,
            IReflectionMethodInfo method,
            IEnumerable<IXunitTestCase> testCases,
            IMessageSink diagnosticMessageSink,
            IMessageBus messageBus,
            ExceptionAggregator aggregator,
            CancellationTokenSource cancellationTokenSource,
            object[] constructorArguments)
            : base(testMethod, @class, method, testCases, diagnosticMessageSink, messageBus, aggregator,
                cancellationTokenSource, constructorArguments)
        {
            _diagnosticMessageSink = diagnosticMessageSink;
            _constructorArguments = constructorArguments;
        }

        protected override async Task<RunSummary> RunTestCaseAsync(IXunitTestCase testCase)
        {
            if (!string.IsNullOrEmpty(testCase.SkipReason))
            {
                // If test case is skipped, bypass load test logic entirely
                return await base.RunTestCaseAsync(testCase);
            }
            var loadTestSettings = GetLoadTestSettings();
            var settings = CreateLoadSettings(loadTestSettings);
            if (settings == null)
            {
                return await base.RunTestCaseAsync(testCase);
            }
            var parameters = GetTestMethodParameters(testCase);
            var test = $"{TestMethod.TestClass.Class.Name}.{TestMethod.Method.Name}({parameters})";
            _diagnosticMessageSink.OnMessage(new DiagnosticMessage($"STARTED: {test}"));
            var xunitTest = new XunitTest(testCase, testCase.DisplayName);

            try
            {


                var executionPlan = CreateExecutionPlan(testCase, settings);
                var loadResult = await LoadRunner.Run(executionPlan);

                // Detailed results reported clearly
                ReportLoadResult(test, loadResult);

                // Aggregated result reporting
                return ReportAggregatedResult(xunitTest, loadResult);
            }
            catch (Exception ex)
            {
                return HandleExecutionException(xunitTest, ex);
            }
        }

        private object? GetLoadTestSettings()
        {
            return TestMethod.TestClass.Class.GetCustomAttributes(typeof(LoadTestSettingsAttribute)).FirstOrDefault()
                   ?? TestMethod.Method.GetCustomAttributes(typeof(LoadTestSettingsAttribute)).FirstOrDefault();
        }

        private LoadSettings? CreateLoadSettings(object? loadTestSettings)
        {
            if (loadTestSettings != null)
            {
                var properties = loadTestSettings.GetType().GetProperties();
                var settingsAttribute =
                    properties.FirstOrDefault(x => x.GetValue(loadTestSettings) is LoadTestSettingsAttribute)
                        ?.GetValue(loadTestSettings) as LoadTestSettingsAttribute;

                if (settingsAttribute != null)
                {
                    return new LoadSettings
                    {
                        Concurrency = settingsAttribute.Concurrency,
                        Duration = TimeSpan.FromMilliseconds(settingsAttribute.DurationInMilliseconds),
                        Interval = TimeSpan.FromMilliseconds(settingsAttribute.IntervalInMilliseconds),
                    };
                }
            }

            return null;
        }

        private string GetTestMethodParameters(IXunitTestCase testCase)
        {
            return testCase.TestMethodArguments != null
                ? string.Join(", ", testCase.TestMethodArguments.Select(a => a?.ToString() ?? "null"))
                : string.Empty;
        }

        private LoadExecutionPlan CreateExecutionPlan(IXunitTestCase testCase, LoadSettings settings)
        {
            return new LoadExecutionPlan
            {
                Name = testCase.DisplayName,
                Action = async () =>
                {
                    // Execute the test case only once per iteration using LoadRunnerCore
                    var summary = await ExecuteSingleTestInvocation(testCase);
                    return summary;
                },
                Settings = settings
            };
        }

        // Properly isolated single invocation clearly using xUnit infrastructure
        private async Task<bool> ExecuteSingleTestInvocation(IXunitTestCase testCase)
        {
            var aggregator = new ExceptionAggregator();
            var cancellationTokenSource = new CancellationTokenSource();
            using var silentBus = new SilentMessageBus();

            var result = await testCase.RunAsync(
                diagnosticMessageSink: _diagnosticMessageSink,
                messageBus: silentBus,
                constructorArguments: _constructorArguments,
                aggregator: aggregator,
                cancellationTokenSource: cancellationTokenSource
            );

            // Determine success from captured messages
            bool passed = result.Failed == 0 && !aggregator.HasExceptions;

            return passed;
        }
        private void ReportLoadResult(string test, LoadResult result)
        {
            var summaryMessage =
                $"[LOAD TEST RESULT] {test}:\n" +
                $"- Total Executions: {result.Total}\n" +
                $"- Success: {result.Success}\n" +
                $"- Failure: {result.Failure}\n" +
                $"- Max Latency: {result.MaxLatency:F2} ms\n" +
                $"- Min Latency: {result.MinLatency:F2} ms\n" +
                $"- Average Latency: {result.AverageLatency:F2} ms\n" +
                $"- 95th Percentile Latency: {result.Percentile95Latency:F2} ms\n" +
                $"- Duration: {result.Time:F2} s";

            // Output directly to IDE and Azure DevOps logs
            _diagnosticMessageSink.OnMessage(new DiagnosticMessage(summaryMessage));
        }
        
        private RunSummary ReportAggregatedResult(XunitTest xunitTest, LoadResult loadResult)
        {
            var aggregatedSummary = new RunSummary
            {
                Total = 1,
                Failed = loadResult.Failure > 0 ? 1 : 0,
                Skipped = 0,
                Time = loadResult.Time
            };

            if (aggregatedSummary.Failed == 0)
            {
                MessageBus.QueueMessage(new TestPassed(xunitTest, loadResult.Time, null));
            }
            else
            {
                MessageBus.QueueMessage(new TestFailed(xunitTest, 0, "One or more tests failed", null, null, null, null));
            }

            MessageBus.QueueMessage(new TestFinished(xunitTest, loadResult.Time, null));

            return aggregatedSummary;
        }
        private RunSummary HandleExecutionException(XunitTest xunitTest, Exception ex)
        {
            MessageBus.QueueMessage(new TestFailed(xunitTest, 0, ex.Message, null, null, null, null));
            MessageBus.QueueMessage(new TestFinished(xunitTest, 0, null));

            return new RunSummary { Total = 1, Failed = 1 };
        }
    }
}