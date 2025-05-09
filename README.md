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
dotnet add package xUnitV3LoadFramework
```

---

## 🚦 Quick Start

### Defining a Load Test
Use the `Load` attribute (inheriting from `FactAttribute`) to configure concurrency level, duration, interval, and execution order.

### Running Your Load Test
Execute your tests using the standard xUnit command:

```bash
dotnet test
```

---

## 📝 Usage Example

Here's a clear example demonstrating how to define and execute load tests using the `Specification` base class and the `[Load]` attribute:

```csharp
using xUnitV3LoadFramework.Attributes;
using xUnitV3LoadFramework.Extensions;
using xUnitV3LoadFramework.Extensions.Framework;
using System;

[assembly: TestFramework(typeof(LoadTestFramework))]

namespace xUnitLoadDemo;

public class ExampleLoadSpecification : Specification
{
    protected override void EstablishContext()
    {
        Console.WriteLine(">> Setup phase");
    }

    protected override void Because()
    {
        Console.WriteLine(">> Action phase");
    }

    [Load(order: 1, concurrency: 2, duration: 5000, interval: 500)]
    public void should_run_load_scenario_1()
    {
        Console.WriteLine(">> Running Load 1");
    }

    [Load(order: 2, concurrency: 3, duration: 7000, interval: 300)]
    public void should_run_load_scenario_2()
    {
        Console.WriteLine(">> Running Load 2");
    }
}
```

Each `[Load]` attribute defines:

- `order`: the test execution order  
- `concurrency`: number of parallel executions  
- `duration`: how long to run (in milliseconds)  
- `interval`: delay between each wave of execution (in milliseconds)

Run your tests using:

```bash
dotnet test
```

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
