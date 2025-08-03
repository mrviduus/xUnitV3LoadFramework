# Performance Optimization Guide

Optimize your load tests and systems under test for maximum performance and reliability with xUnitV3LoadFramework.

##  Framework Optimization

### 1. Choose the Right Worker Model

The framework provides two execution models - choose based on your needs:

```csharp
using xUnitV3LoadFramework.Attributes;
using xUnitV3LoadFramework.Extensions;

[UseLoadFramework]
public class OptimizedLoadTests : Specification
{
    // Hybrid Model (Recommended for most scenarios)
    // - Fixed worker pools with channel-based distribution
    // - Predictable resource usage and consistent performance
    // - Optimal for sustained load and long-running tests

    [Load(order: 1, concurrency: 100, duration: 300000, interval: 1000)]
    public async Task Optimized_With_Hybrid_Workers() 
    {
        // Framework automatically uses hybrid workers for better performance
        await ExecuteTestLogic();
    }

    // Task Model (Good for short bursts or low concurrency)
    // - Dynamic task creation per batch
    // - More flexible but higher GC pressure
    // - Better for short tests with variable workload

    private async Task ExecuteTestLogic()
    {
        // Your test implementation here
        await Task.Delay(10);
    }
}

### 2. Optimal Concurrency Configuration

Calculate optimal concurrency based on system resources:

```csharp
[UseLoadFramework]
public class OptimizedConcurrencyTests : Specification
{
    private static int CalculateOptimalConcurrency()
    {
        var coreCount = Environment.ProcessorCount;
        
        // For CPU-bound operations
        var cpuBoundConcurrency = coreCount;
        
        // For I/O-bound operations (typical for API tests)
        var ioBoundConcurrency = coreCount * 8;
        
        // For mixed workloads
        var mixedConcurrency = coreCount * 4;
        
        // Return appropriate value based on your test type
        return ioBoundConcurrency;
    }
    
    [Load(order: 1, concurrency: 64, duration: 120000, interval: 1000)] // Calculated for 8-core system
    public async Task Should_Use_Optimal_Concurrency()
    {
        // Your I/O-bound test logic
        await _httpClient.GetAsync("/api/data");
    }
    
    private readonly HttpClient _httpClient = new HttpClient();
}
```

### 3. Efficient Result Collection

Optimize metrics collection for high-throughput tests:

```csharp
[UseLoadFramework]
public class OptimizedReportingTests : Specification
{
    // Use appropriate reporting intervals
    [Load(order: 1, concurrency: 1000, duration: 600000, interval: 10000)] // Report every 10s for long tests
    public async Task High_Throughput_With_Optimized_Reporting() 
    {
        await _httpClient.GetAsync("/api/data");
    }

    [Load(order: 2, concurrency: 50, duration: 30000, interval: 1000)] // Report every 1s for short tests
    public async Task Standard_Load_With_Detailed_Reporting() 
    {
        await _httpClient.GetAsync("/api/data");
    }
    
    private readonly HttpClient _httpClient = new HttpClient();
}

##  HTTP Client Optimization

### 1. Connection Pool Optimization

Configure HTTP connections for maximum throughput:

```csharp
[UseLoadFramework]
public class OptimizedHttpLoadTests : Specification
{
    private static readonly HttpClient OptimizedClient;
    
    static OptimizedHttpLoadTests()
    {
        // Configure connection limits
        ServicePointManager.DefaultConnectionLimit = 1000;
        ServicePointManager.MaxServicePointIdleTime = 30000;
        ServicePointManager.UseNagleAlgorithm = false;
        ServicePointManager.Expect100Continue = false;
        
        var handler = new HttpClientHandler()
        {
            MaxConnectionsPerServer = 1000,
            PooledConnectionLifetime = TimeSpan.FromMinutes(2),
            PooledConnectionIdleTimeout = TimeSpan.FromSeconds(30)
        };
        
        OptimizedClient = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
        
        // Warm up the connection pool
        _ = OptimizedClient.GetAsync("https://api.example.com/health").Result;
    }
    
    protected override async void Because()
    {
        // Use the optimized shared client
        var response = await OptimizedClient.GetAsync("/api/data");
        Assert.True(response.IsSuccessStatusCode);
    }
    
    [Load(order: 1, concurrency: 500, duration: 300000, interval: 1000)]
    public async Task Should_Handle_High_Throughput_HTTP() 
    {
        // Test implementation is in Because() method
    }
}
```

### 2. Request/Response Optimization

Optimize HTTP requests and responses for performance:

```csharp
[UseLoadFramework]
public class HttpOptimizationTests : Specification
{
    private static string CachedJsonContent;
    private readonly HttpClient _httpClient = new HttpClient();

    [Load(order: 1, concurrency: 200, duration: 120000, interval: 1000)]
    public async Task Should_Use_Optimized_HTTP_Patterns()
    {
        // Pre-allocate request content when possible
        using var content = new StringContent(
            GetReusableJsonContent(), 
            Encoding.UTF8, 
            "application/json"
        );
        
        var response = await _httpClient.PostAsync("/api/process", content);
        
        // Efficient response handling
        if (response.IsSuccessStatusCode)
        {
            // Stream large responses
            if (response.Content.Headers.ContentLength > 1024 * 1024)
            {
                await using var stream = await response.Content.ReadAsStreamAsync();
                await ProcessLargeResponseStream(stream);
            }
            else
            {
                // Small responses can be read directly
                var responseData = await response.Content.ReadAsStringAsync();
                ProcessResponse(responseData);
            }
        }
        
        // Explicit disposal for high-throughput scenarios
        response?.Dispose();
    }

    private static string GetReusableJsonContent()
    {
        // Cache JSON strings to avoid repeated serialization
        return CachedJsonContent ??= JsonSerializer.Serialize(new { data = "test" });
    }
    
    private async Task ProcessLargeResponseStream(Stream stream)
    {
        // Process stream without loading all into memory
        var buffer = new byte[4096];
        await stream.ReadAsync(buffer, 0, buffer.Length);
    }
    
    private void ProcessResponse(string responseData)
    {
        // Process response data
        Assert.NotNull(responseData);
    }
}
```

### 3. DNS and Connection Optimization

Optimize DNS resolution and connections:

```csharp
[UseLoadFramework]
public class NetworkOptimizedTests : Specification
{
    protected override void EstablishContext()
    {
        // Pre-resolve DNS to avoid resolution during load test
        var addresses = Dns.GetHostAddresses("api.example.com");
        Console.WriteLine($"Resolved {addresses.Length} addresses for api.example.com");
        
        // Configure TCP settings for high throughput
        ServicePointManager.EnableDnsRoundRobin = true;
        ServicePointManager.DnsRefreshTimeout = (int)TimeSpan.FromMinutes(5).TotalMilliseconds;
    }
    
    [Load(order: 1, concurrency: 300, duration: 180000, interval: 1000)]
    public async Task Should_Handle_Optimized_Network_Load() 
    {
        await _httpClient.GetAsync("/api/data");
    }
    
    private readonly HttpClient _httpClient = new HttpClient();
}
```

##  Database Optimization

### 1. Connection Pool Configuration

Optimize database connections for load testing:

```csharp
[UseLoadFramework]
public class OptimizedDatabaseTests : Specification, IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    
    public OptimizedDatabaseTests()
    {
        var services = new ServiceCollection();
        
        // Optimized connection string
        var connectionString = "Server=localhost;Database=LoadTest;" +
            "Trusted_Connection=true;" +
            "Max Pool Size=1000;" +        // Large pool for load testing
            "Min Pool Size=50;" +           // Pre-allocated connections
            "Connection Timeout=30;" +      // Connection timeout
            "Command Timeout=60;" +         // Command timeout
            "Pooling=true;" +              // Enable pooling
            "Connection Lifetime=300;";     // 5 minute connection lifetime
        
        services.AddDbContext<TestDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.CommandTimeout(60);
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorNumbersToAdd: null);
            });
        }, ServiceLifetime.Transient); // Transient for load testing
        
        _serviceProvider = services.BuildServiceProvider();
    }
    
    protected override async void Because()
    {
        using var scope = _serviceProvider.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        
        // Optimized queries
        var userId = Random.Shared.Next(1, 10000);
        
        // Use efficient queries with proper indexing
        var user = await context.Users
            .Where(u => u.Id == userId)
            .Select(u => new { u.Id, u.Name, u.Email }) // Project only needed columns
            .AsNoTracking() // Disable change tracking for read-only operations
            .FirstOrDefaultAsync();
        
        Assert.NotNull(user);
    }
    
    [Load(order: 1, concurrency: 100, duration: 180000, interval: 1000)]
    public async Task Should_Handle_Optimized_Database_Load() 
    {
        // Test implementation is in Because() method
    }
    
    public void Dispose() => _serviceProvider?.Dispose();
}
```

### 2. Query Optimization

Write efficient queries for load testing:

```csharp
[UseLoadFramework]
public class DatabaseQueryOptimizationTests : Specification
{
    private readonly ServiceProvider _serviceProvider;

    public DatabaseQueryOptimizationTests()
    {
        var services = new ServiceCollection();
        services.AddDbContext<TestDbContext>(options => 
            options.UseSqlServer("Server=localhost;Database=LoadTest;Trusted_Connection=true;"));
        _serviceProvider = services.BuildServiceProvider();
    }

    [Load(order: 1, concurrency: 150, duration: 240000, interval: 1000)]
    public async Task Should_Execute_Optimized_Database_Operations()
    {
        using var scope = _serviceProvider.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        
        var scenario = Random.Shared.NextDouble();
        
        if (scenario < 0.7) // 70% read operations
        {
            // Optimized read with pagination
            var users = await context.Users
                .Where(u => u.IsActive)
                .OrderBy(u => u.Id)
                .Skip(Random.Shared.Next(0, 1000))
                .Take(20)
                .AsNoTracking()
                .ToListAsync();
            
            Assert.True(users.Count > 0);
        }
        else if (scenario < 0.95) // 25% update operations
        {
            // Efficient update without loading entity
            var userId = Random.Shared.Next(1, 10000);
            var rowsAffected = await context.Users
                .Where(u => u.Id == userId)
                .ExecuteUpdateAsync(u => u.SetProperty(p => p.LastAccessTime, DateTime.UtcNow));
            
            // Update operations may not find the user (expected)
            Assert.True(rowsAffected <= 1);
        }
        else // 5% write operations
        {
            // Batch inserts for efficiency
            var newUsers = Enumerable.Range(0, 5)
                .Select(i => new User 
                { 
                    Name = $"LoadTestUser_{Guid.NewGuid():N}",
                    Email = $"test_{i}@loadtest.com",
                    IsActive = true
                })
                .ToList();
            
            context.Users.AddRange(newUsers);
            await context.SaveChangesAsync();
        }
    }
}
```

## 🧠 Memory Optimization

### 1. Memory-Efficient Test Patterns

Minimize memory allocation and garbage collection:

```csharp
[UseLoadFramework]
public class MemoryOptimizedTests : Specification
{
    // Reuse objects to reduce GC pressure
    private static readonly ThreadLocal<StringBuilder> StringBuilderCache = 
        new(() => new StringBuilder(1024));
        
    private static readonly ObjectPool<JsonSerializerOptions> JsonOptionsPool = 
        new DefaultObjectPool<JsonSerializerOptions>(new JsonOptionsPooledObjectPolicy());
        
    private readonly HttpClient _httpClient = new HttpClient();
    
    protected override async void Because()
    {
        // Reuse StringBuilder
        var sb = StringBuilderCache.Value;
        sb.Clear();
        sb.Append("{'data':'");
        sb.Append(GenerateTestData());
        sb.Append("'}");
        
        var jsonContent = sb.ToString();
        
        // Use object pooling for expensive objects
        var jsonOptions = JsonOptionsPool.Get();
        try
        {
            using var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/api/data", content);
            Assert.True(response.IsSuccessStatusCode);
        }
        finally
        {
            JsonOptionsPool.Return(jsonOptions);
        }
    }
    
    [Load(order: 1, concurrency: 200, duration: 600000, interval: 2000)] // Long-running test
    public async Task Should_Handle_Long_Running_Load_Without_Memory_Leaks() 
    {
        // Test implementation is in Because() method
    }
    
    private string GenerateTestData()
    {
        return $"test_data_{Guid.NewGuid():N}";
    }
}
```

### 2. Streaming for Large Data

Handle large payloads efficiently:

```csharp
[UseLoadFramework]
public class StreamingOptimizedTests : Specification
{
    private readonly HttpClient _httpClient = new HttpClient();

    [Load(order: 1, concurrency: 50, duration: 300000, interval: 2000)]
    public async Task Should_Handle_Large_Payloads_Efficiently()
    {
        // Stream large requests
        using var requestStream = new MemoryStream();
        await GenerateLargePayload(requestStream);
        requestStream.Position = 0;
        
        using var content = new StreamContent(requestStream);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        
        var response = await _httpClient.PostAsync("/api/upload", content);
        
        if (response.IsSuccessStatusCode)
        {
            // Stream large responses
            await using var responseStream = await response.Content.ReadAsStreamAsync();
            await ProcessLargeResponse(responseStream);
        }
        
        // Explicit cleanup for large objects
        requestStream?.Dispose();
        response?.Dispose();
    }

    private async Task ProcessLargeResponse(Stream responseStream)
    {
        var buffer = new byte[8192];
        var totalBytesRead = 0;
        
        int bytesRead;
        while ((bytesRead = await responseStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            totalBytesRead += bytesRead;
            // Process buffer contents without accumulating in memory
            ProcessBuffer(buffer, bytesRead);
        }
        
        Assert.True(totalBytesRead > 0, "Should have received response data");
    }
    
    private async Task GenerateLargePayload(Stream stream)
    {
        var data = Encoding.UTF8.GetBytes("Large payload data...");
        await stream.WriteAsync(data, 0, data.Length);
    }
    
    private void ProcessBuffer(byte[] buffer, int bytesRead)
    {
        // Process buffer without storing
        Assert.True(bytesRead > 0);
    }
}
```

