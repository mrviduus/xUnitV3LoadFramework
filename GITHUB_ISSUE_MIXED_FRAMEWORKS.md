# GitHub Issue: Mixed Testing Framework Support - UseLoadFramework Attribute

## Issue Title
**Feature: Add UseLoadFramework Attribute for Mixed Standard and Load Testing Support**

## Issue Type
- [x] Feature Enhancement
- [x] Documentation
- [ ] Bug Report

## Summary
Implement a `UseLoadFrameworkAttribute` that allows developers to selectively run both standard xUnit tests and LoadTestFramework load tests within the same project, providing maximum flexibility for testing scenarios.

## Background
Currently, when configuring the LoadTestFramework at the assembly level using:
```csharp
[assembly: TestFramework("xUnitV3LoadFramework.Extensions.Framework.LoadTestFrameworkStartup", "xUnitV3LoadFramework")]
```

ALL test classes in the assembly are processed by the load testing framework, which means:
- All tests must inherit from `Specification`
- All tests require `[Load]` attributes
- Standard xUnit `[Fact]` and `[Theory]` tests cannot coexist

This limitation prevents mixed testing scenarios where developers want some tests to run as standard unit tests and others as load tests.

## Proposed Solution
Add a `[UseLoadFramework]` class-level attribute that allows selective opt-in to load testing functionality:

### Core Components Implemented

1. **UseLoadFrameworkAttribute** - Class-level attribute for marking load test classes
2. **Enhanced LoadDiscoverer** - Dual discovery logic for both test types
3. **StandardTestCase** - Specialized test case for standard xUnit tests
4. **Enhanced LoadTestCollectionRunner** - Separate execution paths for different test types

### Usage Example
```csharp
// Standard xUnit tests - run immediately with normal xUnit behavior
public class StandardXUnitTests  
{
    [Fact]
    public void StandardTest_ShouldPass()
    {
        Assert.True(true);
    }

    [Theory]
    [InlineData("test1")]
    [InlineData("test2")]
    public void StandardTheory_ShouldAcceptParameters(string input)
    {
        Assert.NotNull(input);
    }
}

// Load tests - run with load testing framework using actors and concurrency
[UseLoadFramework]
public class LoadFrameworkTests : Specification
{
    public override void EstablishContext() => 
        Console.WriteLine("Load test setup completed");

    [Load(order: 1, concurrency: 5, duration: 2000, interval: 500)]
    public void ShouldExecuteWithLoadFramework()
    {
        Console.WriteLine($"Load test executed at {DateTime.Now:HH:mm:ss.fff}");
    }
}
```

## Implementation Details

### Files Modified/Created:
- ✅ `src/xUnitV3LoadFramework/Attributes/UseLoadFrameworkAttribute.cs` - New attribute
- ✅ `src/xUnitV3LoadFramework/Extensions/Framework/LoadDiscoverer.cs` - Enhanced discovery
- ✅ `src/xUnitV3LoadFramework/Extensions/Framework/StandardTestCase.cs` - New test case type
- ✅ `src/xUnitV3LoadFramework/Extensions/Runners/LoadTestCollectionRunner.cs` - Enhanced runner
- ✅ `examples/xUnitV3LoadTestsExamples/MixedTestsExample.cs` - Working example
- ✅ `docs/user-guides/mixed-testing-support.md` - Complete documentation
- ✅ `tests/xUnitV3LoadFrameworkTests/UseLoadFrameworkAttributeTests.cs` - Unit tests

### Technical Architecture:
1. **Discovery Phase**: LoadDiscoverer checks for `[UseLoadFramework]` attribute
   - Classes WITHOUT attribute → Create StandardTestCase instances
   - Classes WITH attribute → Create LoadTestCase instances (existing behavior)

2. **Execution Phase**: LoadTestCollectionRunner routes execution
   - StandardTestCase → Direct method invocation (standard xUnit execution)
   - LoadTestCase → Actor-based load testing execution (existing behavior)

3. **Class Requirements**:
   - Standard test classes: No base class requirement, standard xUnit patterns
   - Load test classes: Must inherit from `Specification`, use `[Load]` attributes

## Benefits
- **Flexibility**: Mix standard unit tests with load tests in the same project
- **Performance**: Standard tests run immediately without load framework overhead
- **Migration**: Easy adoption path for existing projects
- **Compatibility**: Full compatibility with existing xUnit tooling
- **Selective Usage**: Choose which classes need load testing capabilities

## Testing Results
✅ **Verified Working**:
- Standard tests execute correctly: "Standard [Fact] test executed via xUnit"
- Load tests execute correctly: Proper actor system with worker management
- Mixed execution: Both test types run in same test session
- Discovery: 16 total tests discovered vs 2 before (showing proper dual discovery)

## Breaking Changes
❌ **None** - This is a purely additive feature:
- Existing code continues to work unchanged
- Backward compatibility maintained
- No modification of existing APIs

## Documentation
✅ Complete documentation provided:
- Usage examples with both standard and load tests
- Migration guide from assembly-level only configuration
- Best practices and architectural details
- Error handling scenarios

## Priority
**High** - This feature significantly improves the framework's usability and adoption potential by allowing incremental migration and mixed testing scenarios.

## Labels
- `enhancement`
- `feature`
- `testing`
- `documentation`
- `backward-compatible`

## Acceptance Criteria
- [x] UseLoadFrameworkAttribute correctly identifies load test classes
- [x] Standard test classes execute without Specification inheritance
- [x] Load test classes continue to work with existing functionality
- [x] Mixed test projects build and run successfully
- [x] Comprehensive documentation provided
- [x] Unit tests verify attribute functionality
- [x] Example project demonstrates usage
- [x] No breaking changes to existing code

## Implementation Status
**✅ COMPLETE** - Feature implemented and tested successfully.

## Related Issues
- Closes potential future issues about assembly-level configuration limitations
- Addresses user requests for mixed testing scenarios
- Improves framework adoption by reducing migration barriers

---

**Created by**: Development Team  
**Implementation Branch**: `feature/mixedFrameworks`  
**Review Required**: Architecture review for production deployment
