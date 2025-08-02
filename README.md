# 🚀 xUnitV3LoadFramework

[![NuGet](https://img.shields.io/nuget/v/xUnitV3LoadFramework.svg)](https://www.nuget.org/packages/xUnitV3LoadFramework)
[![Downloads](https://img.shields.io/nuget/dt/xUnitV3LoadFramework.svg)](https://www.nuget.org/packages/xUnitV3LoadFramework)

**xUnitV3LoadFramework** is a robust and user-friendly load testing framework built to seamlessly integrate with **xUnit** and powered by **Akka.NET actors**. It allows developers to efficiently define, execute, and analyze parallel load test scenarios, making load testing a natural part of your automated testing workflow.

---

## 🌟 Features

### Hybrid Execution Model
- **Channel-based Workers**: Fixed worker pools with pre-allocated channels for consistent performance
- **Task-based Workers**: Dynamic task creation for flexible workload handling
- **Automatic Optimization**: Framework automatically selects optimal worker counts based on system resources

### Comprehensive Metrics
- **Latency Percentiles**: P50, P95, P99 latency measurements
- **Throughput Analysis**: Requests per second with real-time monitoring
- **Resource Utilization**: Memory usage, worker thread tracking, GC pressure
- **Queue Time Tracking**: Average and maximum queue times

### xUnit v3 Integration
- **Native Test Framework**: Seamless integration with xUnit v3 test discovery and execution
- **Attribute-based Configuration**: Simple `[Load]` attributes for test configuration
- **Test Result Integration**: Load test results integrated with xUnit test results

### Actor-based Architecture
- **Akka.NET Foundation**: Built on proven actor model for reliability and scalability
- **Message-driven Processing**: Asynchronous message passing for high throughput
- **Fault Tolerance**: Supervisor hierarchies for robust error handling

## 🔥 What's New in v2

### 🎯 Major Improvements
- **No More Specification Pattern**: Write stress tests like regular xUnit tests
- **Enhanced xUnit Compatibility**: Mix `[Fact]`, `[Theory]`, and `[Stress]` in the same class
- **Better Attribute Naming**: `LoadAttribute` → `StressAttribute` for clearer semantics
- **Standard Lifecycle**: Use constructor/`IDisposable`/`IAsyncDisposable` instead of custom methods
- **Async First**: Full support for `async Task` test methods

### � Migration Support
- **Backward Compatibility**: v1 attributes still work (with deprecation warnings)
- **Gradual Migration**: Migrate one test class at a time
- **Migration Guide**: Comprehensive guide with examples and patterns

---

- **High Throughput**: Tested up to 500,000 requests with sustained performance
- **Low Latency**: Sub-millisecond overhead for test execution framework
- **Resource Efficient**: Optimized memory usage and GC pressure management
- **Scalable**: Automatic scaling based on available system resources

---

## 🔧 System Requirements

- .NET 8.0+
- xUnit v3 (preview)
- Akka.NET 1.5.41+
- Minimum 4GB RAM for high-load scenarios
- Multi-core CPU recommended for optimal performance

---

## ⚡ Installation

Install via NuGet package manager:

```bash
dotnet add package xUnitV3LoadFramework
```

---

## 🚦 Quick Start

### Defining a Load Test
Use the `Load` attribute (inheriting from `FactAttribute`) to configure concurrency level, duration, interval, and execution order.

### Running Your Load Test
Execute your tests using the standard xUnit command:

```bash
dotnet test
```

---

## 📝 Usage Examples

### Basic Stress Test Example (v2 Approach)

Here's the new v2 approach demonstrating stress tests without the Specification pattern:

```csharp
using xUnitV3LoadFramework.Attributes;
using System;

namespace xUnitStressDemo;

[UseStressFramework]
public class ApiStressTests : IAsyncDisposable
{
    private readonly HttpClient _httpClient;

    public ApiStressTests()
    {
        // Setup in constructor (replaces EstablishContext)
        _httpClient = new HttpClient();
        Console.WriteLine(">> Setup phase");
    }

    [Fact]
    public void Quick_Unit_Test()
    {
        // Standard xUnit test - runs immediately
        Assert.True(true);
        Console.WriteLine(">> Standard unit test");
    }

    [Stress(order: 1, concurrency: 2, duration: 5000, interval: 500)]
    public async Task Should_Handle_Light_Stress()
    {
        // Stress test with actor system and performance monitoring
        Console.WriteLine(">> Running Stress Test 1");
        var response = await _httpClient.GetAsync("https://httpbin.org/get");
        Assert.True(response.IsSuccessStatusCode);
    }

    [Stress(order: 2, concurrency: 3, duration: 7000, interval: 300)]
    public async Task Should_Handle_Heavy_Stress()
    {
        // Higher concurrency stress test
        Console.WriteLine(">> Running Stress Test 2");
        var response = await _httpClient.GetAsync("https://httpbin.org/delay/1");
        Assert.True(response.IsSuccessStatusCode);
    }

    public async ValueTask DisposeAsync()
    {
        // Cleanup (replaces DestroyContext)
        _httpClient?.Dispose();
        Console.WriteLine(">> Cleanup phase");
    }
}
```

### Legacy Load Test Example (v1 Compatibility)

The v1 approach still works but is deprecated:

```csharp
using xUnitV3LoadFramework.Attributes;
using xUnitV3LoadFramework.Extensions;
using System;

namespace xUnitLoadDemo;

[UseLoadFramework] // Deprecated - use UseStressFramework
public class ExampleLoadSpecification : Specification // Deprecated pattern
{
    protected override void EstablishContext()
    {
        Console.WriteLine(">> Setup phase");
    }

    protected override void Because()
    {
        Console.WriteLine(">> Action phase");
    }

    [Load(order: 1, concurrency: 2, duration: 5000, interval: 500)] // Deprecated - use Stress
    public void should_run_load_scenario_1()
    {
        Console.WriteLine(">> Running Load 1");
    }

    [Load(order: 2, concurrency: 3, duration: 7000, interval: 300)]
    public void should_run_load_scenario_2()
    {
        Console.WriteLine(">> Running Load 2");
    }
}
```

### API Stress Testing Example

```csharp
using xUnitV3LoadFramework.Attributes;

[UseStressFramework]
public class ApiStressTests : IAsyncDisposable
{
    private readonly HttpClient _httpClient;

    public ApiStressTests()
    {
        _httpClient = new HttpClient();
    }

    [Stress(order: 1, concurrency: 100, duration: 30000, interval: 1000)]
    public async Task Should_Handle_High_Concurrency()
    {
        var response = await _httpClient.GetAsync("https://api.example.com/health");
        Assert.True(response.IsSuccessStatusCode);
    }

    public async ValueTask DisposeAsync()
    {
        _httpClient?.Dispose();
    }
}
```

Each `[Stress]` attribute defines:

- `order`: the test execution order  
- `concurrency`: number of parallel executions  
- `duration`: how long to run (in milliseconds)  
- `interval`: delay between each wave of execution (in milliseconds)

Run your tests using:

```bash
dotnet test
```

---

## 📖 Documentation

### Getting Started
- [Quick Start Guide](docs/user-guides/getting-started.md)
- [Load Attribute Configuration](docs/user-guides/load-attribute-configuration.md)
- [Writing Effective Tests](docs/user-guides/writing-effective-tests.md)

### Architecture & Design
- [Actor System Overview](docs/architecture/actor-system-overview.md)
- [Hybrid Load Worker Design](docs/architecture/hybrid-load-worker.md)

### User Guides
- [Performance Optimization](docs/user-guides/performance-optimization.md)
- [Monitoring & Metrics](docs/user-guides/monitoring-metrics.md)

### API Reference
- [Load Attributes](docs/api-reference/README.md)
- [Core Classes](docs/api-reference/README.md)
- [Actors](docs/api-reference/README.md)
- [Messages](docs/api-reference/README.md)
- [Models](docs/api-reference/README.md)

### Best Practices
- [Load Test Design](docs/best-practices/load-test-design.md)
- [Resource Management](docs/best-practices/resource-management.md)
- [Troubleshooting](docs/best-practices/troubleshooting.md)

### Advanced Topics
- [Migration from xUnit v2](docs/advanced/migration-guide.md)
- [Custom Extensions](docs/advanced/custom-extensions.md)
- [Performance Tuning](docs/advanced/performance-tuning.md)
- [CI/CD Integration](docs/advanced/cicd-integration.md)

### Examples & Scenarios
- [Database Load Testing](docs/examples/database-load-testing.md)
- [API Load Testing](docs/examples/api-load-testing.md)
- [Transactional Scenarios](docs/examples/transactional-scenarios.md)
- [Real-world Examples](docs/examples/real-world-examples.md)

For comprehensive documentation, visit the [docs folder](docs/).

Examples: [examples](https://github.com/mrviduus/xUnitV3LoadFramework/tree/main/examples/)

---

## 🤝 Contributing

Your contributions and feedback are always welcome!
- Submit issues or suggestions via [GitHub Issues](https://github.com/mrviduus/xUnitV3LoadFramework/issues).
- Open pull requests following our [Contributing Guidelines](CONTRIBUTING.md).

---

## 📜 License

This project is licensed under the [MIT License](LICENSE).

---

## 📫 Contact

For questions, suggestions, or feedback, please open an issue or contact directly:

- **Vasyl Vdovychenko**  
  [LinkedIn](https://www.linkedin.com/in/vasyl-vdovychenko) | [Email](mailto:mrviduus@gmail.com)
