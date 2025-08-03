# Getting Started with xUnitV3LoadFramework

Welcome to xUnitV3LoadFramework! This guide will help you set up and run your first load tests in minutes.

## 🚀 Quick Start

### 1. Installation

Add the framework to your test project:

```xml
<PackageReference Include="xUnitV3LoadFramework" Version="1.0.0" />
<PackageReference Include="xunit.v3" Version="1.0.0-pre.100" />
<PackageReference Include="xunit.runner.visualstudio" Version="3.0.0-pre.30" />
```

### 2. Create Your First Load Test

Create a new test file `MyFirstLoadTest.cs`:

```csharp
using xUnitV3LoadFramework.Attributes;
using Xunit;

public class MyFirstLoadTest
{
    [Fact]
    public void Should_Pass_Basic_Test()
    {
        // Standard xUnit test
        Assert.True(true);
        Console.WriteLine("Standard test completed");
    }

    [Load(order: 1, concurrency: 10, duration: 5000, interval: 1000)] // 10 concurrent users for 5 seconds
    public void Should_Handle_Basic_Load()
    {
        // Simulate API call or database operation
        System.Threading.Thread.Sleep(100); // Simulates 100ms operation
        
        // Your test logic here
        Console.WriteLine("Load test iteration completed");
    }
}
```

### 3. Run the Test

Execute using your preferred test runner:

```bash
# Using dotnet CLI
dotnet test --filter "Should_Handle_Basic_Load"

# Using Visual Studio Test Explorer
# Right-click on the test and select "Run Tests"
```

### 4. View Results

The framework provides comprehensive metrics:

```
Test: Should_Handle_Basic_Load
Duration: 5.00s
Requests: 500 (10 concurrent × 5 seconds × ~10 RPS)
Success Rate: 100%
Average Latency: 102ms
P95 Latency: 115ms
P99 Latency: 128ms
Throughput: 99.8 RPS
Worker Threads: 10
```

## 📊 Understanding Load Test Configuration

### The Load Attribute

The `[Load]` attribute configures your test execution:

```csharp
[Load(
    order: 1,             // Execution order (Required)
    concurrency: 50,      // Number of concurrent users/threads (Required)
    duration: 30000,      // Test duration in milliseconds (Required)
    interval: 1000        // Reporting interval in milliseconds (Required)
)]
```

### Configuration Parameters

| Parameter | Description | Required | Example |
|-----------|-------------|----------|---------|
| `order` | Test execution order | Yes | `1` |
| `concurrency` | Concurrent users/threads | Yes | `50` |
| `duration` | Test duration (ms) | Yes | `30000` (30s) |
| `interval` | Progress reporting interval (ms) | Yes | `1000` (1s) |

## 🎯 Test Patterns

### 1. Simple Load Test

Perfect for basic performance validation:

```csharp
[UseLoadFramework]
public class SimpleLoadTests : Specification
{
    private readonly HttpClient _httpClient = new HttpClient();

    [Load(order: 1, concurrency: 20, duration: 10000, interval: 1000)]
    public async Task Should_Handle_Simple_Load()
    {
        var response = await _httpClient.GetAsync("https://api.example.com/health");
        
        // Your test logic and validation here
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"API returned {response.StatusCode}");
        }
    }
}
```

### 2. Database Load Test

Test database performance under load:

```csharp
[UseLoadFramework]
public class DatabaseLoadTests : Specification
{
    [Load(order: 1, concurrency: 100, duration: 60000, interval: 5000)]
    public async Task Should_Handle_Database_Load()
    {
        using var context = new MyDbContext();
        
        var user = await context.Users.FirstOrDefaultAsync(u => u.IsActive);
        if (user == null)
        {
            throw new Exception("No active user found");
        }
        
        // Simulate database update
        user.LastAccessTime = DateTime.UtcNow;
        await context.SaveChangesAsync();
    }
}
```

### 3. Sustained Load Test

Test system performance under sustained load:

```csharp
[UseLoadFramework]
public class SustainedLoadTests : Specification
{
    [Load(order: 1, concurrency: 200, duration: 120000, interval: 10000)]
    public async Task Should_Handle_Sustained_Load()
    {
        // Run 200 concurrent users for 2 minutes
        // Report progress every 10 seconds
        
        var service = new MyService();
        var result = await service.ProcessRequestAsync();
        
        if (!result.IsSuccess)
    {
        throw new Exception("Service request failed");
    }
}
```

### 4. Stress Test

Push system beyond normal capacity:

