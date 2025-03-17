# xUnitLoadRunner

[![NuGet](https://img.shields.io/nuget/v/xUnitLoadFramework.svg)](https://www.nuget.org/packages/xUnitLoadFramework)
[![Downloads](https://img.shields.io/nuget/dt/xUnitLoadFramework.svg)](https://www.nuget.org/packages/xUnitLoadFramework)

## Overview
xUnitLoadRunner is a powerful load testing framework designed to seamlessly integrate with xUnit. By leveraging Akka.NET actors, it enables developers to define and execute load test scenarios in parallel while collecting detailed test results and performance reports.

## Project Status
This project is under active development. Contributions and feedback are welcome!

## Features
- Define load test scenarios using custom attributes.
- Execute load tests in parallel with Akka.NET actors.
- Collect, aggregate, and analyze test results.
- Seamless integration with xUnit for automated testing workflows.

## Installation
To install xUnitLoadRunner, add the NuGet package to your project using the following command:

```sh
dotnet add package xUnitLoadFramework
```

## Usage

### Defining a Load Test
To create a load test, use the `LoadTestSettings` attribute to specify concurrency, duration, and execution intervals for the test.

### Running a Load Test
Execute your xUnit tests as usual. The `LoadTestSettings` attribute ensures that the test runs with the specified load parameters.

```sh
dotnet test
```

## Example
Below is a complete example demonstrating how to use xUnitLoadFramework to define and execute a load test:

```csharp
using xUnitLoadFramework;
using Xunit;

namespace xUnitLoadDemo
{
    public class LoadTests
    {
        [Fact]
        [LoadTestSettings(concurrency: 5, DurationInSeconds = 10, IntervalInSeconds = 2)]
        public void ExampleLoadTest()
        {
            // Simulated test logic
            Console.WriteLine("Running load test...");
        }
    }
}
```

This example defines a load test that runs with a concurrency of 5, for a duration of 10 seconds, with an interval of 2 seconds between each test execution.

## Contributing
Contributions are welcome! If you encounter issues or have suggestions for improvement, feel free to submit a pull request or open an issue.

## License
This project is licensed under the MIT License. See the `LICENSE` file for more details.
