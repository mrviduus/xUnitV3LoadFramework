# xUnitV3LoadFramework v2 Migration Guide

## 🚀 Overview

Version 2 of xUnitV3LoadFramework introduces significant improvements for better xUnit v3 integration while maintaining all performance capabilities. The main changes focus on removing the `Specification` pattern requirement and renaming the core attributes for better semantic clarity.

---

## 🔄 Key Changes

### 1. Specification Pattern Removal
- **Removed**: Abstract `Specification` base class requirement
- **Added**: Direct integration with standard xUnit lifecycle patterns
- **Benefit**: Cleaner code, standard xUnit patterns, better IDE support

### 2. Attribute Renaming
- `LoadAttribute` → `StressAttribute` (with backward compatibility)
- `UseLoadFrameworkAttribute` → `UseStressFrameworkAttribute` (with backward compatibility)

### 3. Enhanced xUnit Compatibility
- Full support for mixing `[Fact]`, `[Theory]`, and `[Stress]` in same class
- Standard xUnit lifecycle support (`IDisposable`, `IAsyncDisposable`)
- Better integration with xUnit fixtures and dependency injection

---

## 📋 Migration Steps

### Step 1: Update Attribute Names (Optional but Recommended)

**Before (v1):**
```csharp
[UseLoadFramework]
public class MyTests : Specification
{
    [Load(order: 1, concurrency: 10, duration: 5000, interval: 1000)]
    public void Should_Handle_Load() { }
}
```

**After (v2):**
```csharp
[UseStressFramework]  // Renamed from UseLoadFramework
public class MyTests
{
    [Stress(order: 1, concurrency: 10, duration: 5000, interval: 1000)]  // Renamed from Load
    public async Task Should_Handle_Stress() { }
}
```

> **Note**: The old attribute names still work but are marked as obsolete and will be removed in v3.0.

### Step 2: Remove Specification Base Class

**Before (v1):**
```csharp
[UseLoadFramework]
public class ApiTests : Specification
{
    private HttpClient _httpClient;

    protected override void EstablishContext()
    {
        _httpClient = new HttpClient();
        Console.WriteLine("Setup completed");
    }

    protected override void Because()
    {
        Console.WriteLine("Common action for all tests");
    }

    protected override void DestroyContext()
    {
        _httpClient?.Dispose();
        Console.WriteLine("Cleanup completed");
    }

    [Load(order: 1, concurrency: 5, duration: 3000, interval: 500)]
    public void Should_Handle_Load()
    {
        // Test implementation
    }
}
```

**After (v2):**
```csharp
[UseStressFramework]
public class ApiTests : IAsyncDisposable
{
    private readonly HttpClient _httpClient;

    public ApiTests()
    {
        // Setup in constructor (replaces EstablishContext)
        _httpClient = new HttpClient();
        Console.WriteLine("Setup completed");
    }

    [Stress(order: 1, concurrency: 5, duration: 3000, interval: 500)]
    public async Task Should_Handle_Stress()
    {
        // Common action + test implementation (replaces Because + test logic)
        Console.WriteLine("Common action for all tests");
        
        // Your test logic here
        var response = await _httpClient.GetAsync("https://api.example.com/test");
        Assert.True(response.IsSuccessStatusCode);
    }

    public async ValueTask DisposeAsync()
    {
        // Cleanup (replaces DestroyContext)
        _httpClient?.Dispose();
        Console.WriteLine("Cleanup completed");
    }
}
```

### Step 3: Update Test Methods

**Before (v1):**
```csharp
[Load(order: 1, concurrency: 10, duration: 5000, interval: 1000)]
public void Should_Process_Orders()
{
    // Test logic here - Because() was called automatically
}
```

**After (v2):**
```csharp
[Stress(order: 1, concurrency: 10, duration: 5000, interval: 1000)]
public async Task Should_Process_Orders()
{
    // Include any common setup logic directly in the test method
    // Your test logic here
    await ProcessOrderAsync();
    Assert.True(orderProcessed);
}
```

