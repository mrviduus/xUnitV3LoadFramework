# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

xUnitV3LoadFramework is a load testing framework for .NET that integrates with xUnit v3, using Akka.NET actors for distributed concurrent test execution. The framework allows developers to test applications under load while maintaining clean test code through attributes and fluent APIs.

## Key Commands

### Build and Test
```bash
# Build the solution
dotnet build xUnitV3LoadFramework.sln -c Release

# Run all tests
dotnet test xUnitV3LoadFramework.sln

# Run specific test project
dotnet test tests/xUnitV3LoadFrameworkTests/xUnitV3LoadFrameworkTests.csproj

# Run single test by name
dotnet test --filter "FullyQualifiedName~LoadTestRunnerTests"

# Run examples (see examples/xUnitV3LoadTestsExamples/)
dotnet test examples/xUnitV3LoadTestsExamples/xUnitV3LoadTestsExamples.csproj

# Create NuGet package
dotnet pack src/xUnitV3LoadFramework/xUnitV3LoadFramework.csproj -c Release
```

## High-Level Architecture

### Core Components

1. **LoadAttribute** (`src/xUnitV3LoadFramework/Attributes/LoadAttribute.cs`)
   - Decorates test methods for load testing
   - Properties: Concurrency, Duration, Interval
   - Inherits from xUnit v3's FactAttribute

2. **LoadTestRunner** (`src/xUnitV3LoadFramework/Extensions/LoadTestRunner.cs`)
   - Entry point for test execution
   - Provides both attribute-based (`ExecuteAsync`) and fluent API (`Create()`)
   - Handles test method context extraction

3. **LoadRunner** (`src/xUnitV3LoadFramework/LoadRunnerCore/Runner/LoadRunner.cs`)
   - Orchestrates actor system creation and management
   - Creates LoadWorkerActor and ResultCollectorActor
   - Manages timeout handling (60s buffer + test duration)

### Actor System

The framework uses Akka.NET actors with message-passing architecture:

```
LoadTestSystem (ActorSystem)
├── LoadWorkerActorHybrid (default) or LoadWorkerActor
│   └── Executes test actions concurrently
└── ResultCollectorActor
    └── Aggregates performance metrics
```

**LoadWorkerActorHybrid** (default): High-performance channel-based implementation using fixed thread pools and unbounded channels. Optimal for 100k+ concurrent operations.

**LoadWorkerActor**: Original task-based implementation for backward compatibility.

### Key Message Types

Located in `src/xUnitV3LoadFramework/LoadRunnerCore/Messages/`:
- `StartLoadMessage`: Initiates test execution
- `StepResultMessage`: Reports individual test results
- `BatchCompletedMessage`: Signals batch completion
- `GetLoadResultMessage`: Queries final results

### Configuration Models

**LoadSettings** (`Models/LoadSettings.cs`):
- Concurrency, Duration, Interval
- TerminationMode (Duration/CompleteCurrentInterval/StrictDuration)
- GracefulStopTimeout (auto-calculated: 30% of duration, 5-60s bounds)

**LoadWorkerConfiguration** (`Configuration/LoadWorkerConfiguration.cs`):
- Mode: TaskBased/ActorBased/Hybrid (default)
- Performance tuning parameters

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
    .RunAsync(async () => { /* test */ });
```

### Direct LoadRunner
```csharp
var plan = new LoadExecutionPlan {
    Name = "TestName",
    Settings = new LoadSettings { /* config */ },
    Action = async () => { /* test */ }
};
var result = await LoadRunner.Run(plan);
```

## Important Implementation Details

1. **Actor System Isolation**: Each test creates its own actor system to prevent cross-test pollution.

2. **Timeout Management**: Uses `PipeTo` pattern for async operations with 60-second buffer plus test duration to prevent Akka timeout exceptions.

3. **Graceful Shutdown**: Configurable grace period (30% of duration) for in-flight operations to complete.

4. **Request Count Accuracy**: Three termination modes ensure precise request counts based on requirements.

5. **Performance Optimization**: Hybrid mode prevents thread pool exhaustion using channel-based work distribution.

## Development Notes

- Target Framework: .NET 8.0
- C# Version: 12
- Package Management: Central package versioning via Directory.Packages.props
- Warnings as Errors: Disabled globally, enabled in Release builds for production code
- Key Dependencies: xUnit v3.0.0, Akka.NET 1.5.54, Microsoft.Extensions 9.0.0

### OpenTelemetry Integration

The framework supports OpenTelemetry diagnostics for observability:
- Package: xUnit.OTel 1.0.0.18
- Usage: Add `services.AddOTelDiagnostics()` in test projects
- Provides detailed tracing and metrics during load test execution

## Testing Guidelines

- **No Mocking Frameworks**: Tests use real implementations without mocking libraries (no Moq, NSubstitute, etc.)
- **Assertions**: Use only standard xUnit v3 Assert methods (Assert.True, Assert.Equal, Assert.NotNull, etc.)
- **Integration Testing**: Tests verify actual actor behavior and message passing without mocks
- **Examples**: Comprehensive examples available in `examples/xUnitV3LoadTestsExamples/` directory

The framework emphasizes production readiness with comprehensive error handling, detailed logging, and industry-standard load testing patterns.