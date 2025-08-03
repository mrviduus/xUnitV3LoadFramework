using System;
using System.Reflection;
using System.Threading.Tasks;
using xUnitV3LoadFramework.Attributes;
using xUnitV3LoadFramework.LoadRunnerCore.Models;
using xUnitV3LoadFramework.LoadRunnerCore.Runner;

namespace xUnitV3LoadFramework.Extensions
{
    /// <summary>
    /// Executes test scenarios under concurrent load conditions.
    /// Provides both direct execution methods and a fluent API for load test configuration.
    /// </summary>
    public static class LoadTestRunner
    {
        /// <summary>
        /// Executes the current test method as a load test if it has a LoadFact attribute.
        /// This method should be called at the beginning of a LoadFact test method.
        /// </summary>
        /// <param name="testAction">The test action to execute under load</param>
        /// <param name="testMethodName">Optional test method name. If not provided, will be inferred from the call stack.</param>
        /// <returns>A task representing the load test execution</returns>
        public static async Task<LoadResult> ExecuteAsync(Func<Task<bool>> testAction, string? testMethodName = null)
        {
            // Get the calling method to find LoadFact attribute
            var callingMethod = GetCallingTestMethod();
            if (callingMethod == null)
            {
                throw new InvalidOperationException("Could not determine the calling test method.");
            }

            // Look for LoadFact attribute
            var loadFactAttribute = callingMethod.GetCustomAttribute<LoadFactAttribute>();
            if (loadFactAttribute == null)
            {
                throw new InvalidOperationException($"Method {callingMethod.Name} is not decorated with LoadFactAttribute.");
            }

            // Create load execution plan
            var executionPlan = new LoadExecutionPlan
            {
                Name = testMethodName ?? $"{callingMethod.DeclaringType?.Name}.{callingMethod.Name}",
                Action = testAction,
                Settings = new LoadSettings
                {
                    Concurrency = loadFactAttribute.Concurrency,
                    Duration = TimeSpan.FromMilliseconds(loadFactAttribute.Duration),
                    Interval = TimeSpan.FromMilliseconds(loadFactAttribute.Interval)
                }
            };

            // Execute the load test
            var result = await LoadRunner.Run(executionPlan);
            
            // Log results
            Console.WriteLine($"Load test '{executionPlan.Name}' completed:");
            Console.WriteLine($"  Order: {loadFactAttribute.Order}");
            Console.WriteLine($"  Total executions: {result.Total}");
            Console.WriteLine($"  Successful executions: {result.Success}");
            Console.WriteLine($"  Failed executions: {result.Failure}");
            Console.WriteLine($"  Execution time: {result.Time:F2} seconds");
            Console.WriteLine($"  Requests per second: {result.RequestsPerSecond:F2}");
            Console.WriteLine($"  Average latency: {result.AverageLatency:F2}ms");
            Console.WriteLine($"  Success rate: {(result.Total > 0 ? (double)result.Success / result.Total * 100 : 0):F2}%");
            
            return result;
        }

        /// <summary>
        /// Executes the current test method as a load test if it has a LoadFact attribute.
        /// This overload is for synchronous test actions.
        /// </summary>
        /// <param name="testAction">The synchronous test action to execute under load</param>
        /// <param name="testMethodName">Optional test method name. If not provided, will be inferred from the call stack.</param>
        /// <returns>A task representing the load test execution</returns>
        public static async Task<LoadResult> ExecuteAsync(Func<bool> testAction, string? testMethodName = null)
        {
            return await ExecuteAsync(() => Task.FromResult(testAction()), testMethodName);
        }

        /// <summary>
        /// Executes the current test method as a load test if it has a LoadFact attribute.
        /// This overload is for test actions that don't return a success indicator (assumes success if no exception).
        /// </summary>
        /// <param name="testAction">The test action to execute under load</param>
        /// <param name="testMethodName">Optional test method name. If not provided, will be inferred from the call stack.</param>
        /// <returns>A task representing the load test execution</returns>
        public static async Task<LoadResult> ExecuteAsync(Func<Task> testAction, string? testMethodName = null)
        {
            return await ExecuteAsync(async () =>
            {
                try
                {
                    await testAction();
                    return true;
                }
                catch
                {
                    return false;
                }
            }, testMethodName);
        }

        /// <summary>
        /// Executes the current test method as a load test if it has a LoadFact attribute.
        /// This overload is for simple test actions without async.
        /// </summary>
        /// <param name="testAction">The simple test action to execute under load</param>
        /// <param name="testMethodName">Optional test method name. If not provided, will be inferred from the call stack.</param>
        /// <returns>A task representing the load test execution</returns>
        public static async Task<LoadResult> ExecuteAsync(Action testAction, string? testMethodName = null)
        {
            return await ExecuteAsync(() =>
            {
                try
                {
                    testAction();
                    return true;
                }
                catch
                {
                    return false;
                }
            }, testMethodName);
        }

        /// <summary>
        /// Executes the test scenario with custom load parameters.
        /// </summary>
        /// <param name="testAction">The test action to execute under load</param>
        /// <param name="options">Custom load test configuration</param>
        /// <returns>A task representing the load test execution</returns>
        public static async Task<LoadResult> ExecuteAsync(Func<Task<bool>> testAction, LoadTestOptions options)
        {
            var executionPlan = new LoadExecutionPlan
            {
                Name = options.Name ?? "CustomLoadTest",
                Action = testAction,
                Settings = new LoadSettings
                {
                    Concurrency = options.Concurrency,
                    Duration = options.Duration,
                    Interval = options.Interval
                }
            };

            return await LoadRunner.Run(executionPlan);
        }

