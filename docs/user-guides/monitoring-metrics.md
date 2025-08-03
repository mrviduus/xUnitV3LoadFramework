# Moni### Built-in Metrics

The framework automatically collects comprehensive metrics for every load test:

```csharp
[UseLoadFramework]
public class MetricsCollectionTests : Specification
{
    private readonly HttpClient _httpClient = new HttpClient();

    [Load(order: 1, concurrency: 100, duration: 120000, interval: 5000)]
    public async Task Should_Collect_Comprehensive_Metrics()
    {
        // Framework automatically tracks:
        // - Request counts (started, success, failure)
        // - Latency percentiles (min, max, avg, median, P95, P99)
        // - Throughput (requests per second)
        // - Resource utilization (worker threads, memory usage)
        // - Queue performance (avg/max queue times)
        
        await _httpClient.GetAsync("/api/data");
    }
}s Guide

Comprehensive guide to monitoring, metrics collection, and analysis in xUnitV3LoadFramework.

## Built-in Metrics

### Core Performance Metrics

The framework automatically collects comprehensive metrics for every load test:

```csharp
[Load(concurrency: 100, duration: 120000, interval: 5000)]
public async Task Should_Collect_Comprehensive_Metrics()
{
    // Framework automatically tracks:
    // - Request counts (started, success, failure)
    // - Latency percentiles (min, max, avg, median, P95, P99)
    // - Throughput (requests per second)
    // - Resource utilization (worker threads, memory usage)
    // - Queue performance (avg/max queue times)
    
    await _httpClient.GetAsync("/api/data");
}
```

### Understanding LoadResult Metrics

```csharp
public class LoadResult
{
    // Request Volume Metrics
    public int RequestsStarted { get; set; }    // Total requests initiated
    public int Total { get; set; }              // Completed requests (success + failure)
    public int Success { get; set; }            // Successful requests
    public int Failure { get; set; }            // Failed requests
    
    // Response Time Metrics (milliseconds)
    public double AverageLatency { get; set; }   // Mean response time
    public double MinLatency { get; set; }       // Fastest response
    public double MaxLatency { get; set; }       // Slowest response
    public double MedianLatency { get; set; }    // 50th percentile
    public double Percentile95Latency { get; set; } // 95th percentile
    public double Percentile99Latency { get; set; } // 99th percentile
    
    // Throughput Metrics
    public double RequestsPerSecond { get; set; } // Average RPS
    
    // Queue Performance
    public double AvgQueueTime { get; set; }     // Average wait time in queue
    public double MaxQueueTime { get; set; }     // Maximum wait time in queue
    
    // Resource Utilization
    public int WorkerThreadsUsed { get; set; }   // Active worker threads
    public double WorkerUtilization { get; set; } // % time workers were busy
    public long PeakMemoryUsage { get; set; }    // Peak memory consumption
    public int BatchesCompleted { get; set; }    // Number of work batches
}
```

## 🔍 Real-time Monitoring

### Progress Reporting

Configure reporting intervals for real-time visibility:

```csharp
[UseLoadFramework]
public class MonitoredLoadTest : Specification
{
    private readonly HttpClient _httpClient = new HttpClient();

