using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using LoadRunnerCore.Models;
using LoadRunnerCore.Runner;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace xUnitLoadFramework;

public class LoadTestFramework : XunitTestFramework
{
    protected LoadTestFramework(IMessageSink messageSink)
        : base(messageSink)
    {
        messageSink.OnMessage(new DiagnosticMessage("Using CustomTestFramework"));
    }

    protected override ITestFrameworkExecutor CreateExecutor(AssemblyName assemblyName)
        => new LoadTestExecutor(assemblyName, SourceInformationProvider, DiagnosticMessageSink);

    








}