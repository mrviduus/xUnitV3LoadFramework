# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

xUnitV3LoadFramework is a load testing framework for .NET that integrates with xUnit v3. It wraps LoadSurge (Akka.NET-based actor system) with xUnit-specific attributes and fluent APIs for easy load test authoring.

## Key Commands

```bash
# Build
dotnet build xUnitV3LoadFramework.sln -c Release

# Run all tests
dotnet test xUnitV3LoadFramework.sln

# Run single test by name
dotnet test --filter "FullyQualifiedName~LoadIntegrationTests"

# Run examples
dotnet test examples/xUnitV3LoadTestsExamples/xUnitV3LoadTestsExamples.csproj

# Create NuGet package
dotnet pack src/xUnitV3LoadFramework/xUnitV3LoadFramework.csproj -c Release
```

## Architecture

### This Package (xUnitV3LoadFramework)

4 source files in two categories:

**Attributes** (`src/xUnitV3LoadFramework/Attributes/`)
- `LoadAttribute.cs` - Decorates test methods with load test params (Concurrency, Duration, Interval). Inherits from xUnit v3's `FactAttribute` and registers `LoadTestCaseDiscoverer`.

**Discovery** (`src/xUnitV3LoadFramework/Discovery/`)
- `LoadTestCaseDiscoverer.cs` - Implements `IXunitTestCaseDiscoverer` to discover `[Load]`-decorated methods
- `LoadTestCase.cs` - Implements `ISelfExecutingXunitTestCase` for native xUnit v3 execution. The test method body becomes the action executed under load.

**Extensions** (`src/xUnitV3LoadFramework/Extensions/`)
- `LoadTestRunner.cs` - Fluent API builder (`Create()`) and legacy `ExecuteAsync()` for manual execution

### xUnit v3 Native Execution Flow

1. `LoadTestCaseDiscoverer` discovers `[Load]`-decorated methods
2. Creates `LoadTestCase` with Concurrency/Duration/Interval from attribute
3. `LoadTestCase.Run()` creates test class instance, wraps test method as `Func<Task<bool>>` action
4. Delegates to `LoadSurge.LoadRunner.Run()` with the action
5. Reports pass/fail via xUnit messaging (fail if any iteration throws)

### External Dependency: LoadSurge

Core load testing engine (separate NuGet package). Contains:
- `LoadRunner`: Orchestrates Akka.NET actor system
- `LoadSettings`: Concurrency, Duration, Interval, TerminationMode, GracefulStopTimeout
- `LoadExecutionPlan`: Test configuration + action
- `LoadResult`: Metrics (Total, Success, Failure, latencies, RPS)

## Test Patterns

### Native Attribute (Recommended)
```csharp
[Load(concurrency: 5, duration: 3000, interval: 500)]
public async Task MyLoadTest()
{
    // Test method body runs under load - no manual call needed
    var response = await _httpClient.GetAsync("https://api.example.com/health");
    response.EnsureSuccessStatusCode();
}
```

**Supported return types:** `async Task`, `void`, `Task<bool>`, `ValueTask<bool>`, `bool`

### Fluent API (when you need metrics)
```csharp
[Fact]
public async Task LoadTest_With_Assertions()
{
    var result = await LoadTestRunner.Create()
        .WithConcurrency(10)
        .WithDuration(TimeSpan.FromSeconds(5))
        .WithMaxIterations(1000) // optional: stop after N iterations
        .RunAsync(async () => { /* test */ });

    // Assert on metrics
    Assert.True(result.Success >= result.Total * 0.95);
}
```

### Mixed Testing
`[Load]`, `[Fact]`, and `[Theory]` coexist in same class:
```csharp
public class ApiTests
{
    [Fact]
    public void UnitTest() { /* ... */ }

    [Theory]
    [InlineData("/health")]
    public async Task TheoryTest(string path) { /* ... */ }

    [Load(concurrency: 5, duration: 3000, interval: 500)]
    public async Task LoadTest() { /* ... */ }
}
```

### Direct LoadSurge
```csharp
var plan = new LoadExecutionPlan {
    Name = "TestName",
    Settings = new LoadSettings { /* config */ },
    Action = async () => { /* test */ }
};
var result = await LoadRunner.Run(plan);
```

## Failure Behavior

- All iterations run to completion (never stops early)
- Exceptions are caught and counted as failures
- Test report shows Total/Success/Failure at the end
- Native `[Load]` tests: FAIL if any iteration fails
- Fluent API tests: You control pass/fail with assertions

## Development Notes

- Target: .NET 8.0, C# 12
- Package Management: Central versioning via Directory.Packages.props
- Key Dependencies: xUnit v3.2.1, LoadSurge (includes Akka.NET)
- Warnings as Errors: Only in Release builds

## Testing Guidelines

- **No Mocking**: Tests use real implementations
- **Assertions**: Standard xUnit Assert methods only
- **Examples**: See `examples/xUnitV3LoadTestsExamples/`