    protected override void EstablishContext()
    {
        Console.WriteLine("Starting monitored load test...");
        Console.WriteLine($"Test started at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
    }
    
    // Report progress every 2 seconds for detailed monitoring
    [Load(order: 1, concurrency: 150, duration: 180000, interval: 2000)]
    public async Task Should_Report_Detailed_Progress()
    {
        var startTime = DateTime.Now;
        
        try
        {
            var response = await _httpClient.GetAsync("/api/users");
            var endTime = DateTime.Now;
            var latency = (endTime - startTime).TotalMilliseconds;
            
            // Optional: Custom logging per request (use sparingly)
            if (latency > 1000) // Log slow requests
            {
                Console.WriteLine($"Slow request detected: {latency:F2}ms at {DateTime.Now:HH:mm:ss}");
            }
            
            Assert.True(response.IsSuccessStatusCode, 
                $"Request failed with status: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Request failed: {ex.GetType().Name} - {ex.Message}");
            throw;
        }
    }
    
    protected override void DestroyContext()
    {
        Console.WriteLine($"Test completed at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
    }
}
```

### System Resource Monitoring

Monitor system resources during load tests:

```csharp
public class SystemMonitoredLoadTests : Specification, IDisposable
{
    private readonly Timer _resourceMonitor;
    private readonly PerformanceCounter _cpuCounter;
    private readonly PerformanceCounter _memoryCounter;
    private readonly PerformanceCounter _networkCounter;
    
    public SystemMonitoredLoadTests()
    {
        // Initialize performance counters
        _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        _memoryCounter = new PerformanceCounter("Memory", "Available MBytes");
        _networkCounter = new PerformanceCounter("Network Interface", "Bytes Total/sec", GetPrimaryNetworkInterface());
        
        // Monitor every 10 seconds
        _resourceMonitor = new Timer(LogResourceMetrics, null, 
            TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));
    }
    
    private void LogResourceMetrics(object state)
    {
        var timestamp = DateTime.Now;
        var cpuUsage = _cpuCounter.NextValue();
        var availableMemoryMB = _memoryCounter.NextValue();
        var networkBytesPerSec = _networkCounter.NextValue();
        
        Console.WriteLine($"[{timestamp:HH:mm:ss}] System Resources:");
        Console.WriteLine($"  CPU Usage: {cpuUsage:F1}%");
        Console.WriteLine($"  Available Memory: {availableMemoryMB:F0} MB");
        Console.WriteLine($"  Network Throughput: {networkBytesPerSec / 1024 / 1024:F2} MB/s");
        
        // Alert on resource constraints
        if (cpuUsage > 90)
            Console.WriteLine("  WARNING: HIGH CPU USAGE DETECTED");
        if (availableMemoryMB < 1000)
            Console.WriteLine("  WARNING: LOW MEMORY DETECTED");
    }
    
    [Load(concurrency: 200, duration: 300000, interval: 15000)]
    public async Task Should_Monitor_System_Resources()
    {
        await _httpClient.GetAsync("/api/data");
    }
    
    public void Dispose()
    {
        _resourceMonitor?.Dispose();
        _cpuCounter?.Dispose();
        _memoryCounter?.Dispose();
        _networkCounter?.Dispose();
    }
    
    private static string GetPrimaryNetworkInterface()
    {
        // Get the most active network interface
        var interfaces = NetworkInterface.GetAllNetworkInterfaces()
            .Where(ni => ni.OperationalStatus == OperationalStatus.Up && 
                        ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
            .OrderByDescending(ni => ni.GetIPv4Statistics().BytesSent + ni.GetIPv4Statistics().BytesReceived)
            .FirstOrDefault();
        
        return interfaces?.Name ?? "Ethernet";
    }
}
```

##  Custom Metrics Collection

### Application-Specific Metrics

Track business-specific performance indicators:

```csharp
public class CustomMetricsLoadTest : Specification
{
    private readonly ConcurrentDictionary<string, AtomicLong> _customCounters = new();
    private readonly ConcurrentBag<CustomMetric> _customMetrics = new();
    
    public class CustomMetric
    {
        public DateTime Timestamp { get; set; }
        public string Operation { get; set; }
        public double Duration { get; set; }
        public bool IsSuccess { get; set; }
        public Dictionary<string, object> Properties { get; set; } = new();
    }
    
    protected override void Because()
    {
        var stopwatch = Stopwatch.StartNew();
        var operation = SelectRandomOperation();
        
        try
        {
            var result = await ExecuteOperation(operation);
            stopwatch.Stop();
            
            // Record success metrics
            _customCounters.GetOrAdd($"{operation}_success", _ => new AtomicLong()).Increment();
            
            // Record detailed metrics
            _customMetrics.Add(new CustomMetric
            {
                Timestamp = DateTime.UtcNow,
                Operation = operation,
                Duration = stopwatch.Elapsed.TotalMilliseconds,
                IsSuccess = true,
                Properties = new Dictionary<string, object>
                {
                    ["ResponseSize"] = result?.Length ?? 0,
                    ["CacheHit"] = result?.Headers?.Contains("X-Cache-Hit") ?? false
                }
            });
            
            // Track business-specific outcomes
            if (operation == "CreateOrder" && result != null)
            {
                _customCounters.GetOrAdd("orders_created", _ => new AtomicLong()).Increment();
                
                // Track order value distribution
                if (ExtractOrderValue(result) > 100)
                {
                    _customCounters.GetOrAdd("high_value_orders", _ => new AtomicLong()).Increment();
                }
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            // Record failure metrics
            _customCounters.GetOrAdd($"{operation}_failure", _ => new AtomicLong()).Increment();
            _customCounters.GetOrAdd($"error_{ex.GetType().Name}", _ => new AtomicLong()).Increment();
            
            _customMetrics.Add(new CustomMetric
            {
                Timestamp = DateTime.UtcNow,
                Operation = operation,
                Duration = stopwatch.Elapsed.TotalMilliseconds,
                IsSuccess = false,
                Properties = new Dictionary<string, object>
                {
                    ["ErrorType"] = ex.GetType().Name,
                    ["ErrorMessage"] = ex.Message
                }
            });
            
            throw;
        }
    }
    
    protected override void DestroyContext()
    {
        GenerateCustomMetricsReport();
    }
    
    private void GenerateCustomMetricsReport()
    {
        Console.WriteLine("\n CUSTOM METRICS REPORT");
        Console.WriteLine("=" + new string('=', 50));
        
        // Summary counters
        Console.WriteLine("\n Operation Counters:");
        foreach (var counter in _customCounters.OrderBy(c => c.Key))
        {
            Console.WriteLine($"  {counter.Key}: {counter.Value.Value:N0}");
        }
        
        // Detailed analysis
        var successMetrics = _customMetrics.Where(m => m.IsSuccess).ToList();
        var failureMetrics = _customMetrics.Where(m => !m.IsSuccess).ToList();
        
        Console.WriteLine($"\n  Response Time Analysis:");
        if (successMetrics.Any())
        {
            var durations = successMetrics.Select(m => m.Duration).OrderBy(d => d).ToArray();
            Console.WriteLine($"  Successful Operations: {durations.Length:N0}");
            Console.WriteLine($"  Min Duration: {durations.First():F2}ms");
            Console.WriteLine($"  Max Duration: {durations.Last():F2}ms");
            Console.WriteLine($"  Average Duration: {durations.Average():F2}ms");
            Console.WriteLine($"  P95 Duration: {durations[(int)(durations.Length * 0.95)]:F2}ms");
            Console.WriteLine($"  P99 Duration: {durations[(int)(durations.Length * 0.99)]:F2}ms");
        }
        
        // Error analysis
        if (failureMetrics.Any())
        {
            Console.WriteLine($"\n Error Analysis:");
            var errorsByType = failureMetrics
                .GroupBy(m => m.Properties.GetValueOrDefault("ErrorType", "Unknown"))
                .OrderByDescending(g => g.Count());
            
            foreach (var errorGroup in errorsByType)
            {
                Console.WriteLine($"  {errorGroup.Key}: {errorGroup.Count():N0} occurrences");
            }
        }
        
        // Business metrics
        Console.WriteLine($"\n💼 Business Metrics:");
        var orderCount = _customCounters.GetValueOrDefault("orders_created")?.Value ?? 0;
        var highValueOrders = _customCounters.GetValueOrDefault("high_value_orders")?.Value ?? 0;
        
        if (orderCount > 0)
        {
            Console.WriteLine($"  Total Orders Created: {orderCount:N0}");
            Console.WriteLine($"  High Value Orders: {highValueOrders:N0} ({(double)highValueOrders / orderCount:P1})");
        }
    }
    
    [Load(concurrency: 100, duration: 180000, interval: 10000)]
    public async Task Should_Track_Custom_Metrics() { }
    
    private string SelectRandomOperation()
    {
        var operations = new[] { "GetUsers", "CreateOrder", "UpdateProfile", "SearchProducts" };
        return operations[Random.Shared.Next(operations.Length)];
    }
}
```

### Performance Baseline Tracking

Track performance trends over time:

```csharp
public class BaselineTrackingTests : Specification
{
    private readonly string _baselineFile = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), 
        "LoadTestBaselines.json");
    
    public class BaselineMetric
    {
        public DateTime TestDate { get; set; }
        public string TestName { get; set; }
        public double AverageLatency { get; set; }
        public double P95Latency { get; set; }
        public double RequestsPerSecond { get; set; }
        public double SuccessRate { get; set; }
        public string GitCommit { get; set; }
        public string Environment { get; set; }
    }
    
    protected override void EstablishContext()
    {
        Console.WriteLine(" Running baseline tracking load test");
    }
    
    protected override void DestroyContext()
    {
        // Note: In actual implementation, you'd get LoadResult from the framework
        // This is pseudocode showing how to track baselines
        
        var currentMetric = new BaselineMetric
        {
            TestDate = DateTime.UtcNow,
            TestName = "API_Load_Test_Baseline",
            AverageLatency = 150.5, // Would come from LoadResult
            P95Latency = 280.3,     // Would come from LoadResult
            RequestsPerSecond = 95.2, // Would come from LoadResult
            SuccessRate = 99.8,       // Would come from LoadResult
            GitCommit = GetCurrentGitCommit(),
            Environment = Environment.GetEnvironmentVariable("ENVIRONMENT") ?? "Development"
        };
        
        SaveBaseline(currentMetric);
        CompareWithPreviousBaselines(currentMetric);
    }
    
    private void SaveBaseline(BaselineMetric metric)
    {
        List<BaselineMetric> baselines;
        
        if (File.Exists(_baselineFile))
        {
            var json = File.ReadAllText(_baselineFile);
            baselines = JsonSerializer.Deserialize<List<BaselineMetric>>(json) ?? new List<BaselineMetric>();
        }
        else
        {
            baselines = new List<BaselineMetric>();
        }
        
        baselines.Add(metric);
        
        // Keep only last 100 runs
        if (baselines.Count > 100)
        {
            baselines = baselines.OrderByDescending(b => b.TestDate).Take(100).ToList();
        }
        
        var updatedJson = JsonSerializer.Serialize(baselines, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_baselineFile, updatedJson);
        
        Console.WriteLine($" Baseline saved: {metric.TestDate:yyyy-MM-dd HH:mm:ss}");
    }
    
    private void CompareWithPreviousBaselines(BaselineMetric current)
    {
        if (!File.Exists(_baselineFile)) return;
        
        var json = File.ReadAllText(_baselineFile);
        var baselines = JsonSerializer.Deserialize<List<BaselineMetric>>(json);
        
        if (baselines?.Count < 2) return;
        
        var previous = baselines
            .Where(b => b.TestName == current.TestName && b.TestDate < current.TestDate)
            .OrderByDescending(b => b.TestDate)
            .FirstOrDefault();
        
        if (previous == null) return;
        
        Console.WriteLine("\n BASELINE COMPARISON");
        Console.WriteLine("=" + new string('=', 40));
        
        CompareMetric("Average Latency", current.AverageLatency, previous.AverageLatency, "ms", false);
        CompareMetric("P95 Latency", current.P95Latency, previous.P95Latency, "ms", false);
        CompareMetric("Requests/Second", current.RequestsPerSecond, previous.RequestsPerSecond, "", true);
        CompareMetric("Success Rate", current.SuccessRate, previous.SuccessRate, "%", true);
        
        Console.WriteLine($"\nPrevious test: {previous.TestDate:yyyy-MM-dd HH:mm:ss} ({previous.GitCommit})");
        Console.WriteLine($"Current test:  {current.TestDate:yyyy-MM-dd HH:mm:ss} ({current.GitCommit})");
    }
    
    private void CompareMetric(string name, double current, double previous, string unit, bool higherIsBetter)
    {
        var change = current - previous;
        var changePercent = Math.Abs(change / previous * 100);
        var isImprovement = higherIsBetter ? change > 0 : change < 0;
        var isRegression = higherIsBetter ? change < 0 : change > 0;
        
        var symbol = isImprovement ? "" : isRegression ? "" : "";
        var direction = change > 0 ? "" : change < 0 ? "" : "=";
        
        Console.WriteLine($"{symbol} {name}: {current:F2}{unit} {direction} {changePercent:F1}% from {previous:F2}{unit}");
        
        // Alert on significant regressions
        if (isRegression && changePercent > 10)
        {
            Console.WriteLine($"   🚨 PERFORMANCE REGRESSION DETECTED: {changePercent:F1}% worse than baseline");
        }
    }
    
    private string GetCurrentGitCommit()
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = "rev-parse --short HEAD",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            
            process.Start();
            var output = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();
            
            return process.ExitCode == 0 ? output : "unknown";
        }
        catch
        {
            return "unknown";
        }
    }
    
    [Load(concurrency: 100, duration: 120000, interval: 10000)]
    public async Task Should_Track_Performance_Baselines()
    {
        await _httpClient.GetAsync("/api/users");
    }
}
```

##  Metrics Analysis and Visualization

### Statistical Analysis

Perform statistical analysis on load test results:

```csharp
public class StatisticalAnalysisTests : Specification
{
    private readonly List<double> _responseTimes = new();
    private readonly List<DateTime> _requestTimestamps = new();
    
