# Load Attribute Configuration

The `[Load]` attribute is the heart of xUnitV3LoadFramework, defining how your load tests execute. This guide covers all configuration options and their optimal usage.

## 📋 Basic Syntax

```csharp
[Load(
    order: 1,             // Required: Execution order
    concurrency: 50,      // Required: Number of concurrent users
    duration: 30000,      // Required: Test duration in milliseconds
    interval: 1000        // Required: Progress reporting interval
)]
```

##  Core Parameters

### Order

**Definition**: Execution order for your load tests (tests run in ascending order)

```csharp
[Load(order: 1, concurrency: 10, duration: 30000, interval: 1000)]
public void First_Load_Test() { }

[Load(order: 2, concurrency: 20, duration: 30000, interval: 1000)]  
public void Second_Load_Test() { }
```

### Concurrency

**Definition**: Number of concurrent users/threads executing your test

```csharp
[Load(order: 1, concurrency: 100, duration: 30000, interval: 5000)]
public void High_Concurrency_Test() { }

[Load(order: 2, concurrency: 10, duration: 30000, interval: 1000)]  
public void Low_Concurrency_Test() { }
```

**Guidelines**:
- **Start Low**: Begin with 1-10 users to validate test correctness
- **Scale Gradually**: Increase by 2x each iteration (10 → 20 → 40 → 80)
- **Consider Resources**: Don't exceed `CPU cores × 10` without monitoring
- **Production Sizing**: Base on expected peak traffic × safety factor

**Common Values**:
```csharp
// Light load testing
[Load(order: 1, concurrency: 25, duration: 60000, interval: 5000)]

// Moderate load testing  
[Load(order: 2, concurrency: 100, duration: 120000, interval: 10000)]

// Heavy load testing
[Load(order: 3, concurrency: 500, duration: 300000, interval: 30000)]

// Stress testing
[Load(order: 4, concurrency: 2000, duration: 600000, interval: 60000)]
```

### Duration

**Definition**: Total test execution time in milliseconds

```csharp
[Load(order: 1, concurrency: 50, duration: 30000, interval: 1000)]   // 30 seconds
[Load(order: 2, concurrency: 50, duration: 300000, interval: 5000)]  // 5 minutes
[Load(order: 3, concurrency: 50, duration: 3600000, interval: 60000)] // 1 hour
```

**Guidelines**:
- **Minimum Duration**: At least 30 seconds for meaningful metrics
- **Warm-up Period**: Include time for JIT compilation and connection pooling
- **Statistical Significance**: Longer tests provide more reliable percentiles
- **Resource Constraints**: Consider CI/CD pipeline time limits

**Duration Planning**:
```csharp
// Quick smoke tests (CI pipeline)
[Load(order: 1, concurrency: 10, duration: 15000, interval: 1000)] // 15 seconds

// Standard load tests (nightly builds)
[Load(order: 2, concurrency: 100, duration: 120000, interval: 10000)] // 2 minutes

// Extended performance tests (weekly)
[Load(order: 3, concurrency: 200, duration: 1800000, interval: 60000)] // 30 minutes

// Soak tests (on-demand)
[Load(order: 4, concurrency: 50, duration: 14400000, interval: 300000)] // 4 hours
```

### Interval

**Definition**: How often to report progress during test execution (in milliseconds)

```csharp
[Load(order: 1, concurrency: 100, duration: 60000, interval: 1000)]  // Report every 1 second
[Load(order: 2, concurrency: 100, duration: 300000, interval: 10000)] // Report every 10 seconds
```

**Guidelines**:
- **Short Tests**: Use 1000ms (1 second) intervals for detailed monitoring
- **Long Tests**: Use longer intervals (10-60 seconds) to reduce noise
- **CI/CD**: Use appropriate intervals to avoid overwhelming logs

##  Optional Parameters

### Skip

**Definition**: Allows you to skip a load test with a reason

```csharp
[Load(order: 1, concurrency: 100, duration: 60000, interval: 1000, Skip = "Under maintenance")]
public void Temporarily_Disabled_Test() 
{ 
    // This test will be skipped
}

[Load(order: 2, concurrency: 500, duration: 300000, interval: 30000, Skip = "Requires production environment")]
public void Production_Only_Test() 
{ 
    // Only run in production
}
```

## Configuration Patterns

### Smoke Testing Pattern

Quick validation that system handles minimal load:

