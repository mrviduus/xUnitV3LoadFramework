using Akka.Actor;
using xUnitV3LoadFramework.LoadRunnerCore.Actors;
using xUnitV3LoadFramework.LoadRunnerCore.Configuration;
using xUnitV3LoadFramework.LoadRunnerCore.Messages;
using xUnitV3LoadFramework.LoadRunnerCore.Models;

namespace xUnitV3LoadFramework.LoadRunnerCore.Runner
{
	public static class LoadRunner
	{
		public static async Task<LoadResult> Run(LoadExecutionPlan executionPlan)
		{
			return await Run(executionPlan, new LoadWorkerConfiguration());
		}

		public static async Task<LoadResult> Run(
			LoadExecutionPlan executionPlan, 
			LoadWorkerConfiguration? configuration = null)
		{
			if (executionPlan.Action == null)
				throw new ArgumentNullException(nameof(executionPlan.Action));

			configuration ??= new LoadWorkerConfiguration();

			// Concurrency is handled by the worker, so we only spawn one worker actor here.
			using var actorSystem = ActorSystem.Create("LoadTestSystem");
			var resultCollector = actorSystem.ActorOf(
				Props.Create(() => new ResultCollectorActor(executionPlan.Name)),
				"resultCollector"
			);

			// Create the appropriate load worker actor based on configuration
			var loadWorkerProps = configuration.Mode switch
			{
				LoadWorkerMode.TaskBased => Props.Create(() => 
					new LoadWorkerActor(executionPlan, resultCollector)),
					
				LoadWorkerMode.Hybrid => Props.Create(() => 
					new LoadWorkerActorHybrid(executionPlan, resultCollector)),
					
				_ => throw new ArgumentException($"LoadWorkerMode {configuration.Mode} is not yet implemented")
			};

			var worker = actorSystem.ActorOf(loadWorkerProps, "worker");

			// Ask the worker to start and wait for its final LoadResult.
			await worker.Ask<LoadResult>(
				new StartLoadMessage(),
				TimeSpan.FromSeconds(executionPlan.Settings.Duration.TotalSeconds + 5)
			);

			// Ask the result collector for the aggregated results.
			var finalResult = await resultCollector.Ask<LoadResult>(
				new GetLoadResultMessage(),
				TimeSpan.FromSeconds(executionPlan.Settings.Duration.TotalSeconds + 5)
			);

			return finalResult;
		}
	}
}