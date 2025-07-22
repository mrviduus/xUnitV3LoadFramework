# Load Test JSON Results Documentation

## Overview

The xUnitV3LoadFramework now automatically exports detailed load test results to JSON files in the `TestResults` folder. Each test execution creates a comprehensive JSON report with test results, configuration, environment information, and summary statistics.

## JSON Export Features

### Automatic Export
- **No configuration required**: JSON export is automatically enabled for all load tests
- **Organized by run**: Each test execution creates a timestamped folder structure
- **Individual test files**: Each test gets its own JSON file with detailed results

### Folder Structure
```
TestResults/
├── Run_20250722_224230/
│   └── Test_Name_20250722_224230.json
├── Run_20250722_224237/
│   └── Another_Test_20250722_224237.json
└── Run_20250722_224240/
    └── Yet_Another_Test_20250722_224240.json
```

### JSON Content Structure

Each JSON file contains comprehensive test information:

```json
{
  "testName": "Test display name",
  "timestamp": "2025-07-22T22:42:30.6772502Z",
  "runId": "20250722_224230",
  "results": {
    "scenarioName": "Test scenario name",
    "total": 20,
    "success": 20,
    "failure": 0,
    "time": 5.0272517,
    "maxLatency": 16.9478,
    "minLatency": 0.0149,
    "averageLatency": 0.9584400000000001,
    "percentile95Latency": 0.4267,
    "percentile99Latency": 16.9478,
    "medianLatency": 0.1073,
    "requestsStarted": 20,
    "requestsInFlight": 0,
    "requestsPerSecond": 3.978316820699469,
    "avgQueueTime": 2.25017,
    "maxQueueTime": 19.1627,
    "workerThreadsUsed": 16,
    "workerUtilization": 0.2486448012937168,
    "peakMemoryUsage": 11837728,
    "batchesCompleted": 10
  },
  "configuration": {
    "concurrency": 2,
    "duration": 5000,
    "interval": 500,
    "testMethod": "test_method_name",
    "testClass": "TestClassName",
    "assembly": "AssemblyName"
  },
  "environment": {
    "machineName": "MACHINE-NAME",
    "userName": "username",
    "osVersion": "Microsoft Windows NT 10.0.22631.0",
    "processorCount": 8,
    "frameworkVersion": ".NET 8.0.18",
    "workingSet": 85819392
  },
  "summary": {
    "totalRequests": 20,
    "successfulRequests": 20,
    "failedRequests": 0,
    "successRate": 100.0,
    "failureRate": 0.0,
    "averageLatency": 0.9584400000000001,
    "maxLatency": 16.9478,
    "minLatency": 0.0149,
    "percentile95": 0.4267,
    "percentile99": 16.9478,
    "throughputRps": 3.978316820699469,
    "testDuration": 5.0272517,
    "status": "PASSED"
  }
}
```

## JSON Schema Reference

### Root Object Properties

| Property | Type | Description |
|----------|------|-------------|
| `testName` | string | Display name of the test |
| `timestamp` | datetime | UTC timestamp when the test was executed |
| `runId` | string | Unique identifier for the test run (format: yyyyMMdd_HHmmss) |
| `results` | LoadResult | Detailed load test metrics |
| `configuration` | TestConfigurationInfo | Test configuration parameters |
| `environment` | EnvironmentInfo | System and environment information |
| `summary` | LoadTestSummary | High-level summary and status |

### Results Object (LoadResult)

| Property | Type | Description |
|----------|------|-------------|
| `scenarioName` | string | Name of the load test scenario |
| `total` | int | Total number of requests executed |
| `success` | int | Number of successful requests |
| `failure` | int | Number of failed requests |
| `time` | double | Total test duration in seconds |
| `maxLatency` | double | Maximum response latency in milliseconds |
| `minLatency` | double | Minimum response latency in milliseconds |
| `averageLatency` | double | Average response latency in milliseconds |
| `percentile95Latency` | double | 95th percentile latency in milliseconds |
| `percentile99Latency` | double | 99th percentile latency in milliseconds |
| `medianLatency` | double | Median latency in milliseconds |
| `requestsStarted` | int | Number of requests that were started |
| `requestsInFlight` | int | Number of requests still in progress |
| `requestsPerSecond` | double | Throughput in requests per second |
| `avgQueueTime` | double | Average time requests spent in queue |
| `maxQueueTime` | double | Maximum time any request spent in queue |
| `workerThreadsUsed` | int | Number of worker threads utilized |
| `workerUtilization` | double | Worker thread utilization percentage |
| `peakMemoryUsage` | long | Peak memory usage during test |
| `batchesCompleted` | int | Number of batches completed |

### Configuration Object (TestConfigurationInfo)

| Property | Type | Description |
|----------|------|-------------|
| `concurrency` | int | Number of concurrent workers/threads |
| `duration` | int | Test duration in milliseconds |
| `interval` | int | Interval between test executions in milliseconds |
| `testMethod` | string | Name of the test method |
| `testClass` | string | Name of the test class |
| `assembly` | string | Name of the test assembly |

