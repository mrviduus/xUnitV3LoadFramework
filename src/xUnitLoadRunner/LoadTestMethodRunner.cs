using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LoadRunnerCore.Models;
using LoadRunnerCore.Runner;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace xUnitLoadRunner
{
    public class LoadTestMethodRunner : XunitTestMethodRunner
    {
        readonly object[] constructorArguments;
        readonly IMessageSink diagnosticMessageSink;

        public LoadTestMethodRunner(ITestMethod testMethod, IReflectionTypeInfo @class, IReflectionMethodInfo method,
            IEnumerable<IXunitTestCase> testCases, IMessageSink diagnosticMessageSink, IMessageBus messageBus,
            ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource,
            object[] constructorArguments)
            : base(testMethod, @class, method, testCases, diagnosticMessageSink, messageBus, aggregator,
                cancellationTokenSource, constructorArguments)
        {
            this.constructorArguments = constructorArguments;
            this.diagnosticMessageSink = diagnosticMessageSink;
        }

        protected override async Task<RunSummary> RunTestCasesAsync()
        {
            var disableParallelization = TestMethod.TestClass.Class
                                             .GetCustomAttributes(typeof(DisableParallelizationAttribute)).Any()
                                         || TestMethod.TestClass.Class.GetCustomAttributes(typeof(CollectionAttribute))
                                             .Any()
                                         || TestMethod.Method
                                             .GetCustomAttributes(typeof(DisableParallelizationAttribute)).Any()
                                         || TestMethod.Method.GetCustomAttributes(typeof(MemberDataAttribute)).Any(a =>
                                             a.GetNamedArgument<bool>(nameof(MemberDataAttribute
                                                 .DisableDiscoveryEnumeration)));

            if (disableParallelization)
                return await base.RunTestCasesAsync().ConfigureAwait(false);

            var summary = new RunSummary();

            var caseTasks = TestCases.Select(RunTestCaseAsync);
            var caseSummaries = await Task.WhenAll(caseTasks).ConfigureAwait(false);

            foreach (var caseSummary in caseSummaries)
            {
                summary.Aggregate(caseSummary);
            }

            return summary;
        }

        protected override async Task<RunSummary> RunTestCaseAsync(IXunitTestCase testCase)
        {
            var loadTestSettings = TestMethod.TestClass.Class.GetCustomAttributes(typeof(LoadTestSettingsAttribute))
                                       .FirstOrDefault()
                                   ?? TestMethod.Method.GetCustomAttributes(typeof(LoadTestSettingsAttribute))
                                       .FirstOrDefault();

            LoadSettings settings = null;
            if (loadTestSettings != null)
            {
                var settingsAttribute = loadTestSettings.GetNamedArgument<LoadTestSettingsAttribute>("Instance");
                settings = new LoadSettings
                {
                    Concurrency = settingsAttribute.Concurrency,
                    Duration = settingsAttribute.Duration,
                    Interval = settingsAttribute.Interval,
                };
            }

            // Create a new TestOutputHelper for each test case since they cannot be reused when running in parallel
            var args = constructorArguments.Select(a => a is TestOutputHelper ? new TestOutputHelper() : a).ToArray();

            var action = () => testCase.RunAsync(diagnosticMessageSink, MessageBus, args,
                new ExceptionAggregator(Aggregator), CancellationTokenSource);

            var executionPlan = new LoadExecutionPlan
            {
                Name = testCase.DisplayName,
                Action = async () =>
                {
                    await action();
                    return true;
                },
                Settings = settings
            };

            await LoadRunner.Run(executionPlan);

            // Respect MaxParallelThreads by using the MaxConcurrencySyncContext if it exists, mimicking how collections are run
            if (SynchronizationContext.Current != null)
            {
                var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
                return await Task.Factory
                    .StartNew(action, CancellationTokenSource.Token,
                        TaskCreationOptions.DenyChildAttach | TaskCreationOptions.HideScheduler, scheduler).Unwrap()
                    .ConfigureAwait(false);
            }

            return await Task.Run(action, CancellationTokenSource.Token).ConfigureAwait(false);
        }
    }
}