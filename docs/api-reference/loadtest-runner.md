# LoadTestRunner API Reference

The `LoadTestRunner` class provides the core functionality for executing load tests with LoadFact attributes. It serves as the bridge between your test logic and the underlying load testing engine, offering both direct execution methods and a fluent API for load test configuration.

##  Namespace
```csharp
xUnitV3LoadFramework.Extensions
```

## Class Overview

```csharp
public static class LoadTestRunner
{
    // Core execution methods - automatic attribute detection
    public static async Task<LoadResult> ExecuteAsync(Func<Task<bool>> testAction, string? testMethodName = null)
    public static async Task<LoadResult> ExecuteAsync(Func<bool> testAction, string? testMethodName = null)
    public static async Task<LoadResult> ExecuteAsync(Func<Task> testAction, string? testMethodName = null)
    public static async Task<LoadResult> ExecuteAsync(Action testAction, string? testMethodName = null)
    
    // Custom configuration methods
    public static async Task<LoadResult> ExecuteAsync(Func<Task<bool>> testAction, LoadTestOptions options)
    
    // Simplified API
    public static async Task<LoadResult> RunAsync(Func<Task> action)
    
    // Fluent API
    public static ILoadTestBuilder Create()
}

// Fluent API Interface
public interface ILoadTestBuilder
{
    ILoadTestBuilder WithConcurrency(int concurrency);
    ILoadTestBuilder WithDuration(int milliseconds);
    ILoadTestBuilder WithInterval(int milliseconds);
    ILoadTestBuilder WithName(string name);
    Task<LoadResult> RunAsync(Func<Task> action);
    Task<LoadResult> RunAsync(Func<Task<bool>> action);
}
```

## 📋 Methods

### ExecuteAsync(Func<Task<bool>>, string?)

**Primary method for async load testing with explicit success indication.**

```csharp
public static async Task<LoadResult> ExecuteAsync(
    Func<Task<bool>> testAction, 
    string? testMethodName = null)
```

#### Parameters
- **testAction**: Async function that returns `true` for success, `false` for failure
- **testMethodName**: Optional method name for reporting (auto-detected if null)

#### Returns
`Task<LoadResult>` containing comprehensive test execution metrics

#### Example
```csharp
[LoadFact(order: 1, concurrency: 5, duration: 3000, interval: 200)]
public async Task TestWithExplicitSuccessIndication()
{
    var result = await LoadTestRunner.ExecuteAsync(async () =>
    {
        var response = await httpClient.GetAsync("/api/endpoint");
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return !string.IsNullOrEmpty(content); // Explicit success condition
        }
        
        return false; // Explicit failure
    });
    
    Assert.True(result.Success > 0);
}
```

### ExecuteAsync(Func<bool>, string?)

**Synchronous version for non-async operations.**

```csharp
public static async Task<LoadResult> ExecuteAsync(
    Func<bool> testAction, 
    string? testMethodName = null)
```

#### Parameters
- **testAction**: Synchronous function that returns `true` for success, `false` for failure
- **testMethodName**: Optional method name for reporting

#### Example
```csharp
[LoadFact(order: 1, concurrency: 3, duration: 2000, interval: 300)]
public async Task TestSynchronousOperation()
{
    var result = await LoadTestRunner.ExecuteAsync(() =>
    {
        var calculation = Math.Sqrt(16);
        return calculation == 4.0; // Success condition
    });
    
    Assert.True(result.Success > 0);
}
```

### ExecuteAsync(Func<Task>, string?)

**Async method where success is determined by absence of exceptions.**

```csharp
public static async Task<LoadResult> ExecuteAsync(
    Func<Task> testAction, 
    string? testMethodName = null)
```

#### Parameters
- **testAction**: Async action where success = no exception thrown
- **testMethodName**: Optional method name for reporting

#### Example
```csharp
[LoadFact(order: 1, concurrency: 4, duration: 4000, interval: 250)]
public async Task TestWithImplicitSuccessIndication()
{
    var result = await LoadTestRunner.ExecuteAsync(async () =>
    {
        var response = await httpClient.GetAsync("/api/endpoint");
        response.EnsureSuccessStatusCode(); // Throws on failure
        
        // If we reach here, the operation succeeded
    });
    
    Assert.True(result.Success > 0);
}
```

