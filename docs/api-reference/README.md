# API Reference

Complete reference for all public APIs in the xUnitV3LoadFramework.

## 📋 Overview

The framework provides the following main API surfaces:

- **[Attributes](#attributes)** - Test decoration and configuration
- **[Helper Classes](#helper-classes)** - Load test execution utilities
- **[Base Classes](#base-classes)** - Test infrastructure and lifecycle management  
- **[Actors](#actors)** - Core execution engine components
- **[Models](#models)** - Data structures for results and configuration
- **[Messages](#messages)** - Actor communication contracts
- **[Extensions](#extensions)** - Helper methods and utilities

---

## 🏷️ Attributes

### LoadFactAttribute

A load testing attribute that inherits from xUnit's `FactAttribute`, enabling load tests to be discovered and executed as standard xUnit tests.

```csharp
namespace xUnitV3LoadFramework.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class LoadFactAttribute : FactAttribute
    {
        public LoadFactAttribute(
            int order = 0, 
            int concurrency = 1, 
            int duration = 1000, 
            int interval = 100,
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0);
        
        public int Order { get; set; }
        public int Concurrency { get; set; }
        public int Duration { get; set; }
        public int Interval { get; set; }
        public string FilePath { get; set; }
        public int LineNumber { get; set; }
    }
}
```

**Constructor Parameters**:
- `order` - Test execution order (Default: 0)
- `concurrency` - Number of concurrent executions (Default: 1)
- `duration` - Test duration in milliseconds (Default: 1000)
- `interval` - Progress reporting interval in milliseconds (Default: 100)
- `filePath` - Automatically captured source file path
- `lineNumber` - Automatically captured source line number

**Properties**:
- `Skip` - Inherited from FactAttribute for skipping tests
- `DisplayName` - Inherited from FactAttribute for custom test names
- `Timeout` - Inherited from FactAttribute for test timeouts

**Example**:
```csharp
[LoadFact(order: 1, concurrency: 100, duration: 60000, interval: 5000)]
public async Task My_Load_Test()
{
    var result = await LoadTestHelper.ExecuteLoadTestAsync(async () =>
    {
        // Test implementation
        return true;
    });
    
    Assert.True(result.Success > 0);
}

[LoadFact(order: 2, concurrency: 50, duration: 30000, interval: 1000, Skip = "Under maintenance")]
public async Task Skipped_Load_Test()
{
    // This test will be skipped by xUnit
    await LoadTestHelper.ExecuteLoadTestAsync(async () => true);
}
```

---

## 🔧 Helper Classes

### LoadTestHelper

Provides methods for executing load tests within xUnit test methods.

```csharp
namespace xUnitV3LoadFramework.Extensions
{
    public static class LoadTestHelper
    {
        // Execute with automatic attribute detection
        public static Task<LoadTestResult> ExecuteLoadTestAsync<T>(
            Func<Task<T>> testAction,
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0);

        // Execute with explicit configuration
        public static Task<LoadTestResult> ExecuteLoadTestAsync<T>(
            Func<Task<T>> testAction,
            int concurrency,
            TimeSpan duration,
            TimeSpan interval,
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0);

        // Execute with LoadFactAttribute configuration
        public static Task<LoadTestResult> ExecuteLoadTestAsync<T>(
            Func<Task<T>> testAction,
            LoadFactAttribute loadAttribute);
    }
}
```

**Methods**:
- `ExecuteLoadTestAsync<T>(Func<Task<T>>)` - Automatically detects LoadFact attribute configuration
- `ExecuteLoadTestAsync<T>(Func<Task<T>>, int, TimeSpan, TimeSpan)` - Uses explicit configuration parameters
- `ExecuteLoadTestAsync<T>(Func<Task<T>>, LoadFactAttribute)` - Uses provided attribute configuration

**Return Type**: `LoadTestResult` containing execution statistics

**Example**:
```csharp
[LoadFact(concurrency: 10, duration: 5000)]
public async Task Should_Handle_Concurrent_Requests()
{
    // Automatic configuration from attribute
    var result = await LoadTestHelper.ExecuteLoadTestAsync(async () =>
    {
        var response = await httpClient.GetAsync("https://api.example.com");
        return response.IsSuccessStatusCode;
    });
    
    Assert.True(result.Success > 0);
    Assert.True(result.Total >= result.Success);
}

// Custom configuration override
[LoadFact(concurrency: 5, duration: 2000)]
public async Task Should_Handle_Custom_Configuration()
{
    var result = await LoadTestHelper.ExecuteLoadTestAsync(
        testAction: async () => {
            // Test logic
            return true;
        },
        concurrency: 20,  // Override attribute
        duration: TimeSpan.FromSeconds(10),
        interval: TimeSpan.FromMilliseconds(500)
    );
    
    Assert.True(result.Success > 0);
}
```

---

## 🏗️ Base Classes

### TestSetup

Optional base class providing dependency injection and test context for load tests.

```csharp
namespace xUnitV3LoadFramework
{
    public abstract class TestSetup : IDisposable
    {
        protected IServiceProvider ServiceProvider { get; }
        protected TestContext TestContext { get; }
        
        protected T GetService<T>() where T : class;
        protected T GetRequiredService<T>();
        
        public virtual void Dispose();
    }
}
```

**Properties**:
- `ServiceProvider` - Access to dependency injection container
- `TestContext` - Current test execution context

**Methods**:
- `GetService<T>()` - Retrieve optional service from DI container
- `GetRequiredService<T>()` - Retrieve required service from DI container

### Standard xUnit Patterns

The framework supports standard xUnit patterns for test organization and lifecycle management.

```csharp
using xUnitV3LoadFramework.Attributes;
using xUnitV3LoadFramework.Extensions;
using Xunit;

namespace YourTests
{
    // Simple class - no special inheritance needed
    public class SimpleLoadTests
    {
        [LoadFact(concurrency: 5, duration: 2000)]
        public async Task Should_Handle_Basic_Load()
        {
            var result = await LoadTestHelper.ExecuteLoadTestAsync(async () =>
            {
                // Test logic
                return true;
            });
            
            Assert.True(result.Success > 0);
        }
    }
    
    // With dependency injection via TestSetup
    public class ApiLoadTests : TestSetup
        
        public void Dispose()
        {
            // Cleanup executed once per test class
            _httpClient?.Dispose();
        }
        
        [Fact]
        public async Task Should_Connect_Successfully()
        {
            // Standard xUnit functional test
            var response = await _httpClient.GetAsync("https://api.example.com/health");
            Assert.True(response.IsSuccessStatusCode);
        }
        
        [Load(concurrency: 50, duration: 30000, order: 1, interval: 1000)]
        public async Task Should_Handle_Load() 
        { 
            // Load test - executed concurrently
            var response = await _httpClient.GetAsync("https://api.example.com/data");
            Assert.True(response.IsSuccessStatusCode);
        }
    }
}
```

**Lifecycle Patterns**:
- **Constructor** - Setup executed once before test class instantiation
- **IDisposable.Dispose()** - Cleanup executed once after all tests in class complete
- **[Fact] methods** - Standard xUnit functional tests
- **[LoadFact] methods** - Load tests executed with LoadTestHelper and specified concurrency/duration

---

## 🎭 Actors

### LoadWorkerActorHybrid

Core actor responsible for executing load tests using channel-based hybrid workers.

```csharp
namespace xUnitV3LoadFramework.LoadRunnerCore.Actors
{
    public class LoadWorkerActorHybrid : ReceiveActor
    {
        public LoadWorkerActorHybrid(LoadExecutionPlan plan, IActorRef resultCollector);
    }
}
```

**Constructor Parameters**:
- `plan` - Load execution plan containing test configuration
- `resultCollector` - Actor reference for result collection

**Supported Messages**:
- `StartLoadMessage` - Initiates load test execution
- `StopLoadMessage` - Stops ongoing load test
- `GetResultsMessage` - Requests current test results

**Features**:
- Fixed worker pool with optimal thread count calculation
- Channel-based work distribution for high performance
- Comprehensive metrics collection
- Memory-efficient execution model

### LoadWorkerActor

Traditional task-based load worker actor.

```csharp
namespace xUnitV3LoadFramework.LoadRunnerCore.Actors
{
    public class LoadWorkerActor : ReceiveActor
    {
        public LoadWorkerActor(LoadExecutionPlan plan, IActorRef resultCollector);
    }
}
```

**Constructor Parameters**:
- `plan` - Load execution plan containing test configuration
- `resultCollector` - Actor reference for result collection

**Features**:
- Dynamic task creation for each batch
- Flexible concurrency management
- Traditional .NET Task-based execution

### ResultCollectorActor

Actor responsible for collecting and aggregating test results.

```csharp
namespace xUnitV3LoadFramework.LoadRunnerCore.Actors
{
    public class ResultCollectorActor : ReceiveActor
    {
        public ResultCollectorActor(string testName);
    }
}
```

**Constructor Parameters**:
- `testName` - Name identifier for the test being executed

**Supported Messages**:
- `StepResultMessage` - Individual test step result
- `WorkerThreadCountMessage` - Worker thread count notification
- `GetResultsMessage` - Request for aggregated results
- `ResetMessage` - Reset collected results
- `BatchCompletionMessage` - Notification of batch completion

**Features**:
- Real-time latency percentile calculation
- Throughput metrics computation
- Resource utilization tracking
- Memory usage monitoring

---

## 📨 Messages

### StartLoadMessage

Initiates load test execution.

```csharp
namespace xUnitV3LoadFramework.LoadRunnerCore.Messages
{
    public class StartLoadMessage
    {
        // Message contains execution parameters from LoadExecutionPlan
    }
}
```

### StepResultMessage

Reports individual test step execution results.

```csharp
namespace xUnitV3LoadFramework.LoadRunnerCore.Messages
{
    public class StepResultMessage
    {
        public bool IsSuccess { get; set; }
        public double Latency { get; set; }
        public string Error { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
```

**Properties**:
- `IsSuccess` - Whether the test step succeeded
- `Latency` - Execution time in milliseconds
- `Error` - Error message if step failed
- `Timestamp` - When the step completed

### WorkerThreadCountMessage

Notifies result collector of active worker thread count.

```csharp
namespace xUnitV3LoadFramework.LoadRunnerCore.Messages
{
    public class WorkerThreadCountMessage
    {
        public int ThreadCount { get; set; }
    }
}
```

### BatchCompletionMessage

Signals completion of a work batch.

```csharp
namespace xUnitV3LoadFramework.LoadRunnerCore.Messages
{
    public class BatchCompletionMessage
    {
        public int BatchId { get; set; }
        public int CompletedTasks { get; set; }
    }
}
```

---

## 📊 Models

### LoadResult

Comprehensive test result data structure.

```csharp
namespace xUnitV3LoadFramework.LoadRunnerCore.Models
{
    public class LoadResult
    {
        // Request counts
        public int RequestsStarted { get; set; }
        public int Total { get; set; }
        public int Success { get; set; }
        public int Failure { get; set; }
        
        // Latency metrics (milliseconds)
        public double AverageLatency { get; set; }
        public double MinLatency { get; set; }
        public double MaxLatency { get; set; }
        public double MedianLatency { get; set; }
        public double Percentile95Latency { get; set; }
        public double Percentile99Latency { get; set; }
        
        // Throughput
        public double RequestsPerSecond { get; set; }
        
        // Queue performance
        public double AvgQueueTime { get; set; }
        public double MaxQueueTime { get; set; }
        
        // Resource utilization
        public int WorkerThreadsUsed { get; set; }
        public double WorkerUtilization { get; set; }
        public long PeakMemoryUsage { get; set; }
        public int BatchesCompleted { get; set; }
    }
}
```

**Metric Categories**:

**Request Metrics**:
- `RequestsStarted` - Total requests initiated
- `Total` - Total requests completed (success + failure)
- `Success` - Number of successful requests
- `Failure` - Number of failed requests

**Latency Metrics** (all in milliseconds):
- `AverageLatency` - Mean response time
- `MinLatency` - Fastest response time
- `MaxLatency` - Slowest response time
- `MedianLatency` - 50th percentile response time
- `Percentile95Latency` - 95th percentile response time
- `Percentile99Latency` - 99th percentile response time

**Throughput Metrics**:
- `RequestsPerSecond` - Average requests processed per second

**Queue Performance Metrics**:
- `AvgQueueTime` - Average time work items spent in queue
- `MaxQueueTime` - Maximum queue time for any work item

**Resource Utilization Metrics**:
- `WorkerThreadsUsed` - Number of worker threads employed
- `WorkerUtilization` - Percentage of time workers were active
- `PeakMemoryUsage` - Maximum memory consumption during test
- `BatchesCompleted` - Total number of work batches processed

### LoadExecutionPlan

Configuration object for load test execution.

```csharp
namespace xUnitV3LoadFramework.LoadRunnerCore.Models
{
    public class LoadExecutionPlan
    {
        public string Name { get; set; }
        public LoadSettings Settings { get; set; }
        public Func<Task<bool>> Action { get; set; }
    }
}
```

**Properties**:
- `Name` - Identifier for the load test
- `Settings` - Configuration parameters (duration, concurrency, etc.)
- `Action` - Test action to execute under load

### LoadSettings

Load test configuration parameters.

```csharp
namespace xUnitV3LoadFramework.LoadRunnerCore.Models
{
    public class LoadSettings
    {
        public int Concurrency { get; set; }
        public TimeSpan Duration { get; set; }
        public TimeSpan Interval { get; set; }
    }
}
```

---

## 🔧 Extensions

### Framework Extensions

Extensions for integrating with xUnit test framework.

```csharp
namespace xUnitV3LoadFramework.Extensions.Framework
{
    public class LoadTestFramework : XunitTestFramework
    {
        // Custom test framework for load test execution
    }
}
```

### ObjectModel Extensions

Extensions for test object model integration.

```csharp
namespace xUnitV3LoadFramework.Extensions.ObjectModel
{
    public class LoadTestCase : XunitTestCase
    {
        // Custom test case for load test scenarios
    }
    
    public class LoadTestRunner : XunitTestRunner
    {
        // Custom test runner for load test execution
    }
}
```

---

## � Usage Examples

### Basic Load Test

```csharp
[Fact]
[Load(concurrency: 50, duration: 30000)]
public async Task Should_Handle_Load()
{
    var httpClient = new HttpClient();
    var response = await httpClient.GetAsync("https://api.example.com/data");
    Assert.True(response.IsSuccessStatusCode);
}
```

### Mixed Testing Approach

```csharp
public class ApiLoadTests : IDisposable
{
    private readonly HttpClient _httpClient;
    
    public ApiLoadTests()
    {
        _httpClient = new HttpClient();
    }
    
    public void Dispose()
    {
        _httpClient?.Dispose();
    }
    
    [Fact]
    public async Task Should_Connect_To_API()
    {
        // Standard functional test
        var response = await _httpClient.GetAsync("https://api.example.com/health");
        Assert.True(response.IsSuccessStatusCode);
    }
    
    [Load(order: 1, concurrency: 100, duration: 60000, interval: 5000)]
    public async Task Should_Handle_User_List_Load() 
    {
        // Load test - executed concurrently
        var response = await _httpClient.GetAsync("https://api.example.com/users");
        Assert.True(response.IsSuccessStatusCode);
    }
}
```

### Custom Actor Usage

```csharp
var system = ActorSystem.Create("LoadTestSystem");
var plan = new LoadExecutionPlan
{
    Name = "CustomTest",
    Settings = new LoadSettings 
    { 
        Duration = TimeSpan.FromSeconds(30),
        Concurrency = 100
    },
    Action = async () => 
    {
        await DoWorkAsync();
        return true;
    }
};

var resultCollector = system.ActorOf(Props.Create(() => new ResultCollectorActor("test")));
var loadWorker = system.ActorOf(Props.Create(() => new LoadWorkerActorHybrid(plan, resultCollector)));

var result = await loadWorker.Ask<LoadResult>(new StartLoadMessage(), TimeSpan.FromMinutes(1));
```

## 🔍 Error Handling

### Common Exceptions

**LoadTestException**:
```csharp
public class LoadTestException : Exception
{
    public LoadTestException(string message);
    public LoadTestException(string message, Exception innerException);
}
```

**ActorTimeoutException**:
- Thrown when actor operations exceed configured timeouts
- Common during high-load scenarios or system resource exhaustion

### Exception Handling Patterns

```csharp
[Load(concurrency: 100, duration: 30000)]
public async Task Should_Handle_Errors_Gracefully()
{
    try
    {
        await DoRiskyOperationAsync();
    }
    catch (TimeoutException)
    {
        // Expected under high load - system is protecting itself
        return true; // Count as successful handling
    }
    catch (Exception ex)
    {
        // Log unexpected errors but don't fail the entire test
        Console.WriteLine($"Unexpected error: {ex.Message}");
        return false; // Count as failure in metrics
    }
}
```

This comprehensive API reference covers all public interfaces provided by the xUnitV3LoadFramework. For more detailed examples and usage patterns, see the [User Guides](../user-guides/) and [Examples](../examples/) sections.
