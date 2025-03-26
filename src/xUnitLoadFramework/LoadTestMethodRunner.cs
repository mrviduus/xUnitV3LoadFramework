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
                if (settings == null) return await base.RunTestCaseAsync(testCase);
                var executionPlan = CreateExecutionPlan(testCase, settings);
                var loadResult = await LoadRunner.Run(executionPlan);

                var status = loadResult.Failure > 0 ? "FAILURE" : "SUCCESS";
                _diagnosticMessageSink.OnMessage(new DiagnosticMessage($"{status}: {test} ({loadResult.Time}s)"));

                return CreateRunSummary(loadResult);
            }
            catch (Exception ex)
            {
                _diagnosticMessageSink.OnMessage(new DiagnosticMessage($"ERROR: {test} ({ex.Message})"));
                throw;
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
                        Duration = TimeSpan.FromSeconds(settingsAttribute.DurationInSeconds),
                        Interval = TimeSpan.FromSeconds(settingsAttribute.IntervalInSeconds),
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
            var action = async () => await base.RunTestCaseAsync(testCase);
            return new LoadExecutionPlan
            {
                Name = testCase.DisplayName,
                Action = async () =>
                {
                    await action();
                    return true;
                },
                Settings = settings
            };
        }

        private RunSummary CreateRunSummary(LoadResult loadResult)
        {
            return new RunSummary
            {
                Total = loadResult.Total,
                Failed = loadResult.Failure,
                Skipped = loadResult.Total - loadResult.Failure,
                Time = loadResult.Time
            };
        }
    }
}