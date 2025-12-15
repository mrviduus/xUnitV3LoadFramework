# xUnitV3LoadFramework

xUnit v3-native load-style test runner for executing actions concurrently over a duration and reporting throughput/success/failure.

[![NuGet](https://img.shields.io/nuget/v/xUnitV3LoadFramework.svg)](https://www.nuget.org/packages/xUnitV3LoadFramework)
[![Downloads](https://img.shields.io/nuget/dt/xUnitV3LoadFramework.svg)](https://www.nuget.org/packages/xUnitV3LoadFramework)
[![xUnit v3](https://img.shields.io/badge/xUnit-v3.0-blue)](https://xunit.net/)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

## What It Is / What It Isn't

**Best for:**
- CI performance smoke tests
- Concurrency and regression checks
- Integration-style load tests inside `dotnet test`
- Quick validation that your API handles N concurrent requests

**Not for:**
- Distributed load generation across machines
- Protocol-specific clients (HTTP/2, gRPC, WebSocket)
- Full observability platform with dashboards

## Install

```bash
dotnet add package xUnitV3LoadFramework
```

## Why xUnit v3 Native?

This framework is a true xUnit v3 extension:
- Runs in `dotnet test` — no extra tooling needed
- Produces test failures visible in IDE and CI
- Supports filtering with `--filter`
- Respects xUnit cancellation
- Test method body becomes the action — no manual runner call needed

## Quickstart

### Native Attribute (Recommended)

The test method body runs automatically under load — no manual `ExecuteAsync()` call needed:

```csharp
using xUnitV3LoadFramework.Attributes;

public class ApiLoadTests
{
    private static readonly HttpClient _httpClient = new();

    [Load(concurrency: 5, duration: 3000, interval: 500)]
    public async Task Api_Should_Handle_Concurrent_Requests()
    {
        // This entire method body runs N times under load
        var response = await _httpClient.GetAsync("https://api.example.com/health");
        response.EnsureSuccessStatusCode();
    }
}
```

Test passes if all iterations complete without exception. Test fails if any iteration throws or returns `false`.

**Supported return types:**
- `async Task` — success if no exception
- `void` — success if no exception
- `Task<bool>` / `ValueTask<bool>` — success if returns `true`
- `bool` — success if returns `true`

### Fluent API

```csharp
using xUnitV3LoadFramework.Extensions;

public class ApiLoadTests
{
    private static readonly HttpClient _httpClient = new();

    [Fact]
    public async Task Api_Load_Test_Fluent()
    {
        var result = await LoadTestRunner.Create()
            .WithName("HealthCheck_Load")
            .WithConcurrency(10)
            .WithDuration(TimeSpan.FromSeconds(5))
            .WithInterval(TimeSpan.FromMilliseconds(200))
            .RunAsync(async () =>
            {
                var response = await _httpClient.GetAsync("https://api.example.com/health");
                response.EnsureSuccessStatusCode();
            });

        Assert.True(result.Success >= result.Total * 0.95);
    }
}
```

### Mixed Testing — All Attributes Work Together

`[Load]`, `[Fact]`, and `[Theory]` can coexist in the same test class:

```csharp
public class ApiTests
{
    private static readonly HttpClient _httpClient = new();

    [Fact]
    public void Should_Have_Valid_BaseUrl()
    {
        Assert.NotNull(_httpClient.BaseAddress);
    }

    [Theory]
    [InlineData("/health")]
    [InlineData("/ready")]
    public async Task Endpoint_Should_Exist(string path)
    {
        var response = await _httpClient.GetAsync(path);
        Assert.True(response.IsSuccessStatusCode);
    }

    [Load(concurrency: 5, duration: 3000, interval: 500)]
    public async Task Api_Should_Handle_Load()
    {
        var response = await _httpClient.GetAsync("/health");
        response.EnsureSuccessStatusCode();
    }
}
```

### Sample Output

**Native `[Load]` test output:**
```
Load Test Results:
  Total: 30, Success: 28, Failure: 2
  RPS: 9.8, Avg: 102ms, P95: 150ms, P99: 180ms
  Result: FAILED (93.3% success rate)
```

**Fluent API test output:**
```
Load test 'HealthCheck_Load' completed:
  Total executions: 50
  Successful executions: 48
  Failed executions: 2
  Execution time: 5.12 seconds
  Requests per second: 9.77
  Average latency: 102.34ms
  Success rate: 96.00%
```

## Core Concepts

| Setting | Description |
|---------|-------------|
| **Concurrency** | Number of concurrent operations launched per interval |
| **Duration** | Total time the load test runs |
| **Interval** | Time between launching batches of concurrent operations |
| **TerminationMode** | How the test stops: `Duration` (immediate), `CompleteCurrentInterval` (finish current batch), or `StrictDuration` (exact timing) |
| **GracefulStopTimeout** | Max time to wait for in-flight requests after duration expires. Default: 30% of duration, bounded 5-60s |
| **Success** | Action returns `true` or completes without exception |
| **Failure** | Action returns `false` or throws an exception |
| **RequestsPerSecond** | `Total / Time` — completed operations per second |

### How Interval Works

Every `Interval`, the framework launches `Concurrency` concurrent operations. For example:
- `Concurrency: 5, Duration: 3s, Interval: 500ms` = 6 batches × 5 operations = ~30 total operations

## Advanced Configuration

### Using LoadExecutionPlan Directly

For full control, use `LoadExecutionPlan` with `LoadRunner.Run()`:

```csharp
using LoadSurge.Models;
using LoadSurge.Runner;

[Fact]
public async Task Advanced_Load_Test()
{
    var plan = new LoadExecutionPlan
    {
        Name = "Database_Connection_Pool",
        Settings = new LoadSettings
        {
            Concurrency = 20,
            Duration = TimeSpan.FromSeconds(30),
            Interval = TimeSpan.FromMilliseconds(100),
            TerminationMode = TerminationMode.CompleteCurrentInterval,
            GracefulStopTimeout = TimeSpan.FromSeconds(10)
        },
        Action = async () =>
        {
            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();
            return true;
        }
    };

    var result = await LoadRunner.Run(plan);

    Assert.True(result.Success >= result.Total * 0.99);
}
```

### Termination Modes

| Mode | Behavior |
|------|----------|
| `Duration` | Stops immediately when duration expires (default) |
| `CompleteCurrentInterval` | Waits for current batch to finish before stopping |
| `StrictDuration` | Strict timing — may cut off final batch |

### MaxIterations (Fluent API)

Stop after a fixed number of operations regardless of duration:

```csharp
var result = await LoadTestRunner.Create()
    .WithConcurrency(10)
    .WithDuration(TimeSpan.FromMinutes(5))
    .WithMaxIterations(1000) // Stop after 1000 operations
    .RunAsync(async () => { /* ... */ });
```

## Assertions / CI Gating

Use `LoadResult` fields to fail tests based on performance criteria:

```csharp
var result = await LoadTestRunner.Create()
    .WithConcurrency(10)
    .WithDuration(TimeSpan.FromSeconds(10))
    .RunAsync(async () => { /* ... */ });

// Success rate gate
var successRate = (double)result.Success / result.Total;
Assert.True(successRate >= 0.99, $"Success rate {successRate:P} below 99%");

// Throughput gate
Assert.True(result.RequestsPerSecond >= 50, $"RPS {result.RequestsPerSecond} below 50");

// Latency gate
Assert.True(result.Percentile95Latency < 500, $"P95 latency {result.Percentile95Latency}ms exceeds 500ms");
Assert.True(result.AverageLatency < 200, $"Avg latency {result.AverageLatency}ms exceeds 200ms");
```

### Available Result Fields

- `Total`, `Success`, `Failure` — counts
- `Time` — execution time in seconds
- `RequestsPerSecond` — throughput
- `AverageLatency`, `MinLatency`, `MaxLatency` — in milliseconds
- `MedianLatency`, `Percentile95Latency`, `Percentile99Latency` — percentiles in ms
- `PeakMemoryUsage` — bytes

## How Failures Work

The framework runs **all iterations to completion** before determining pass/fail:

1. All iterations execute regardless of individual failures
2. Exceptions are caught and counted as failures (not thrown)
3. At the end, a report shows Total/Success/Failure counts
4. Test is marked **FAILED** if `Failure > 0`

This means you always get complete metrics, even when some iterations fail.

**Native `[Load]` tests**: Pass/fail is automatic based on iteration results.

**Fluent API tests**: You control pass/fail with assertions:
```csharp
var result = await LoadTestRunner.Create()
    .WithConcurrency(10)
    .WithDuration(TimeSpan.FromSeconds(5))
    .RunAsync(async () => { /* ... */ });

// Allow up to 5% failure rate
var successRate = (double)result.Success / result.Total;
Assert.True(successRate >= 0.95, $"Success rate {successRate:P} below 95%");
```

## Safety & Gotchas

- **Thread-safety**: Your action runs concurrently. Avoid shared mutable state unless protected.
- **Reuse HttpClient**: Create a single `static readonly HttpClient` — don't instantiate per request.
- **Start low**: Begin with low concurrency and short duration. Increase gradually.
- **Timeouts**: Add your own timeout in the action. The framework won't kill hung operations:
  ```csharp
  using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
  await httpClient.GetAsync(url, cts.Token);
  ```
- **Cancellation**: The framework uses `GracefulStopTimeout` to wait for in-flight requests. Long-running actions without cancellation support may delay test completion.
- **CI filtering**: Use `dotnet test --filter "FullyQualifiedName~LoadTests"` to run only load tests or exclude them from fast CI runs.

## When to Choose This

Use this framework when:
- You want load tests as part of `dotnet test` without extra tooling
- You need quick concurrency smoke tests in CI
- Your tests are integration-style (HTTP calls, database queries)
- You want xUnit v3 native attributes and test discovery

Use a dedicated tool when:
- You need distributed load from multiple machines
- You need protocol-specific features (HTTP/2 multiplexing, WebSocket)
- You need real-time dashboards and detailed analytics

## Fun Explanation (optional)

Think of this like a playground stress test. You set:
- How many kids play at once (`Concurrency`)
- How long the playground is open (`Duration`)
- How often new groups arrive (`Interval`)

The framework tells you how many kids had fun (success), how many fell off the swings (failure), and how fast the line moved (RPS).

## Requirements

- .NET 8.0+ or .NET Framework 4.7.2+ (netstandard2.0)
- xUnit v3

## Contributing

PRs welcome. [Open an issue](https://github.com/mrviduus/xUnitV3LoadFramework/issues) for bugs or feature requests.

---

Made by [Vasyl](https://github.com/mrviduus)
