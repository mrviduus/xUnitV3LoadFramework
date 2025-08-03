using System;
using System.Reflection;
using System.Threading.Tasks;
using xUnitV3LoadFramework.Attributes;
using xUnitV3LoadFramework.LoadRunnerCore.Models;
using xUnitV3LoadFramework.LoadRunnerCore.Runner;

namespace xUnitV3LoadFramework.Extensions
{
    /// <summary>
    /// Helper class for executing load tests with LoadFact attributes.
    /// Provides integration between standard xUnit test execution and load testing infrastructure.
    /// </summary>
    public static class LoadTestHelper
    {
        /// <summary>
        /// Executes the current test method as a load test if it has a LoadFact attribute.
        /// This method should be called at the beginning of a LoadFact test method.
        /// </summary>
        /// <param name="testAction">The test action to execute under load</param>
        /// <param name="testMethodName">Optional test method name. If not provided, will be inferred from the call stack.</param>
        /// <returns>A task representing the load test execution</returns>
        public static async Task<LoadResult> ExecuteLoadTestAsync(Func<Task<bool>> testAction, string? testMethodName = null)
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
        public static async Task<LoadResult> ExecuteLoadTestAsync(Func<bool> testAction, string? testMethodName = null)
        {
            return await ExecuteLoadTestAsync(() => Task.FromResult(testAction()), testMethodName);
        }

        /// <summary>
        /// Executes the current test method as a load test if it has a LoadFact attribute.
        /// This overload is for test actions that don't return a success indicator (assumes success if no exception).
        /// </summary>
        /// <param name="testAction">The test action to execute under load</param>
        /// <param name="testMethodName">Optional test method name. If not provided, will be inferred from the call stack.</param>
        /// <returns>A task representing the load test execution</returns>
        public static async Task<LoadResult> ExecuteLoadTestAsync(Func<Task> testAction, string? testMethodName = null)
        {
            return await ExecuteLoadTestAsync(async () =>
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
        public static async Task<LoadResult> ExecuteLoadTestAsync(Action testAction, string? testMethodName = null)
        {
            return await ExecuteLoadTestAsync(() =>
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
}
