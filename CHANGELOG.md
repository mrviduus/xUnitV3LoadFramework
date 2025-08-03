# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2.0.0] - 2025-08-03

### 🚀 Major Release - Production Ready

**xUnit v3 Compatibility & Enhanced Load Testing**

### ✨ Added
- **xUnit v3 3.0.0 Full Compatibility** - Complete support for xUnit v3 breaking changes
- **Fluent API Interface** - New `LoadTestRunner.Create()` fluent API for programmatic load test configuration
- **Enhanced Source Location Support** - Added CallerFilePath/CallerLineNumber support to LoadAttribute
- **Hybrid Load Worker Architecture** - New actor-based load execution engine with better performance
- **Comprehensive Documentation** - Complete technical documentation and examples
- **Production-Grade Error Handling** - Robust error handling and logging capabilities
- **Performance Metrics Collection** - Detailed performance and latency reporting
- **OpenTelemetry Integration** - Built-in observability with xUnit.OTel support

### 🛠️ Changed
- **Breaking**: Removed `UseLoadFramework` attribute requirement - tests with `[Load]` attribute automatically use load framework
- **Breaking**: Updated minimum .NET version to .NET 8.0
- **Breaking**: Refactored LoadTestMethod and LoadTestCase classes for better xUnit v3 integration
- Updated package dependencies to latest stable versions
- Improved actor system scalability and reliability
- Enhanced test discovery and execution pipeline

### 🔧 Fixed
- Fixed source information extraction in LoadDiscoverer using reflection
- Resolved TestMethodArity property implementation in test classes
- Fixed StandardTestCase to support new source location parameters
- Corrected repository URLs in package metadata
- All framework tests now passing (30 succeeded, 4 intentional failures, 1 skipped)

### 📚 Documentation
- Added comprehensive README with quick start guide
- Created architecture documentation with actor system overview
- Added multiple usage examples (attribute-based, fluent API, mixed testing)
- Included troubleshooting and best practices guides

### 🏗️ Infrastructure
- Central Package Management with Directory.Packages.props
- Reproducible builds configuration
- GitHub Actions CI/CD pipeline
- Source Link integration for debugging
- Code analysis and quality tools

## [1.0.0-alpha.1] - 2024-XX-XX

### Initial Release
- Basic load testing framework for xUnit
- Akka.NET actor-based architecture
- Load attribute for test decoration
- Basic performance reporting

---

## Legend
- 🚀 Major features
- ✨ New features  
- 🛠️ Changes
- 🔧 Bug fixes
- 📚 Documentation
- 🏗️ Infrastructure
- ⚠️ Breaking changes
