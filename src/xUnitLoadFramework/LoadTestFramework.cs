// using System.Reflection;
// using Xunit.Abstractions;
// using Xunit.Sdk;
//
// namespace xUnitLoadFramework
// {
//     public class LoadTestFramework : XunitTestFramework
//     {
//         public LoadTestFramework(IMessageSink messageSink)
//             : base(messageSink)
//         {
//             messageSink.OnMessage(new DiagnosticMessage("Using CustomTestFramework"));
//         }
//
//         protected override ITestFrameworkExecutor CreateExecutor(AssemblyName assemblyName)
//             => new LoadTestExecutor(assemblyName, SourceInformationProvider, DiagnosticMessageSink);
//     }
// }