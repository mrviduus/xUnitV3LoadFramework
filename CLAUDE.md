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

Only 2 source files:

1. **LoadAttribute** (`src/xUnitV3LoadFramework/Attributes/LoadAttribute.cs`)
   - Decorates test methods with load test params (Concurrency, Duration, Interval)
   - Inherits from xUnit v3's FactAttribute

2. **LoadTestRunner** (`src/xUnitV3LoadFramework/Extensions/LoadTestRunner.cs`)
   - Entry point: `ExecuteAsync()` for attribute-based, `Create()` for fluent API
   - Extracts LoadAttribute from calling method via reflection
   - Delegates to LoadSurge's `LoadRunner.Run()`

### External Dependency: LoadSurge

Core load testing engine (separate NuGet package). Contains:
- `LoadRunner`: Orchestrates Akka.NET actor system
- `LoadSettings`: Concurrency, Duration, Interval, TerminationMode, GracefulStopTimeout
- `LoadExecutionPlan`: Test configuration + action
- `LoadResult`: Metrics (Total, Success, Failure, latencies, RPS)

## Test Patterns

### Attribute-Based
```csharp
[Load(concurrency: 5, duration: 3000, interval: 500)]
public async Task MyLoadTest()
{
    var result = await LoadTestRunner.ExecuteAsync(async () => {
        // Test action
        return true;
    });
}
```

### Fluent API
```csharp
var result = await LoadTestRunner.Create()
    .WithConcurrency(10)
    .WithDuration(TimeSpan.FromSeconds(5))
    .WithMaxIterations(1000) // optional: stop after N iterations
    .RunAsync(async () => { /* test */ });
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

## Development Notes

- Target: .NET 8.0, C# 12
- Package Management: Central versioning via Directory.Packages.props
- Key Dependencies: xUnit v3.2.1, LoadSurge (includes Akka.NET)
- Warnings as Errors: Only in Release builds

## Testing Guidelines

- **No Mocking**: Tests use real implementations
- **Assertions**: Standard xUnit Assert methods only
- **Examples**: See `examples/xUnitV3LoadTestsExamples/`