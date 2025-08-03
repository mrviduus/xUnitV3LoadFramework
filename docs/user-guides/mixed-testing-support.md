# Mixed Testing Support - Load Tests and Standard xUnit Tests

The xUnitV3LoadFramework now provides seamless integration of both standard xUnit tests and load tests within the same project using the `[LoadFact]` attribute approach.

## Overview

The framework provides a `LoadFactAttribute` that inherits from xUnit's `FactAttribute`, enabling you to:

- Run standard xUnit tests (`[Fact]`, `[Theory]`) normally
- Run load tests with the `[LoadFact]` attribute using the actor-based load framework
- Mix both types of tests in the same test class
- Use standard xUnit v3 discovery and execution without custom test frameworks

## Usage

### Basic Configuration

1. **Standard xUnit setup** - No special framework configuration needed in `GlobalUsings.cs`:

```csharp
global using Xunit;
global using xUnitV3LoadFramework.Attributes;
global using xUnitV3LoadFramework.Extensions;

// No special TestFramework declaration needed - uses standard xUnit v3
```

2. **Create mixed test classes** with both standard and load tests:

```csharp
// This class contains both standard xUnit tests and load tests
public class MyMixedTests : TestSetup
{
    [Fact]
    public void ShouldExecuteAsStandardTest()
    {
        // Standard xUnit test - runs normally
        var httpClient = GetService<IHttpClientFactory>().CreateClient();
        Assert.NotNull(httpClient);
    }
        Assert.NotNull(_testData);
    }

    [Theory]
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void ShouldWorkWithTheories(int value)
    {
        // Theory test - runs normally
        Assert.True(value > 0);
    }

    [LoadFact(order: 1, concurrency: 5, duration: 2000, interval: 500)]
    public async Task ShouldHandleHighLoad()
    {
        // Load test - runs with actor-based load framework
        var result = await LoadTestRunner.ExecuteAsync(async () =>
        {
            var httpClient = GetService<IHttpClientFactory>().CreateClient();
            var response = await httpClient.GetAsync("https://httpbin.org/status/200", TestContext.Current.CancellationToken);
            response.EnsureSuccessStatusCode();
            return true;
        });
        
        Assert.True(result.Success > 0, "Load test should have successful executions");
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
public class MyLoadTests : TestSetup
{
    [LoadFact(order: 1, concurrency: 5, duration: 2000, interval: 500)]
    public async Task ShouldHandleHighLoad()
    {
        var result = await LoadTestRunner.ExecuteAsync(async () =>
        {
            var httpClient = GetService<IHttpClientFactory>().CreateClient();
            var response = await httpClient.GetAsync("https://httpbin.org/status/200", TestContext.Current.CancellationToken);
            response.EnsureSuccessStatusCode();
            return true;
        });
        
        Assert.True(result.Success > 0, "Load test should have successful executions");
    }
}
```

## Configuration Options

### LoadFact Attribute Parameters

```csharp
[LoadFact(
    order: 1,           // Execution order (optional)
    concurrency: 5,     // Number of concurrent executions
    duration: 2000,     // Duration in milliseconds
    interval: 500       // Interval between executions in milliseconds
)]
```

### LoadTestHelper Methods

The framework provides several overloads for `LoadTestRunner.ExecuteAsync`:

```csharp
// Basic execution - automatically detects LoadFact attribute
var result = await LoadTestRunner.ExecuteAsync(async () => {
    // Your load test logic
    return true;
});

// With custom configuration
var result = await LoadTestRunner.ExecuteAsync(
    testAction: async () => { /* test logic */ return true; },
    concurrency: 10,
    duration: TimeSpan.FromSeconds(30),
    interval: TimeSpan.FromMilliseconds(100)
);
```

## Test Organization Patterns

### Option 1: Mixed Classes
Combine standard tests and load tests in the same class:

```csharp
public class ApiTests : TestSetup
{
    [Fact]
    public void ShouldValidateConfiguration()
    {
        // Standard unit test
    }
    
    [LoadFact(concurrency: 3, duration: 1000)]
    public async Task ShouldHandleMultipleRequests()
    {
        var result = await LoadTestRunner.ExecuteAsync(async () =>
        {
            // Load test logic
            return true;
        });
        
        Assert.True(result.Success > 0);
    }
}
```

### Option 2: Separate Classes
Keep different test types in separate classes:

```csharp
public class ApiUnitTests
{
    [Fact]
    public void ShouldValidateConfiguration() { }
}

public class ApiLoadTests : TestSetup  
{
    [LoadFact(concurrency: 3, duration: 1000)]
    public async Task ShouldHandleMultipleRequests() 
    {
        var result = await LoadTestRunner.ExecuteAsync(async () =>
        {
            // Load test logic
            return true;
        });
        
        Assert.True(result.Success > 0);
    }
}
```

## Complete Example

