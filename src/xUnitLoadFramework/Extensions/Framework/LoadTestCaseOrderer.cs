using Xunit.Sdk;
using Xunit.v3;
using xUnitLoadFramework.Extensions.ObjectModel;

namespace xUnitLoadFramework.Extensions.Framework;

public class LoadTestCaseOrderer : ITestCaseOrderer
{
    public static LoadTestCaseOrderer Instance { get; } = new();

    public IReadOnlyCollection<TTestCase> OrderTestCases<TTestCase>(IReadOnlyCollection<TTestCase> testCases)
        where TTestCase : notnull, ITestCase =>
            [.. testCases.OrderBy(tc => tc is LoadTestCase otc ? otc.Order : 0)];
}
