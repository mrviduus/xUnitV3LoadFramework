using xUnitV3LoadFramework.Attributes;
using Xunit;

namespace xUnitV3LoadFramework.Tests.Unit;

/// <summary>
/// Core unit tests for LoadAttribute.
/// Tests the attribute's basic constructor and properties.
/// </summary>
public class LoadAttributeTests
{
    [Fact]
    public void LoadAttribute_Should_Initialize_With_Valid_Parameters()
    {
        // Arrange & Act
        var attribute = new LoadAttribute(
            concurrency: 5,
            duration: 3000,
            interval: 500);

        // Assert
        Assert.Equal(5, attribute.Concurrency);
        Assert.Equal(3000, attribute.Duration);
        Assert.Equal(500, attribute.Interval);
    }

    [Fact]
    public void LoadAttribute_Should_Inherit_From_FactAttribute()
    {
        // Arrange
        var attribute = new LoadAttribute(2, 1000, 100);

        // Act & Assert
        Assert.IsAssignableFrom<Xunit.FactAttribute>(attribute);
    }

    [Theory]
    [InlineData(1, 1, 1)]
    [InlineData(50, 60000, 5000)]
    [InlineData(10, 2500, 250)]
    public void LoadAttribute_Should_Store_All_Parameters_Correctly(int concurrency, int duration, int interval)
    {
        // Arrange & Act
        var attribute = new LoadAttribute(concurrency, duration, interval);

        // Assert
        Assert.Equal(concurrency, attribute.Concurrency);
        Assert.Equal(duration, attribute.Duration);
        Assert.Equal(interval, attribute.Interval);
    }
}