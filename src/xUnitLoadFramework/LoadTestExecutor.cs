// using System.Collections.Generic;
// using System.Reflection;
// using Xunit.Abstractions;
// using Xunit.Sdk;
//
// namespace xUnitLoadFramework
// {
//     public class LoadTestExecutor : XunitTestFrameworkExecutor
//     {
//         public LoadTestExecutor(
//             AssemblyName assemblyName,
//             ISourceInformationProvider sourceInformationProvider,
//             IMessageSink diagnosticMessageSink)
//             : base(assemblyName, sourceInformationProvider, diagnosticMessageSink)
//         {
//         }
//
//         protected override void RunTestCases(
//             IEnumerable<IXunitTestCase> testCases,
//             IMessageSink executionMessageSink,
//             ITestFrameworkExecutionOptions executionOptions)
//         {
//             using (var assemblyRunner = new LoadTestAssemblyRunner(
//                        TestAssembly,
//                        testCases,
//                        DiagnosticMessageSink,
//                        executionMessageSink,
//                        executionOptions))
//             {
//                 assemblyRunner.RunAsync().GetAwaiter().GetResult();
//             }
//         }
//     }
// }