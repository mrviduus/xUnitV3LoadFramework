using Xunit;
using xUnitV3LoadFramework.Attributes;
using xUnitV3LoadFramework.Extensions;

namespace xUnitV3LoadTests;

/// <summary>
/// Standard xUnit test class - no UseLoadFramework attribute
/// These tests should use standard xUnit framework behavior
/// </summary>
public class StandardXUnitTests
{
    [Fact]
    public void StandardFact_ShouldExecuteWithXUnit()
    {
        // This test runs with standard xUnit framework
        var result = 2 + 2;
        Assert.Equal(4, result);
        Console.WriteLine("Standard [Fact] test executed via xUnit");
    }

    [Theory]
    [InlineData(1, 2, 3)]
    [InlineData(5, 5, 10)]
    [InlineData(10, -5, 5)]
    public void StandardTheory_ShouldExecuteWithXUnit(int a, int b, int expected)
    {
        // This test runs with standard xUnit framework
        var result = a + b;
        Assert.Equal(expected, result);
        Console.WriteLine($"Standard [Theory] test executed: {a} + {b} = {result}");
    }
}

/// <summary>
/// Load test class - marked with UseLoadFramework attribute
/// These tests will run using the LoadTestFramework
/// </summary>
[UseLoadFramework]
public class LoadFrameworkTests : Specification
{
    private string? _testData;

    protected override void EstablishContext()
    {
        _testData = "Test data initialized";
        Console.WriteLine("EstablishContext: Load test setup completed");
    }

    protected override void Because()
    {
        Console.WriteLine("Because: Load test action executed");
    }

    [Load(order: 1, concurrency: 5, duration: 2000, interval: 500)]
    public void LoadTest_ShouldExecuteWithLoadFramework()
    {
        Assert.NotNull(_testData);
        Console.WriteLine($"Load test executed at {DateTime.Now:HH:mm:ss.fff}");
    }

    [Load(order: 2, concurrency: 3, duration: 1500, interval: 300)]
    public void AnotherLoadTest_ShouldExecuteWithLowerLoad()
    {
        Console.WriteLine($"Another load test executed at {DateTime.Now:HH:mm:ss.fff}");
    }
}

/// <summary>
/// Another standard test class to verify framework selection
/// </summary>
public class AnotherStandardTestClass
{
    [Fact]
    public void AnotherStandardTest_ShouldUseXUnit()
    {
        Assert.True(true);
        Console.WriteLine("Another standard test executed");
    }
}
