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
        }

        protected override async Task<RunSummary> RunTestCaseAsync(IXunitTestCase testCase)
        {
            var loadTestSettings = GetLoadTestSettings();
            var settings = CreateLoadSettings(loadTestSettings);
            var parameters = GetTestMethodParameters(testCase);
            var test = $"{TestMethod.TestClass.Class.Name}.{TestMethod.Method.Name}({parameters})";
            _diagnosticMessageSink.OnMessage(new DiagnosticMessage($"STARTED: {test}"));

            try
            {
                if (settings == null)
                    return await base.RunTestCaseAsync(testCase);

                var executionPlan = CreateExecutionPlan(testCase, settings);
                var loadResult = await LoadRunner.Run(executionPlan);

                var status = loadResult.Failure > 0 ? "FAILURE" : "SUCCESS";
                _diagnosticMessageSink.OnMessage(new DiagnosticMessage($"{status}: {test} ({loadResult.Time}s)"));

                return new RunSummary
                {
                    Total = 1,
                    Failed = loadResult.Failure,
                    Skipped = 0,
                    Time = loadResult.Time
                };
            }
            catch (Exception ex)
            {
                _diagnosticMessageSink.OnMessage(new DiagnosticMessage($"ERROR: {test} ({ex.Message})"));
                return new RunSummary
                {
                    Total = 1,
                    Failed = 1,
                    Skipped = 0,
                    Time = 0
                };
            }
        }

        private object? GetLoadTestSettings()
        {
            return TestMethod.TestClass.Class.GetCustomAttributes(typeof(LoadTestSettingsAttribute)).FirstOrDefault()
                   ?? TestMethod.Method.GetCustomAttributes(typeof(LoadTestSettingsAttribute)).FirstOrDefault();
        }

        private LoadSettings? CreateLoadSettings(object? loadTestSettings)
        {
            if (loadTestSettings is LoadTestSettingsAttribute settingsAttribute)
            {
                return new LoadSettings
                {
                    Concurrency = settingsAttribute.Concurrency,
                    Duration = TimeSpan.FromMilliseconds(settingsAttribute.DurationInMilliseconds),
                    Interval = TimeSpan.FromMilliseconds(settingsAttribute.IntervalInMilliseconds)
                };
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
                    var summary = await base.RunTestCaseAsync(testCase);
                    return summary.Failed == 0;
                },
                Settings = settings
            };
        }
    }
}
