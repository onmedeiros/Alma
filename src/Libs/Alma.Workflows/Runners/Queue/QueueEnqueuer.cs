using Alma.Workflows.Core.Abstractions;
using Alma.Workflows.Core.ApprovalsAndChecks.Models;
using Alma.Workflows.Core.Contexts;
using Alma.Workflows.Enums;
using Alma.Workflows.States;
using Microsoft.Extensions.Logging;

namespace Alma.Workflows.Runners.Queue
{
    /// <summary>
    /// Implementation of queue enqueueing operations.
    /// Responsible for adding activities to the execution queue.
    /// </summary>
    public class QueueEnqueuer : IQueueEnqueuer
    {
        private readonly ILogger<QueueEnqueuer> _logger;
        private readonly IQueueStateManager _stateManager;

        public QueueEnqueuer(
            ILogger<QueueEnqueuer> logger,
            IQueueStateManager stateManager)
        {
            _logger = logger;
            _stateManager = stateManager;
        }

        public void EnqueueStart(FlowExecutionContext context)
        {
            var startActivity = context.Flow.GetStart();

            // To avoid duplicate execution, if the start activity is already in the queue, do not add it again.
            if (context.State.Queue.Any(x => x.ActivityId == startActivity.Id))
            {
                _logger.LogDebug("Start activity {ActivityId} already in queue, skipping enqueue", startActivity.Id);
                return;
            }

            _logger.LogInformation("Enqueueing start activity {ActivityId}", startActivity.Id);

            var item = new QueueItem(startActivity, 0);
            context.State.Queue.Add(item);

            // The start activity is always ready for execution and does not require approval
            _stateManager.Ready(context, item);
            _stateManager.Approve(context, item);
        }

        public void Enqueue(FlowExecutionContext context, IActivity activity)
        {
            if (!CanEnqueue(context, activity))
            {
                _logger.LogDebug(
                    "Activity {ActivityId} ({ActivityType}) cannot be enqueued (already in queue or invalid state)",
                    activity.Id,
                    activity.GetType().Name);
                return;
            }

            _logger.LogDebug("Enqueueing activity {ActivityId} ({ActivityType})",
                activity.Id,
                activity.GetType().Name);

            var item = new QueueItem(activity, GetNextSequential(context));
            context.State.Queue.Add(item);
        }

        public void Enqueue(FlowExecutionContext context, ExecutedConnection executedConnection)
        {
            if (!CanEnqueue(context, executedConnection.Target))
            {
                _logger.LogDebug(
                    "Activity {ActivityId} from executed connection cannot be enqueued",
                    executedConnection.Target.Id);
                return;
            }

            _logger.LogDebug("Enqueueing activity {ActivityId} from executed connection",
                executedConnection.Target.Id);

            var item = new QueueItem(executedConnection, GetNextSequential(context));
            context.State.Queue.Add(item);
        }

        public int GetNextSequential(FlowExecutionContext context)
        {
            return context.State.Queue.Count + 1;
        }

        public bool CanEnqueue(FlowExecutionContext context, IActivity activity)
        {
            // To avoid duplicate execution, if the same activity is already in the queue
            // and its status is not completed or failed, do not add it again.
            var existingItem = context.State.Queue.FirstOrDefault(x => x.ActivityId == activity.Id);

            if (existingItem == null)
                return true;

            // Can re-enqueue if the activity previously completed or failed
            var canReEnqueue = existingItem.ExecutionStatus == ActivityExecutionStatus.Completed
                            || existingItem.ExecutionStatus == ActivityExecutionStatus.Failed;

            if (!canReEnqueue)
            {
                _logger.LogDebug(
                    "Activity {ActivityId} is already in queue with status {Status}, cannot enqueue",
                    activity.Id,
                    existingItem.ExecutionStatus);
            }

            return canReEnqueue;
        }
    }
}