    protected override void Because()
    {
        var startTime = DateTime.UtcNow;
        var stopwatch = Stopwatch.StartTime();
        
        try
        {
            var response = await _httpClient.GetAsync("/api/data");
            stopwatch.Stop();
            
            // Collect detailed timing data
            _responseTimes.Add(stopwatch.Elapsed.TotalMilliseconds);
            _requestTimestamps.Add(startTime);
            
            Assert.True(response.IsSuccessStatusCode);
        }
        catch (Exception)
        {
            stopwatch.Stop();
            _responseTimes.Add(stopwatch.Elapsed.TotalMilliseconds);
            _requestTimestamps.Add(startTime);
            throw;
        }
    }
    
    protected override void DestroyContext()
    {
        PerformStatisticalAnalysis();
    }
    
    private void PerformStatisticalAnalysis()
    {
        if (!_responseTimes.Any()) return;
        
        var sortedTimes = _responseTimes.OrderBy(t => t).ToArray();
        var count = sortedTimes.Length;
        
        // Basic statistics
        var min = sortedTimes.First();
        var max = sortedTimes.Last();
        var mean = sortedTimes.Average();
        var median = CalculatePercentile(sortedTimes, 50);
        
        // Percentiles
        var p90 = CalculatePercentile(sortedTimes, 90);
        var p95 = CalculatePercentile(sortedTimes, 95);
        var p99 = CalculatePercentile(sortedTimes, 99);
        var p999 = CalculatePercentile(sortedTimes, 99.9);
        
        // Variability measures
        var variance = _responseTimes.Sum(t => Math.Pow(t - mean, 2)) / count;
        var standardDeviation = Math.Sqrt(variance);
        var coefficientOfVariation = standardDeviation / mean;
        
        Console.WriteLine("\n STATISTICAL ANALYSIS");
        Console.WriteLine("=" + new string('=', 50));
        Console.WriteLine($"Sample Size: {count:N0} requests");
        Console.WriteLine();
        
        Console.WriteLine(" Response Time Distribution:");
        Console.WriteLine($"  Minimum:  {min:F2}ms");
        Console.WriteLine($"  Maximum:  {max:F2}ms");
        Console.WriteLine($"  Mean:     {mean:F2}ms");
        Console.WriteLine($"  Median:   {median:F2}ms");
        Console.WriteLine();
        
        Console.WriteLine(" Percentiles:");
        Console.WriteLine($"  P90:      {p90:F2}ms");
        Console.WriteLine($"  P95:      {p95:F2}ms");
        Console.WriteLine($"  P99:      {p99:F2}ms");
        Console.WriteLine($"  P99.9:    {p999:F2}ms");
        Console.WriteLine();
        
        Console.WriteLine(" Variability:");
        Console.WriteLine($"  Std Dev:  {standardDeviation:F2}ms");
        Console.WriteLine($"  CV:       {coefficientOfVariation:F3} ({coefficientOfVariation:P1})");
        Console.WriteLine();
        
        // Performance assessment
        AssessPerformanceCharacteristics(mean, p95, coefficientOfVariation);
        
        // Throughput analysis
        AnalyzeThroughputPattern();
    }
    
