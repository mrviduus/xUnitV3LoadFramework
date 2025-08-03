# LoadFact Attribute Guide

The `LoadFactAttribute` is the cornerstone of the xUnitV3LoadFramework, enabling you to transform any test method into a comprehensive load test with simple attribute decoration.

## ?? Basic Syntax

```csharp
[LoadFact(order: 1, concurrency: 2, duration: 5000, interval: 500)]
public async Task MyLoadTest()
{
    var result = await LoadTestHelper.ExecuteLoadTestAsync(async () =>
    {
        // Your test logic here
        return true;
    });
    
    Assert.True(result.Success > 0);
}
```

## ?? Parameters

### `order` (int)
**Purpose**: Defines the execution sequence when multiple LoadFact tests are present.  
**Range**: Any integer value (can be negative)  
**Default**: No default - must be specified  
**Example**: `order: 1` executes before `order: 2`

```csharp
[LoadFact(order: 1, concurrency: 2, duration: 1000, interval: 100)]
public async Task FirstTest() { /* ... */ }

[LoadFact(order: 2, concurrency: 3, duration: 2000, interval: 200)]
public async Task SecondTest() { /* ... */ }
```

### `concurrency` (int)
**Purpose**: Number of parallel operations to execute simultaneously.  
**Range**: 1 to 1000+ (practical limits depend on system resources)  
**Validation**: Must be ? 1  
**Impact**: Higher values increase system load and resource usage

```csharp
// Light load - 2 parallel requests
[LoadFact(order: 1, concurrency: 2, duration: 5000, interval: 500)]

// Heavy load - 20 parallel requests
[LoadFact(order: 1, concurrency: 20, duration: 5000, interval: 500)]
```

### `duration` (int)
**Purpose**: Total time the load test will run (in milliseconds).  
**Range**: 1 to int.MaxValue  
**Validation**: Must be ? 1  
**Note**: Actual test time may be slightly longer due to cleanup operations

```csharp
// 5-second test
[LoadFact(order: 1, concurrency: 5, duration: 5000, interval: 200)]

// 30-second test
[LoadFact(order: 1, concurrency: 5, duration: 30000, interval: 200)]
```

### `interval` (int)
**Purpose**: Time between each batch of concurrent operations (in milliseconds).  
**Range**: 1 to int.MaxValue  
**Validation**: Must be ? 1  
**Impact**: Shorter intervals = higher request rate

```csharp
// High frequency - new batch every 100ms
[LoadFact(order: 1, concurrency: 5, duration: 5000, interval: 100)]

// Lower frequency - new batch every 1000ms (1 second)
[LoadFact(order: 1, concurrency: 5, duration: 5000, interval: 1000)]
```

## ?? Load Calculation Examples

### Example 1: Moderate Load
```csharp
[LoadFact(order: 1, concurrency: 3, duration: 10000, interval: 500)]
```
- **Batches**: 10000ms ÷ 500ms = 20 batches
- **Requests per batch**: 3
- **Total requests**: ~60 requests
- **Rate**: ~6 requests/second

### Example 2: High Load
```csharp
[LoadFact(order: 1, concurrency: 10, duration: 5000, interval: 100)]
```
- **Batches**: 5000ms ÷ 100ms = 50 batches
- **Requests per batch**: 10
- **Total requests**: ~500 requests
- **Rate**: ~100 requests/second

## ??? Usage Patterns

### Pattern 1: API Endpoint Testing
```csharp
[LoadFact(order: 1, concurrency: 5, duration: 3000, interval: 200)]
public async Task LoadTest_UserAPI()
{
    var result = await LoadTestHelper.ExecuteLoadTestAsync(async () =>
    {
        var response = await httpClient.GetAsync("/api/users");
        response.EnsureSuccessStatusCode();
        
        var users = await response.Content.ReadFromJsonAsync<List<User>>();
        Assert.NotNull(users);
        return true;
    });
    
    Assert.True(result.Success > 0, "API should handle load successfully");
}
```