```csharp
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using xUnitV3LoadFramework.Attributes;
using xUnitV3LoadFramework.Extensions;

namespace MyTestProject
{
    // Standard xUnit tests - run immediately with normal xUnit behavior
    public class StandardApiTests  
    {
        [Fact]
        public void ShouldValidateConfiguration()
        {
            Console.WriteLine("Standard [Fact] test executed via xUnit");
            Assert.True(true);
        }

        [Theory]
        [InlineData("user1")]
        [InlineData("user2")]
        public void ShouldAcceptDifferentUsers(string userId)
        {
            Console.WriteLine($"Standard [Theory] test executed with: {userId}");
            Assert.NotNull(userId);
        }
    }

    // Load tests - using LoadFact attribute with LoadTestHelper
    public class LoadApiTests : TestSetup
    {
        [LoadFact(order: 1, concurrency: 3, duration: 2000, interval: 500)]
        public async Task ShouldHandleMultipleApiCalls()
        {
            var result = await LoadTestRunner.ExecuteAsync(async () =>
            {
                var httpClient = GetService<IHttpClientFactory>().CreateClient();
                var response = await httpClient.GetAsync("https://httpbin.org/status/200", TestContext.Current.CancellationToken);
                response.EnsureSuccessStatusCode();
                Console.WriteLine($"Load test executed at {DateTime.Now:HH:mm:ss.fff}");
                return true;
            });
            
            Assert.True(result.Success > 0, "Load test should have successful executions");
            Assert.True(result.Total >= result.Success, "Total executions should be >= successful ones");
        }

        [LoadFact(order: 2, concurrency: 2, duration: 1500, interval: 300)]
        public async Task ShouldHandleLowerLoadScenario()
        {
            var result = await LoadTestRunner.ExecuteAsync(async () =>
            {
                // Simulate some work
                await Task.Delay(50, TestContext.Current.CancellationToken);
                Console.WriteLine($"Low load test executed at {DateTime.Now:HH:mm:ss.fff}");
                return true;
            });
            
            Assert.True(result.Success > 0);
        }
    }
}
```

## Key Benefits

1. **Flexibility**: Mix standard unit tests with load tests in the same project
2. **Standard Compatibility**: Full compatibility with existing xUnit tooling and features  
3. **Selective Usage**: Choose which tests need load testing capabilities
4. **Performance**: Standard tests run immediately without load framework overhead
5. **Migration**: Easy migration path for projects that want to add load testing

## How It Works

### Test Discovery and Execution

- **Standard Tests (`[Fact]`, `[Theory]`)**: Execute immediately using standard xUnit execution patterns
- **Load Tests (`[LoadFact]`)**: Execute as standard xUnit tests but use `LoadTestHelper` to run load scenarios with actor-based concurrency

### Class Requirements

- **No special inheritance**: Standard xUnit test classes work as-is
- **Optional TestSetup**: Load tests can inherit from `TestSetup` for dependency injection
- **Mixed Classes**: Can contain both standard and load test methods

## Advanced Usage

### Custom Load Test Configurations

```csharp
[LoadFact(concurrency: 10, duration: 5000, interval: 100)]
public async Task HighIntensityLoadTest()
{
    var result = await LoadTestRunner.ExecuteAsync(
        testAction: async () => {
            // Custom test logic
            return await PerformComplexOperation();
        },
        // Override attribute settings if needed
        concurrency: 15,
        duration: TimeSpan.FromSeconds(10)
    );
    
    Assert.True(result.Success > 0);
}
```

### Error Handling and Metrics

```csharp
[LoadFact(concurrency: 5, duration: 3000)]
public async Task ShouldHandlePartialFailures()
{
    var result = await LoadTestRunner.ExecuteAsync(async () =>
    {
        // Test logic that might occasionally fail
        var random = new Random();
        if (random.Next(100) < 10) // 10% failure rate
        {
            throw new InvalidOperationException("Simulated failure");
        }
        return true;
    });
    
    // Verify that some tests succeeded even with failures
    Assert.True(result.Success > 0, "Should have some successful executions");
    Assert.True(result.Total > result.Success, "Should have some failed executions");
    
    // Calculate success rate
    var successRate = (double)result.Success / result.Total;
    Assert.True(successRate > 0.8, "Success rate should be above 80%");
}
```

## Migration Guide

### From Previous Versions
If you previously used custom test frameworks or Specification classes:

1. **Remove custom framework dependencies** - standard xUnit is now sufficient
2. **Convert to LoadFact** - replace custom attributes with `[LoadFact]`
3. **Use LoadTestHelper** - call `LoadTestRunner.ExecuteAsync()` within test methods
4. **Inherit from TestSetup** - for dependency injection and test context access

### From Standard xUnit Only
If you have existing standard xUnit tests and want to add load testing:

1. **Keep existing tests** - they'll continue to run as standard tests
2. **Add LoadFact tests** - create new methods with `[LoadFact]` attributes
3. **Use TestSetup base class** - for tests that need dependency injection
4. **Add LoadTestHelper calls** - within LoadFact methods for load execution

## Best Practices

1. **Use descriptive method names** to distinguish between standard and load test methods
2. **Consider separation** - while you can mix test types, separate classes might be clearer for complex scenarios
3. **Document testing strategy** in your project to help other developers understand when to use each approach
4. **Run load tests separately** in CI/CD pipelines if they take significantly longer than standard tests
5. **Use appropriate assertions** - verify both success counts and business logic outcomes

This approach provides the best of both worlds: standard xUnit compatibility with powerful load testing capabilities when needed.
