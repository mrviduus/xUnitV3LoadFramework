# xUnitV3LoadFramework 

**Make your tests super fast! 🚀**

Think of this like having many people test your website at the same time to see if it can handle lots of visitors - just like when everyone tries to buy concert tickets at once!

[![NuGet](https://img.shields.io/nuget/v/xUnitV3LoadFramework.svg)](https://www.nuget.org/packages/xUnitV3LoadFramework)
[![Downloads](https://img.shields.io/nuget/dt/xUnitV3LoadFramework.svg)](https://www.nuget.org/packages/xUnitV3LoadFramework)
[![.NET 8](https://img.shields.io/badge/.NET-8.0-purple)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![xUnit v3](https://img.shields.io/badge/xUnit-v3.0-blue)](https://xunit.net/)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

## What does it do?

* **Easy to use** - Just add one line to your test and it becomes a speed test! 
* **Many tests at once** - Like having 100 people click your button at the same time
* **Shows you numbers** - Tells you how fast your app is and if anything breaks
* **Works with xUnit** - Uses the tests you already know how to write

## Quick Start

### 1. Install it
```bash
dotnet add package xUnitV3LoadFramework
```

### 2. Write a simple test

**Method 1: Using an attribute (like a sticker on your test)**
```csharp
using xUnitV3LoadFramework.Attributes;
using xUnitV3LoadFramework.Extensions;

public class MyTests
{
    private readonly HttpClient _httpClient = new HttpClient();

    [Load(concurrency: 5, duration: 3000, interval: 500)] // 5 people testing for 3 seconds
    public async Task Test_My_Website()
    {
        var result = await LoadTestRunner.ExecuteAsync(async () =>
        {
            var response = await _httpClient.GetAsync("https://api.example.com/data");
            response.EnsureSuccessStatusCode();
            return true; // Return true for success
        });
        
        Assert.True(result.Success > 0, "Should have successful executions");
    }
}
```

**Method 2: Using fluent API (like building with blocks)**
```csharp
using xUnitV3LoadFramework.Extensions;

public class MyFluentTests
{
    private readonly HttpClient _httpClient = new HttpClient();

    [Fact]
    public async Task Test_With_Fluent_API()
    {
        var result = await LoadTestRunner.Create()
            .WithName("My_Cool_Test")
            .WithConcurrency(10)        // 10 people
            .WithDuration(5000)         // for 5 seconds  
            .WithInterval(200)          // pause 200ms between batches
            .RunAsync(async () =>
            {
                var response = await _httpClient.GetAsync("https://api.example.com");
                response.EnsureSuccessStatusCode();
                // No need to return anything - success is assumed if no exception
            });

        Assert.True(result.Success > 0, "Should have successful executions");
        Console.WriteLine($"Success rate: {result.Success}/{result.Total}");
    }
}
```

### 3. See the results
```
✅ Test Results:
   Total tests: 50
   Successful: 48
   Failed: 2
   Speed: 16 tests per second
   Time taken: 3.2 seconds
```

## 🤔 Which method should I use?

**Use Method 1 (Load attribute)** when:
- You want the framework to automatically discover and run your load tests
- You prefer attribute-based configuration

**Use Method 2 (Fluent API)** when:
- You want more control over when and how the load test runs
- You prefer explicit configuration in your test code
- You want to mix regular xUnit tests with load tests in the same class

### 💡 More Examples

**Mixed testing (both regular and load tests):**
```csharp
public class MixedTests
{
    private readonly HttpClient _httpClient = new HttpClient();

    [Fact] // Regular xUnit test
    public async Task Regular_Test()
    {
        var response = await _httpClient.GetAsync("https://api.example.com");
        Assert.True(response.IsSuccessStatusCode);
    }

    [Fact] // Load test using fluent API
    public async Task Load_Test_Using_Fluent_API()
    {
        var result = await LoadTestRunner.Create()
            .WithConcurrency(5)
            .WithDuration(2000)
            .RunAsync(async () =>
            {
                var response = await _httpClient.GetAsync("https://api.example.com");
                response.EnsureSuccessStatusCode();
            });

        Assert.True(result.Success > 0);
    }
}
```

**Advanced load testing with explicit success/failure:**
```csharp
[Fact]
public async Task Advanced_Load_Test_With_Custom_Success_Logic()
{
    var result = await LoadTestRunner.Create()
        .WithName("Advanced_API_Test")
        .WithConcurrency(10)
        .WithDuration(5000)
        .WithInterval(100)
        .RunAsync(async () =>
        {
            var response = await _httpClient.GetAsync("https://api.example.com/data");
            
            if (!response.IsSuccessStatusCode) 
                return false;
                
            var content = await response.Content.ReadAsStringAsync();
            return !string.IsNullOrEmpty(content); // Custom success logic
        });

    Assert.True(result.Success > result.Total * 0.8, "Should have 80%+ success rate");
    Console.WriteLine($"Achieved {result.RequestsPerSecond:F2} requests per second");
}
```

## Want to see what's happening? Use logs! 📝

Add this to see detailed logs with xUnit.OTel:
```csharp
// Add to your test project
services.AddOTelDiagnostics();
```

This shows you exactly what your tests are doing, like a diary of your test!

## What the numbers mean

- **concurrency**: How many people test at the same time (like 5 friends)
- **duration**: How long the test runs (like counting to 3000)
- **Success/Failed**: How many tests worked vs broke
- **Speed**: How fast your app can handle requests

## Requirements

- **.NET 8.0** or newer
- **xUnit v3** for testing

## Need help?

- � **Found a bug?**: [Tell us here](https://github.com/mrviduus/xUnitV3LoadFramework/issues)
xUnitV3LoadFramework/discussions)
- 📧 **Email**: [mrviduus@gmail.com](mailto:mrviduus@gmail.com)

---

**Made with ❤️ by [Vasyl](https://github.com/mrviduus) to help make apps faster!**