```csharp
[Load(order: 4, concurrency: 1000, duration: 300000, interval: 30000)] // 1000 users for 5 minutes
public async Task Should_Survive_Stress_Test()
{
    var service = CreateService();
    
    try
    {
        var result = await service.ExecuteWithRetryAsync();
        // Even under stress, some operations should succeed
        // or fail gracefully
    }
    catch (TimeoutException)
    {
        // Acceptable under stress - system is throttling
    }
    catch (ServiceUnavailableException)
    {
        // Acceptable under stress - system is protecting itself
    }
}
```

## 🔧 Test Setup and Teardown

### Using Specification Base Class

The `Specification` base class provides test lifecycle hooks:

```csharp
[UseLoadFramework]
public class ApiLoadTests : Specification
{
    private HttpClient _httpClient;
    private string _baseUrl;
    
    protected override void EstablishContext()
    {
        // Setup executed once before all load test iterations
        _baseUrl = "https://api.example.com";
        _httpClient = new HttpClient();
        
        // Warm up the system
        var warmup = _httpClient.GetAsync($"{_baseUrl}/health").Result;
    }
    
    protected override async void Because()
    {
        // This is called for each load test iteration
        var response = await _httpClient.GetAsync($"{_baseUrl}/users");
        Assert.True(response.IsSuccessStatusCode);
    }
    
    protected override void DestroyContext()
    {
        // Cleanup executed once after all iterations complete
        _httpClient?.Dispose();
    }
    
    [Load(order: 1, concurrency: 50, duration: 30000, interval: 1000)]
    public async Task Should_Handle_User_List_Load() 
    {
        // The Because() method will be executed under load
        // No additional code needed here
    }
}
```

### Manual Setup

For more control, handle setup manually:

```csharp
[UseLoadFramework]
public class CustomLoadTest : Specification
{
    [Load(order: 1, concurrency: 25, duration: 15000, interval: 1000)]
    public async Task Should_Handle_Custom_Load()
    {
        // Per-iteration setup (executed by each concurrent user)
        using var scope = CreateScope();
        var service = scope.GetService<IMyService>();
        
        // Execute test operation
        var result = await service.PerformOperationAsync();
        
        // Assertions
        Assert.NotNull(result);
        Assert.True(result.IsValid);
        
        // Cleanup happens automatically with 'using'
    }
    
    private IServiceScope CreateScope()
    {
        // Your DI container setup
        return serviceProvider.CreateScope();
    }
}
```

## 🏗️ Working with Dependency Injection

### Setting up DI Container

```csharp
[UseLoadFramework]
public class DatabaseLoadTests : Specification, IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    
    public DatabaseLoadTests()
    {
        var services = new ServiceCollection();
        services.AddDbContext<MyDbContext>(options =>
            options.UseSqlite("Data Source=:memory:"));
        services.AddTransient<IUserService, UserService>();
        
        _serviceProvider = services.BuildServiceProvider();
    }
    
    [Load(order: 1, concurrency: 100, duration: 60000, interval: 2000)]
    public async Task Should_Handle_Database_Load()
    {
        using var scope = _serviceProvider.CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
        
        var users = await userService.GetActiveUsersAsync();
        Assert.True(users.Count() > 0, "Should have active users");
    }
    
    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }
}
```

## 📈 Interpreting Results

### Key Metrics Explained

**Request Metrics**:
- `RequestsStarted`: Total requests initiated
- `Success/Failure`: Count of successful/failed requests
- `Success Rate`: Percentage of successful requests

**Latency Metrics**:
- `AverageLatency`: Mean response time
- `MedianLatency`: 50th percentile (half of requests faster)
- `P95Latency`: 95th percentile (95% of requests faster)
- `P99Latency`: 99th percentile (99% of requests faster)

**Throughput Metrics**:
- `RequestsPerSecond`: Average requests processed per second
- `BatchesCompleted`: Number of work batches completed

**Resource Metrics**:
- `WorkerThreadsUsed`: Number of worker threads active
- `WorkerUtilization`: Percentage of time workers were busy
- `PeakMemoryUsage`: Maximum memory consumption during test

### Performance Targets

Establish performance targets for your application:

```csharp
[Fact]
[Load(concurrency: 50, duration: 30000)]
public async Task Api_Should_Meet_Performance_Targets()
{
    // Your load test logic here
    await CallApiAsync();
    
    // Results are automatically captured by the framework
    // You can access them in test output or custom assertions
}

// Custom assertion helper (optional)
private void AssertPerformanceTargets(LoadResult result)
{
    Assert.True(result.RequestsPerSecond >= 100, 
        $"Expected >= 100 RPS, got {result.RequestsPerSecond}");
    
    Assert.True(result.Percentile95Latency <= 500, 
        $"Expected P95 <= 500ms, got {result.Percentile95Latency}ms");
    
    Assert.True(result.Success / (double)result.Total >= 0.99, 
        $"Expected >= 99% success rate, got {result.Success / (double)result.Total:P}");
}
```

