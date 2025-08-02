# Complete Migration Plan to xUnitV3LoadFramework v2

## 🔍 Current State Analysis (January 2025)

### ✅ **Already Implemented**
- `StressAttribute` inherits from `FactAttribute` - ✅ COMPLETE
- `UseStressFrameworkAttribute` class attribute - ✅ COMPLETE  
- Backward compatibility with deprecated `LoadAttribute` and `UseLoadFrameworkAttribute` - ✅ COMPLETE
- Basic framework infrastructure (LoadDiscoverer, LoadExecutor, etc.) - ✅ WORKING
- Actor system for performance testing - ✅ WORKING
- Example projects structure - ✅ WORKING

### ❌ **Issues to Fix**
1. **Build Issues**: .NET 8.0 ASP.NET Core dependency problems
2. **Specification Pattern**: Still partially referenced in some areas
3. **Documentation**: Needs updating for v2 patterns
4. **Examples**: Need migration to v2 syntax
5. **Framework Naming**: Still uses "Load" prefix in many places

---

## 📋 **Phase-by-Phase Migration Plan**

## Phase 1: Critical Fixes (Week 1) 🚨

### 1.1 Fix Build and Test Issues
**Priority: CRITICAL**

- [x] Fix missing Specification references in tests
- [ ] Resolve .NET version compatibility issues
- [ ] Ensure all tests pass
- [ ] Fix ASP.NET Core dependency issues

### 1.2 Update Framework Discovery
**Priority: HIGH**

The framework needs to handle both `LoadAttribute` and `StressAttribute` during the transition:

```csharp
// In LoadDiscoverer.cs - Update attribute detection
var stressAttribute = testMethod.Method.GetCustomAttributes<StressAttribute>().FirstOrDefault();
var loadAttribute = testMethod.Method.GetCustomAttributes<LoadAttribute>().FirstOrDefault(); // backward compatibility

var attribute = stressAttribute ?? loadAttribute;
if (attribute is null) return true;
```

### 1.3 Update Examples to V2 Pattern
**Priority: HIGH**

Convert examples from:
```csharp
// OLD v1 Pattern
[UseLoadFramework]
public class MyTests : Specification
{
    protected override void EstablishContext() { /* setup */ }
    protected override void Because() { /* action */ }
    
    [Load(order: 1, concurrency: 10, duration: 5000)]
    public void Should_Handle_Load() { }
}
```

To:
```csharp
// NEW v2 Pattern  
[UseStressFramework]
public class MyTests
{
    public MyTests() { /* setup in constructor */ }
    
    [Stress(order: 1, concurrency: 10, duration: 5000)]
    public async Task Should_Handle_Stress()
    {
        // Direct test implementation
        await SomeApiCall();
        Assert.True(result.IsSuccess);
    }
}
```

---

## Phase 2: Complete Specification Removal (Week 2)

### 2.1 Framework Integration Updates
**Priority: HIGH**

Update the framework to work without `Specification` base class:

1. **Update LoadDiscoverer.cs**:
   - Remove requirement for `Specification` inheritance
   - Handle plain class instances
   - Support standard xUnit lifecycle

2. **Update LoadRunner.cs**:
   - Remove calls to `OnStart()` and `OnFinish()`
   - Use direct method invocation
   - Support async test methods natively

3. **Update Test Execution**:
   - Integrate with xUnit's standard test execution
   - Support constructor injection
   - Support `IDisposable` and `IAsyncDisposable`

### 2.2 Enhanced xUnit v3 Compatibility
**Priority: MEDIUM**

Enable mixed test scenarios:
```csharp
public class MixedTestClass
{
    [Fact]
    public void Standard_Unit_Test() 
    {
        Assert.True(true);
    }
    
    [Theory]
    [InlineData(1, 2)]
    public void Parameterized_Test(int a, int b) 
    {
        Assert.Equal(3, a + b);
    }
    
    [Stress(concurrency: 50, duration: 10000)]
    public async Task Stress_Test() 
    {
        await SomeApiCall();
        Assert.True(result.IsSuccess);
    }
}
```

---

## Phase 3: Framework Renaming (Week 3)

### 3.1 Core Framework Classes
**Priority: MEDIUM**

Rename while maintaining backward compatibility:

```csharp
// New primary classes
public class StressTestFramework : TestFramework { }
public class StressDiscoverer : TestFrameworkDiscoverer<StressTestClass> { }
public class StressExecutor : TestFrameworkExecutor<ITestCase> { }

// Backward compatibility aliases (marked obsolete)
[Obsolete("Use StressTestFramework")]
public class LoadTestFramework : StressTestFramework { }

[Obsolete("Use StressDiscoverer")]
public class LoadDiscoverer : StressDiscoverer { }
```

### 3.2 Object Model Updates
**Priority: MEDIUM**