---

## 🎯 Migration Patterns

### Pattern 1: Simple Test Class

**v1 Approach:**
```csharp
[UseLoadFramework]
public class SimpleTests : Specification
{
    protected override void EstablishContext()
    {
        // Setup
    }

    [Load(order: 1, concurrency: 5, duration: 2000, interval: 500)]
    public void Test1() { /* logic */ }

    [Load(order: 2, concurrency: 10, duration: 3000, interval: 300)]
    public void Test2() { /* logic */ }
}
```

**v2 Approach:**
```csharp
[UseStressFramework]
public class SimpleTests
{
    public SimpleTests()
    {
        // Setup
    }

    [Stress(order: 1, concurrency: 5, duration: 2000, interval: 500)]
    public async Task Test1() { /* logic */ }

    [Stress(order: 2, concurrency: 10, duration: 3000, interval: 300)]
    public async Task Test2() { /* logic */ }
}
```

### Pattern 2: Mixed Test Types

**v2 Only (New Capability):**
```csharp
[UseStressFramework]
public class MixedTests
{
    [Fact]
    public void Quick_Unit_Test()
    {
        // Runs immediately with standard xUnit
        Assert.True(IsSystemReady());
    }

    [Theory]
    [InlineData(1, 2, 3)]
    [InlineData(5, 10, 15)]
    public void Parameterized_Test(int a, int b, int expected)
    {
        // Runs immediately with standard xUnit
        Assert.Equal(expected, a + b);
    }

    [Stress(order: 1, concurrency: 20, duration: 10000, interval: 1000)]
    public async Task Heavy_Load_Test()
    {
        // Runs with actor system and performance monitoring
        await PerformHeavyOperation();
        Assert.True(operationSucceeded);
    }
}
```

### Pattern 3: Resource Management

**v1 Approach:**
```csharp
[UseLoadFramework]
public class DatabaseTests : Specification
{
    private DbContext _context;

    protected override void EstablishContext()
    {
        _context = new MyDbContext();
    }

    protected override void DestroyContext()
    {
        _context?.Dispose();
    }
}
```

**v2 Approach:**
```csharp
[UseStressFramework]
public class DatabaseTests : IDisposable
{
    private readonly DbContext _context;

    public DatabaseTests()
    {
        _context = new MyDbContext();
    }

    [Stress(order: 1, concurrency: 15, duration: 8000, interval: 600)]
    public async Task Should_Handle_Database_Stress()
    {
        // Test implementation
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}
```

### Pattern 4: Async Resource Management

**v2 Approach (New):**
```csharp
[UseStressFramework]
public class AsyncResourceTests : IAsyncDisposable
{
    private readonly IHost _host;
    private readonly HttpClient _httpClient;

    public AsyncResourceTests()
    {
        _host = CreateHost();
        _host.StartAsync().GetAwaiter().GetResult();
        _httpClient = _host.Services.GetRequiredService<HttpClient>();
    }

    [Stress(order: 1, concurrency: 25, duration: 12000, interval: 800)]
    public async Task Should_Handle_Http_Stress()
    {
        var response = await _httpClient.GetAsync("/api/endpoint");
        Assert.True(response.IsSuccessStatusCode);
    }

    public async ValueTask DisposeAsync()
    {
        _httpClient?.Dispose();
        if (_host != null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }
    }
}
```

---

## 🔧 Advanced Migration Scenarios

### Scenario 1: Complex Setup Logic

**v1:**
```csharp
[UseLoadFramework]
public class ComplexTests : Specification
{
    private ServiceProvider _serviceProvider;
    private IMyService _service;

    protected override void EstablishContext()
    {
        var services = new ServiceCollection();
        services.AddTransient<IMyService, MyService>();
        _serviceProvider = services.BuildServiceProvider();
        _service = _serviceProvider.GetRequiredService<IMyService>();
    }

    protected override void Because()
    {
        _service.Initialize();
    }
}
```

