# API Reference

Complete reference for all public APIs in the xUnitV3LoadFramework.

## 📋 Overview

The framework provides the following main API surfaces:

- **[Attributes](#attributes)** - Test decoration and configuration
- **[Base Classes](#base-classes)** - Test infrastructure and lifecycle management  
- **[Actors](#actors)** - Core execution engine components
- **[Models](#models)** - Data structures for results and configuration
- **[Messages](#messages)** - Actor communication contracts
- **[Extensions](#extensions)** - Helper methods and utilities

---

## 🏷️ Attributes

### LoadAttribute

Configures load test execution parameters.

```csharp
namespace xUnitV3LoadFramework.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class LoadAttribute : FactAttribute
    {
        public LoadAttribute(int order, int concurrency, int duration, int interval);
        
        public int Order { get; set; }
        public int Concurrency { get; set; }
        public int Duration { get; set; }
        public int Interval { get; set; }
        public string Skip { get; set; }
    }
}
```

**Constructor Parameters**:
- `order` - Test execution order (Required)
- `concurrency` - Number of concurrent users/threads (Required)
- `duration` - Test duration in milliseconds (Required)
- `interval` - Progress reporting interval in milliseconds (Required)

**Properties**:
- `Skip` - Reason for skipping the test (Optional)

**Example**:
```csharp
[Load(order: 1, concurrency: 100, duration: 60000, interval: 5000)]
public void My_Load_Test()
{
    // Test implementation
}

[Load(order: 2, concurrency: 50, duration: 30000, interval: 1000, Skip = "Under maintenance")]
public void Skipped_Load_Test()
{
    // This test will be skipped
}
```

---

## 🏗️ Base Classes

### Standard xUnit Patterns

The framework supports standard xUnit patterns for test organization and lifecycle management.

```csharp
using xUnitV3LoadFramework.Attributes;
using Xunit;

namespace YourTests
{
    public class ApiLoadTests : IDisposable
    {
        private readonly HttpClient _httpClient;
        
        public ApiLoadTests()
        {
            // Constructor - setup executed once per test class
            _httpClient = new HttpClient();
        }
        
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
- **[Load] methods** - Load tests executed with specified concurrency and duration

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