        /// <summary>
        /// Simplified API that assumes success if no exception is thrown.
        /// </summary>
        /// <param name="testAction">The test action to execute</param>
        /// <returns>A task representing the load test execution</returns>
        public static async Task<LoadResult> RunAsync(Func<Task> testAction)
        {
            return await ExecuteAsync(async () =>
            {
                try
                {
                    await testAction();
                    return true;
                }
                catch
                {
                    return false;
                }
            });
        }

        /// <summary>
        /// Creates a fluent API builder for load test configuration.
        /// </summary>
        /// <returns>A new load test builder instance</returns>
        public static ILoadTestBuilder Create() => new LoadTestBuilder();

        private static MethodInfo? GetCallingTestMethod()
        {
            var stackTrace = new System.Diagnostics.StackTrace();
            
            // Look through the stack trace to find a method with LoadFactAttribute
            for (int i = 1; i < stackTrace.FrameCount; i++)
            {
                var frame = stackTrace.GetFrame(i);
                var method = frame?.GetMethod();
                
                if (method != null && method.GetCustomAttribute<LoadFactAttribute>() != null)
                {
                    return method as MethodInfo;
                }
            }

            return null;
        }
    }

    /// <summary>
    /// Fluent API interface for configuring load tests.
    /// </summary>
    public interface ILoadTestBuilder
    {
        /// <summary>
        /// Sets the number of concurrent executions.
        /// </summary>
        /// <param name="concurrency">Number of concurrent operations</param>
        /// <returns>The builder instance for method chaining</returns>
        ILoadTestBuilder WithConcurrency(int concurrency);

        /// <summary>
        /// Sets the test duration.
        /// </summary>
        /// <param name="duration">Duration in milliseconds</param>
        /// <returns>The builder instance for method chaining</returns>
        ILoadTestBuilder WithDuration(int duration);

        /// <summary>
        /// Sets the test duration.
        /// </summary>
        /// <param name="duration">Duration as TimeSpan</param>
        /// <returns>The builder instance for method chaining</returns>
        ILoadTestBuilder WithDuration(TimeSpan duration);

        /// <summary>
        /// Sets the interval between batches.
        /// </summary>
        /// <param name="interval">Interval in milliseconds</param>
        /// <returns>The builder instance for method chaining</returns>
        ILoadTestBuilder WithInterval(int interval);

        /// <summary>
        /// Sets the interval between batches.
        /// </summary>
        /// <param name="interval">Interval as TimeSpan</param>
        /// <returns>The builder instance for method chaining</returns>
        ILoadTestBuilder WithInterval(TimeSpan interval);

        /// <summary>
        /// Sets the test name for reporting.
        /// </summary>
        /// <param name="name">Test name</param>
        /// <returns>The builder instance for method chaining</returns>
        ILoadTestBuilder WithName(string name);

        /// <summary>
        /// Executes the load test with the configured parameters.
        /// </summary>
        /// <param name="action">The action to execute under load</param>
        /// <returns>A task representing the load test execution</returns>
        Task<LoadResult> RunAsync(Func<Task> action);

        /// <summary>
        /// Executes the load test with the configured parameters and explicit success indication.
        /// </summary>
        /// <param name="action">The action to execute under load with success indication</param>
        /// <returns>A task representing the load test execution</returns>
        Task<LoadResult> RunAsync(Func<Task<bool>> action);
    }

    /// <summary>
    /// Implementation of the fluent API builder for load tests.
    /// </summary>
    internal class LoadTestBuilder : ILoadTestBuilder
    {
        private int _concurrency = 1;
        private TimeSpan _duration = TimeSpan.FromSeconds(1);
        private TimeSpan _interval = TimeSpan.FromMilliseconds(100);
        private string _name = "FluentLoadTest";

        public ILoadTestBuilder WithConcurrency(int concurrency)
        {
            _concurrency = concurrency;
            return this;
        }

        public ILoadTestBuilder WithDuration(int duration)
        {
            _duration = TimeSpan.FromMilliseconds(duration);
            return this;
        }

        public ILoadTestBuilder WithDuration(TimeSpan duration)
        {
            _duration = duration;
            return this;
        }

        public ILoadTestBuilder WithInterval(int interval)
        {
            _interval = TimeSpan.FromMilliseconds(interval);
            return this;
        }

        public ILoadTestBuilder WithInterval(TimeSpan interval)
        {
            _interval = interval;
            return this;
        }

        public ILoadTestBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public async Task<LoadResult> RunAsync(Func<Task> action)
        {
            return await RunAsync(async () =>
            {
                try
                {
                    await action();
                    return true;
                }
                catch
                {
                    return false;
                }
            });
        }

        public async Task<LoadResult> RunAsync(Func<Task<bool>> action)
        {
            var executionPlan = new LoadExecutionPlan
            {
                Name = _name,
                Action = action,
                Settings = new LoadSettings
                {
                    Concurrency = _concurrency,
                    Duration = _duration,
                    Interval = _interval
                }
            };

            return await LoadRunner.Run(executionPlan);
        }
    }

    /// <summary>
    /// Configuration options for load tests.
    /// </summary>
    public class LoadTestOptions
    {
        /// <summary>
        /// Gets or sets the test name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the number of concurrent executions.
        /// </summary>
        public int Concurrency { get; set; } = 1;

        /// <summary>
        /// Gets or sets the test duration.
        /// </summary>
        public TimeSpan Duration { get; set; } = TimeSpan.FromSeconds(1);

        /// <summary>
        /// Gets or sets the interval between batches.
        /// </summary>
        public TimeSpan Interval { get; set; } = TimeSpan.FromMilliseconds(100);
    }
}