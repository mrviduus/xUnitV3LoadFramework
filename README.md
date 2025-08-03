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

---

## 📊 Performance Highlights

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

### Basic Load Test Example

Here's a clear example demonstrating how to define and execute load tests using the `Specification` base class and the `[Load]` attribute:

```csharp
using xUnitV3LoadFramework.Attributes;
using xUnitV3LoadFramework.Extensions;
using System;

namespace xUnitLoadDemo;

public class ExampleLoadSpecification : Specification
{
    protected override void EstablishContext()
    {
        Console.WriteLine(">> Setup phase");
    }

    protected override void Because()
    {
        Console.WriteLine(">> Action phase");
    }

    [Load(order: 1, concurrency: 2, duration: 5000, interval: 500)]
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

### API Load Testing Example

```csharp
using xUnitV3LoadFramework.Attributes;
using xUnitV3LoadFramework.Extensions;

public class ApiLoadTests : Specification
{
    private HttpClient _httpClient;

    protected override void EstablishContext()
    {
        _httpClient = new HttpClient();
    }

    protected override void Because()
    {
        // Common setup for all load tests
    }

    [Load(order: 1, concurrency: 100, duration: 30000, interval: 1000)]
    public async Task When_testing_api_endpoint()
    {
        var response = await _httpClient.GetAsync("https://api.example.com/health");
        Assert.True(response.IsSuccessStatusCode);
    }

    protected override void DestroyContext()
    {
        _httpClient?.Dispose();
    }
}
```

Each `[Load]` attribute defines:

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