    private double CalculatePercentile(double[] sortedValues, double percentile)
    {
        var index = (percentile / 100.0) * (sortedValues.Length - 1);
        var lower = (int)Math.Floor(index);
        var upper = (int)Math.Ceiling(index);
        
        if (lower == upper)
            return sortedValues[lower];
        
        var weight = index - lower;
        return sortedValues[lower] * (1 - weight) + sortedValues[upper] * weight;
    }
    
    private void AssessPerformanceCharacteristics(double mean, double p95, double cv)
    {
        Console.WriteLine(" Performance Assessment:");
        
        // Response time assessment
        if (mean < 100)
            Console.WriteLine("   Excellent response times (< 100ms average)");
        else if (mean < 500)
            Console.WriteLine("   Good response times (< 500ms average)");
        else if (mean < 1000)
            Console.WriteLine("    Acceptable response times (< 1s average)");
        else
            Console.WriteLine("   Poor response times (> 1s average)");
        
        // Consistency assessment
        if (cv < 0.3)
            Console.WriteLine("   Very consistent performance (CV < 0.3)");
        else if (cv < 0.6)
            Console.WriteLine("   Good consistency (CV < 0.6)");
        else if (cv < 1.0)
            Console.WriteLine("    Moderate variability (CV < 1.0)");
        else
            Console.WriteLine("   High variability (CV > 1.0)");
        
        // P95 vs mean ratio
        var p95Ratio = p95 / mean;
        if (p95Ratio < 2.0)
            Console.WriteLine("   Low tail latency (P95/mean < 2.0)");
        else if (p95Ratio < 3.0)
            Console.WriteLine("    Moderate tail latency (P95/mean < 3.0)");
        else
            Console.WriteLine("   High tail latency (P95/mean > 3.0)");
    }
    
