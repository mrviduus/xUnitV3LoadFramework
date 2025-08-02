# xUnitV3LoadFramework v2 - Migration Guide

## 🚀 Welcome to Version 2

xUnitV3LoadFramework v2 brings full compatibility with xUnit v3 while maintaining all powerful load testing capabilities. The key changes remove dependencies on the `Specification` pattern and rename `Load` attributes to `Stress` for better semantic clarity.

---

## 🔄 **What's New in V2**

### ✨ **Key Improvements**
- **No More Specification Base Class** - Use plain classes with standard xUnit patterns
- **Full xUnit v3 Integration** - Mix `[Fact]`, `[Theory]`, and `[Stress]` tests seamlessly
- **Async/Await Native Support** - First-class async test method support
- **Better Lifecycle Management** - Standard constructor/dispose pattern
- **Enhanced Performance** - Improved actor system with hybrid execution modes

### 🏷️ **Attribute Changes**
- `[Load]` → `[Stress]` (with full backward compatibility)
- `[UseLoadFramework]` → `[UseStressFramework]` (with full backward compatibility)

---

## 📋 **Migration Examples**

### **V1 Pattern (Deprecated but still works)**
```csharp
[UseLoadFramework]
public class ApiTests : Specification
{
    protected override void EstablishContext()
    {
        // Setup code here
    }

    protected override void Because()
    {
        // Action code here
    }

    [Load(order: 1, concurrency: 10, duration: 5000)]
    public void Should_Handle_Load()
    {
        // Test verification here
    }

    protected override void DestroyContext()
    {
        // Cleanup code here
    }
}
```

### **V2 Pattern (Recommended)**
```csharp
[UseStressFramework]
public class ApiTests
{
    private readonly HttpClient _client;

    // Standard constructor for setup
    public ApiTests()
    {
        _client = new HttpClient();
    }

    [Stress(order: 1, concurrency: 10, duration: 5000)]
    public async Task Should_Handle_Stress()
    {
        // Complete test implementation in one method
        var response = await _client.GetAsync("/api/endpoint");
        
        // Direct assertions
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.NotEmpty(content);
    }

    // Standard disposal for cleanup
    public void Dispose()
    {
        _client?.Dispose();
    }
}
```

---

## 🎯 **Core Features**

### **1. Mixed Testing Support**
Run standard xUnit tests alongside stress tests:

```csharp
public class MixedTestClass
{
    [Fact]
    public void Unit_Test_User_Validation()
    {
        var user = new User { Name = "Test" };
        Assert.NotNull(user.Name);
    }

    [Theory]
    [InlineData(1, 2, 3)]
    [InlineData(5, 5, 10)]
    public void Theory_Test_Addition(int a, int b, int expected)
    {
        Assert.Equal(expected, a + b);
    }

    [UseStressFramework]
    [Stress(concurrency: 50, duration: 15000)]
    public async Task Stress_Test_User_Creation()
    {
        var user = await CreateUserAsync();
        Assert.NotNull(user);
        Assert.True(user.Id > 0);
    }
}
```

### **2. Advanced Async Support**
Full async/await support throughout:

```csharp
[UseStressFramework]
public class AsyncStressTests
{
    [Stress(concurrency: 100, duration: 30000)]
    public async Task Should_Handle_Async_Database_Operations()
    {
        using var context = new ApplicationDbContext();
        
        var user = new User 
        { 
            Name = $"User_{Guid.NewGuid()}", 
            CreatedAt = DateTime.UtcNow 
        };

        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        var savedUser = await context.Users.FindAsync(user.Id);
        Assert.NotNull(savedUser);
        Assert.Equal(user.Name, savedUser.Name);
    }
}
```

### **3. Dependency Injection Support**
Works with xUnit's dependency injection patterns:

```csharp
[UseStressFramework]
public class DIStressTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public DIStressTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Stress(concurrency: 50, duration: 10000)]
    public async Task Should_Handle_Web_Api_Load()
    {
        var response = await _client.GetAsync("/api/users");
        response.EnsureSuccessStatusCode();
        
        var users = await response.Content.ReadFromJsonAsync<User[]>();
        Assert.NotNull(users);
    }
}
```

---

## 🔧 **Stress Attribute Configuration**

### **Basic Parameters**
```csharp
[Stress(
    order: 1,           // Execution order
    concurrency: 50,    // Concurrent executions
    duration: 30000,    // Duration in milliseconds
    interval: 1000      // Interval between batches
)]
```

### **Advanced Scenarios**

#### **Smoke Testing**
```csharp
[Stress(concurrency: 1, duration: 5000)]
public async Task Smoke_Test_Basic_Functionality() { }
```

#### **Load Testing**
```csharp
[Stress(concurrency: 100, duration: 300000)]
public async Task Load_Test_Expected_Traffic() { }
```

#### **Stress Testing**
```csharp
[Stress(concurrency: 500, duration: 600000)]
public async Task Stress_Test_High_Load() { }
```

