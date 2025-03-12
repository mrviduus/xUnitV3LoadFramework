using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace xUnitLoadFramework
{
    public class LoadTestClassRunner : XunitTestClassRunner
    {
        public LoadTestClassRunner(
            ITestClass testClass,
            IReflectionTypeInfo @class,
            IEnumerable<IXunitTestCase> testCases,
            IMessageSink diagnosticMessageSink,
            IMessageBus messageBus,
            ITestCaseOrderer testCaseOrderer,
            ExceptionAggregator aggregator,
            CancellationTokenSource cancellationTokenSource,
            IDictionary<Type, object> collectionFixtureMappings)
            : base(testClass, @class, testCases, diagnosticMessageSink, messageBus, testCaseOrderer, aggregator,
                cancellationTokenSource, collectionFixtureMappings)
        {
        }

        protected override Task<RunSummary> RunTestMethodAsync(
            ITestMethod testMethod,
            IReflectionMethodInfo method,
            IEnumerable<IXunitTestCase> testCases,
            object[] constructorArguments)
        {
            return new LoadTestMethodRunner(
                testMethod,
                this.Class,
                method,
                testCases,
                this.DiagnosticMessageSink,
                this.MessageBus,
                new ExceptionAggregator(this.Aggregator),
                this.CancellationTokenSource,
                constructorArguments).RunAsync();
        }
    }
}