# Migration Guide: From Specification to Standard xUnit Patterns

## Overview

The `Specification` base class has been removed from the xUnitV3LoadFramework to simplify the framework and make it more compatible with standard xUnit patterns. This change enables you to use standard xUnit constructor and `IDisposable` patterns while still getting the benefits of load testing.

## What Changed

### Removed
- `Specification` abstract base class
- `EstablishContext()` method
- `Because()` method  
- `DestroyContext()` method
- `OnStart()` and `OnFinish()` internal methods

### Now Use Standard xUnit Patterns
- **Constructor** for setup (replaces `EstablishContext()`)
- **IDisposable.Dispose()** for cleanup (replaces `DestroyContext()`)
- **Test method content** for test logic (replaces `Because()`)
- **[Fact] and [Theory]** alongside [Load] in the same class

## Migration Steps

### Before (with Specification)
```csharp
public class MyLoadTests : Specification
{
    private HttpClient _httpClient;
    
    protected override void EstablishContext()
    {
        _httpClient = new HttpClient();
        Console.WriteLine("Setup completed");
    }
    
    protected override void Because()
    {
        // This was called for each iteration
        Console.WriteLine("Action executed");
    }
    
    protected override void DestroyContext()
    {
        _httpClient?.Dispose();
        Console.WriteLine("Cleanup completed");
    }
    
    [Load(order: 1, concurrency: 5, duration: 3000, interval: 500)]
    public void Should_Handle_Load()
    {
        // Test specific logic here
    }
}
```

### After (Standard xUnit)
```csharp
public class MyLoadTests : IDisposable
{
    private readonly HttpClient _httpClient;
    
    public MyLoadTests()
    {
        // Setup - replaces EstablishContext()
        _httpClient = new HttpClient();
        Console.WriteLine("Setup completed");
    }
    
    public void Dispose()
    {
        // Cleanup - replaces DestroyContext()
        _httpClient?.Dispose();
        Console.WriteLine("Cleanup completed");
    }
    
    [Fact]
    public async Task Should_Connect_Successfully()
    {
        // Standard xUnit test
        var response = await _httpClient.GetAsync("https://httpbin.org/get");
        Assert.True(response.IsSuccessStatusCode);
    }
    
    [Load(order: 1, concurrency: 5, duration: 3000, interval: 500)]
    public async Task Should_Handle_Load()
    {
        // Load test logic - includes what was in Because() 
        var response = await _httpClient.GetAsync("https://httpbin.org/get");
        Assert.True(response.IsSuccessStatusCode);
        Console.WriteLine("Load test iteration completed");
    }
}
```

## Benefits of the Change

1. **Standard xUnit Compatibility**: Full compatibility with standard xUnit patterns
2. **Mixed Testing**: Can use [Fact], [Theory], and [Load] in the same class
3. **Simplified Learning**: No custom base class to learn
4. **Better Tooling Support**: Standard patterns work with all xUnit tooling
5. **Cleaner Code**: Less abstraction, more explicit test logic

## Key Differences

| Aspect | Old (Specification) | New (Standard xUnit) |
|--------|-------------------|---------------------|
| Base Class | `Specification` | None (or `IDisposable`) |
| Setup | `EstablishContext()` | Constructor |
| Action | `Because()` | Test method content |
| Cleanup | `DestroyContext()` | `Dispose()` |
| Test Types | Load tests only | [Fact], [Theory], [Load] |

## Examples

### Simple Load Test
```csharp
public class SimpleLoadTest
{
    [Load(order: 1, concurrency: 10, duration: 5000, interval: 1000)]
    public void Should_Handle_Simple_Load()
    {
        // Your load test logic here
        Thread.Sleep(100); // Simulate work
        Console.WriteLine($"Executed at {DateTime.Now:HH:mm:ss.fff}");
    }
}
```

### Mixed Test Class
```csharp
public class MixedTests : IDisposable
{
    private readonly HttpClient _client;
    
    public MixedTests()
    {
        _client = new HttpClient();
    }
    
    public void Dispose()
    {
        _client?.Dispose();
    }
    
    [Fact]
    public async Task Should_Connect_To_API()
    {
        var response = await _client.GetAsync("https://api.example.com/health");
        Assert.True(response.IsSuccessStatusCode);
    }
    
    [Load(order: 1, concurrency: 50, duration: 30000, interval: 2000)]
    public async Task Should_Handle_API_Load()
    {
        var response = await _client.GetAsync("https://api.example.com/data");
        Assert.True(response.IsSuccessStatusCode);
    }
}
```

### Database Load Test
```csharp
public class DatabaseLoadTest : IDisposable
{
    private readonly DbContext _context;
    
    public DatabaseLoadTest()
    {
        var options = new DbContextOptionsBuilder<MyDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new MyDbContext(options);
        _context.Database.EnsureCreated();
    }
    
    public void Dispose()
    {
        _context?.Dispose();
    }
    
    [Load(order: 1, concurrency: 20, duration: 10000, interval: 500)]
    public void Should_Handle_Database_Operations()
    {
        var user = new User { Name = $"User_{Guid.NewGuid()}" };
        _context.Users.Add(user);
        _context.SaveChanges();
        
        var count = _context.Users.Count();
        Assert.True(count > 0);
    }
}
```

## Troubleshooting

### Common Issues

1. **Compilation Errors**: Remove `using xUnitV3LoadFramework.Extensions;` and inheritance from `Specification`
2. **Missing Setup**: Move `EstablishContext()` logic to constructor
3. **Missing Cleanup**: Move `DestroyContext()` logic to `Dispose()` method
4. **Test Logic**: Move `Because()` logic directly into test methods

### Framework Configuration

Make sure your `GlobalUsings.cs` or test assembly still has:
```csharp
[assembly: TestFramework(typeof(LoadTestFramework))]
```

This ensures load tests run with the Load Testing Framework while standard tests use regular xUnit.

## Need Help?

If you encounter issues during migration:
1. Check the examples in this guide
2. Review the updated documentation
3. Look at the example test files in the project
4. Open an issue on GitHub if you need assistance
