# Writing Effective Load Tests

This guide covers best practices for creating meaningful, maintainable, and reliable load tests using xUnitV3LoadFramework.

## Pro Tips

### 1. Test Realistic Scenarios

Your load tests should mirror real user behavior and system usage patterns.

```csharp
[UseLoadFramework]
public class RealisticLoadTests : Specification
{
    private readonly HttpClient _httpClient = new HttpClient();
    private static readonly StringContent content = new StringContent("test", Encoding.UTF8, "application/json");

    // BAD - Unrealistic uniform load
    [Load(order: 1, concurrency: 1000, duration: 60000, interval: 1000)]
    public async Task Unrealistic_Load_Test()
    {
        // All users hit same endpoint at exact same time
        await _httpClient.GetAsync("/api/users/1");
    }

    //  Good - Realistic varied load
    [Load(order: 2, concurrency: 100, duration: 60000, interval: 1000)]
    public async Task Realistic_User_Behavior()
    {
        // Simulate realistic user patterns
        var userId = Random.Shared.Next(1, 10000);
        await _httpClient.GetAsync($"/api/users/{userId}");
        
        // Add think time between requests
        await Task.Delay(Random.Shared.Next(500, 2000));
        
        // Occasionally perform updates (20% of requests)
        if (Random.Shared.NextDouble() < 0.2)
        {
            await _httpClient.PostAsync($"/api/users/{userId}/activity", content);
        }
    }
}
```

### 2. Use Progressive Load Testing

Build up load gradually to understand system behavior.

```csharp
[UseLoadFramework]
public class ProgressiveLoadTests : Specification
{
    // Phase 1: Smoke test
    [Load(order: 1, concurrency: 5, duration: 30000, interval: 1000)]
    [Trait("Phase", "Smoke")]
    public async Task Phase1_Smoke_Test() { await ExecuteUserScenario(); }
    
    // Phase 2: Normal load
    [Load(order: 2, concurrency: 50, duration: 120000, interval: 2000)]
    [Trait("Phase", "Normal")]
    public async Task Phase2_Normal_Load() { await ExecuteUserScenario(); }
    
    // Phase 3: Peak load
    [Load(order: 3, concurrency: 200, duration: 300000, interval: 5000)]
    [Trait("Phase", "Peak")]
    public async Task Phase3_Peak_Load() { await ExecuteUserScenario(); }
    
    // Phase 4: Stress test
    [Load(order: 4, concurrency: 500, duration: 600000, interval: 10000)]
    [Trait("Phase", "Stress")]
    public async Task Phase4_Stress_Test() { await ExecuteUserScenario(); }
    
    private async Task ExecuteUserScenario()
    {
        // Common user scenario implementation
        await Task.Delay(100);
    }
}
```

### 3. Include Error Scenarios

Test how your system handles failures gracefully.

```csharp
[UseLoadFramework]
public class ErrorHandlingLoadTests : Specification
{
    private readonly HttpClient _httpClient = new HttpClient();

    [Load(order: 1, concurrency: 100, duration: 60000, interval: 1000)]
    public async Task Should_Handle_Errors_Gracefully()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/data");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Assert.False(string.IsNullOrEmpty(content));
                return; // Success case
            }
            
            // Handle expected error responses
            switch (response.StatusCode)
            {
                case HttpStatusCode.ServiceUnavailable:
                    // Expected under high load - system protecting itself
                    Console.WriteLine("Service temporarily unavailable - expected behavior");
                    break;
                    
                case HttpStatusCode.TooManyRequests:
                    // Rate limiting triggered - good defensive behavior
                    Console.WriteLine("Rate limit hit - system is protecting itself");
                    break;
                    
                default:
                Assert.True(false, $"Unexpected status: {response.StatusCode}");
                break;
        }
    }
    catch (TimeoutException)
    {
        // Timeouts are expected under stress
        Console.WriteLine("Request timeout - acceptable under load");
    }
    catch (HttpRequestException ex) when (ex.Message.Contains("connection"))
    {
        // Connection issues under load are often acceptable
        Console.WriteLine($"Connection issue: {ex.Message}");
    }
}
```

## Test Structure Patterns

### 1. Specification Pattern (Recommended)

Use the Specification base class for clean test organization:

