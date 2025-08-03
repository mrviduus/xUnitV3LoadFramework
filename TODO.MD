### TODO (Post v2.0.0)
- [ ] Performance benchmarking suite
- [ ] Advanced error handling scenarios
- [ ] Write xUnit test to simulate high-load (10k+ requests) scenario, assert >95% completion, log planned vs actual executions, and verify `LoadWorkerActor` emits tasks at correct rate under configured concurrency/interval
- [ ] Database load testing examples
- [ ] WebSocket load testing support
- [ ] Distributed load testing capabilities

### In Progress
- [ ] Create more unit tests for edge cases
- [ ] Create more integration tests [api, db, web sockets]
- [ ] Advanced metrics and reporting features

### Done  (Version 2.0.0 - Released 2025-08-03)
- [x] **Documentation** - Complete technical documentation and user guides
- [x] **Clean test all tests should pass** - All framework tests passing (30 succeeded, 4 intentional failures, 1 skipped)
- [x] **Version 2.0.0 Release Preparation** - Updated versions, changelog, package metadata
- [x] **Repository URL Fixes** - Corrected all package URLs to match current repository
- [x] **Package Tags and Release Notes** - Added comprehensive NuGet package metadata
- [x] Create xUnit Attribute for test
- [x] Add Time start Time duration and Time End to Report
- [x] Create build for nuget (framework should be connected when nuget was setuped)
- [x] Create pipeline
- [x] **xUnit v3 3.0.0 Compatibility** - Updated framework to support xUnit v3 3.0.0 breaking changes:
  - Added CallerFilePath/CallerLineNumber support to LoadAttribute for source location information
  - Implemented TestMethodArity property in LoadTestMethod and LoadTestCase classes
  - Fixed source information extraction in LoadDiscoverer using reflection
  - Updated StandardTestCase to support new source location parameters
  - Removed UseLoadFramework attribute requirement - tests with [Load] attribute automatically use load framework
  - All framework tests passing (30 succeeded, 4 intentional failures, 1 skipped)
- [x] **Fluent API Implementation** - Added LoadTestRunner.Create() fluent interface
- [x] **Actor System Enhancement** - Improved Akka.NET integration and performance
- [x] **Comprehensive Examples** - Added multiple usage patterns and examples

### Issues (Resolved in v2.0.0)
- ✅ All major issues resolved for production release