```csharp
[Load(order: 1, concurrency: 1, duration: 10000, interval: 1000)]
public void Smoke_Test_Single_User() { }

[Load(order: 2, concurrency: 5, duration: 15000, interval: 1000)]  
public void Smoke_Test_Few_Users() { }
```

### Load Testing Pattern

Standard performance validation under expected load:

```csharp
[Load(order: 1, concurrency: 100, duration: 120000, interval: 10000)]
public void Standard_Load_Test() { }

[Load(order: 2, concurrency: 200, duration: 300000, interval: 15000)]
public void Extended_Load_Test() { }
```

### Stress Testing Pattern

Push system beyond normal limits:

```csharp
[Load(order: 1, concurrency: 500, duration: 300000, interval: 30000)]
public void Stress_Test_High_Concurrency() { }

[Load(order: 2, concurrency: 1000, duration: 600000, interval: 60000)]
public void Stress_Test_Very_High_Load() { }
```

### Spike Testing Pattern

Sudden traffic increases:

```csharp
[Load(concurrency: 200, duration: 60000, rampUp: 5000)] // Quick spike
public async Task Spike_Test_Sudden_Load() { }

[Load(concurrency: 500, duration: 120000, rampUp: 10000, rampDown: 5000)]
public async Task Spike_Test_Flash_Crowd() { }
```

### Soak Testing Pattern

Extended duration under moderate load:

```csharp
[Load(concurrency: 50, duration: 3600000)] // 1 hour
public async Task Soak_Test_Memory_Leaks() { }

[Load(concurrency: 100, duration: 14400000)] // 4 hours
public async Task Soak_Test_Extended_Stability() { }
```

##  Environment-Specific Configuration

### Development Environment

```csharp
#if DEBUG
[Load(concurrency: 10, duration: 15000)] // Light load for development
#else
[Load(concurrency: 100, duration: 120000)] // Full load for CI/CD
#endif
public async Task Environment_Aware_Test() { }
```

### Configuration via Environment Variables

```csharp
public class EnvironmentConfiguredTests
{
    private static int GetConcurrency() =>
        int.Parse(Environment.GetEnvironmentVariable("LOAD_CONCURRENCY") ?? "50");
    
    private static int GetDuration() =>
        int.Parse(Environment.GetEnvironmentVariable("LOAD_DURATION") ?? "30000");
    
    [Fact]
    [Load(concurrency: 50, duration: 30000)] // Defaults, overridden by environment
    public async Task Configurable_Load_Test()
    {
        // Test implementation uses environment-specific values
        await DoWorkAsync();
    }
}
```

### CI/CD Pipeline Configuration

```csharp
public class PipelineLoadTests
{
    // Quick tests for PR validation
    [Trait("Category", "QuickLoad")]
    [Load(concurrency: 25, duration: 30000)]
    public async Task PR_Validation_Load_Test() { }
    
    // Medium tests for nightly builds
    [Trait("Category", "NightlyLoad")]
    [Load(concurrency: 100, duration: 300000, rampUp: 30000)]
    public async Task Nightly_Load_Test() { }
    
    // Heavy tests for weekly performance validation
    [Trait("Category", "WeeklyLoad")]
    [Load(concurrency: 500, duration: 1800000, rampUp: 120000)]
    public async Task Weekly_Performance_Test() { }
}
```

## Parameter Selection Guidelines

### Concurrency Selection

```csharp
// Based on expected users
var expectedDailyUsers = 10000;
var peakConcurrentUsers = expectedDailyUsers * 0.1; // 10% concurrent
var testConcurrency = peakConcurrentUsers * 1.5; // 50% safety margin

[Load(concurrency: testConcurrency, duration: 120000)]
```

### Duration Selection

```csharp
// Minimum durations by test type
var smokeDuration = 15000;     // 15 seconds
var loadDuration = 120000;     // 2 minutes  
var stressDuration = 300000;   // 5 minutes
var soakDuration = 3600000;    // 1 hour

// Statistical significance (rough guide)
var minRequestsNeeded = 1000;
var expectedRPS = 100;
var minDuration = minRequestsNeeded / expectedRPS * 1000; // milliseconds
```

### Ramp-up Selection

```csharp
// Conservative ramp-up (20-25% of total duration)
[Load(concurrency: 100, duration: 120000, rampUp: 30000)]

// Aggressive ramp-up (5-10% of total duration)
[Load(concurrency: 100, duration: 120000, rampUp: 10000)]

// Extended ramp-up for capacity testing (30-50% of total duration)
[Load(concurrency: 200, duration: 300000, rampUp: 120000)]
```

