# xUnitV3LoadFramework 

A powerful, enterprise-grade load testing framework that seamlessly integrates with xUnit v3, enabling declarative load testing through simple attributes while providing comprehensive performance metrics and reporting.

[![.NET 9](https://img.shields.io/badge/.NET-9.0-purple)](https://dotnet.microsoft.com/download/dotnet/9.0)
[![xUnit v3](https://img.shields.io/badge/xUnit-v3.0-blue)](https://xunit.net/)
[![Akka.NET](https://img.shields.io/badge/Akka.NET-1.5-red)](https://getakka.net/)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Build Status](https://img.shields.io/badge/build-passing-brightgreen)]()

## Features

* **Declarative Load Testing** - Transform any test method into a comprehensive load test with a single attribute  
* **High Performance** - Built on Akka.NET actor system for maximum scalability and throughput  
* **Rich Metrics** - Comprehensive performance reporting with latency percentiles, throughput, and resource utilization  
* **Mixed Testing** - Seamlessly combine load tests with unit tests, integration tests, and theory tests  
* **Developer Friendly** - Minimal learning curve with familiar xUnit patterns and conventions  
* **Production Ready** - Battle-tested architecture suitable for enterprise environments  

## Quick Start

### Installation

```bash
dotnet add package xUnitV3LoadFramework
```

### Your First Load Test

```csharp
using xUnitV3LoadFramework.Attributes;
using xUnitV3LoadFramework.Extensions;

public class APILoadTests : TestSetup
{
    [Load(order: 1, concurrency: 5, duration: 3000, interval: 200)]
    public async Task LoadTest_UserAPI()
    {
        var result = await LoadTestRunner.ExecuteAsync(async () =>
        {
            var response = await httpClient.GetAsync("https://api.example.com/data");
            response.EnsureSuccessStatusCode();
            return true;
        });
        
        Assert.True(result.Success > 0, "Load test should succeed");
        Assert.True(result.RequestsPerSecond >= 10, "Should achieve 10+ req/sec");
    }
}
```

### Comprehensive Results

```
===============================================
LOAD TEST RESULTS: APILoadTests.LoadTest_UserAPI
===============================================
Test Configuration:
   Order: 1
   Concurrency: 5 parallel executions
   Duration: 3000ms (3.0s)
   Interval: 200ms between batches

Execution Summary:
   Total Executions: 75
   Successful: 73
   Failed: 2
   Success Rate: 97.33%
   Total Time: 3.12s
   Requests/Second: 24.04

Performance Metrics:
   Average Latency: 156.23ms
   Median Latency: 145.67ms
   Min Latency: 89.12ms
   Max Latency: 289.45ms
   95th Percentile: 234.56ms
   99th Percentile: 267.89ms

Resource Utilization:
   Worker Threads: 8
   Worker Utilization: 87.50%
   Peak Memory: 45.67 MB
   Batches Completed: 15
===============================================
```

##  Documentation

### Technical Documentation

| Component | Documentation |
|-----------|---------------|
| **API Reference** | [LoadTestRunner API](docs/api-reference/loadtest-runner.md) |
| **Architecture** | [Actor System Overview](docs/architecture/actor-system-overview.md) |
| **Framework Design** | [Hybrid Load Worker](docs/architecture/hybrid-load-worker.md) |

###  Complete Documentation
- **[API Reference](docs/api-reference/)** - Detailed API documentation
- **[Architecture](docs/architecture/)** - Framework internals and design

## Load Attribute

The heart of the framework - transform any method into a load test:

```csharp
[Load(order: 1, concurrency: 10, duration: 5000, interval: 100)]
```

### Parameters Explained

| Parameter | Description | Example |
|-----------|-------------|---------|
| **order** | Execution sequence (1 runs before 2) | `order: 1` |
| **concurrency** | Parallel operations per batch | `concurrency: 10` |
| **duration** | Total test time in milliseconds | `duration: 5000` (5 seconds) |
| **interval** | Time between batches in milliseconds | `interval: 100` (0.1 seconds) |

## Usage Examples

### HTTP API Load Testing
```csharp
[Load(order: 1, concurrency: 5, duration: 3000, interval: 200)]
public async Task LoadTest_CreateUser()
{
    var result = await LoadTestRunner.ExecuteAsync(async () =>
    {
        var user = new { Name = "Test User", Email = "test@example.com" };
        var response = await httpClient.PostAsJsonAsync("/api/users", user);
        response.EnsureSuccessStatusCode();
        return true;
    });
    
    Assert.True(result.Success >= result.Total * 0.95, "95% success rate expected");
}
```

### Database Load Testing
```csharp
[Load(order: 1, concurrency: 3, duration: 5000, interval: 300)]
public async Task LoadTest_HttpRequests()
{
    var result = await LoadTestRunner.ExecuteAsync(async () =>
    {
        using var httpClient = new HttpClient();
        var response = await httpClient.GetAsync("https://api.example.com/data");
        return response.IsSuccessStatusCode;
    });
    
    Assert.True(result.AverageLatency <= 200, "HTTP requests should be fast");
}
```

### Mixed Testing (Load + Unit Tests)
```csharp
public class MixedTests : TestSetup
{
    // Regular unit test
    [Fact]
    public void UnitTest_ValidateBusinessLogic()
    {
        var calculator = new Calculator();
        Assert.Equal(4, calculator.Add(2, 2));
    }
    
    // Load test
    [Load(order: 1, concurrency: 4, duration: 2000, interval: 250)]
    public async Task LoadTest_CalculatorPerformance()
    {
        var result = await LoadTestRunner.ExecuteAsync(() =>
        {
            var calculator = new Calculator();
            var result = calculator.ComplexCalculation(1000);
            return result > 0;
        });
        
        Assert.True(result.Success > 0);
    }
    
    // Theory test with parameters
    [Theory]
    [InlineData(1, 2, 3)]
    [InlineData(5, 5, 10)]
    public void Theory_Addition(int a, int b, int expected)
    {
        Assert.Equal(expected, a + b);
    }
}
```

## � Enhanced LoadTestRunner API

### Traditional Approach with Load Attribute
```csharp
[Load(order: 1, concurrency: 5, duration: 3000, interval: 200)]
public async Task Should_Handle_HTTP_Load()
{
    var result = await LoadTestRunner.ExecuteAsync(async () =>
    {
        var response = await httpClient.GetAsync("/api/users");
        response.EnsureSuccessStatusCode();
        return true;
    });
    
    Assert.True(result.Success > 0);
}
```

### Simplified API (No Return Value Needed)
```csharp
[Load(order: 2, concurrency: 3, duration: 2000, interval: 300)]
public async Task Should_Handle_Simple_Load()
{
    var result = await LoadTestRunner.RunAsync(async () =>
    {
        var response = await httpClient.GetAsync("/api/health");
        response.EnsureSuccessStatusCode();
        // Success is implicit if no exception is thrown
    });
    
    Assert.True(result.Success > 0);
}
```

### Fluent API for Dynamic Configuration
```csharp
[Fact]
public async Task Should_Use_Fluent_API()
{
    var result = await LoadTestRunner.Create()
        .WithConcurrency(10)
        .WithDuration(5000)
        .WithInterval(100)
        .WithName("Custom_API_Test")
        .RunAsync(async () =>
        {
            var response = await httpClient.PostAsJsonAsync("/api/orders", 
                new { ProductId = 1, Quantity = 2 });
            response.EnsureSuccessStatusCode();
        });
        
    Assert.True(result.RequestsPerSecond >= 5);
    Console.WriteLine($"Test '{result.Name}' achieved {result.RequestsPerSecond:F2} req/sec");
}
```

### Parameterized Load Testing
```csharp
[Theory]
[InlineData("Light", 3, 2000)]
[InlineData("Heavy", 15, 6000)]
public async Task Should_Handle_Variable_Load(string level, int concurrency, int duration)
{
    var result = await LoadTestRunner.Create()
        .WithName($"API_Load_Test_{level}")
        .WithConcurrency(concurrency)
        .WithDuration(duration)
        .WithInterval(200)
        .RunAsync(async () =>
        {
            var response = await httpClient.GetAsync("/api/data");
            response.EnsureSuccessStatusCode();
        });
        
    var expectedRps = level == "Heavy" ? 20 : 5;
    Assert.True(result.RequestsPerSecond >= expectedRps);
}
```

## Architecture

### Actor-Based Engine
Built on **Akka.NET** for high-performance, concurrent execution:

- **LoadWorkerActor**: Manages concurrent test execution
- **ResultCollectorActor**: Aggregates metrics and results
- **Hybrid Mode**: Optimizes between task-based and actor-based execution

### Framework Components

```
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│  Load       │───▶│  LoadTestRunner  │───▶│  LoadRunner     │
│  Attribute      │    │                  │    │                 │
└─────────────────┘    └──────────────────┘    └─────────────────┘
                                                         │
                                                         |
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│  Test Results   │◀───│  Result          │◀───│  Akka.NET       │
│  & Metrics      │    │  Collector       │    │  Actor System   │
└─────────────────┘    └──────────────────┘    └─────────────────┘
```

## Performance Metrics

Every load test provides comprehensive metrics:

### Execution Metrics
- **Total/Success/Failure counts**
- **Success rate percentage**
- **Execution time and throughput**

### Latency Analysis
- **Average, Median, Min, Max latency**
- **95th and 99th percentile latency**
- **Latency distribution analysis**

### Resource Utilization
- **Worker thread usage**
- **Memory consumption**
- **Batch execution efficiency**

## Requirements

- **.NET 9.0** or higher
- **xUnit v3.0** test framework
- **C# 12.0** language features

## NuGet Package

```xml
<PackageReference Include="xUnitV3LoadFramework" Version="1.0.0-alpha.1" />
```

## Contributing

We welcome contributions! Please see our [Contributing Guide](CONTRIBUTING.md) for details.

### Development Setup
```bash
# Clone the repository
git clone https://github.com/mrviduus/xUnitV3LoadFramework.git

# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run tests
dotnet test
```

## 📄 License

This project is licensed under the [MIT License](LICENSE).

## 🙏 Acknowledgments

- **[xUnit.net](https://xunit.net/)** - The foundation for .NET testing
- **[Akka.NET](https://getakka.net/)** - The actor framework powering our load engine
- **[.NET Community](https://dotnet.microsoft.com/community)** - For continuous inspiration and support

##  Support & Community

- 🐛 **Issues**: [GitHub Issues](https://github.com/mrviduus/xUnitV3LoadFramework/issues)
- 💬 **Discussions**: [GitHub Discussions](https://github.com/mrviduus/xUnitV3LoadFramework/discussions)
- 📧 **Email**: [mrviduus@gmail.com](mailto:mrviduus@gmail.com)
- 💼 **LinkedIn**: [Vasyl Vdovychenko](https://www.linkedin.com/in/vasyl-vdovychenko)

---

**If this project helps you, please consider giving it a star!**

*Built with love by [Vasyl Vdovychenko](https://github.com/mrviduus) and the .NET community.*