```csharp
public class ApiUserManagementLoadTests : Specification
{
    private HttpClient _httpClient;
    private string _authToken;
    private readonly Random _random = new();
    
    protected override void EstablishContext()
    {
        // One-time setup before load test
        _httpClient = new HttpClient 
        { 
            BaseAddress = new Uri("https://api.example.com"),
            Timeout = TimeSpan.FromSeconds(30)
        };
        
        // Authenticate once
        _authToken = GetAuthToken();
        _httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", _authToken);
    }
    
    protected override void Because()
    {
        // This runs for each concurrent user iteration
        await ExecuteUserScenario();
    }
    
    protected override void DestroyContext()
    {
        // Cleanup after all load testing completes
        _httpClient?.Dispose();
    }
    
    [Load(order: 1, concurrency: 100, duration: 120000, interval: 10000)]
    public void Should_Handle_User_Management_Load()
    {
        // Because() method is called automatically
        // Test results and metrics are collected automatically
    }
    
    private async Task ExecuteUserScenario()
    {
        // 70% read operations
        if (_random.NextDouble() < 0.7)
        {
            var userId = _random.Next(1, 10000);
            await _httpClient.GetAsync($"/users/{userId}");
        }
        // 20% update operations
        else if (_random.NextDouble() < 0.9)
        {
            await UpdateRandomUser();
        }
        // 10% create operations
        else
        {
            await CreateNewUser();
        }
        
        // Simulate user think time
        await Task.Delay(_random.Next(200, 1500));
    }
}
```

### 2. Parameterized Test Pattern

Create data-driven load tests:

```csharp
public class ParameterizedLoadTests
{
    public static IEnumerable<object[]> LoadScenarios =>
        new[]
        {
            new object[] { "Light", 25, 60000 },
            new object[] { "Medium", 100, 120000 },
            new object[] { "Heavy", 300, 300000 }
        };
    
    [Theory]
    [MemberData(nameof(LoadScenarios))]
    [Load(concurrency: 1, duration: 1000)] // Overridden by test data
    public async Task Should_Handle_Various_Load_Levels(
        string scenario, int concurrency, int duration)
    {
        // Use reflection or configuration to apply actual load parameters
        Console.WriteLine($"Running {scenario} load test: {concurrency} users for {duration}ms");
        
        await SimulateWork();
    }
}
```

### 3. Composite Scenario Pattern

Test multiple operations in sequence:

```csharp
[Load(concurrency: 50, duration: 180000)]
public async Task Should_Handle_Complete_User_Journey()
{
    // Step 1: Login
    var loginResponse = await _httpClient.PostAsync("/auth/login", loginData);
    Assert.True(loginResponse.IsSuccessStatusCode, "Login should succeed");
    
    var token = await ExtractToken(loginResponse);
    
    // Step 2: Browse products (multiple requests)
    for (int i = 0; i < Random.Shared.Next(3, 8); i++)
    {
        var productId = Random.Shared.Next(1, 1000);
        await _httpClient.GetAsync($"/products/{productId}");
        await Task.Delay(Random.Shared.Next(200, 800)); // Browse time
    }
    
    // Step 3: Add to cart (30% chance)
    if (Random.Shared.NextDouble() < 0.3)
    {
        await _httpClient.PostAsync("/cart/items", cartItemData);
    }
    
    // Step 4: Checkout (10% of add-to-cart actions)
    if (Random.Shared.NextDouble() < 0.1)
    {
        await _httpClient.PostAsync("/orders", orderData);
    }
    
    // Step 5: Logout
    await _httpClient.PostAsync("/auth/logout", null);
}
```

## Data Management

### 1. Test Data Isolation

Ensure tests don't interfere with each other:

```csharp
public class IsolatedDataLoadTests : Specification
{
    private readonly string _testRunId = Guid.NewGuid().ToString("N")[..8];
    private readonly List<int> _createdUserIds = new();
    
    protected override void EstablishContext()
    {
        // Create isolated test data set
        for (int i = 0; i < 1000; i++)
        {
            var userId = CreateTestUser($"loadtest_{_testRunId}_{i}");
            _createdUserIds.Add(userId);
        }
    }
    
    protected override void Because()
    {
        // Use only test-specific data
        var userId = _createdUserIds[Random.Shared.Next(_createdUserIds.Count)];
        await _httpClient.GetAsync($"/users/{userId}");
    }
    
    protected override void DestroyContext()
    {
        // Clean up test data
        foreach (var userId in _createdUserIds)
        {
            DeleteTestUser(userId);
        }
    }
    
    [Load(concurrency: 100, duration: 60000)]
    public async Task Should_Handle_Load_With_Isolated_Data() { }
}
```

### 2. Database Load Testing

Handle database connections properly:

