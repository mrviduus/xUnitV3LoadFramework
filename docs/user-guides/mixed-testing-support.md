# UseLoadFramework Attribute - Mixed Testing Support

The `UseLoadFrameworkAttribute` enables you to run both standard xUnit tests and LoadFramework load tests within the same project, providing maximum flexibility for your testing needs.

## Overview

By default, when you configure the LoadTestFramework at the assembly level using `[assembly: TestFramework("xUnitV3LoadFramework.Extensions.Framework.LoadTestFrameworkStartup", "xUnitV3LoadFramework")]`, ALL test classes in that assembly are processed by the load testing framework.

The `UseLoadFrameworkAttribute` allows you to selectively choose which test classes should use the load testing capabilities, while other classes can run as standard xUnit tests.

## Usage

### Basic Configuration

1. **Configure the assembly** to use the LoadTestFramework in your `GlobalUsings.cs`:

```csharp
global using Xunit;
global using xUnitV3LoadFramework;
global using xUnitV3LoadFramework.Attributes;

[assembly: TestFramework("xUnitV3LoadFramework.Extensions.Framework.LoadTestFrameworkStartup", "xUnitV3LoadFramework")]
```

2. **Mark load test classes** with `[UseLoadFramework]`:

```csharp
// This class will use the LoadTestFramework
[UseLoadFramework]
public class MyLoadTests : Specification
{
    [Load(order: 1, concurrency: 5, duration: 2000, interval: 500)]
    public void ShouldHandleHighLoad()
    {
        // Load test implementation
        System.Console.WriteLine($"Load test executed at {DateTime.Now:HH:mm:ss.fff}");
    }
}
```

3. **Standard test classes** run without the attribute:

```csharp
// This class will run as standard xUnit tests
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
    [UseLoadFramework]
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

### Test Discovery

- **Standard Test Classes**: Discovered and executed using standard xUnit test discovery and execution pipeline
- **Load Test Classes**: Discovered using the LoadTestFramework's custom discoverer and executed with the actor-based load testing system

### Test Execution

- **Standard Tests**: Execute immediately using normal xUnit execution patterns
- **Load Tests**: Execute using the LoadTestFramework's actor system with specified concurrency, duration, and interval settings

### Class Requirements

- **Standard Test Classes**: No base class requirement, standard xUnit test class
- **Load Test Classes**: Must inherit from `Specification` class and use `[Load]` attributes

## Error Handling

The framework properly handles:
- Constructor exceptions in both standard and load test classes
- Method execution failures
- Mixed success/failure scenarios across different test types

## Migration Guide

### From Assembly-Level Only
If you currently use assembly-level configuration where all tests are load tests:

1. Add `[UseLoadFramework]` to classes that should remain as load tests
2. Remove the attribute from classes that should become standard tests
3. Standard test classes can remove the `Specification` base class inheritance

### From Standard xUnit Only
If you have existing standard xUnit tests and want to add load testing:

1. Configure the assembly for LoadTestFramework
2. Keep existing test classes as-is (they'll run as standard tests)
3. Add new test classes with `[UseLoadFramework]` and inherit from `Specification`

## Best Practices

1. **Use descriptive class names** to distinguish between standard and load test classes
2. **Group related tests** in the same class type (all standard or all load)
3. **Document the testing strategy** in your project to help other developers understand when to use each approach
4. **Run load tests separately** in CI/CD pipelines if they take significantly longer than standard tests