### Pattern 2: Database Operations
```csharp
[LoadFact(order: 1, concurrency: 3, duration: 5000, interval: 400)]
public async Task LoadTest_DatabaseOperations()
{
    var result = await LoadTestHelper.ExecuteLoadTestAsync(async () =>
    {
        using var context = GetService<MyDbContext>();
        var user = await context.Users.FirstOrDefaultAsync();
        Assert.NotNull(user);
        return true;
    });
    
    Assert.True(result.Success > 0, "Database should handle concurrent access");
}
```

### Pattern 3: External Service Integration
```csharp
[LoadFact(order: 1, concurrency: 4, duration: 8000, interval: 300)]
public async Task LoadTest_ExternalService()
{
    var result = await LoadTestHelper.ExecuteLoadTestAsync(async () =>
    {
        var response = await httpClient.GetAsync("https://api.external-service.com/data");
        response.EnsureSuccessStatusCode();
        return true;
    });
    
    Assert.True(result.Success >= result.Total * 0.95, "95% success rate expected");
}
```

## ? Performance Considerations

### CPU-Intensive Operations
```csharp
// Lower concurrency for CPU-bound tasks
[LoadFact(order: 1, concurrency: 2, duration: 5000, interval: 200)]
```

### I/O-Intensive Operations
```csharp
// Higher concurrency for I/O-bound tasks
[LoadFact(order: 1, concurrency: 10, duration: 5000, interval: 100)]
```

### Memory-Sensitive Operations
```csharp
// Moderate settings to avoid memory pressure
[LoadFact(order: 1, concurrency: 5, duration: 3000, interval: 300)]
```

## ?? Common Mistakes

### ? Incorrect: Zero or Negative Values
```csharp
[LoadFact(order: 1, concurrency: 0, duration: 1000, interval: 100)]  // Error!
[LoadFact(order: 1, concurrency: 5, duration: 0, interval: 100)]     // Error!
[LoadFact(order: 1, concurrency: 5, duration: 1000, interval: 0)]    // Error!
```

### ? Incorrect: Unrealistic Parameters
```csharp
// Too aggressive - may overwhelm system
[LoadFact(order: 1, concurrency: 1000, duration: 60000, interval: 1)]
```

### ? Correct: Balanced Configuration
```csharp
// Realistic settings for most scenarios
[LoadFact(order: 1, concurrency: 5, duration: 5000, interval: 200)]
```

## ?? Best Practices

1. **Start Small**: Begin with low concurrency and short duration
2. **Gradual Increase**: Incrementally increase load parameters
3. **Monitor Resources**: Watch CPU, memory, and network usage
4. **Realistic Scenarios**: Match production traffic patterns
5. **Multiple Tests**: Use different parameter combinations
6. **Assertion Strategy**: Validate both success rate and performance metrics

## ?? Debugging Tips

### Enable Detailed Logging
```csharp
[LoadFact(order: 1, concurrency: 2, duration: 2000, interval: 500)]
public async Task DebugLoadTest()
{
    var result = await LoadTestHelper.ExecuteLoadTestAsync(async () =>
    {
        Console.WriteLine($"Executing at {DateTime.Now:HH:mm:ss.fff}");
        // Your test logic
        return true;
    });
    
    Console.WriteLine($"Results: {result.Success}/{result.Total} successful");
}
```

### Parameter Validation
```csharp
[Fact]
public void ValidateLoadTestParameters()
{
    // Test your LoadFact configuration
    var attr = new LoadFactAttribute(1, 5, 3000, 200);
    Assert.Equal(5, attr.Concurrency);
    Assert.Equal(3000, attr.Duration);
}
```

## ?? Related Documentation

- [LoadTestHelper API Reference](../api-reference/loadtest-helper.md)
- [Load Results Guide](../api-reference/load-results.md)
- [Performance Optimization](performance-optimization.md)
- [Mixed Testing Support](mixed-testing-support.md)