```csharp
public class DatabaseLoadTests : Specification, IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    
    public DatabaseLoadTests()
    {
        var services = new ServiceCollection();
        
        // Configure connection pooling
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.CommandTimeout(30);
                sqlOptions.EnableRetryOnFailure(3);
            }), ServiceLifetime.Transient); // Important: Transient for load tests
        
        services.AddTransient<IUserService, UserService>();
        _serviceProvider = services.BuildServiceProvider();
    }
    
    protected override void Because()
    {
        // Create new scope for each iteration
        using var scope = _serviceProvider.CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
        
        // Perform database operations
        var users = await userService.GetActiveUsersAsync();
        Assert.True(users.Any(), "Should have active users");
        
        // Simulate some updates (10% chance)
        if (Random.Shared.NextDouble() < 0.1)
        {
            var user = users.First();
            user.LastActivityTime = DateTime.UtcNow;
            await userService.UpdateUserAsync(user);
        }
    }
    
    [Load(concurrency: 50, duration: 120000)]
    public async Task Should_Handle_Database_Load() { }
    
    public void Dispose() => _serviceProvider?.Dispose();
}
```

## Performance Optimization

### 1. Resource Management

Manage resources efficiently during load tests:

```csharp
public class OptimizedLoadTests : Specification
{
    // Shared resources for all concurrent users
    private static readonly HttpClient SharedHttpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(30)
    };
    
    // Connection pooling configuration
    static OptimizedLoadTests()
    {
        ServicePointManager.DefaultConnectionLimit = 1000;
        ServicePointManager.MaxServicePointIdleTime = 30000;
    }
    
    protected override void EstablishContext()
    {
        // Warm up connections
        _ = SharedHttpClient.GetAsync("https://api.example.com/health").Result;
    }
    
    protected override void Because()
    {
        // Use shared client - efficient connection reuse
        var response = await SharedHttpClient.GetAsync("/api/data");
        
        // Efficient response handling
        if (response.IsSuccessStatusCode)
        {
            // Don't read content unless necessary
            _ = response.Content.Headers.ContentLength;
        }
    }
    
    [Load(concurrency: 200, duration: 300000)]
    public async Task Should_Handle_Optimized_Load() { }
}
```

### 2. Memory-Efficient Testing

Avoid memory leaks in long-running tests:

```csharp
[Load(concurrency: 100, duration: 1800000)] // 30 minutes
public async Task Should_Handle_Long_Running_Load()
{
    // Avoid creating large objects
    var requestData = GetReusableRequestData();
    
    using var response = await _httpClient.PostAsync("/api/process", requestData);
    
    // Stream large responses instead of loading into memory
    if (response.Content.Headers.ContentLength > 1024 * 1024) // 1MB
    {
        using var stream = await response.Content.ReadAsStreamAsync();
        await ProcessStreamAsync(stream);
    }
    else
    {
        var content = await response.Content.ReadAsStringAsync();
        ProcessContent(content);
    }
    
    // Explicitly dispose when done
    response?.Dispose();
}
```

## 🔍 Monitoring and Assertions

### 1. Custom Metrics Collection

Collect application-specific metrics:

```csharp
public class MetricsCollectionLoadTest : Specification
{
    private readonly ConcurrentBag<double> _customLatencies = new();
    private readonly Counter _businessOperationsCounter = new();
    
    protected override void Because()
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // Your load test operation
            var result = await ExecuteBusinessOperation();
            
            stopwatch.Stop();
            
            // Collect custom metrics
            _customLatencies.Add(stopwatch.Elapsed.TotalMilliseconds);
            _businessOperationsCounter.Increment();
            
            // Business-specific assertions
            Assert.True(result.IsValid, "Business operation should be valid");
            Assert.True(result.Value > 0, "Business value should be positive");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            Console.WriteLine($"Operation failed after {stopwatch.ElapsedMilliseconds}ms: {ex.Message}");
            throw; // Re-throw to count as failure
        }
    }
    
    protected override void DestroyContext()
    {
        // Report custom metrics
        if (_customLatencies.Any())
        {
            var avg = _customLatencies.Average();
            var p95 = _customLatencies.OrderBy(x => x).Skip((int)(_customLatencies.Count * 0.95)).First();
            
            Console.WriteLine($"Custom Metrics - Avg: {avg:F2}ms, P95: {p95:F2}ms");
            Console.WriteLine($"Business Operations: {_businessOperationsCounter.Value}");
        }
    }
    
    [Load(concurrency: 100, duration: 120000)]
    public async Task Should_Collect_Custom_Metrics() { }
}
```

### 2. Conditional Assertions

Make assertions appropriate for load testing context:

```csharp
[Load(concurrency: 500, duration: 300000)] // High stress test
public async Task Should_Handle_Stress_With_Appropriate_Expectations()
{
    var response = await _httpClient.GetAsync("/api/data");
    
    // Under high load, some degradation is acceptable
    if (response.StatusCode == HttpStatusCode.ServiceUnavailable ||
        response.StatusCode == HttpStatusCode.TooManyRequests)
    {
        // System is protecting itself - this is good behavior
        Console.WriteLine($"Expected load response: {response.StatusCode}");
        return; // Count as successful handling
    }
    
    // For successful responses, verify they're reasonable
    if (response.IsSuccessStatusCode)
    {
        // Response time expectations under load
        var responseTime = response.Headers.GetValues("X-Response-Time")?.FirstOrDefault();
        if (responseTime != null && double.TryParse(responseTime, out var time))
        {
            // Under stress, allow higher response times
            Assert.True(time < 5000, $"Response time {time}ms should be < 5s under stress");
        }
        
        // Verify response is valid but don't check every detail under stress
        var content = await response.Content.ReadAsStringAsync();
        Assert.False(string.IsNullOrEmpty(content), "Response should have content");
    }
    else
    {
        Assert.True(false, $"Unexpected error status: {response.StatusCode}");
    }
}
```

##  Advanced Patterns

### 1. Multi-Phase Testing

Test different phases of system behavior:

```csharp
public class MultiPhaseLoadTest : Specification
{
    private Phase _currentPhase = Phase.Warmup;
    private DateTime _phaseStartTime;
    
    enum Phase { Warmup, Normal, Peak, Cooldown }
    
    protected override void EstablishContext()
    {
        _phaseStartTime = DateTime.UtcNow;
    }
    
    protected override void Because()
    {
        UpdateCurrentPhase();
        
        switch (_currentPhase)
        {
            case Phase.Warmup:
                await WarmupBehavior();
                break;
            case Phase.Normal:
                await NormalBehavior();
                break;
            case Phase.Peak:
                await PeakBehavior();
                break;
            case Phase.Cooldown:
                await CooldownBehavior();
                break;
        }
    }
    
    private void UpdateCurrentPhase()
    {
        var elapsed = DateTime.UtcNow - _phaseStartTime;
        
        _currentPhase = elapsed.TotalSeconds switch
        {
            < 30 => Phase.Warmup,
            < 120 => Phase.Normal,
            < 180 => Phase.Peak,
            _ => Phase.Cooldown
        };
    }
    
    [Load(concurrency: 100, duration: 240000)] // 4 minutes
    public async Task Should_Handle_Multi_Phase_Load() { }
}
```

### 2. Coordinated Multi-User Scenarios

Simulate realistic user interactions:

```csharp
public class CoordinatedUserScenarios : Specification
{
    private static readonly ConcurrentQueue<string> AvailableResources = new();
    private static readonly SemaphoreSlim ResourceSemaphore = new(10); // Limit concurrent access
    
    protected override void EstablishContext()
    {
        // Populate shared resources
        for (int i = 1; i <= 100; i++)
        {
            AvailableResources.Enqueue($"resource_{i}");
        }
    }
    
    protected override void Because()
    {
        // Simulate realistic resource contention
        await ResourceSemaphore.WaitAsync();
        
        try
        {
            if (AvailableResources.TryDequeue(out var resource))
            {
                // Use the resource exclusively
                await UseResourceExclusively(resource);
                
                // Return resource to pool
                AvailableResources.Enqueue(resource);
            }
            else
            {
                // No resources available - simulate waiting or alternative behavior
                await HandleResourceUnavailable();
            }
        }
        finally
        {
            ResourceSemaphore.Release();
        }
    }
    
    [Load(concurrency: 50, duration: 120000)]
    public async Task Should_Handle_Resource_Contention() { }
}
```

## 📋 Testing Checklist

Before running load tests, verify:

### Pre-Test Checklist
- [ ] Test data is isolated and won't affect production
- [ ] Database connections are properly pooled
- [ ] HTTP clients are shared/reused appropriately
- [ ] Authentication tokens are handled correctly
- [ ] Test duration is appropriate for scenario
- [ ] Concurrency level matches expected load
- [ ] Error handling covers expected failure modes
- [ ] Resource cleanup is implemented
- [ ] Test environment matches production architecture

### Post-Test Analysis
- [ ] Success rate meets expectations (typically > 95%)
- [ ] Response times are within acceptable limits
- [ ] No memory leaks detected
- [ ] Error patterns make sense (timeouts vs. real errors)
- [ ] System recovered properly after test
- [ ] Resource utilization was reasonable
- [ ] Database connections didn't leak

## Pro Tips

1. **Start Small**: Always begin with single-user tests, then scale up
2. **Monitor Everything**: Watch CPU, memory, connections, and application metrics
3. **Test Realistic Scenarios**: Match real user behavior patterns
4. **Handle Errors Gracefully**: Expect and plan for partial failures under load
5. **Use Progressive Loading**: Gradually increase load to find breaking points
6. **Isolate Test Data**: Never use production data for load testing
7. **Clean Up Resources**: Prevent connection leaks and memory issues
8. **Document Expectations**: Clear success criteria and acceptable failure modes

Following these patterns will help you create effective, maintainable load tests that provide valuable insights into your system's behavior under stress.