## ⚡ System Resource Optimization

### 1. Thread Pool Optimization

Configure thread pool for optimal performance:

```csharp
[UseLoadFramework]
public class ThreadPoolOptimizedTests : Specification
{
    private readonly HttpClient _httpClient = new HttpClient();

    protected override void EstablishContext()
    {
        // Configure thread pool for load testing
        var coreCount = Environment.ProcessorCount;
        
        ThreadPool.GetMinThreads(out var minWorkers, out var minIOCP);
        ThreadPool.SetMinThreads(
            minWorkers: Math.Max(minWorkers, coreCount * 4),
            minCompletionPortThreads: Math.Max(minIOCP, coreCount * 4)
        );
        
        ThreadPool.GetMaxThreads(out var maxWorkers, out var maxIOCP);
        Console.WriteLine($"Thread pool configured: {minWorkers}-{maxWorkers} workers, {minIOCP}-{maxIOCP} IOCP");
        
        // Warm up thread pool
        for (int i = 0; i < coreCount * 2; i++)
        {
            ThreadPool.QueueUserWorkItem(_ => Thread.Sleep(1));
        }
        
        Thread.Sleep(100); // Allow warm-up to complete
    }
    
    [Load(order: 1, concurrency: 400, duration: 300000, interval: 1000)]
    public async Task Should_Handle_High_Concurrency_With_Optimized_Thread_Pool() 
    {
        await _httpClient.GetAsync("/api/data");
    }
}
```

### 2. GC Optimization

Minimize garbage collection impact:

```csharp
[UseLoadFramework]
public class GCOptimizedTests : Specification
{
    private int _initialGen0, _initialGen1, _initialGen2;
    private readonly HttpClient _httpClient = new HttpClient();

    protected override void EstablishContext()
    {
        // Force initial GC to start with clean slate
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        
        // Monitor GC during test
        _initialGen0 = GC.CollectionCount(0);
        _initialGen1 = GC.CollectionCount(1);
        _initialGen2 = GC.CollectionCount(2);
        
        Console.WriteLine($"Initial GC counts - Gen0: {_initialGen0}, Gen1: {_initialGen1}, Gen2: {_initialGen2}");
    }
    
    protected override void DestroyContext()
    {
        // Report GC activity after test
        var finalGen0 = GC.CollectionCount(0);
        var finalGen1 = GC.CollectionCount(1);
        var finalGen2 = GC.CollectionCount(2);
        
        Console.WriteLine($"Final GC counts - Gen0: {finalGen0}, Gen1: {finalGen1}, Gen2: {finalGen2}");
        Console.WriteLine($"GC pressure - Gen0: {finalGen0 - _initialGen0}, Gen1: {finalGen1 - _initialGen1}, Gen2: {finalGen2 - _initialGen2}");
    }
    
    // Use value types and object pooling to reduce allocations
    [Load(order: 1, concurrency: 300, duration: 600000, interval: 2000)]
    public async Task Should_Minimize_GC_Pressure() 
    {
        // Efficient patterns that minimize allocations
        await ExecuteWithMinimalAllocations();
    }
    
    private async Task ExecuteWithMinimalAllocations()
    {
        // Use efficient patterns to minimize allocations
        await _httpClient.GetAsync("/api/data");
    }
}
```

##  Monitoring and Profiling

### 1. Performance Counters

Monitor system resources during load tests:

```csharp
[UseLoadFramework]
public class MonitoredLoadTests : Specification
{
    private PerformanceCounter _cpuCounter;
    private PerformanceCounter _memoryCounter;
    private Timer _monitoringTimer;
    private readonly HttpClient _httpClient = new HttpClient();
    
    protected override void EstablishContext()
    {
        _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        _memoryCounter = new PerformanceCounter("Memory", "Available MBytes");
        
        // Monitor system resources every 5 seconds
        _monitoringTimer = new Timer(LogSystemMetrics, null, 
            TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
    }
    
    private void LogSystemMetrics(object state)
    {
        var cpuUsage = _cpuCounter.NextValue();
        var availableMemoryMB = _memoryCounter.NextValue();
        
        Console.WriteLine($"System metrics - CPU: {cpuUsage:F1}%, Available Memory: {availableMemoryMB:F0}MB");
        
        // Alert if resources are constrained
        if (cpuUsage > 90)
            Console.WriteLine("WARNING: High CPU usage detected");
        if (availableMemoryMB < 1000)
            Console.WriteLine("WARNING: Low memory detected");
    }
    
    [Load(order: 1, concurrency: 200, duration: 300000, interval: 1000)]
    public async Task Should_Monitor_System_Resources() 
    {
        await _httpClient.GetAsync("/api/data");
    }
    
    protected override void DestroyContext()
    {
        _monitoringTimer?.Dispose();
        _cpuCounter?.Dispose();
        _memoryCounter?.Dispose();
    }
}
```

