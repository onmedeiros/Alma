using Alma.Workflows.Activities.Interaction;
using Alma.Workflows.Core.Activities.Abstractions;
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
    /// Specialized execution strategy for user interaction activities.
    /// Handles activities that require user input or approval.
    /// </summary>
    public class UserInteractionActivityExecutionStrategy : IActivityExecutionStrategy
    {
        private readonly ILogger<UserInteractionActivityExecutionStrategy> _logger;
        private readonly IQueueManager _queueManager;

        public UserInteractionActivityExecutionStrategy(
            ILogger<UserInteractionActivityExecutionStrategy> logger,
            IQueueManager queueManager)
        {
            _logger = logger;
            _queueManager = queueManager;
        }

        public bool CanHandle(IActivity activity)
        {
            // This strategy handles UserInteractionActivity and FormActivity
            return activity is UserInteractionActivity or FormActivity;
        }

        public async Task<ActivityExecutionResult> ExecuteAsync(
            IActivity activity,
            WorkflowExecutionContext context,
            ActivityRunner runner)
        {
            _logger.LogDebug(
                "Executing user interaction activity {ActivityId} ({ActivityType}) using UserInteractionStrategy",
                activity.Id, 
                activity.GetType().Name);

            // Execute the activity
            var result = await runner.ExecuteAsync();

            // Log interaction activity execution
            if (result.ExecutionStatus == Enums.ActivityExecutionStatus.Waiting)
            {
                _logger.LogInformation(
                    "User interaction activity {ActivityId} is waiting for user input",
                    activity.Id);
            }

            return result;
        }

        public Task HandlePostExecutionAsync(
            IActivity activity,
            WorkflowExecutionContext context,
            ActivityExecutionResult result,
            QueueItem queueItem)
        {
            switch (result.ExecutionStatus)
            {
                case Enums.ActivityExecutionStatus.Completed:
                    _logger.LogInformation(
                        "User interaction activity {ActivityId} completed - user provided input",
                        activity.Id);
                    _queueManager.Complete(context, queueItem);
                    break;

                case Enums.ActivityExecutionStatus.Waiting:
                    _logger.LogInformation(
                        "User interaction activity {ActivityId} is waiting for user input",
                        activity.Id);
                    _queueManager.Wait(context, queueItem, "Waiting for user input");
                    break;

                case Enums.ActivityExecutionStatus.Failed:
                    _logger.LogError(
                        "User interaction activity {ActivityId} failed: {Reason}",
                        activity.Id, 
                        result.ExecutionStatusDetails);
                    _queueManager.Fail(context, queueItem);
                    break;

                default:
                    _logger.LogWarning(
                        "User interaction activity {ActivityId} finished with unexpected status: {Status}",
                        activity.Id, 
                        result.ExecutionStatus);
                    _queueManager.Fail(context, queueItem);
                    break;
            }

            return Task.CompletedTask;
        }
    }
}
