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
    /// Standard execution strategy for regular activities.
    /// This is the default strategy used for most activity types.
    /// </summary>
    public class StandardActivityExecutionStrategy : IActivityExecutionStrategy
    {
        private readonly ILogger<StandardActivityExecutionStrategy> _logger;
        private readonly IQueueManager _queueManager;

        public StandardActivityExecutionStrategy(
            ILogger<StandardActivityExecutionStrategy> logger,
            IQueueManager queueManager)
        {
            _logger = logger;
            this._queueManager = queueManager;
        }

        /// <summary>
        /// This strategy can handle any activity that is not handled by a specialized strategy.
        /// It acts as a fallback/default strategy.
        /// </summary>
        public virtual bool CanHandle(IActivity activity)
        {
            // This is the default strategy, so it can handle any activity
            // However, more specific strategies should be checked first
            return true;
        }

        public virtual async Task<ActivityExecutionResult> ExecuteAsync(
            IActivity activity,
            FlowExecutionContext context,
            ActivityRunner runner)
        {
            _logger.LogDebug("Executing activity {ActivityId} ({ActivityType}) using StandardExecutionStrategy",
                activity.Id, activity.GetType().Name);

            // Execute the activity using the runner
            var result = await runner.ExecuteAsync();

            return result;
        }

        public virtual Task HandlePostExecutionAsync(
            IActivity activity,
            FlowExecutionContext context,
            ActivityExecutionResult result,
            QueueItem queueItem)
        {
            switch (result.ExecutionStatus)
            {
                case Enums.ActivityExecutionStatus.Completed:
                    _logger.LogInformation("Activity {ActivityId} completed successfully", activity.Id);
                    _queueManager.Complete(context, queueItem);
                    break;

                case Enums.ActivityExecutionStatus.Waiting:
                    _logger.LogInformation("Activity {ActivityId} is waiting", activity.Id);
                    _queueManager.Wait(context, queueItem, result.ExecutionStatusDetails);
                    break;

                case Enums.ActivityExecutionStatus.Failed:
                    _logger.LogError("Activity {ActivityId} failed: {Reason}", 
                        activity.Id, result.ExecutionStatusDetails);
                    _queueManager.Fail(context, queueItem);
                    break;

                default:
                    _logger.LogWarning("Activity {ActivityId} finished with unexpected status: {Status}",
                        activity.Id, result.ExecutionStatus);
                    _queueManager.Fail(context, queueItem);
                    break;
            }

            return Task.CompletedTask;
        }
    }
}