### 2. Custom Performance Metrics

Track application-specific performance metrics:

```csharp
[UseLoadFramework]
public class CustomMetricsTests : Specification
{
    private readonly ConcurrentDictionary<string, Counter> _customCounters = new();
    private readonly ConcurrentBag<TimeSpan> _operationTimes = new();
    
    protected override async void Because()
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var result = await ExecuteBusinessOperation();
            stopwatch.Stop();
            
            // Track custom metrics
            _operationTimes.Add(stopwatch.Elapsed);
            _customCounters.GetOrAdd("successful_operations", _ => new Counter()).Increment();
            
            if (result.RequiredSpecialProcessing)
            {
                _customCounters.GetOrAdd("special_processing", _ => new Counter()).Increment();
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _customCounters.GetOrAdd($"error_{ex.GetType().Name}", _ => new Counter()).Increment();
            throw;
        }
    }
    
    protected override void DestroyContext()
    {
        // Report custom metrics
        Console.WriteLine("\n=== Custom Performance Metrics ===");
        
        foreach (var counter in _customCounters)
        {
            Console.WriteLine($"{counter.Key}: {counter.Value.Value}");
        }
        
        if (_operationTimes.Any())
        {
            var times = _operationTimes.Select(t => t.TotalMilliseconds).OrderBy(t => t).ToArray();
            Console.WriteLine($"Operation Times - Min: {times.First():F2}ms, Max: {times.Last():F2}ms");
            Console.WriteLine($"Operation Times - Avg: {times.Average():F2}ms, P95: {times[(int)(times.Length * 0.95)]:F2}ms");
        }
    }
    
    [Load(order: 1, concurrency: 100, duration: 180000, interval: 1000)]
    public async Task Should_Track_Custom_Performance_Metrics() 
    {
        // Test implementation is in Because() method
    }
    
    private async Task<BusinessResult> ExecuteBusinessOperation()
    {
        // Simulate business operation
        await Task.Delay(Random.Shared.Next(10, 100));
        return new BusinessResult { RequiredSpecialProcessing = Random.Shared.NextDouble() > 0.8 };
    }
    
    private class BusinessResult
    {
        public bool RequiredSpecialProcessing { get; set; }
    }
    
    private class Counter
    {
        private int _value;
        public int Value => _value;
        public void Increment() => Interlocked.Increment(ref _value);
    }
}
```

##  Best Practices Summary

### Framework-Level Optimizations
1. **Use Hybrid Workers**: Leverage channel-based execution for consistent performance
2. **Optimize Concurrency**: Calculate based on CPU cores and workload type
3. **Configure Reporting**: Use appropriate intervals based on test duration
4. **Monitor Resources**: Track CPU, memory, and GC pressure

### HTTP Optimizations
1. **Shared Clients**: Reuse HttpClient instances across all concurrent users
2. **Connection Pooling**: Configure appropriate connection limits
3. **Efficient Payloads**: Stream large requests/responses
4. **DNS Pre-resolution**: Resolve DNS before load testing

### Database Optimizations
1. **Connection Pooling**: Large pools with proper lifetime management
2. **Efficient Queries**: Use projections, no-tracking, and proper indexing
3. **Batch Operations**: Group operations when possible
4. **Transient Scope**: Use transient DbContext for load testing

### Memory Optimizations
1. **Object Reuse**: Pool expensive objects and reuse buffers
2. **Streaming**: Handle large payloads without loading into memory
3. **GC Monitoring**: Track garbage collection pressure
4. **Explicit Disposal**: Clean up resources promptly

### Monitoring Best Practices
1. **System Resources**: Monitor CPU, memory, and network
2. **Custom Metrics**: Track application-specific performance indicators
3. **Baseline Comparisons**: Compare results across test runs
4. **Alert Thresholds**: Set up monitoring for resource constraints

Following these optimization guidelines will help you achieve maximum performance and reliability in your load testing scenarios.
