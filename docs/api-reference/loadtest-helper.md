# LoadTestHelper API Reference

The `LoadTestHelper` class provides the core functionality for executing load tests with LoadFact attributes. It serves as the bridge between your test logic and the underlying load testing engine.

## ?? Namespace
```csharp
xUnitV3LoadFramework.Extensions
```

## ??? Class Overview

```csharp
public static class LoadTestHelper
{
    // Core execution methods
    public static async Task<LoadResult> ExecuteLoadTestAsync(Func<Task<bool>> testAction, string? testMethodName = null)
    public static async Task<LoadResult> ExecuteLoadTestAsync(Func<bool> testAction, string? testMethodName = null)
    public static async Task<LoadResult> ExecuteLoadTestAsync(Func<Task> testAction, string? testMethodName = null)
    public static async Task<LoadResult> ExecuteLoadTestAsync(Action testAction, string? testMethodName = null)
}
```

## ?? Methods

### ExecuteLoadTestAsync(Func<Task<bool>>, string?)

**Primary method for async load testing with explicit success indication.**

```csharp
public static async Task<LoadResult> ExecuteLoadTestAsync(
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
    var result = await LoadTestHelper.ExecuteLoadTestAsync(async () =>
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

### ExecuteLoadTestAsync(Func<bool>, string?)

**Synchronous version for non-async operations.**

```csharp
public static async Task<LoadResult> ExecuteLoadTestAsync(
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
    var result = await LoadTestHelper.ExecuteLoadTestAsync(() =>
    {
        var calculation = Math.Sqrt(16);
        return calculation == 4.0; // Success condition
    });
    
    Assert.True(result.Success > 0);
}
```

### ExecuteLoadTestAsync(Func<Task>, string?)

**Async method where success is determined by absence of exceptions.**

```csharp
public static async Task<LoadResult> ExecuteLoadTestAsync(
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
    var result = await LoadTestHelper.ExecuteLoadTestAsync(async () =>
    {
        var response = await httpClient.GetAsync("/api/endpoint");
        response.EnsureSuccessStatusCode(); // Throws on failure
        
        // If we reach here, the operation succeeded
    });
    
    Assert.True(result.Success > 0);
}
```

### ExecuteLoadTestAsync(Action, string?)

**Synchronous action where success is determined by absence of exceptions.**

```csharp
public static async Task<LoadResult> ExecuteLoadTestAsync(
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
    var result = await LoadTestHelper.ExecuteLoadTestAsync(() =>
    {
        var data = File.ReadAllText("test-file.txt");
        if (string.IsNullOrEmpty(data))
            throw new InvalidOperationException("File is empty");
            
        // Success if no exception thrown
    });
    
    Assert.True(result.Success > 0);
}
```

## ?? Method Resolution Logic

The LoadTestHelper automatically detects the calling method's LoadFactAttribute using reflection and stack trace analysis:

1. **Scans the call stack** to find the method with `LoadFactAttribute`
2. **Extracts load test parameters** (concurrency, duration, interval)
3. **Creates LoadExecutionPlan** with the specified configuration
4. **Executes the load test** using the Akka.NET-based engine
5. **Returns comprehensive results** with performance metrics

## ?? LoadResult Properties

The returned `LoadResult` object contains comprehensive metrics:

```csharp
public class LoadResult
{
    public string ScenarioName { get; set; }        // Test method name
    public int Total { get; set; }                  // Total executions attempted
    public int Success { get; set; }                // Successful executions
    public int Failure { get; set; }                // Failed executions
    public double Time { get; set; }                // Total execution time (seconds)
    public double RequestsPerSecond { get; set; }   // Throughput metric
    public double AverageLatency { get; set; }      // Average response time (ms)
    public double MedianLatency { get; set; }       // Median response time (ms)
    public double MinLatency { get; set; }          // Minimum response time (ms)
    public double MaxLatency { get; set; }          // Maximum response time (ms)
    public double Percentile95Latency { get; set; } // 95th percentile (ms)
    public double Percentile99Latency { get; set; } // 99th percentile (ms)
    public long PeakMemoryUsage { get; set; }       // Peak memory usage (bytes)
    public int WorkerThreadsUsed { get; set; }      // Thread pool utilization
    public double WorkerUtilization { get; set; }   // Worker efficiency percentage
    public int BatchesCompleted { get; set; }       // Number of execution batches
    public int RequestsStarted { get; set; }        // Total requests initiated
    public int RequestsInFlight { get; set; }       // Concurrent requests at end
}
```

## ?? Usage Patterns

### Pattern 1: HTTP API Testing
```csharp
[LoadFact(order: 1, concurrency: 5, duration: 3000, interval: 200)]
public async Task LoadTest_API()
{
    var result = await LoadTestHelper.ExecuteLoadTestAsync(async () =>
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
    var result = await LoadTestHelper.ExecuteLoadTestAsync(async () =>
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
    var result = await LoadTestHelper.ExecuteLoadTestAsync(async () =>
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

## ?? Important Notes

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

## ?? Advanced Usage

### Custom Metrics Collection
```csharp
[LoadFact(order: 1, concurrency: 5, duration: 3000, interval: 200)]
public async Task LoadTest_WithCustomMetrics()
{
    var customMetrics = new ConcurrentBag<double>();
    
    var result = await LoadTestHelper.ExecuteLoadTestAsync(async () =>
    {
        var stopwatch = Stopwatch.StartNew();
        
        // Your test logic here
        await SomeOperation();
        
        stopwatch.Stop();
        customMetrics.Add(stopwatch.ElapsedMilliseconds);
        return true;
    });
    
    var avgCustomMetric = customMetrics.Average();
    Console.WriteLine($"Custom metric average: {avgCustomMetric:F2}ms");
}
```

### Conditional Success Criteria
```csharp
[LoadFact(order: 1, concurrency: 3, duration: 2000, interval: 400)]
public async Task LoadTest_ConditionalSuccess()
{
    var result = await LoadTestHelper.ExecuteLoadTestAsync(async () =>
    {
        var response = await httpClient.GetAsync("/api/data");
        
        // Custom success criteria
        if (response.StatusCode == HttpStatusCode.OK)
            return true;
        if (response.StatusCode == HttpStatusCode.NotModified)
            return true; // 304 is acceptable
            
        return false; // All other status codes are failures
    });
    
    Assert.True(result.Success > 0);
}
```

## ?? Related Documentation

- [LoadFact Attribute Guide](../user-guides/loadfact-attribute-guide.md)
- [Load Results Reference](load-results.md)
- [Performance Optimization](../user-guides/performance-optimization.md)
- [Troubleshooting Guide](../advanced/troubleshooting.md)