### ExecuteAsync(Action, string?)

**Synchronous action where success is determined by absence of exceptions.**

```csharp
public static async Task<LoadResult> ExecuteAsync(
    Action testAction, 
    string? testMethodName = null)
```

#### Parameters
- **testAction**: Synchronous action where success = no exception thrown
- **testMethodName**: Optional method name for reporting

#### Example
```csharp
[LoadFact(order: 1, concurrency: 2, duration: 1500, interval: 400)]
public async Task TestSynchronousAction()
{
    var result = await LoadTestRunner.ExecuteAsync(() =>
    {
        var data = File.ReadAllText("test-file.txt");
        if (string.IsNullOrEmpty(data))
            throw new InvalidOperationException("File is empty");
            
        // Success if no exception thrown
    });
    
    Assert.True(result.Success > 0);
}
```

### RunAsync(Func<Task>) - Simplified API

**Quick execution method for simple load tests with automatic configuration detection.**

```csharp
public static async Task<LoadResult> RunAsync(Func<Task> action)
```

#### Example
```csharp
[LoadFact(order: 1, concurrency: 5, duration: 3000, interval: 200)]
public async Task SimpleLoadTest()
{
    var result = await LoadTestRunner.RunAsync(async () =>
    {
        var response = await httpClient.GetAsync("/api/users");
        response.EnsureSuccessStatusCode();
    });
    
    Assert.True(result.Success > 0);
}
```

##  Fluent API

### Create() - Fluent Builder

**Creates a fluent API builder for load test configuration.**

```csharp
public static ILoadTestBuilder Create()
```

#### Examples

**Basic Fluent API Usage:**
```csharp
[Fact]
public async Task FluentAPI_BasicExample()
{
    var result = await LoadTestRunner.Create()
        .WithConcurrency(10)
        .WithDuration(5000)
        .WithInterval(100)
        .RunAsync(async () =>
        {
            await httpClient.GetAsync("/api/endpoint");
        });
        
    Assert.True(result.Success > 0);
}
```

**Named Load Test:**
```csharp
[Fact]
public async Task FluentAPI_NamedTest()
{
    var result = await LoadTestRunner.Create()
        .WithName("API_Performance_Test")
        .WithConcurrency(5)
        .WithDuration(3000)
        .WithInterval(200)
        .RunAsync(async () =>
        {
            var response = await httpClient.PostAsJsonAsync("/api/users", 
                new { Name = "Test User" });
            response.EnsureSuccessStatusCode();
            return true;
        });
        
    Console.WriteLine($"Test '{result.Name}' completed with {result.RequestsPerSecond:F2} req/sec");
}
```

**Chained Configuration:**
```csharp
[Theory]
[InlineData(5, 2000)]
[InlineData(10, 4000)]
public async Task FluentAPI_ParameterizedTest(int concurrency, int duration)
{
    var result = await LoadTestRunner.Create()
        .WithConcurrency(concurrency)
        .WithDuration(duration)
        .WithInterval(150)
        .RunAsync(async () =>
        {
            await Task.Delay(50); // Simulate work
        });
        
    Assert.True(result.AverageLatency < 200);
}
```

## Method Resolution Logic

The LoadTestRunner automatically detects the calling method's LoadFactAttribute using reflection and stack trace analysis:

1. **Scans the call stack** to find the method with `LoadFactAttribute`
2. **Extracts load test parameters** (concurrency, duration, interval)
3. **Creates LoadExecutionPlan** with the specified configuration
4. **Executes the load test** using the Akka.NET-based engine
5. **Returns comprehensive results** with performance metrics

## LoadResult Properties

The `LoadResult` object contains comprehensive metrics:

```csharp
public class LoadResult
{
    public string Name { get; set; }           // Test name
    public int Total { get; set; }             // Total executions
    public int Success { get; set; }           // Successful executions
    public int Failure { get; set; }           // Failed executions
    public double Time { get; set; }           // Total execution time (seconds)
    public double AverageLatency { get; set; } // Average response time (ms)
    public double RequestsPerSecond { get; set; } // Throughput
    public long PeakMemoryUsage { get; set; }  // Peak memory consumption (bytes)
}
```

