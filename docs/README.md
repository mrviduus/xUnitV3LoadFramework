# xUnitV3LoadFramework Documentation

Technical documentation for xUnitV3LoadFramework - a high-performance load testing framework for .NET applications.

##  Documentation Structure

###  Architecture
- [Actor System Overview](architecture/actor-system-overview.md) - Understanding the Akka.NET foundation
- [Hybrid Load Worker](architecture/hybrid-load-worker.md) - Load execution engine details

##  Framework Overview

 **Actor-based Engine** - Powered by Akka.NET for scalability  
 **Declarative Testing** - Attribute-based load test configuration  
 **Comprehensive Metrics** - Performance and latency reporting  
 **Production Ready** - Enterprise-grade architecture  

##  Basic Usage

### Traditional Approach (with Load attribute)
```csharp
public class MyLoadTests
{
    private readonly HttpClient httpClient = new HttpClient();

    [Load(order: 1, concurrency: 5, duration: 3000, interval: 200)]
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
}
```

### Fluent API Approach (no Load attribute needed)
```csharp
public class MyFluentTests
{
    private readonly HttpClient httpClient = new HttpClient();

    [Fact]
    public async Task LoadTest_With_Fluent_API()
    {
        var result = await LoadTestRunner.Create()
            .WithConcurrency(5)
            .WithDuration(3000)
            .WithInterval(200)
            .WithName("API_Endpoint_Test")
            .RunAsync(async () =>
            {
                var response = await httpClient.GetAsync("/api/users");
                response.EnsureSuccessStatusCode();
            });
        
        Assert.True(result.Success > 0);
    }
}
```

---

*For technical specifications and API details, visit our [GitHub repository](https://github.com/mrviduus/xUnitV3LoadFramework).*
