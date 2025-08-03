using xUnitV3LoadFramework.Attributes;
using Xunit;

namespace xUnitV3LoadFramework.Tests.Unit;

/// <summary>
/// Unit tests for LoadFactAttribute configuration and validation.
/// Tests the attribute's constructor, properties, and validation logic.
/// </summary>
public class LoadFactAttributeTests
{
    [Fact]
    public void LoadFactAttribute_Should_Initialize_With_Valid_Parameters()
    {
        // Arrange & Act
        var attribute = new LoadFactAttribute(
            order: 1,
            concurrency: 5,
            duration: 3000,
            interval: 500);

        // Assert
        Assert.Equal(1, attribute.Order);
        Assert.Equal(5, attribute.Concurrency);
        Assert.Equal(3000, attribute.Duration);
        Assert.Equal(500, attribute.Interval);
    }

    [Theory]
    [InlineData(0, 5, 1000, 100)]
    [InlineData(-1, 5, 1000, 100)]
    public void LoadFactAttribute_Should_Accept_Any_Order_Value(int order, int concurrency, int duration, int interval)
    {
        // Arrange & Act & Assert
        var exception = Record.Exception(() => new LoadFactAttribute(order, concurrency, duration, interval));
        Assert.Null(exception);
    }

    [Theory]
    [InlineData(1, 0, 1000, 100)]
    [InlineData(1, -1, 1000, 100)]
    public void LoadFactAttribute_Should_Throw_ArgumentOutOfRangeException_For_Invalid_Concurrency(int order, int concurrency, int duration, int interval)
    {
        // Arrange, Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => 
            new LoadFactAttribute(order, concurrency, duration, interval));
        Assert.Equal("concurrency", exception.ParamName);
    }

    [Theory]
    [InlineData(1, 5, 0, 100)]
    [InlineData(1, 5, -1, 100)]
    public void LoadFactAttribute_Should_Throw_ArgumentOutOfRangeException_For_Invalid_Duration(int order, int concurrency, int duration, int interval)
    {
        // Arrange, Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => 
            new LoadFactAttribute(order, concurrency, duration, interval));
        Assert.Equal("duration", exception.ParamName);
    }

    [Theory]
    [InlineData(1, 5, 1000, 0)]
    [InlineData(1, 5, 1000, -1)]
    public void LoadFactAttribute_Should_Throw_ArgumentOutOfRangeException_For_Invalid_Interval(int order, int concurrency, int duration, int interval)
    {
        // Arrange, Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => 
            new LoadFactAttribute(order, concurrency, duration, interval));
        Assert.Equal("interval", exception.ParamName);
    }

    [Fact]
    public void LoadFactAttribute_Should_Inherit_From_FactAttribute()
    {
        // Arrange
        var attribute = new LoadFactAttribute(1, 2, 1000, 100);

        // Act & Assert
        Assert.IsAssignableFrom<Xunit.FactAttribute>(attribute);
    }

    [Theory]
    [InlineData(1, 1, 1, 1)]
    [InlineData(100, 50, 60000, 5000)]
    [InlineData(5, 10, 2500, 250)]
    public void LoadFactAttribute_Should_Store_All_Parameters_Correctly(int order, int concurrency, int duration, int interval)
    {
        // Arrange & Act
        var attribute = new LoadFactAttribute(order, concurrency, duration, interval);

        // Assert
        Assert.Equal(order, attribute.Order);
        Assert.Equal(concurrency, attribute.Concurrency);
        Assert.Equal(duration, attribute.Duration);
        Assert.Equal(interval, attribute.Interval);
    }
}