**v2:**
```csharp
[UseStressFramework]
public class ComplexTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly IMyService _service;

    public ComplexTests()
    {
        var services = new ServiceCollection();
        services.AddTransient<IMyService, MyService>();
        _serviceProvider = services.BuildServiceProvider();
        _service = _serviceProvider.GetRequiredService<IMyService>();
        
        // Initialize service (replaces Because)
        _service.Initialize();
    }

    [Stress(order: 1, concurrency: 8, duration: 6000, interval: 750)]
    public async Task Should_Handle_Service_Stress()
    {
        await _service.ProcessAsync();
        Assert.True(_service.IsHealthy);
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }
}
```

### Scenario 2: Shared Test Data

**v1:**
```csharp
[UseLoadFramework]
public class SharedDataTests : Specification
{
    private List<TestData> _testData;

    protected override void EstablishContext()
    {
        _testData = LoadTestData();
    }

    protected override void Because()
    {
        _testData = ProcessTestData(_testData);
    }
}
```

**v2:**
```csharp
[UseStressFramework]
public class SharedDataTests
{
    private readonly List<TestData> _testData;

    public SharedDataTests()
    {
        _testData = LoadTestData();
        _testData = ProcessTestData(_testData);
    }

    [Stress(order: 1, concurrency: 12, duration: 7000, interval: 900)]
    public async Task Should_Process_Shared_Data()
    {
        var item = _testData[Random.Shared.Next(_testData.Count)];
        await ProcessItem(item);
        Assert.NotNull(item.ProcessedResult);
    }
}
```

---

## 🚨 Breaking Changes

### 1. Specification Base Class Removed
- **Impact**: High - All classes inheriting from `Specification` need updates
- **Solution**: Remove inheritance and move logic to constructor/test methods

### 2. Lifecycle Method Changes
- **EstablishContext()** → Constructor
- **Because()** → Include logic in test methods
- **DestroyContext()** → `Dispose()` or `DisposeAsync()`

### 3. Test Method Signatures
- Methods can now be `async Task` for better async support
- No restrictions on method signatures (can use standard xUnit patterns)

---

## ✅ Benefits of v2

### 1. Standard xUnit Patterns
- Familiar constructor/dispose patterns
- Better IDE support and debugging
- Standard test runner integration

### 2. Enhanced Flexibility
- Mix stress tests with unit tests in same class
- Use standard xUnit features (fixtures, collections, etc.)
- Better async/await support

### 3. Improved Performance
- Reduced overhead from Specification pattern
- Direct method invocation
- Better memory usage

### 4. Better Tooling Support
- Standard xUnit test discovery
- Better integration with test explorers
- Improved debugging experience

---

## 🛠️ Automated Migration Tools

### Code Analyzer (Coming Soon)
We're developing a Roslyn analyzer to detect v1 patterns and suggest migrations:

```
CA2001: Replace Specification inheritance with standard constructor pattern
CA2002: Move EstablishContext logic to constructor
CA2003: Move DestroyContext logic to Dispose method
CA2004: Consider using StressAttribute instead of LoadAttribute
```

### Migration Script Example
```bash
# Find all classes inheriting from Specification
grep -r "class.*: Specification" --include="*.cs" .

# Find all LoadAttribute usages
grep -r "\[Load(" --include="*.cs" .

# Find all UseLoadFramework usages  
grep -r "\[UseLoadFramework\]" --include="*.cs" .
```

---

## 📞 Need Help?

- Check the [examples folder](../examples/V2Examples.cs) for complete working examples
- Review the [API documentation](../docs/api-reference/) for detailed reference
- Open an issue on GitHub for specific migration questions

---

## 🎯 Next Steps

1. **Start Small**: Migrate one test class at a time
2. **Test Thoroughly**: Ensure performance characteristics are maintained
3. **Update Documentation**: Update your team's documentation
4. **Plan Timeline**: The old attributes work until v3.0, giving you time to migrate
