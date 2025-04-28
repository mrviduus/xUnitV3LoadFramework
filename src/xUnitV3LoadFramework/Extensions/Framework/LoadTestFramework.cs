using System.Reflection;
using Xunit.v3;
using xUnitV3LoadFramework.Extensions.ObjectModel;

namespace xUnitV3LoadFramework.Extensions.Framework;

public class LoadTestFramework : TestFramework
{
	public override string TestFrameworkDisplayName =>
		"Load Framework";

	protected override ITestFrameworkDiscoverer CreateDiscoverer(Assembly assembly) =>
		new LoadDiscoverer(new LoadTestAssembly(assembly));

	protected override ITestFrameworkExecutor CreateExecutor(Assembly assembly) =>
		new LoadExecutor(new LoadTestAssembly(assembly));
}
