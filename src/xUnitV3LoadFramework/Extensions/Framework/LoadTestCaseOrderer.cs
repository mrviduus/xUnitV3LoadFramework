using Xunit.Sdk;
using Xunit.v3;
using xUnitV3LoadFramework.Extensions.ObjectModel;

namespace xUnitV3LoadFramework.Extensions.Framework;

public class LoadTestCaseOrderer : ITestCaseOrderer
{
    public static LoadTestCaseOrderer Instance { get; } = new();

    public IReadOnlyCollection<TTestCase> OrderTestCases<TTestCase>(IReadOnlyCollection<TTestCase> testCases)
        where TTestCase : notnull, ITestCase =>
            [.. testCases.OrderBy(tc => tc is LoadTestCase otc ? otc.Order : 0)];
}