## 🚨 Common Pitfalls and Solutions

### 1. Resource Exhaustion

**Problem**: Tests failing due to connection pool exhaustion

```csharp
// ❌ Bad - Creates new HttpClient for each iteration
[Load(concurrency: 100, duration: 60000)]
public async Task BadExample()
{
    var client = new HttpClient(); // Creates 100 instances!
    await client.GetAsync("https://api.example.com/data");
}

// ✅ Good - Reuse HttpClient instances
private static readonly HttpClient SharedClient = new HttpClient();

[Load(concurrency: 100, duration: 60000)]
public async Task GoodExample()
{
    await SharedClient.GetAsync("https://api.example.com/data");
}
```

### 2. Database Connection Issues

**Problem**: Database connection pool exhaustion

```csharp
// ✅ Good - Proper connection management
[Load(concurrency: 50, duration: 30000)]
public async Task Should_Handle_Database_Load()
{
    using var scope = _serviceProvider.CreateScope();
    using var context = scope.ServiceProvider.GetRequiredService<MyDbContext>();
    
    var result = await context.Users.FirstOrDefaultAsync();
    Assert.NotNull(result);
    
    // Connections automatically returned to pool with 'using'
}
```

### 3. Thread Safety Issues

**Problem**: Shared state causing race conditions

```csharp
// ❌ Bad - Shared mutable state
private int _counter = 0;

[Load(concurrency: 100, duration: 10000)]
public async Task BadThreadSafety()
{
    _counter++; // Race condition!
    await DoWorkAsync();
}

// ✅ Good - Thread-local or immutable state
[Load(concurrency: 100, duration: 10000)]
public async Task GoodThreadSafety()
{
    var localData = CreateLocalData();
    await DoWorkAsync(localData);
}
```

### 4. Unrealistic Test Conditions

**Problem**: Tests that don't reflect real usage

```csharp
// ❌ Bad - All users hit same endpoint simultaneously
[Load(concurrency: 1000, duration: 5000)]
public async Task UnrealisticLoad()
{
    await _httpClient.GetAsync("/api/users/1"); // Everyone gets user 1
}

// ✅ Good - Realistic user behavior
private readonly Random _random = new Random();

[Load(concurrency: 100, duration: 60000)]
public async Task RealisticLoad()
{
    var userId = _random.Next(1, 10000);
    await _httpClient.GetAsync($"/api/users/{userId}");
    
    // Add think time
    await Task.Delay(_random.Next(100, 1000));
}
```

## 🔧 Configuration Tips

### Test Environment Setup

```csharp
public class EnvironmentSpecificTests : Specification
{
    private readonly string _baseUrl;
    
    public EnvironmentSpecificTests()
    {
        _baseUrl = Environment.GetEnvironmentVariable("API_BASE_URL") 
            ?? "https://localhost:5000";
    }
    
    [Fact]
    [Load(concurrency: GetConcurrency(), duration: 30000)]
    public async Task Environment_Appropriate_Load()
    {
        await _httpClient.GetAsync($"{_baseUrl}/api/health");
    }
    
    private int GetConcurrency()
    {
        return Environment.GetEnvironmentVariable("ENVIRONMENT") switch
        {
            "Production" => 10,   // Conservative in production
            "Staging" => 50,      // Moderate load in staging
            "Development" => 100, // Full load in development
            _ => 25
        };
    }
}
```

### CI/CD Integration

```yaml
# GitHub Actions example
- name: Run Load Tests
  run: dotnet test --filter "Category=Load" --logger "console;verbosity=detailed"
  env:
    API_BASE_URL: ${{ secrets.STAGING_API_URL }}
    ENVIRONMENT: Staging
```

## 🎯 Next Steps

Now that you understand the basics:

1. **[Load Attribute Configuration](load-attribute-configuration.md)** - Deep dive into configuration options
2. **[Writing Effective Tests](writing-effective-tests.md)** - Best practices for load test design
3. **[Performance Optimization](performance-optimization.md)** - Optimize your tests and system under test
4. **[Monitoring and Metrics](monitoring-metrics.md)** - Advanced metrics collection and analysis

## 💡 Pro Tips

1. **Start Small**: Begin with low concurrency and short duration, then scale up
2. **Monitor Resources**: Watch CPU, memory, and network during tests
3. **Use Realistic Data**: Test with data volumes similar to production
4. **Test Different Scenarios**: Mix read/write operations, different user types
5. **Automate Everything**: Include load tests in your CI/CD pipeline

Happy load testing! 🚀