### Environment Object (EnvironmentInfo)

| Property | Type | Description |
|----------|------|-------------|
| `machineName` | string | Name of the machine running the test |
| `userName` | string | User account running the test |
| `osVersion` | string | Operating system version |
| `processorCount` | int | Number of logical processors |
| `frameworkVersion` | string | .NET framework version |
| `workingSet` | long | Process working set memory usage |

### Summary Object (LoadTestSummary)

| Property | Type | Description |
|----------|------|-------------|
| `totalRequests` | int | Total number of requests |
| `successfulRequests` | int | Number of successful requests |
| `failedRequests` | int | Number of failed requests |
| `successRate` | double | Success percentage (0-100) |
| `failureRate` | double | Failure percentage (0-100) |
| `averageLatency` | double | Average latency in milliseconds |
| `maxLatency` | double | Maximum latency in milliseconds |
| `minLatency` | double | Minimum latency in milliseconds |
| `percentile95` | double | 95th percentile latency |
| `percentile99` | double | 99th percentile latency |
| `throughputRps` | double | Throughput in requests per second |
| `testDuration` | double | Test duration in seconds |
| `status` | string | Test status: "PASSED", "FAILED", or "UNKNOWN" |

## Usage Examples

### Running Tests and Viewing Results

1. **Run your load tests as usual:**
   ```bash
   dotnet test
   ```

2. **Check the TestResults folder:**
   ```bash
   # List all test runs
   ls TestResults/
   
   # View specific test results
   cat TestResults/Run_20250722_224230/TestName_20250722_224230.json
   ```

3. **Parse with tools:**
   ```bash
   # Pretty print with jq
   cat TestResults/Run_20250722_224230/TestName_20250722_224230.json | jq '.'
   
   # Extract specific metrics
   cat TestResults/Run_20250722_224230/TestName_20250722_224230.json | jq '.summary.successRate'
   ```

### Programmatic Analysis

You can easily parse these JSON files programmatically:

```csharp
using System.Text.Json;

// Read and deserialize
var json = await File.ReadAllTextAsync("TestResults/Run_20250722_224230/TestName.json");
var testResult = JsonSerializer.Deserialize<LoadTestResult>(json);

// Analyze results
Console.WriteLine($"Test: {testResult.TestName}");
Console.WriteLine($"Success Rate: {testResult.Summary.SuccessRate:F2}%");
Console.WriteLine($"Average Latency: {testResult.Summary.AverageLatency:F2}ms");
Console.WriteLine($"Throughput: {testResult.Summary.ThroughputRps:F2} RPS");
```

### Continuous Integration

In CI/CD pipelines, you can:

1. **Archive test results:**
   ```yaml
   - name: Archive Test Results
     uses: actions/upload-artifact@v3
     with:
       name: load-test-results
       path: TestResults/
   ```

2. **Parse and report metrics:**
   ```bash
   # Extract key metrics for reporting
   jq -r '.summary | "Success Rate: \(.successRate)%, Avg Latency: \(.averageLatency)ms, Throughput: \(.throughputRps) RPS"' TestResults/*/*.json
   ```

3. **Set up performance thresholds:**
   ```bash
   # Check if success rate is above threshold
   SUCCESS_RATE=$(jq '.summary.successRate' TestResults/*/latest.json)
   if (( $(echo "$SUCCESS_RATE < 95" | bc -l) )); then
     echo "Performance test failed: Success rate $SUCCESS_RATE% below threshold"
     exit 1
   fi
   ```

## Error Handling

If JSON export fails for any reason:
- The test will continue to run normally
- A diagnostic message will be logged indicating the export failure
- The console output will still show the standard load test results

## File Location Logic

The JSON exporter automatically:
1. Finds the project root by looking for a `.sln` file
2. Creates a `TestResults` folder if it doesn't exist
3. Creates run-specific folders with timestamp format `Run_yyyyMMdd_HHmmss`
4. Generates safe filenames from test display names
5. Appends timestamps to ensure uniqueness

## Benefits

1. **Comprehensive Metrics**: All load test data is captured in structured format
2. **Easy Analysis**: JSON format allows easy parsing and analysis
3. **Historical Tracking**: Organized folder structure enables trend analysis
4. **CI/CD Integration**: Machine-readable format perfect for automated pipelines
5. **Rich Context**: Environment and configuration information for debugging
6. **Zero Configuration**: Works automatically without any setup required

## Troubleshooting

### JSON Files Not Generated
- Check that the test actually runs (not skipped)
- Verify write permissions to the TestResults folder
- Look for diagnostic messages in test output

### Invalid JSON Content
- This should not occur, but if it does, check the diagnostic messages
- The JSON serializer uses safe defaults and error handling

### Large File Sizes
- JSON files are typically small (few KB each)
- If concerned about space, implement cleanup policies for old test runs

## Future Enhancements

Potential future features:
- Run summary aggregation across multiple tests
- Performance trend analysis
- Integration with monitoring tools
- Custom export formats (CSV, XML, etc.)
- Real-time streaming of results during long-running tests