    private void AnalyzeThroughputPattern()
    {
        if (_requestTimestamps.Count < 10) return;
        
        Console.WriteLine(" Throughput Analysis:");
        
        // Calculate throughput in 5-second windows
        var testStart = _requestTimestamps.Min();
        var testEnd = _requestTimestamps.Max();
        var totalDuration = testEnd - testStart;
        
        if (totalDuration.TotalSeconds < 5) return;
        
        var windowSize = TimeSpan.FromSeconds(5);
        var windows = new List<(DateTime Start, int Count)>();
        
        for (var windowStart = testStart; windowStart < testEnd; windowStart += windowSize)
        {
            var windowEnd = windowStart + windowSize;
            var requestsInWindow = _requestTimestamps.Count(t => t >= windowStart && t < windowEnd);
            windows.Add((windowStart, requestsInWindow));
        }
        
        if (windows.Any())
        {
            var throughputs = windows.Select(w => w.Count / windowSize.TotalSeconds).ToArray();
            var avgThroughput = throughputs.Average();
            var minThroughput = throughputs.Min();
            var maxThroughput = throughputs.Max();
            var throughputCV = throughputs.Length > 1 ? 
                Math.Sqrt(throughputs.Sum(t => Math.Pow(t - avgThroughput, 2)) / throughputs.Length) / avgThroughput : 0;
            
            Console.WriteLine($"  Average Throughput: {avgThroughput:F2} req/s");
            Console.WriteLine($"  Min Throughput:     {minThroughput:F2} req/s");
            Console.WriteLine($"  Max Throughput:     {maxThroughput:F2} req/s");
            Console.WriteLine($"  Throughput CV:      {throughputCV:F3}");
            
            if (throughputCV < 0.1)
                Console.WriteLine("   Very stable throughput");
            else if (throughputCV < 0.2)
                Console.WriteLine("   Stable throughput");
            else
                Console.WriteLine("    Variable throughput - investigate bottlenecks");
        }
    }
    
