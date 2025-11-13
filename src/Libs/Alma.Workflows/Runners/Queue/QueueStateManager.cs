using Alma.Workflows.Core.ApprovalsAndChecks.Enums;
using Alma.Workflows.Core.Contexts;
using Alma.Workflows.Enums;
using Alma.Workflows.States;
using Microsoft.Extensions.Logging;

namespace Alma.Workflows.Runners.Queue
{
    /// <summary>
    /// Implementation of queue state management operations.
    /// Handles transitions between different execution statuses.
    /// </summary>
    public class QueueStateManager : IQueueStateManager
    {
        private readonly ILogger<QueueStateManager> _logger;

        public QueueStateManager(ILogger<QueueStateManager> logger)
        {
            _logger = logger;
        }

        public void Complete(FlowExecutionContext context, QueueItem queueItem)
        {
            var item = FindQueueItem(context, queueItem);
            item.ExecutionStatus = ActivityExecutionStatus.Completed;

            _logger.LogDebug("Queue item {ItemId} (Activity: {ActivityId}) marked as Completed",
                item.Id, item.ActivityId);
        }

        public void Pending(FlowExecutionContext context, QueueItem queueItem)
        {
            var item = FindQueueItem(context, queueItem);
            item.ExecutionStatus = ActivityExecutionStatus.Pending;

            _logger.LogDebug("Queue item {ItemId} (Activity: {ActivityId}) marked as Pending",
                item.Id, item.ActivityId);
        }

        public void Wait(FlowExecutionContext context, QueueItem queueItem, string? reason = null)
        {
            var item = FindQueueItem(context, queueItem);
            item.ExecutionStatus = ActivityExecutionStatus.Waiting;
            item.ExecutionStatusReason = reason;

            _logger.LogDebug("Queue item {ItemId} (Activity: {ActivityId}) marked as Waiting. Reason: {Reason}",
                item.Id, item.ActivityId, reason ?? "No reason provided");
        }

        public void Ready(FlowExecutionContext context, QueueItem queueItem)
        {
            var item = FindQueueItem(context, queueItem);
            item.ExecutionStatus = ActivityExecutionStatus.Ready;
            item.ExecutionStatusReason = null;

            _logger.LogDebug("Queue item {ItemId} (Activity: {ActivityId}) marked as Ready",
                item.Id, item.ActivityId);
        }

        public void Fail(FlowExecutionContext context, QueueItem queueItem)
        {
            var item = FindQueueItem(context, queueItem);
            item.ExecutionStatus = ActivityExecutionStatus.Failed;

            _logger.LogWarning("Queue item {ItemId} (Activity: {ActivityId}) marked as Failed",
                item.Id, item.ActivityId);
        }

        public void Reject(FlowExecutionContext context, QueueItem queueItem)
        {
            var item = FindQueueItem(context, queueItem);
            item.ApprovalAndCheckStatus = ApprovalAndCheckStatus.Rejected;
            item.ExecutionStatus = ActivityExecutionStatus.Failed;

            _logger.LogWarning("Queue item {ItemId} (Activity: {ActivityId}) marked as Rejected",
                item.Id, item.ActivityId);
        }

        public void Approve(FlowExecutionContext context, QueueItem queueItem)
        {
            var item = FindQueueItem(context, queueItem);
            item.ApprovalAndCheckStatus = ApprovalAndCheckStatus.Approved;

            _logger.LogDebug("Queue item {ItemId} (Activity: {ActivityId}) marked as Approved",
                item.Id, item.ActivityId);
        }

        /// <summary>
        /// Finds a queue item in the state queue by its sequential number.
        /// </summary>
        private QueueItem FindQueueItem(FlowExecutionContext context, QueueItem queueItem)
        {
            var item = context.State.Queue.FirstOrDefault(x => x.Sequential == queueItem.Sequential);

            if (item == null)
            {
                _logger.LogError("Queue item with Sequential {Sequential} not found in state queue", queueItem.Sequential);
                throw new InvalidOperationException($"Queue item with Sequential {queueItem.Sequential} not found.");
            }

            return item;
        }
    }
}
