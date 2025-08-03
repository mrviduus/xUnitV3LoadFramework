# xUnitV3LoadFramework Documentation

Welcome to the comprehensive documentation for xUnitV3LoadFramework - a powerful load testing framework that seamlessly integrates with xUnit v3.

## 📚 Documentation Structure

### 🚀 Getting Started
- [Quick Start Guide](user-guides/getting-started.md) - Get up and running in minutes
- [Installation](user-guides/installation.md) - Installation and setup instructions
- [First Load Test](user-guides/first-load-test.md) - Create your first load test

### 📖 User Guides
- [LoadFact Attribute Guide](user-guides/loadfact-attribute-guide.md) - Complete LoadFact usage guide
- [Mixed Testing Support](user-guides/mixed-testing-support.md) - Combining load tests with unit tests
- [Performance Optimization](user-guides/performance-optimization.md) - Tips for optimal performance
- [Monitoring & Metrics](user-guides/monitoring-metrics.md) - Understanding test metrics
- [Writing Effective Tests](user-guides/writing-effective-tests.md) - Best practices and patterns

### 🏗️ Architecture
- [Actor System Overview](architecture/actor-system-overview.md) - Understanding the Akka.NET foundation
- [Hybrid Load Worker](architecture/hybrid-load-worker.md) - Load execution engine details
- [Framework Components](architecture/framework-components.md) - Core component overview

### 📚 API Reference
- [LoadFactAttribute](api-reference/loadfact-attribute.md) - LoadFact attribute reference
- [LoadTestHelper](api-reference/loadtest-helper.md) - Helper methods and utilities
- [Load Results](api-reference/load-results.md) - Understanding test results

### 🔧 Advanced Topics
- [Custom Extensions](advanced/custom-extensions.md) - Extending the framework
- [CI/CD Integration](advanced/cicd-integration.md) - Continuous integration setup
- [Troubleshooting](advanced/troubleshooting.md) - Common issues and solutions
- [Migration Guide](advanced/migration-guide.md) - Migrating from other frameworks

### 💡 Examples
- [Basic Examples](examples/basic-examples.md) - Simple load test examples
- [Real-world Scenarios](examples/real-world-scenarios.md) - Production-ready examples
- [Performance Testing](examples/performance-testing.md) - Advanced performance scenarios

## 🎯 Quick Navigation

| What you want to do | Where to go |
|---------------------|-------------|
| Create your first load test | [Quick Start Guide](user-guides/getting-started.md) |
| Understand LoadFact parameters | [LoadFact Attribute Guide](user-guides/loadfact-attribute-guide.md) |
| See real examples | [Basic Examples](examples/basic-examples.md) |
| Troubleshoot issues | [Troubleshooting](advanced/troubleshooting.md) |
| Understand the architecture | [Actor System Overview](architecture/actor-system-overview.md) |

## 📊 Framework Features

✅ **Seamless xUnit Integration** - Works with existing xUnit v3 test projects  
✅ **Declarative Load Testing** - Simple attribute-based configuration  
✅ **Mixed Testing Support** - Combine load tests with unit and integration tests  
✅ **Rich Metrics** - Comprehensive performance and latency reporting  
✅ **Actor-based Engine** - Powered by Akka.NET for scalability  
✅ **Production Ready** - Battle-tested for enterprise scenarios  

## 🚀 Example Usage

```csharp
[LoadFact(order: 1, concurrency: 5, duration: 3000, interval: 200)]
public async Task LoadTest_API_Endpoint()
{
    var result = await LoadTestHelper.ExecuteLoadTestAsync(async () =>
    {
        var response = await httpClient.GetAsync("/api/users");
        response.EnsureSuccessStatusCode();
        return true;
    });
    
    Assert.True(result.Success > 0);
}
```

## 📞 Support

- **Issues**: [GitHub Issues](https://github.com/mrviduus/xUnitV3LoadFramework/issues)
- **Discussions**: [GitHub Discussions](https://github.com/mrviduus/xUnitV3LoadFramework/discussions)
- **Email**: [mrviduus@gmail.com](mailto:mrviduus@gmail.com)

---

*For the latest updates and releases, visit our [GitHub repository](https://github.com/mrviduus/xUnitV3LoadFramework).*
