# 🚀 xUnitLoadRunner

[![NuGet](https://img.shields.io/nuget/v/xUnitLoadFramework.svg)](https://www.nuget.org/packages/xUnitLoadFramework)
[![Downloads](https://img.shields.io/nuget/dt/xUnitLoadFramework.svg)](https://www.nuget.org/packages/xUnitLoadFramework)

**xUnitLoadRunner** is a robust and user-friendly load testing framework built to seamlessly integrate with **xUnit** and powered by **Akka.NET actors**. It allows developers to efficiently define, execute, and analyze parallel load test scenarios, making load testing a natural part of your automated testing workflow.

---

## 🌟 Features

- ✅ Easily define load test scenarios with intuitive attributes.
- ✅ Parallel load test execution using Akka.NET actors.
- ✅ Detailed aggregation and analysis of test results.
- ✅ Fully integrated with xUnit testing framework.

---

## ⚡ Installation

Install via NuGet package manager:

```bash
dotnet add package xUnitLoadFramework
```

---

## 🚦 Quick Start

### Defining a Load Test
Use the `LoadTestSettings` attribute to configure your test's concurrency level, duration, and intervals.

### Running Your Load Test
Execute your tests using the standard xUnit command:

```bash
dotnet test
```

---

## 📝 Usage Example

Here's a quick example demonstrating how to define and execute load tests:

```csharp
using xUnitLoadFramework;
using Xunit;

namespace xUnitLoadDemo
{
    public class LoadTests
    {
        [Fact]
        [LoadTestSettings(concurrency: 5, durationInMilliseconds: 10, intervalInMilliseconds: 2)]
        public void ExampleLoadTest()
        {
            // Your test logic goes here
            Console.WriteLine("Running load test...");
        }
    }
}
```

The above example runs your test concurrently with 5 actors for 10 seconds, executing every 2 seconds.

---

## 📖 Documentation

For detailed documentation, examples, and more information, visit the [official documentation](#).

---

## 🤝 Contributing

Your contributions and feedback are always welcome!
- Submit issues or suggestions via [GitHub Issues](#).
- Open pull requests following our [Contributing Guidelines](CONTRIBUTING.md).

---

## 📜 License

This project is licensed under the [MIT License](LICENSE).

---

## 📫 Contact

For questions, suggestions, or feedback, please open an issue or contact directly:

- **Vasyl Vdovychenko**  
  [LinkedIn](https://www.linkedin.com/in/vasyl-vdovychenko) | [Email](mailto:mrviduus@gmail.com)