    [Load(concurrency: 100, duration: 180000, interval: 10000)]
    public async Task Should_Perform_Statistical_Analysis() { }
}
```

## 🚨 Alerting and Thresholds

### Performance Threshold Monitoring

Set up automated performance threshold monitoring:

```csharp
public class ThresholdMonitoringTests : Specification
{
    public class PerformanceThresholds
    {
        public double MaxAverageLatency { get; set; } = 500; // ms
        public double MaxP95Latency { get; set; } = 1000;   // ms
        public double MinThroughput { get; set; } = 50;     // req/s
        public double MinSuccessRate { get; set; } = 95;    // %
        public double MaxErrorRate { get; set; } = 5;       // %
    }
    
    private readonly PerformanceThresholds _thresholds = new();
    private readonly List<double> _latencies = new();
    private int _successCount = 0;
    private int _totalCount = 0;
    
    protected override void Because()
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var response = await _httpClient.GetAsync("/api/data");
            stopwatch.Stop();
            
            _latencies.Add(stopwatch.Elapsed.TotalMilliseconds);
            _totalCount++;
            
            if (response.IsSuccessStatusCode)
            {
                _successCount++;
            }
            
            // Real-time threshold checking (sample occasionally to avoid overhead)
            if (_totalCount % 100 == 0)
            {
                CheckRealTimeThresholds();
            }
        }
        catch (Exception)
        {
            stopwatch.Stop();
            _latencies.Add(stopwatch.Elapsed.TotalMilliseconds);
            _totalCount++;
            throw;
        }
    }
    
    private void CheckRealTimeThresholds()
    {
        if (_latencies.Count < 50) return; // Need minimum sample size
        
        var recentLatencies = _latencies.TakeLast(100).OrderBy(l => l).ToArray();
        var avgLatency = recentLatencies.Average();
        var p95Latency = recentLatencies[(int)(recentLatencies.Length * 0.95)];
        var recentSuccessRate = _successCount / (double)_totalCount * 100;
        
        // Check thresholds and alert if breached
        if (avgLatency > _thresholds.MaxAverageLatency)
        {
            Console.WriteLine($"🚨 ALERT: Average latency ({avgLatency:F2}ms) exceeds threshold ({_thresholds.MaxAverageLatency}ms)");
        }
        
        if (p95Latency > _thresholds.MaxP95Latency)
        {
            Console.WriteLine($"🚨 ALERT: P95 latency ({p95Latency:F2}ms) exceeds threshold ({_thresholds.MaxP95Latency}ms)");
        }
        
        if (recentSuccessRate < _thresholds.MinSuccessRate)
        {
            Console.WriteLine($"🚨 ALERT: Success rate ({recentSuccessRate:F1}%) below threshold ({_thresholds.MinSuccessRate}%)");
        }
    }
    
    protected override void DestroyContext()
    {
        ValidateFinalThresholds();
    }
    
    private void ValidateFinalThresholds()
    {
        if (!_latencies.Any()) return;
        
        var sortedLatencies = _latencies.OrderBy(l => l).ToArray();
        var avgLatency = sortedLatencies.Average();
        var p95Latency = sortedLatencies[(int)(sortedLatencies.Length * 0.95)];
        var successRate = _successCount / (double)_totalCount * 100;
        var errorRate = 100 - successRate;
        
        var testDurationSeconds = 180; // From Load attribute
        var throughput = _totalCount / (double)testDurationSeconds;
        
        Console.WriteLine("\n PERFORMANCE THRESHOLD VALIDATION");
        Console.WriteLine("=" + new string('=', 50));
        
        ValidateThreshold("Average Latency", avgLatency, _thresholds.MaxAverageLatency, "ms", false);
        ValidateThreshold("P95 Latency", p95Latency, _thresholds.MaxP95Latency, "ms", false);
        ValidateThreshold("Throughput", throughput, _thresholds.MinThroughput, "req/s", true);
        ValidateThreshold("Success Rate", successRate, _thresholds.MinSuccessRate, "%", true);
        ValidateThreshold("Error Rate", errorRate, _thresholds.MaxErrorRate, "%", false);
        
        // Overall assessment
        var violations = CountThresholdViolations(avgLatency, p95Latency, throughput, successRate, errorRate);
        
        if (violations == 0)
        {
            Console.WriteLine("\n ALL PERFORMANCE THRESHOLDS PASSED");
        }
        else
        {
            Console.WriteLine($"\n {violations} PERFORMANCE THRESHOLD(S) VIOLATED");
            Console.WriteLine("Consider investigating performance issues or adjusting thresholds.");
        }
    }
    
    private void ValidateThreshold(string metric, double actual, double threshold, string unit, bool higherIsBetter)
    {
        var passed = higherIsBetter ? actual >= threshold : actual <= threshold;
        var symbol = passed ? "" : "";
        var comparison = higherIsBetter ? "≥" : "≤";
        
        Console.WriteLine($"{symbol} {metric}: {actual:F2}{unit} (threshold: {comparison} {threshold:F2}{unit})");
        
        if (!passed)
        {
            var deviation = higherIsBetter ? threshold - actual : actual - threshold;
            var deviationPercent = Math.Abs(deviation / threshold * 100);
            Console.WriteLine($"   Deviation: {deviation:F2}{unit} ({deviationPercent:F1}% over threshold)");
        }
    }
    
    private int CountThresholdViolations(double avgLatency, double p95Latency, double throughput, double successRate, double errorRate)
    {
        var violations = 0;
        
        if (avgLatency > _thresholds.MaxAverageLatency) violations++;
        if (p95Latency > _thresholds.MaxP95Latency) violations++;
        if (throughput < _thresholds.MinThroughput) violations++;
        if (successRate < _thresholds.MinSuccessRate) violations++;
        if (errorRate > _thresholds.MaxErrorRate) violations++;
        
        return violations;
    }
    
    [Load(concurrency: 100, duration: 180000, interval: 15000)]
    public async Task Should_Monitor_Performance_Thresholds() { }
}
```

## 📋 Monitoring Best Practices

### Essential Monitoring Checklist

- [ ] **Response Time Metrics**: Track mean, median, P95, P99 latencies
- [ ] **Throughput Monitoring**: Monitor requests per second and batch completion rates  
- [ ] **Error Rate Tracking**: Monitor success/failure ratios and error patterns
- [ ] **Resource Utilization**: Track CPU, memory, and network usage
- [ ] **Queue Performance**: Monitor queue depths and wait times
- [ ] **System Health**: Monitor GC pressure and thread pool utilization

### Reporting Intervals

Choose appropriate reporting intervals based on test duration:

```csharp
// Short tests (< 1 minute): Frequent reporting for detailed visibility
[Load(concurrency: 50, duration: 30000, interval: 2000)]

// Medium tests (1-10 minutes): Moderate reporting for balance
[Load(concurrency: 100, duration: 300000, interval: 10000)]

// Long tests (> 10 minutes): Infrequent reporting to reduce overhead
[Load(concurrency: 200, duration: 1800000, interval: 30000)]
```

### Performance Analysis Workflow

1. **Real-time Monitoring**: Watch for immediate issues during test execution
2. **Statistical Analysis**: Perform detailed analysis after test completion
3. **Baseline Comparison**: Compare results with historical baselines
4. **Threshold Validation**: Verify performance meets established criteria
5. **Root Cause Analysis**: Investigate any performance regressions or failures

This comprehensive monitoring approach ensures you have full visibility into your system's performance characteristics under load.
