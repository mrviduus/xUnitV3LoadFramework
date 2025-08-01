### TODO
- [ ] Documentation
- [ ] Clean test all tests should pass
- [ ] Error handlers 
- [ ] Write xUnit test to simulate high-load (10k+ requests) scenario, assert >95% completion, log planned vs actual executions, and verify `LoadWorkerActor` emits tasks at correct rate under configured concurrency/interval.
- [ ] Fix all tests
### In progress
- [ ] Branch policy
- [ ] Create more unit tests
- [ ] Create more integration tests [api, db, web sockets]
### Done
- [x] Create xUnit Attribute for test
- [x] Add Time start Time duration and Time End to Report
- [x] Create build for nuget (framework should be connected when nuget was setuped)
- [x] Create pilpline
- [x] **xUnit v3 3.0.0 Compatibility** - Updated framework to support xUnit v3 3.0.0 breaking changes:
  - Added CallerFilePath/CallerLineNumber support to LoadAttribute for source location information
  - Implemented TestMethodArity property in LoadTestMethod and LoadTestCase classes
  - Fixed source information extraction in LoadDiscoverer using reflection
  - Updated StandardTestCase to support new source location parameters
  - Ensured all Specification-based test classes have [UseLoadFramework] attribute
  - All framework tests passing (30 succeeded, 4 intentional failures, 1 skipped)
### Issues