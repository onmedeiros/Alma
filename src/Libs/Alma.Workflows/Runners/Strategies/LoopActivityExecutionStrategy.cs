using Alma.Workflows.Activities.Flow;
using Alma.Workflows.Core.Abstractions;
using Alma.Workflows.Core.Activities.Base;
using Alma.Workflows.Core.Activities.Enums;
using Alma.Workflows.Core.Activities.Models;
using Alma.Workflows.Core.Contexts;
using Alma.Workflows.Enums;
using Alma.Workflows.States;
using Microsoft.Extensions.Logging;

namespace Alma.Workflows.Runners.Strategies
{
    /// <summary>
    /// Specialized execution strategy for Loop activities.
    /// Handles the complex lifecycle of loop execution including body iteration.
    /// </summary>
    public class LoopActivityExecutionStrategy : IActivityExecutionStrategy
    {
        private readonly ILogger<LoopActivityExecutionStrategy> _logger;
        private readonly IQueueManager _queueManager;

        public LoopActivityExecutionStrategy(
            ILogger<LoopActivityExecutionStrategy> logger,
            IQueueManager _queueManager)
        {
            _logger = logger;
            this._queueManager = _queueManager;
        }

        public bool CanHandle(IActivity activity)
        {
            return activity is LoopActivity;
        }

        public async Task<ActivityExecutionResult> ExecuteAsync(
            IActivity activity,
            WorkflowExecutionContext context,
            ActivityRunner runner)
        {
            _logger.LogDebug("Executing loop activity {ActivityId} using LoopExecutionStrategy", activity.Id);

            var result = await runner.ExecuteAsync();

            return result;
        }

        public Task HandlePostExecutionAsync(
            IActivity activity,
            WorkflowExecutionContext context,
            ActivityExecutionResult result,
            QueueItem queueItem)
        {
            if (result.ExecutionStatus != Enums.ActivityExecutionStatus.Completed)
            {
                // Handle non-completed statuses
                HandleNonCompletedExecution(activity, context, result, queueItem);
                return Task.CompletedTask;
            }

            var executedPorts = result.ExecutedPorts;

            // Check if this is a loop body execution (not Done or Error)
            if (IsLoopBodyExecution(executedPorts))
            {
                HandleLoopBodyExecution(activity, context, queueItem);
            }
            else
            {
                // Loop completed (Done or Error port executed)
                _logger.LogInformation("Loop activity {ActivityId} completed", activity.Id);
                _queueManager.Complete(context, queueItem);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Checks if the loop executed the Body port (not Done or Error).
        /// </summary>
        private bool IsLoopBodyExecution(IEnumerable<Port> executedPorts)
        {
            var bodyPortExecuted = executedPorts.Any(p => p.Descriptor.Name == nameof(LoopActivity.Body));
            var donePortExecuted = executedPorts.Any(p => p.Descriptor.Name == nameof(LoopActivity.Done));
            var errorPortExecuted = executedPorts.Any(p => p.Descriptor.Name == nameof(LoopActivity.Error));

            return bodyPortExecuted && !donePortExecuted && !errorPortExecuted;
        }

        /// <summary>
        /// Handles the execution when the loop body port was executed.
        /// Marks the loop as waiting for the body to complete.
        /// </summary>
        private void HandleLoopBodyExecution(
            IActivity activity,
            WorkflowExecutionContext context,
            QueueItem queueItem)
        {
            _logger.LogInformation(
                "Loop activity {ActivityId} executed Body port, waiting for body completion",
                activity.Id);

            // Mark loop as waiting (not completed) so it can be re-executed later
            _queueManager.Wait(context, queueItem, "Waiting for loop body to complete");
        }

        /// <summary>
        /// Handles non-completed execution statuses (Waiting, Failed, etc.)
        /// </summary>
        private void HandleNonCompletedExecution(
            IActivity activity,
            WorkflowExecutionContext context,
            ActivityExecutionResult result,
            QueueItem queueItem)
        {
            switch (result.ExecutionStatus)
            {
                case Enums.ActivityExecutionStatus.Waiting:
                    _logger.LogInformation("Loop activity {ActivityId} is waiting", activity.Id);
                    _queueManager.Wait(context, queueItem, result.ExecutionStatusDetails);
                    break;

                case Enums.ActivityExecutionStatus.Failed:
                    _logger.LogError("Loop activity {ActivityId} failed: {Reason}",
                        activity.Id, result.ExecutionStatusDetails);
                    _queueManager.Fail(context, queueItem);
                    break;

                default:
                    _logger.LogWarning("Loop activity {ActivityId} finished with unexpected status: {Status}",
                        activity.Id, result.ExecutionStatus);
                    _queueManager.Fail(context, queueItem);
                    break;
            }
        }
    }
}
