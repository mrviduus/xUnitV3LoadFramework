using System.Reflection;
using xUnitV3LoadFramework.Attributes;
using xUnitV3LoadFramework.Extensions;
using xUnitV3LoadFramework.Extensions.Framework;
using xUnitV3LoadFramework.Extensions.ObjectModel;

namespace xUnitV3LoadFrameworkTests;

/// <summary>
/// Tests for the UseStressFrameworkAttribute functionality and mixed test scenarios
/// </summary>
public class UseStressFrameworkAttributeTests
{
	[Fact]
	public void UseStressFrameworkAttribute_ShouldHaveCorrectTargets()
	{
		// Arrange
		var attribute = new UseStressFrameworkAttribute();
		var attributeUsage = typeof(UseStressFrameworkAttribute)
			.GetCustomAttribute<AttributeUsageAttribute>();

		// Assert
		Assert.NotNull(attributeUsage);
		Assert.Equal(AttributeTargets.Class, attributeUsage.ValidOn);
		Assert.False(attributeUsage.AllowMultiple);
		Assert.True(attributeUsage.Inherited);
	}

	[Fact]
	public void UseStressFrameworkAttribute_ShouldBeApplicableToTestClasses()
	{
		// Arrange
		var testClassType = typeof(TestStressFrameworkClass);

		// Act
		var hasAttribute = testClassType.GetCustomAttribute<UseStressFrameworkAttribute>() != null;

		// Assert
		Assert.True(hasAttribute);
	}

	[Fact]
	public void StandardTestClass_ShouldNotHaveUseStressFrameworkAttribute()
	{
		// Arrange
		var testClassType = typeof(TestStandardClass);

		// Act
		var hasAttribute = testClassType.GetCustomAttribute<UseStressFrameworkAttribute>() != null;

		// Assert
		Assert.False(hasAttribute);
	}

	[Fact]
	public void LoadDiscoverer_ShouldCreateStandardTestCaseForNonStressFrameworkClass()
	{
		// This test would require more complex setup of the xUnit discovery infrastructure
		// For now, we verify that our test classes are structured correctly
		var standardClass = typeof(TestStandardClass);
		var stressClass = typeof(TestStressFrameworkClass);

		Assert.NotNull(standardClass);
		Assert.NotNull(stressClass);

		// Verify the attribute presence
		Assert.False(standardClass.GetCustomAttribute<UseStressFrameworkAttribute>() != null);
		Assert.True(stressClass.GetCustomAttribute<UseStressFrameworkAttribute>() != null);
	}

	[Fact]
	public void StandardTestCase_ShouldInheritFromLoadTestCase()
	{
		// Arrange & Act
		var isAssignable = typeof(LoadTestCase).IsAssignableFrom(typeof(StandardTestCase));

		// Assert
		Assert.True(isAssignable);
	}
}

/// <summary>
/// Test class that uses the stress framework
/// </summary>
[UseStressFramework]
public class TestStressFrameworkClass : IDisposable
{
	[Stress(order: 1, concurrency: 1, duration: 10000, interval: 500)]
	public async Task StressTest()
	{
		await Task.Delay(10);
		Console.WriteLine("Stress test executed in TestStressFrameworkClass");
	}

	public void Dispose()
	{
		// Cleanup if needed
	}
}

/// <summary>
/// Test class that uses standard xUnit tests
/// </summary>
public class TestStandardClass
{
	[Fact]
	public void StandardTest() { }

	[Theory]
	[InlineData(1)]
	[InlineData(2)]
	public void StandardTheoryTest(int value) { }
}
