# Migration Guide: To LoadFact Attribute Approach

## Overview

The xUnitV3LoadFramework has evolved to use a simpler `LoadFactAttribute` approach that provides full compatibility with standard xUnit v3 while maintaining powerful load testing capabilities. This change eliminates the need for custom test frameworks and makes the framework much easier to use.

## What Changed

### Removed
- Custom `LoadTestFramework` implementation
- `Specification` abstract base class
- Complex framework configuration requirements
- `[Load]` attribute (replaced with `[LoadFact]`)

### New Approach: LoadFact + LoadTestHelper
- **`[LoadFact]` attribute** that inherits from xUnit's `[Fact]` 
- **`LoadTestHelper.ExecuteLoadTestAsync()`** for running load tests
- **Standard xUnit v3 compatibility** with no custom framework needed
- **Mixed testing** - combine `[Fact]`, `[Theory]`, and `[LoadFact]` in same class

## Migration Steps

### Before (Old Load Attribute)
```csharp
[assembly: TestFramework(typeof(LoadTestFramework))]

public class MyLoadTests : IDisposable
{
    private HttpClient _httpClient;
    
    public MyLoadTests()
    {
        _httpClient = new HttpClient();
    }
    
    public void Dispose()
    {
        _httpClient?.Dispose();
    }
    
    [Load(order: 1, concurrency: 5, duration: 3000, interval: 500)]
    public void Should_Handle_Load()
    {
        var response = _httpClient.GetAsync("https://httpbin.org/get").Result;
        Assert.True(response.IsSuccessStatusCode);
    }
}
```

```

### After (LoadFact Approach)
```csharp
// No assembly configuration needed - uses standard xUnit v3

public class MyLoadTests : TestSetup // Optional for dependency injection
{
    [Fact]
    public async Task Should_Connect_Successfully()
    {
        // Standard xUnit test
        var httpClient = GetService<IHttpClientFactory>().CreateClient();
        var response = await httpClient.GetAsync("https://httpbin.org/get");
        Assert.True(response.IsSuccessStatusCode);
    }
    
    [LoadFact(order: 1, concurrency: 5, duration: 3000, interval: 500)]
    public async Task Should_Handle_Load()
    {
        // Load test using LoadTestHelper
        var result = await LoadTestHelper.ExecuteLoadTestAsync(async () =>
        {
            var httpClient = GetService<IHttpClientFactory>().CreateClient();
            var response = await httpClient.GetAsync("https://httpbin.org/get", TestContext.Current.CancellationToken);
            response.EnsureSuccessStatusCode();
            return true;
        });
        
        Assert.True(result.Success > 0, "Load test should have successful executions");
    }
}
```

## Benefits of the New Approach

1. **Standard xUnit Compatibility**: Uses standard xUnit v3 test discovery and execution
2. **No Custom Framework**: Eliminates complex framework configuration 
3. **Mixed Testing**: Combine `[Fact]`, `[Theory]`, and `[LoadFact]` in same class
4. **Better Tooling Support**: Works with all standard xUnit tooling and runners
5. **Simplified Setup**: No assembly attributes or framework configuration needed
6. **Async Support**: Full async/await support in load tests

## Key Differences

| Aspect | Old (Load + Framework) | New (LoadFact + Helper) |
|--------|----------------------|------------------------|
| Attribute | `[Load]` | `[LoadFact]` |
| Base Class | Any class | Optional `TestSetup` for DI |
| Framework Config | `[assembly: TestFramework]` | None needed |
| Test Method | Sync methods | Async methods with LoadTestHelper |
| Execution | Custom framework | Standard xUnit + load helper |

## Examples

### Simple Load Test
```csharp
public class SimpleLoadTest
{
    [LoadFact(order: 1, concurrency: 10, duration: 5000, interval: 1000)]
    public async Task Should_Handle_Simple_Load()
    {
        var result = await LoadTestHelper.ExecuteLoadTestAsync(async () =>
        {
            // Your load test logic here
            await Task.Delay(100); // Simulate work
            Console.WriteLine($"Executed at {DateTime.Now:HH:mm:ss.fff}");
            return true;
        });
        
        Assert.True(result.Success > 0);
    }
}
```

### Mixed Test Class
```csharp
public class MixedTests : TestSetup
{
    [Fact]
    public async Task Should_Connect_To_API()
    {
        var client = GetService<IHttpClientFactory>().CreateClient();
        var response = await client.GetAsync("https://api.example.com/health");
        Assert.True(response.IsSuccessStatusCode);
    }
    
    [LoadFact(order: 1, concurrency: 50, duration: 30000, interval: 2000)]
    public async Task Should_Handle_API_Load()
    {
        var result = await LoadTestHelper.ExecuteLoadTestAsync(async () =>
        {
            var client = GetService<IHttpClientFactory>().CreateClient();
            var response = await client.GetAsync("https://api.example.com/data", TestContext.Current.CancellationToken);
            response.EnsureSuccessStatusCode();
            return true;
        });
        
        Assert.True(result.Success > 0);
    }
}
```

### Database Load Test
```csharp
public class DatabaseLoadTest : TestSetup
{
    [LoadFact(order: 1, concurrency: 20, duration: 10000, interval: 500)]
    public async Task Should_Handle_Database_Operations()
    {
        var result = await LoadTestHelper.ExecuteLoadTestAsync(async () =>
        {
            var context = GetService<MyDbContext>();
            var user = new User { Name = $"User_{Guid.NewGuid()}" };
            context.Users.Add(user);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
            
            var count = await context.Users.CountAsync(TestContext.Current.CancellationToken);
            return count > 0;
        });
        
        Assert.True(result.Success > 0);
        Assert.True(result.Total > 0);
    }
}
```

## Troubleshooting

### Common Migration Issues

1. **Remove Assembly Configuration**: Delete `[assembly: TestFramework]` declarations
2. **Update Attributes**: Change `[Load]` to `[LoadFact]` 
3. **Add LoadTestHelper**: Wrap test logic in `LoadTestHelper.ExecuteLoadTestAsync()`
4. **Make Methods Async**: LoadFact methods should return `Task` and use async/await
5. **Use TestSetup**: Inherit from `TestSetup` for dependency injection support

### Updated GlobalUsings.cs
```csharp
global using Xunit;
global using xUnitV3LoadFramework.Attributes;
global using xUnitV3LoadFramework.Extensions;

// No TestFramework assembly attribute needed
```

### Before/After Comparison

**Before:**
```csharp
[assembly: TestFramework(typeof(LoadTestFramework))]

[Load(concurrency: 5, duration: 2000)]
public void MyLoadTest()
{
    // Sync test logic
}
```

**After:**
```csharp
// No assembly attribute needed

[LoadFact(concurrency: 5, duration: 2000)]
public async Task MyLoadTest()
{
    var result = await LoadTestHelper.ExecuteLoadTestAsync(async () =>
    {
        // Async test logic
        return true;
    });
    
    Assert.True(result.Success > 0);
}
```

## Need Help?

If you encounter issues during migration:
1. Check that you've removed all `[assembly: TestFramework]` declarations
2. Ensure all `[Load]` attributes are changed to `[LoadFact]`
3. Verify test methods use `LoadTestHelper.ExecuteLoadTestAsync()`
4. Make sure test methods are async and return `Task`
5. Review the examples in this guide
6. Look at the working test files like `WebTests.cs` in the project