## Performance Impact

### Framework Overhead

The Load attribute configuration affects framework overhead:

```csharp
// Low overhead configuration
[Load(concurrency: 50, duration: 60000, interval: 10000)]
// ~0.1% CPU overhead, infrequent reporting

// High overhead configuration  
[Load(concurrency: 1000, duration: 60000, interval: 100)]
// ~2-3% CPU overhead, frequent reporting
```

### Memory Usage

```csharp
// Memory-efficient configuration
[Load(concurrency: 100, duration: 300000)]
// ~10MB working set for result collection

// Memory-intensive configuration
[Load(concurrency: 2000, duration: 3600000, interval: 1000)]
// ~100MB+ working set for detailed metrics
```

##  Troubleshooting Configuration Issues

### Common Problems

**1. Test Timeout**
```csharp
// Problem: Test runner timeout < Load duration
[Load(concurrency: 50, duration: 600000)] // 10 minutes
// Solution: Configure test runner timeout > load duration

// xunit.runner.json
{
  "methodDisplay": "method",
  "diagnosticMessages": true,
  "maxParallelThreads": 1,
  "parallelizeAssembly": false,
  "parallelizeTestCollections": false,
  "preEnumerateTheories": false,
  "shadowCopy": false,
  "stopOnFail": false,
  "longRunningTestSeconds": 3600 // 1 hour timeout
}
```

**2. Resource Exhaustion**
```csharp
// Problem: Too many concurrent connections
[Load(concurrency: 5000, duration: 60000)] // May exhaust connection pool

// Solution: Configure connection limits or reduce concurrency
services.Configure<HttpClientOptions>(options => 
{
    options.Timeout = TimeSpan.FromSeconds(30);
    options.ConnectionLimit = 1000;
});
```

**3. Unrealistic Ramp Patterns**
```csharp
// Problem: Ramp-up longer than total duration
[Load(concurrency: 100, duration: 30000, rampUp: 60000)] // Invalid!

// Solution: Ensure rampUp + rampDown < duration
[Load(concurrency: 100, duration: 60000, rampUp: 20000, rampDown: 10000)]
// 20s ramp up + 30s steady + 10s ramp down = 60s total
```

##  Best Practices

### 1. Progressive Load Testing

```csharp
// Test suite with increasing load levels
[Load(concurrency: 10, duration: 30000)]   // Level 1: Smoke
public async Task Load_Test_Level_1() { }

[Load(concurrency: 50, duration: 120000)]  // Level 2: Light Load  
public async Task Load_Test_Level_2() { }

[Load(concurrency: 200, duration: 300000)] // Level 3: Heavy Load
public async Task Load_Test_Level_3() { }

[Load(concurrency: 1000, duration: 600000)] // Level 4: Stress
public async Task Load_Test_Level_4() { }
```

### 2. Realistic Traffic Patterns

```csharp
// Weekday morning traffic pattern
[Load(concurrency: 100, duration: 300000, rampUp: 60000, rampDown: 30000)]
public async Task Morning_Rush_Pattern() { }

// Flash sale / marketing campaign pattern
[Load(concurrency: 500, duration: 180000, rampUp: 10000, rampDown: 60000)]
public async Task Flash_Sale_Pattern() { }

// Gradual growth pattern
[Load(concurrency: 200, duration: 600000, rampUp: 180000, rampDown: 120000)]
public async Task Organic_Growth_Pattern() { }
```

### 3. Test Categorization

```csharp
// Use attributes to categorize tests
[Trait("LoadType", "Smoke")]
[Load(concurrency: 5, duration: 15000)]
public async Task Quick_Smoke_Test() { }

[Trait("LoadType", "Standard")]
[Load(concurrency: 100, duration: 120000)]
public async Task Standard_Load_Test() { }

[Trait("LoadType", "Stress")]
[Load(concurrency: 1000, duration: 600000)]
public async Task Stress_Test() { }
```

## 🔮 Advanced Configuration

### Custom Load Patterns (Future)

```csharp
// Planned: Custom load curves
[LoadPattern("morning-rush.json")]
public async Task Custom_Load_Pattern() { }

// Planned: Conditional scaling
[Load(concurrency: "auto", duration: 300000, targetRPS: 1000)]
public async Task Auto_Scaling_Test() { }
```

The Load attribute provides powerful configuration options for comprehensive performance testing. Start with simple configurations and gradually increase complexity as you understand your system's behavior under load.
