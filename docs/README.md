# xUnitV3LoadFramework Documentation

Technical documentation for xUnitV3LoadFramework - a high-performance load testing framework for .NET applications.

##  Documentation Structure

###  Architecture
- [Actor System Overview](architecture/actor-system-overview.md) - Understanding the Akka.NET foundation
- [Hybrid Load Worker](architecture/hybrid-load-worker.md) - Load execution engine details

###  API Reference
- [LoadTestRunner](api-reference/loadtest-runner.md) - Test runner methods and utilities

##  Framework Overview

 **Actor-based Engine** - Powered by Akka.NET for scalability  
 **Declarative Testing** - Attribute-based load test configuration  
 **Comprehensive Metrics** - Performance and latency reporting  
 **Production Ready** - Enterprise-grade architecture  

##  Basic Usage

```csharp
[LoadFact(order: 1, concurrency: 5, duration: 3000, interval: 200)]
public async Task LoadTest_API_Endpoint()
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

---

*For technical specifications and API details, visit our [GitHub repository](https://github.com/mrviduus/xUnitV3LoadFramework).*