```csharp
// New object model
public class StressTestAssembly : ITestAssembly { }
public class StressTestCase : ITestCase { }
public class StressTestClass : ITestClass { }

// Backward compatibility
[Obsolete("Use StressTestAssembly")]
public class LoadTestAssembly : StressTestAssembly { }
```

---

## Phase 4: Actor System Updates (Week 4)

### 4.1 Actor and Message Renaming
**Priority: LOW**

```csharp
// New actor system
public class StressWorkerActor : ReceiveActor { }
public class StressWorkerActorHybrid : ReceiveActor { }

// New messages  
public class StartStressMessage { }
public class GetStressResultMessage { }

// New models
public class StressExecutionPlan { }
public class StressSettings { }
public class StressResult { }
```

---

## Phase 5: Documentation & Examples (Week 5)

### 5.1 Update Documentation
**Priority: HIGH**

1. **README.md** - Update with v2 syntax and examples
2. **Getting Started Guide** - Show v2 patterns
3. **Migration Guide** - v1 to v2 conversion examples
4. **API Reference** - Update all Load* → Stress* references

### 5.2 Update Example Projects
**Priority: HIGH**

1. **xUnitV3LoadTestsExamples** - Convert to v2 patterns
2. **ResultAnalysisExample.cs** - Update to show v2 usage
3. **V2Examples.cs** - Comprehensive v2 examples
4. **Database examples** - Without Specification pattern
5. **Web API examples** - Standard xUnit patterns

---

## 🎯 **Implementation Priority Order**

### Week 1: Foundation Fixes
1. ✅ Fix Specification references in tests (DONE)
2. 🔧 Fix .NET/ASP.NET Core dependency issues
3. 🔧 Ensure build and tests pass
4. 🔧 Update framework to handle both Load and Stress attributes

### Week 2: Pattern Migration  
1. 🔧 Remove Specification pattern dependency completely
2. 🔧 Enable plain class stress testing
3. 🔧 Update examples to v2 patterns
4. 🔧 Comprehensive testing of mixed scenarios

### Week 3: Framework Renaming
1. 🔧 Rename core framework classes
2. 🔧 Maintain backward compatibility
3. 🔧 Update internal references

### Week 4: Polish & Performance
1. 🔧 Actor system updates
2. 🔧 Performance validation
3. 🔧 Memory usage optimization

### Week 5: Documentation & Release
1. 🔧 Complete documentation updates  
2. 🔧 Example project migration
3. 🔧 Release preparation

---

## 🚀 **Key V2 Features**

### 1. **Direct xUnit v3 Integration**
```csharp
[UseStressFramework]
public class WebApiTests
{
    private readonly HttpClient _client;
    
    public WebApiTests()
    {
        _client = new HttpClient();
    }
    
    [Stress(concurrency: 100, duration: 30000)]
    public async Task Should_Handle_Concurrent_Requests()
    {
        var response = await _client.GetAsync("/api/users");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
    
    public void Dispose() => _client?.Dispose();
}
```

### 2. **Mixed Test Support**
```csharp
public class MixedApiTests
{
    [Fact]
    public void Unit_Test_User_Creation()
    {
        var user = new User { Name = "Test" };
        Assert.NotNull(user.Name);
    }
    
    [Stress(concurrency: 50, duration: 15000)]
    public async Task Stress_Test_User_Api()
    {
        // Stress test implementation
        var response = await CreateUser();
        Assert.True(response.IsSuccess);
    }
}
```

### 3. **Full Async Support**
```csharp
[Stress(concurrency: 200, duration: 60000)]
public async Task Should_Handle_Async_Database_Operations()
{
    using var context = new ApplicationDbContext();
    var user = await context.Users.FirstOrDefaultAsync();
    Assert.NotNull(user);
}
```

### 4. **Standard xUnit Features**
- Constructor injection
- IDisposable/IAsyncDisposable support
- IClassFixture and ICollectionFixture support
- Theory and InlineData support alongside Stress tests
- TestContext.Current.CancellationToken support

---

## 🎯 **Success Criteria**

- [x] ✅ Project builds successfully
- [ ] 🔧 All tests pass
- [ ] 🔧 Backward compatibility maintained for existing users
- [ ] 🔧 New v2 patterns work seamlessly with xUnit v3
- [ ] 🔧 Performance equals or exceeds v1
- [ ] 🔧 Documentation is complete and accurate
- [ ] 🔧 Migration path is clear and well-documented

---

## 🔧 **Next Immediate Steps**

1. **Fix Build Issues** - Resolve ASP.NET Core dependency problems
2. **Update Framework Discovery** - Handle both Load and Stress attributes  
3. **Create V2 Examples** - Show the new patterns in action
4. **Remove Specification Dependency** - Enable plain class testing
5. **Test Mixed Scenarios** - Ensure Fact, Theory, and Stress work together

This migration will make xUnitV3LoadFramework fully compatible with standard xUnit v3 while maintaining all performance testing capabilities and providing a smooth upgrade path for existing users.
