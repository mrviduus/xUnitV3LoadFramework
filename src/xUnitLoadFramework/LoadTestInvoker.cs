// using System.Reflection;
// using Xunit.Abstractions;
// using Xunit.Sdk;
//
// namespace xUnitLoadFramework
// {
//     public class LoadTestInvoker : XunitTestInvoker
//     {
//         private readonly IMessageSink _diagnosticMessageSink;
//
//         public LoadTestInvoker(ITest test,
//                                IMessageBus messageBus,
//                                Type testClass,
//                                object[] constructorArguments,
//                                MethodInfo testMethod,
//                                object[] testMethodArguments,
//                                ExceptionAggregator aggregator,
//                                CancellationTokenSource cancellationTokenSource,
//                                IMessageSink diagnosticMessageSink)
//             : base(test, messageBus, testClass, constructorArguments, testMethod, testMethodArguments, aggregator, cancellationTokenSource)
//         {
//             _diagnosticMessageSink = diagnosticMessageSink;
//         }
//
//         protected override object CallTestMethod(object testClassInstance)
//         {
//             try
//             {
//                 _diagnosticMessageSink.OnMessage(new DiagnosticMessage($"Invoking test method '{TestMethod.Name}'"));
//                 var result = base.CallTestMethod(testClassInstance);
//                 _diagnosticMessageSink.OnMessage(new DiagnosticMessage($"Successfully invoked '{TestMethod.Name}'"));
//                 return result;
//             }
//             catch (Exception ex)
//             {
//                 _diagnosticMessageSink.OnMessage(new DiagnosticMessage($"Exception during invocation of '{TestMethod.Name}': {ex.Message}"));
//                 throw;
//             }
//         }
//
//         protected override Task BeforeTestMethodInvokedAsync()
//         {
//             _diagnosticMessageSink.OnMessage(new DiagnosticMessage($"Before invoking '{TestMethod.Name}'"));
//             return base.BeforeTestMethodInvokedAsync();
//         }
//
//         protected override Task AfterTestMethodInvokedAsync()
//         {
//             _diagnosticMessageSink.OnMessage(new DiagnosticMessage($"After invoking '{TestMethod.Name}'"));
//             return base.AfterTestMethodInvokedAsync();
//         }
//     }
// }