## 📝 Usage Patterns

### Pattern 1: HTTP API Testing
```csharp
[LoadFact(order: 1, concurrency: 5, duration: 3000, interval: 200)]
public async Task LoadTest_API()
{
    var result = await LoadTestRunner.ExecuteAsync(async () =>
    {
        var response = await httpClient.PostAsJsonAsync("/api/users", new { Name = "Test" });
        response.EnsureSuccessStatusCode();
        return true;
    });
    
    // Validate performance expectations
    Assert.True(result.RequestsPerSecond >= 10, "Should achieve at least 10 req/sec");
    Assert.True(result.AverageLatency <= 500, "Average latency should be under 500ms");
}
```

### Pattern 2: Database Load Testing
```csharp
[LoadFact(order: 1, concurrency: 3, duration: 5000, interval: 300)]
public async Task LoadTest_Database()
{
    var result = await LoadTestRunner.ExecuteAsync(async () =>
    {
        using var context = GetService<DbContext>();
        var users = await context.Users.Take(10).ToListAsync();
        return users.Count > 0;
    });
    
    Assert.True(result.Success >= result.Total * 0.95, "95% success rate expected");
}
```

### Pattern 3: Complex Business Logic
```csharp
[LoadFact(order: 1, concurrency: 4, duration: 4000, interval: 250)]
public async Task LoadTest_BusinessLogic()
{
    var result = await LoadTestRunner.ExecuteAsync(async () =>
    {
        var orderService = GetService<IOrderService>();
        var order = await orderService.CreateOrderAsync(new CreateOrderRequest
        {
            CustomerId = Guid.NewGuid(),
            Items = new[] { new OrderItem { ProductId = 1, Quantity = 2 } }
        });
        
        Assert.NotNull(order);
        Assert.True(order.Id > 0);
        return true;
    });
    
    Assert.True(result.Success > 0, "Business logic should handle load successfully");
}
```

### Pattern 4: Fluent API for Dynamic Configuration
```csharp
[Theory]
[InlineData("light", 3, 2000)]
[InlineData("moderate", 8, 4000)]
[InlineData("heavy", 15, 6000)]
public async Task LoadTest_Dynamic(string level, int concurrency, int duration)
{
    var result = await LoadTestRunner.Create()
        .WithName($"API_Load_Test_{level}")
        .WithConcurrency(concurrency)
        .WithDuration(duration)
        .WithInterval(100)
        .RunAsync(async () =>
        {
            var response = await httpClient.GetAsync("/api/health");
            response.EnsureSuccessStatusCode();
        });
        
    // Different expectations based on load level
    var expectedRps = level switch
    {
        "light" => 5,
        "moderate" => 15,
        "heavy" => 25,
        _ => 1
    };
    
    Assert.True(result.RequestsPerSecond >= expectedRps, 
        $"{level} load should achieve at least {expectedRps} req/sec");
}
```

## Important Notes

### Exception Handling
- **Func<Task<bool>>**: Return `false` for controlled failures
- **Func<Task>**: Throw exceptions for failures  
- **Automatic wrapping**: All exceptions are caught and recorded as failures

### Method Name Resolution
- **Automatic detection**: Uses reflection to find LoadFactAttribute
- **Manual override**: Provide `testMethodName` parameter if needed
- **Fallback behavior**: Uses calling method name if detection fails

### Thread Safety
- **Concurrent execution**: Test actions may run simultaneously
- **Resource management**: Ensure thread-safe access to shared resources
- **Disposal pattern**: Use `using` statements for proper cleanup

### Performance Considerations
- **Memory usage**: Monitor `PeakMemoryUsage` for resource-intensive tests
- **Latency expectations**: Set realistic `AverageLatency` thresholds
- **Throughput goals**: Use `RequestsPerSecond` for performance validation

## 🔗 Related Documentation

- [LoadFact Attribute Guide](../user-guides/loadfact-attribute-guide.md)
- [Load Attribute Configuration](../user-guides/load-attribute-configuration.md)
- [Performance Optimization](../user-guides/performance-optimization.md)
- [Writing Effective Tests](../user-guides/writing-effective-tests.md)
