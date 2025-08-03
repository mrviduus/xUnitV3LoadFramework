# Mixed Testing Support - Load Tests and Standard xUnit Tests

The xUnitV3LoadFramework now supports seamless integration of both standard xUnit tests and load tests within the same project without requiring any special attributes.

## Overview

The framework automatically detects which tests should be executed as load tests based on the presence of the `[Load]` attribute on test methods. This means you can:

- Run standard xUnit tests (Facts, Theories) normally
- Run load tests with the `[Load]` attribute using the actor-based load framework
- Mix both types of tests in the same test class
- No longer need the `UseLoadFrameworkAttribute` or special configuration

## Usage

### Basic Configuration

1. **Configure the assembly** to use the LoadTestFramework in your `GlobalUsings.cs`:

```csharp
global using Xunit;
global using xUnitV3LoadFramework;
global using xUnitV3LoadFramework.Attributes;

[assembly: TestFramework("xUnitV3LoadFramework.Extensions.Framework.LoadTestFrameworkStartup", "xUnitV3LoadFramework")]
```

2. **Create mixed test classes** with both standard and load tests:

```csharp
// This class contains both standard xUnit tests and load tests
public class MyMixedTests : Specification
{
    [Fact]
    public void ShouldExecuteAsStandardTest()
    {
        // Standard xUnit test - runs normally
        Assert.True(true);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void ShouldWorkWithTheories(int value)
    {
        // Theory test - runs normally
        Assert.True(value > 0);
    }

    [Load(order: 1, concurrency: 5, duration: 2000, interval: 500)]
    public void ShouldHandleHighLoad()
    {
        // Load test - runs with actor-based load framework
        System.Console.WriteLine($"Load test executed at {DateTime.Now:HH:mm:ss.fff}");
    }
}
```

3. **Or use separate classes** if you prefer:

```csharp
// Standard xUnit test class
public class MyStandardTests
{
    [Fact]
    public void ShouldExecuteAsStandardTest()
    {
        // Standard xUnit test
        Assert.True(true);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void ShouldWorkWithTheories(int value)
    {
        Assert.True(value > 0);
    }
}

// Load test class  
public class MyLoadTests : Specification
{
    [Load(order: 1, concurrency: 5, duration: 2000, interval: 500)]
    public void ShouldHandleHighLoad()
    {
        // Load test - runs with actor-based framework
        System.Console.WriteLine($"Load test executed at {DateTime.Now:HH:mm:ss.fff}");
    }
}
```

## Complete Example

```csharp
using System;
using Xunit;
using xUnitV3LoadFramework;
using xUnitV3LoadFramework.Attributes;

namespace MyTestProject
{
    // Standard xUnit tests - run immediately with normal xUnit behavior
    public class StandardXUnitTests  
    {
        [Fact]
        public void StandardTest_ShouldPass()
        {
            Console.WriteLine("Standard [Fact] test executed via xUnit");
            Assert.True(true);
        }

        [Theory]
        [InlineData("test1")]
        [InlineData("test2")]
        public void StandardTheory_ShouldAcceptParameters(string input)
        {
            Console.WriteLine($"Standard [Theory] test executed with: {input}");
            Assert.NotNull(input);
        }
    }

    // Load tests - run with load testing framework using actors and concurrency
    public class LoadFrameworkTests : Specification
    {
        protected override void EstablishContext() 
        {
            Console.WriteLine("EstablishContext: Load test setup completed");
        }

        protected override void Because() 
        {
            Console.WriteLine("Because: Load test action executed");
        }

        [Load(order: 1, concurrency: 5, duration: 2000, interval: 500)]
        public void ShouldExecuteWithLoadFramework()
        {
            Console.WriteLine($"Load test executed at {DateTime.Now:HH:mm:ss.fff}");
        }

        [Load(order: 2, concurrency: 3, duration: 1500, interval: 300)]
        public void ShouldExecuteWithLowerLoad()
        {
            Console.WriteLine($"Another load test executed at {DateTime.Now:HH:mm:ss.fff}");
        }
    }
}
```

## Key Benefits

1. **Flexibility**: Mix standard unit tests with load tests in the same project
2. **Selective Usage**: Choose which classes need load testing capabilities
3. **Performance**: Standard tests run immediately without load framework overhead
4. **Compatibility**: Full compatibility with existing xUnit tooling and features
5. **Migration**: Easy migration path for projects that want to add load testing

## Technical Details

## How It Works

### Test Discovery

The framework automatically analyzes each test method:
- **Methods with `[Load]` attribute**: Discovered and executed using the LoadTestFramework's actor-based load testing system
- **Methods with standard xUnit attributes**: Discovered and executed using standard xUnit test discovery and execution pipeline

### Test Execution

- **Standard Tests (`[Fact]`, `[Theory]`)**: Execute immediately using normal xUnit execution patterns
- **Load Tests (`[Load]`)**: Execute using the LoadTestFramework's actor system with specified concurrency, duration, and interval settings

### Class Requirements

- **No special requirements**: Any test class can contain both standard tests and load tests
- **Load Test Classes**: If inheriting from `Specification`, you get access to lifecycle hooks (EstablishContext, Because, DestroyContext)
- **Mixed Classes**: Can contain both `[Fact]`/`[Theory]` methods and `[Load]` methods

## Error Handling

The framework properly handles:
- Constructor exceptions in both standard and load test classes
- Method execution failures
- Mixed success/failure scenarios across different test types

## Migration Guide

### From Previous UseLoadFramework Approach
If you currently use `[UseLoadFramework]` attribute:

1. Remove all `[UseLoadFramework]` attributes from test classes
2. Your existing `[Load]` methods will continue to work as load tests
3. You can now add standard `[Fact]` and `[Theory]` methods to the same classes

### From Standard xUnit Only
If you have existing standard xUnit tests and want to add load testing:

1. Configure the assembly for LoadTestFramework in GlobalUsings.cs
2. Keep existing test classes as-is (they'll continue to run as standard tests)
3. Add `[Load]` attributes to any methods you want to convert to load tests
4. Optionally inherit from `Specification` for lifecycle hooks

## Best Practices

1. **Use descriptive method names** to distinguish between standard and load test methods
2. **Consider separation** - while you can mix test types, separate classes might be clearer for complex scenarios
3. **Document the testing strategy** in your project to help other developers understand when to use each approach
4. **Run load tests separately** in CI/CD pipelines if they take significantly longer than standard tests