#### **Spike Testing**
```csharp
[Stress(concurrency: 200, duration: 60000, interval: 500)]
public async Task Spike_Test_Sudden_Load() { }
```

---

## 📊 **Performance and Monitoring**

### **Actor System Architecture**
V2 maintains the powerful actor-based execution system:

- **Standard Mode**: Uses Akka.NET actors for reliable execution
- **Hybrid Mode**: High-performance channel-based execution for 100k+ requests
- **Result Aggregation**: Real-time metrics collection and reporting

### **Built-in Metrics**
- Total requests executed
- Requests per second
- Success/failure rates
- Latency percentiles (95th, 99th)
- Memory usage tracking
- Concurrent execution monitoring

### **JSON Results Export**
```csharp
// Results are automatically collected and can be exported
var results = await stressTest.GetResultsAsync();
var json = results.ExportToJson();
```

---

## 🛠️ **Migration Steps**

### **Step 1: Update Attributes**
```csharp
// Change this:
[UseLoadFramework]
[Load(order: 1, concurrency: 10, duration: 5000)]

// To this:
[UseStressFramework]
[Stress(order: 1, concurrency: 10, duration: 5000)]
```

### **Step 2: Remove Specification Base Class**
```csharp
// Change this:
public class MyTests : Specification

// To this:
public class MyTests
```

### **Step 3: Move Setup to Constructor**
```csharp
// Change this:
protected override void EstablishContext()
{
    _client = new HttpClient();
}

// To this:
public MyTests()
{
    _client = new HttpClient();
}
```

### **Step 4: Combine Because() and Test Method**
```csharp
// Change this:
protected override void Because()
{
    _response = await _client.GetAsync("/api/test");
}

[Load(concurrency: 10, duration: 5000)]
public void Should_Return_Success()
{
    Assert.Equal(HttpStatusCode.OK, _response.StatusCode);
}

// To this:
[Stress(concurrency: 10, duration: 5000)]
public async Task Should_Return_Success()
{
    var response = await _client.GetAsync("/api/test");
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
}
```

### **Step 5: Move Cleanup to Dispose**
```csharp
// Change this:
protected override void DestroyContext()
{
    _client?.Dispose();
}

// To this:
public void Dispose()
{
    _client?.Dispose();
}
```

---

## 🔄 **Backward Compatibility**

V2 maintains full backward compatibility. Your existing V1 code will continue to work:

- `[Load]` attributes automatically inherit from `[Stress]`
- `[UseLoadFramework]` automatically inherits from `[UseStressFramework]`
- Specification-based tests continue to function
- All V1 examples remain operational

**Migration Timeline:**
- **V2.x**: V1 patterns work with deprecation warnings
- **V3.0**: V1 patterns will be removed

---

## 🚀 **Getting Started with V2**

### **1. Install/Update Package**
```xml
<PackageReference Include="xUnitV3LoadFramework" Version="2.0.0" />
```

### **2. Update Global Usings**
```csharp
global using Xunit;
global using xUnitV3LoadFramework.Attributes;

[assembly: TestFramework("xUnitV3LoadFramework.Extensions.Framework.LoadTestFramework", "xUnitV3LoadFramework")]
```

### **3. Create Your First V2 Stress Test**
```csharp
[UseStressFramework]
public class MyFirstStressTest
{
    [Stress(concurrency: 10, duration: 5000)]
    public async Task Should_Handle_Concurrent_Requests()
    {
        // Your test logic here
        await Task.Delay(100);
        Assert.True(true);
    }
}
```

---

## 🎯 **Best Practices**

### **1. Use Async Methods**
Always use async methods for stress tests to enable proper concurrency:
```csharp
[Stress(concurrency: 50, duration: 10000)]
public async Task Good_Async_Test()
{
    await SomeAsyncOperation();
}
```

### **2. Resource Management**
Use proper disposal patterns:
```csharp
public class StressTests : IDisposable
{
    private readonly HttpClient _client = new();
    
    public void Dispose() => _client?.Dispose();
}
```

### **3. Test Isolation**
Ensure tests don't interfere with each other:
```csharp
[Stress(concurrency: 10, duration: 5000)]
public async Task Isolated_Test()
{
    var uniqueId = Guid.NewGuid();
    var resource = await CreateUniqueResource(uniqueId);
    // Test with unique resource
}
```

### **4. Meaningful Assertions**
Include assertions that validate the actual functionality:
```csharp
[Stress(concurrency: 20, duration: 8000)]
public async Task Should_Validate_Business_Logic()
{
    var result = await BusinessOperation();
    
    Assert.NotNull(result);
    Assert.True(result.IsValid);
    Assert.InRange(result.ProcessingTime, 0, 5000);
}
```

---

## 📚 **Additional Resources**

- **API Reference**: Complete API documentation
- **Examples Repository**: Real-world examples and patterns
- **Performance Tuning Guide**: Optimize your stress tests
- **Troubleshooting Guide**: Common issues and solutions

---

**Welcome to xUnitV3LoadFramework v2! Happy stress testing! 🚀**
