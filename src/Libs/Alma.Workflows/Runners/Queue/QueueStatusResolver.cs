using Alma.Workflows.Core.Activities.Models;
using Alma.Workflows.Core.ApprovalsAndChecks.Enums;
using Alma.Workflows.Core.ApprovalsAndChecks.Models;
using Alma.Workflows.Core.Contexts;
using Alma.Workflows.States;
using Microsoft.Extensions.Logging;

namespace Alma.Workflows.Runners.Queue
{
    /// <summary>
    /// Implementation of queue status resolution operations.
    /// Determines if activities are ready to execute and handles approvals.
    /// </summary>
    public class QueueStatusResolver : IQueueStatusResolver
    {
        private readonly ILogger<QueueStatusResolver> _logger;
        private readonly IQueueNavigator _navigator;
        private readonly IQueueStateManager _stateManager;
        private readonly IApprovalAndCheckResolverFactory _approvalAndCheckResolverFactory;

        public QueueStatusResolver(
            ILogger<QueueStatusResolver> logger,
            IQueueNavigator navigator,
            IQueueStateManager stateManager,
            IApprovalAndCheckResolverFactory approvalAndCheckResolverFactory)
        {
            _logger = logger;
            _navigator = navigator;
            _stateManager = stateManager;
            _approvalAndCheckResolverFactory = approvalAndCheckResolverFactory;
        }

        public async Task UpdateExecutionStatusAsync(FlowExecutionContext context)
        {
            _logger.LogDebug("Updating execution status for all queue items");

            // Update pending items to waiting
            UpdatePendingItems(context);

            // Update waiting and ready items
            await UpdateWaitingAndReadyStatusAsync(context);
        }

        public async Task UpdateExecutionStatusAsync(FlowExecutionContext context, QueueItem queueItem)
        {
            _logger.LogDebug("Updating execution status for queue item {ItemId} (Activity: {ActivityId})",
                queueItem.Id, queueItem.ActivityId);

            await ResolveReadyStatusAsync(context, queueItem);
            await ResolveApprovalAndChecksAsync(context, queueItem);
        }

        public void UpdateExecutionStatus(FlowExecutionContext context, QueueItem item, ActivityValidationResult validationResult)
        {
            item.ExecutionStatus = validationResult.ReadinessStatus;
            item.ExecutionStatusReason = validationResult.ReadinessDetails;
            item.ApprovalAndCheckStatus = validationResult.ApprovalStatus;

            _logger.LogDebug(
                "Updated queue item {ItemId} execution status: {Status}, Approval: {ApprovalStatus}",
                item.Id,
                validationResult.ReadinessStatus,
                validationResult.ApprovalStatus);
        }

        public async Task ResolveReadyStatusAsync(FlowExecutionContext context, QueueItem item)
        {
            var activity = context.Flow.Activities.First(x => x.Id == item.ActivityId);
            var isReadyResult = await activity.IsReadyToExecuteAsync(context);

            if (isReadyResult.IsReady)
            {
                _stateManager.Ready(context, item);
            }
            else
            {
                _stateManager.Wait(context, item, isReadyResult.Reason);
            }
        }

        public async Task ResolveApprovalAndChecksAsync(FlowExecutionContext context, QueueItem item)
        {
            var activity = context.Flow.Activities.First(x => x.Id == item.ActivityId);
            var approvalAndCheckResults = new List<ApprovalAndCheckResult>();

            foreach (var approvalAndCheck in activity.ApprovalAndChecks)
            {
                var resolver = _approvalAndCheckResolverFactory.Create(approvalAndCheck, context.State, context.Options);
                approvalAndCheckResults.Add(await resolver.Resolve());
            }

            if (approvalAndCheckResults.Any(x => x.Status == ApprovalAndCheckStatus.Rejected))
            {
                _stateManager.Reject(context, item);
            }
            else if (approvalAndCheckResults.All(x => x.Status == ApprovalAndCheckStatus.Approved))
            {
                _stateManager.Approve(context, item);
            }
            else
            {
                _stateManager.Wait(context, item, "Waiting for approvals and checks");
            }
        }

        private void UpdatePendingItems(FlowExecutionContext context)
        {
            // Check if there are any pending activities that can be executed.
            // Pending activities are those that are waiting for the execution of
            // the activities that are connected to them.
            foreach (var item in _navigator.PeekPending(context))
            {
                _stateManager.Wait(context, item, "Waiting for required connections");
            }
        }

        private async Task UpdateWaitingAndReadyStatusAsync(FlowExecutionContext context)
        {
            // Check if there are any waiting activities that can be executed.
            foreach (var item in _navigator.PeekWaitingAndReady(context))
            {
                await ResolveReadyStatusAsync(context, item);
                await ResolveApprovalAndChecksAsync(context, item);
            }
        }
    }
}
