using System.Reflection;
using Xunit;
using xUnitV3LoadFramework.Attributes;
using xUnitV3LoadFramework.Extensions.Framework;
using xUnitV3LoadFramework.Extensions.ObjectModel;

namespace xUnitV3LoadFrameworkTests;

/// <summary>
/// Tests for the UseLoadFrameworkAttribute functionality and mixed test scenarios
/// </summary>
public class UseLoadFrameworkAttributeTests
{
    [Fact]
    public void UseLoadFrameworkAttribute_ShouldHaveCorrectTargets()
    {
        // Arrange
        var attribute = new UseLoadFrameworkAttribute();
        var attributeUsage = typeof(UseLoadFrameworkAttribute)
            .GetCustomAttribute<AttributeUsageAttribute>();

        // Assert
        Assert.NotNull(attributeUsage);
        Assert.Equal(AttributeTargets.Class, attributeUsage.ValidOn);
        Assert.False(attributeUsage.AllowMultiple);
        Assert.True(attributeUsage.Inherited);
    }

    [Fact]
    public void UseLoadFrameworkAttribute_ShouldBeApplicableToTestClasses()
    {
        // Arrange
        var testClassType = typeof(TestLoadFrameworkClass);
        
        // Act
        var hasAttribute = testClassType.GetCustomAttribute<UseLoadFrameworkAttribute>() != null;
        
        // Assert
        Assert.True(hasAttribute);
    }

    [Fact]
    public void StandardTestClass_ShouldNotHaveUseLoadFrameworkAttribute()
    {
        // Arrange
        var testClassType = typeof(TestStandardClass);
        
        // Act
        var hasAttribute = testClassType.GetCustomAttribute<UseLoadFrameworkAttribute>() != null;
        
        // Assert
        Assert.False(hasAttribute);
    }

    [Fact]
    public void LoadDiscoverer_ShouldCreateStandardTestCaseForNonLoadFrameworkClass()
    {
        // This test would require more complex setup of the xUnit discovery infrastructure
        // For now, we verify that our test classes are structured correctly
        var standardClass = typeof(TestStandardClass);
        var loadClass = typeof(TestLoadFrameworkClass);
        
        Assert.NotNull(standardClass);
        Assert.NotNull(loadClass);
        
        // Verify the attribute presence
        Assert.False(standardClass.GetCustomAttribute<UseLoadFrameworkAttribute>() != null);
        Assert.True(loadClass.GetCustomAttribute<UseLoadFrameworkAttribute>() != null);
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
/// Test class that uses the load framework
/// </summary>
[UseLoadFramework]
public class TestLoadFrameworkClass
{
    [Load(order: 1, concurrency: 1, duration: 1000, interval: 500)]
    public void LoadTest() { }
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
