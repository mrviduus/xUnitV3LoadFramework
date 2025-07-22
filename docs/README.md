# xUnitV3LoadFramework Documentation

A high-performance load testing framework built on xUnit v3 and Akka.NET, featuring hybrid channel-based and task-based execution models.

## 📚 Documentation Index

### Getting Started
- [Quick Start Guide](user-guides/quickstart.md)
- [Installation](user-guides/installation.md)
- [Basic Usage](user-guides/basic-usage.md)

### Architecture & Design
- [Framework Architecture](architecture/framework-architecture.md)
- [Hybrid Load Worker Design](architecture/hybrid-load-worker.md)
- [Actor System Overview](architecture/actor-system.md)
- [Message Flow](architecture/message-flow.md)

### User Guides
- [Load Attribute Configuration](guides/load-attribute.md)
- [Writing Load Tests](guides/writing-load-tests.md)
- [Performance Optimization](guides/performance-optimization.md)
- [Monitoring & Metrics](guides/monitoring-metrics.md)

### API Reference
- [Load Attributes](api/load-attributes.md)
- [Core Classes](api/core-classes.md)
- [Actors](api/actors.md)
- [Messages](api/messages.md)
- [Models](api/models.md)

### Best Practices
- [Load Test Design](best-practices/load-test-design.md)
- [Resource Management](best-practices/resource-management.md)
- [Troubleshooting](best-practices/troubleshooting.md)

### Advanced Topics
- [Migration from xUnit v2](advanced/migration-guide.md)
- [Custom Extensions](advanced/custom-extensions.md)
- [Performance Tuning](advanced/performance-tuning.md)
- [CI/CD Integration](advanced/cicd-integration.md)

### Examples & Scenarios
- [Database Load Testing](examples/database-load-testing.md)
- [API Load Testing](examples/api-load-testing.md)
- [Transactional Scenarios](examples/transactional-scenarios.md)
- [Real-world Examples](examples/real-world-examples.md)

## 🚀 Key Features

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

## 📊 Performance Highlights

- **High Throughput**: Tested up to 500,000 requests with sustained performance
- **Low Latency**: Sub-millisecond overhead for test execution framework
- **Resource Efficient**: Optimized memory usage and GC pressure management
- **Scalable**: Automatic scaling based on available system resources

## 🔧 System Requirements

- .NET 8.0+
- xUnit v3 (preview)
- Akka.NET 1.5.41+
- Minimum 4GB RAM for high-load scenarios
- Multi-core CPU recommended for optimal performance

## 💡 Quick Example

```csharp
public class ApiLoadTests : Specification
{
    [Load(order: 1, concurrency: 100, duration: 30000, interval: 1000)]
    public async Task<bool> When_testing_api_endpoint()
    {
        using var client = new HttpClient();
        var response = await client.GetAsync("https://api.example.com/health");
        return response.IsSuccessStatusCode;
    }
}
```

## 🤝 Contributing

We welcome contributions! Please see our [Contributing Guide](CONTRIBUTING.md) for details.

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](../LICENSE) file for details.
