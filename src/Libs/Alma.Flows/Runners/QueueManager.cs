using Alma.Flows.Core.Abstractions;
using Alma.Flows.Core.Activities.Models;
using Alma.Flows.Core.ApprovalsAndChecks.Enums;
using Alma.Flows.Core.ApprovalsAndChecks.Models;
using Alma.Flows.Core.Contexts;
using Alma.Flows.Enums;
using Alma.Flows.States;
using Microsoft.Extensions.Logging;

namespace Alma.Flows.Runners
{
    /// <summary>
    /// Interface for managing the flow execution queue.
    /// </summary>
    public interface IQueueManager
    {
        #region Navigation Loadings

        /// <summary>
        /// Load property navigations of the queue;
        /// </summary>
        /// <param name="context">The flow execution context.</param>
        void LoadNavigations(FlowExecutionContext context);

        #endregion

        /// <summary>
        /// Enqueues the start activity of the flow.
        /// </summary>
        /// <param name="context">The flow execution context.</param>
        void EnqueueStart(FlowExecutionContext context);

        /// <summary>
        /// Enqueues an activity to the flow execution queue.
        /// </summary>
        /// <param name="context">The flow execution context.</param>
        /// <param name="activity">The activity to be enqueued.</param>
        void Enqueue(FlowExecutionContext context, IActivity activity);

        /// <summary>
        /// Enqueues an activity to the flow execution queue.
        /// </summary>
        /// <param name="context">The flow execution context.</param>
        /// <param name="executedConnection">The executed connection containing the activity to be enqueued.</param>
        void Enqueue(FlowExecutionContext context, ExecutedConnection executedConnection);

        /// <summary>
        /// Gets the next sequential number for the queue.
        /// </summary>
        /// <param name="context">The flow execution context.</param>
        /// <returns>The next sequential number.</returns>
        int GetNextSequential(FlowExecutionContext context);

        /// <summary>
        /// Checks if there are any activities ready to be executed in the queue.
        /// </summary>
        /// <param name="context">The flow execution context.</param>
        /// <returns>True if there are activities ready to be executed, otherwise false.</returns>
        bool HasNext(FlowExecutionContext context);

        /// <summary>
        /// Peeks the next activities ready to be executed in the queue.
        /// </summary>
        /// <param name="context">The flow execution context.</param>
        /// <returns>A collection of queue items ready to be executed.</returns>
        IEnumerable<QueueItem> PeekNextReady(FlowExecutionContext context);

        /// <summary>
        /// Peeks the pending activities in the queue.
        /// </summary>
        /// <param name="context">The flow execution context.</param>
        /// <returns>A collection of pending queue items.</returns>
        IEnumerable<QueueItem> PeekPending(FlowExecutionContext context);

        /// <summary>
        /// Peeks the waiting activities in the queue.
        /// </summary>
        /// <param name="context">The flow execution context.</param>
        /// <returns>A collection of waiting queue items.</returns>
        IEnumerable<QueueItem> PeekWaiting(FlowExecutionContext context);

        IEnumerable<QueueItem> PeekWaitingAndReady(FlowExecutionContext context);

        IEnumerable<QueueItem> PeekCompleted(FlowExecutionContext context);

        IEnumerable<QueueItem> PeekNext(FlowExecutionContext context, int count);

        QueueItem PeekById(FlowExecutionContext context, string id);

        /// <summary>
        /// Marks a queue item as completed.
        /// </summary>
        /// <param name="context">The flow execution context.</param>
        /// <param name="queueItem">The queue item to be marked as completed.</param>
        void Complete(FlowExecutionContext context, QueueItem queueItem);

        /// <summary>
        /// Marks a queue item as pending.
        /// </summary>
        /// <param name="context">The flow execution context.</param>
        /// <param name="queueItem">The queue item to be marked as pending.</param>
        void Pending(FlowExecutionContext context, QueueItem queueItem);

        /// <summary>
        /// Marks a queue item as waiting.
        /// </summary>
        /// <param name="context">The flow execution context.</param>
        /// <param name="queueItem">The queue item to be marked as waiting.</param>
        void Wait(FlowExecutionContext context, QueueItem queueItem, string? reason = null);

        /// <summary>
        /// Marks a queue item as ready.
        /// </summary>
        /// <param name="context">The flow execution context.</param>
        /// <param name="queueItem">The queue item to be marked as ready.</param>
        void Ready(FlowExecutionContext context, QueueItem queueItem);

        /// <summary>
        /// Marks a queue item as failed.
        /// </summary>
        /// <param name="context">The flow execution context.</param>
        /// <param name="queueItem">The queue item to be marked as failed.</param>
        void Fail(FlowExecutionContext context, QueueItem queueItem);

        /// <summary>
        /// Marks a queue item as rejected.
        /// </summary>
        /// <param name="context">The flow execution context.</param>
        /// <param name="queueItem">The queue item to be marked as rejected.</param>
        void Reject(FlowExecutionContext context, QueueItem queueItem);

        /// <summary>
        /// Marks a queue item as approved.
        /// </summary>
        /// <param name="context">The flow execution context.</param>
        /// <param name="queueItem">The queue item to be marked as approved.</param>
        void Approve(FlowExecutionContext context, QueueItem queueItem);

        /// <summary>
        /// Updates the execution status of the queue.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Task UpdateExecutionStatus(FlowExecutionContext context);

        /// <summary>
        /// Updates the execution status of a specific queue item.
        /// </summary>
        Task UpdateExecutionStatus(FlowExecutionContext context, QueueItem queueItem);

        void UpdateExecutionStatus(FlowExecutionContext context, QueueItem item, ActivityValidationResult validationResult);
    }

    /// <summary>
    /// Implementation of <see cref="IQueueManager"/> that manages the flow execution queue.
    /// </summary>
    public class QueueManager : IQueueManager
    {
        private readonly ILogger<QueueManager> _logger;
        private readonly IApprovalAndCheckResolverFactory _approvalAndCheckResolverFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueueManager"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        public QueueManager(ILogger<QueueManager> logger, IApprovalAndCheckResolverFactory approvalAndCheckResolverFactory)
        {
            _logger = logger;
            _approvalAndCheckResolverFactory = approvalAndCheckResolverFactory;
        }

        #region Navigation loading

        public void LoadNavigations(FlowExecutionContext context)
        {
            foreach (var item in context.State.Queue)
            {
                item.Activity = context.Flow.Activities.First(x => x.Id == item.ActivityId);

                if (item.ExecutedConnectionIds is null)
                    continue;

                foreach (var executedConnectionId in item.ExecutedConnectionIds)
                {
                    var executedConnection = context.State.ExecutedConnections.First(x => x.Id == executedConnectionId);
                    item.ExecutedConnections.Add(executedConnection);
                }
            }
        }

        #endregion

        #region Enqueueing

        /// <inheritdoc />
        public void EnqueueStart(FlowExecutionContext context)
        {
            var startActivity = context.Flow.GetStart();

            // To avoid duplicate execution, if the start activity is already in the queue, do not add it again.
            if (context.State.Queue.Any(x => x.ActivityId == startActivity.Id))
                return;

            var item = new QueueItem(startActivity, 0);
            context.State.Queue.Add(item);

            Ready(context, item); // The start activity is always ready for execution.
            Approve(context, item); // The start activity does not require approval.
        }

        /// <inheritdoc />
        public void Enqueue(FlowExecutionContext context, IActivity activity)
        {
            if (!CanEnqueue(context, activity))
                return;

            var item = new QueueItem(activity, GetNextSequential(context));

            context.State.Queue.Add(item);
        }

        public void Enqueue(FlowExecutionContext context, ExecutedConnection executedConnection)
        {
            if (!CanEnqueue(context, executedConnection.Target))
                return;

            var item = new QueueItem(executedConnection, GetNextSequential(context));

            context.State.Queue.Add(item);
        }

        /// <inheritdoc />
        public int GetNextSequential(FlowExecutionContext context)
        {
            return context.State.Queue.Count + 1;
        }

        #endregion

        #region Peeking

        /// <inheritdoc />
        public bool HasNext(FlowExecutionContext context)
        {
            return context.State.Queue.Any(x => x.CanExecute);
        }

        /// <inheritdoc />
        public IEnumerable<QueueItem> PeekNextReady(FlowExecutionContext context)
        {
            if (context.State.Queue.Count == 0)
                EnqueueStart(context);

            var count = context.Options.MaxDegreeOfParallelism;
            count = count > 0 ? count : 1;

            var items = context.State.Queue
                .Where(x => x.ExecutionStatus == ActivityExecutionStatus.Ready)
                .OrderBy(x => x.Sequential)
                .Take(count);

            if (!items.Any())
                throw new InvalidOperationException("Queue doesn't have pending items.");

            return items;
        }

        /// <inheritdoc />
        public IEnumerable<QueueItem> PeekPending(FlowExecutionContext context)
        {
            return context.State.Queue.Where(x => x.ExecutionStatus == ActivityExecutionStatus.Pending);
        }

        /// <inheritdoc />
        public IEnumerable<QueueItem> PeekWaiting(FlowExecutionContext context)
        {
            return context.State.Queue.Where(x => x.ExecutionStatus == ActivityExecutionStatus.Waiting);
        }

        public IEnumerable<QueueItem> PeekWaitingAndReady(FlowExecutionContext context)
        {
            return context.State.Queue.Where(x =>
                x.ExecutionStatus == ActivityExecutionStatus.Waiting
                || x.ExecutionStatus == ActivityExecutionStatus.Ready);
        }

        public IEnumerable<QueueItem> PeekCompleted(FlowExecutionContext context)
        {
            return context.State.Queue.Where(x => x.ExecutionStatus == ActivityExecutionStatus.Completed);
        }

        public IEnumerable<QueueItem> PeekNext(FlowExecutionContext context, int count)
        {
            return context.State.Queue.OrderBy(x => x.Sequential)
                .Where(x =>
                    x.ExecutionStatus == ActivityExecutionStatus.Pending
                    || x.ExecutionStatus == ActivityExecutionStatus.Waiting
                    || x.ExecutionStatus == ActivityExecutionStatus.WaitingApprovalAndChecks
                    || x.ExecutionStatus == ActivityExecutionStatus.Ready)
                .Take(count);
        }

        public QueueItem PeekById(FlowExecutionContext context, string id)
        {
            return context.State.Queue.First(x => x.Id == id);
        }

        #endregion

        #region Queue execution status managing

        public async Task UpdateExecutionStatus(FlowExecutionContext context)
        {
            UpdatePendingItems(context);
            await UpdateWaitingAndReadyStatus(context);
        }

        public async Task UpdateExecutionStatus(FlowExecutionContext context, QueueItem queueItem)
        {
            await ResolveReadyStatus(context, queueItem);
            await ResolveApprovalAndChecks(context, queueItem);
        }

        public void UpdatePendingItems(FlowExecutionContext context)
        {
            // Check if there are any pending activities that can be executed.
            // Pending activities are those that are waiting for the execution of
            // the activities that are connected to them.
            foreach (var item in PeekPending(context))
            {
                //if (RequiredConnectionsExecuted(context, item))
                Wait(context, item);
            }
        }

        public async Task UpdateWaitingAndReadyStatus(FlowExecutionContext context)
        {
            // Check if there are any waiting activities that can be executed.
            foreach (var item in PeekWaitingAndReady(context))
            {
                await ResolveReadyStatus(context, item);
                await ResolveApprovalAndChecks(context, item);
            }
        }

        public async Task ResolveReadyStatus(FlowExecutionContext context, QueueItem item)
        {
            // if (!RequiredConnectionsExecuted(context, item))
            // {
            //     Wait(context, item, "Aguardando atividades anteriores.");
            //     return;
            // }

            var activity = context.Flow.Activities.First(x => x.Id == item.ActivityId);
            var isReadyResult = await activity.IsReadyToExecuteAsync(context);

            if (isReadyResult.IsReady)
                Ready(context, item);
            else
                Wait(context, item, isReadyResult.Reason);
        }

        public async Task ResolveApprovalAndChecks(FlowExecutionContext context, QueueItem item)
        {
            var activity = context.Flow.Activities.First(x => x.Id == item.ActivityId);
            var approvalAndCheckResults = new List<ApprovalAndCheckResult>();

            foreach (var approvalAndCheck in activity.ApprovalAndChecks)
            {
                var resolver = _approvalAndCheckResolverFactory.Create(approvalAndCheck, context.State, context.Options);
                approvalAndCheckResults.Add(await resolver.Resolve());
            }

            if (approvalAndCheckResults.Any(x => x.Status == ApprovalAndCheckStatus.Rejected))
                Reject(context, item);
            else if (approvalAndCheckResults.All(x => x.Status == ApprovalAndCheckStatus.Approved))
                Approve(context, item);
            else
                Wait(context, item);
        }

        public void UpdateExecutionStatus(FlowExecutionContext context, QueueItem item, ActivityValidationResult validationResult)
        {
            item.ExecutionStatus = validationResult.ReadinessStatus;
            item.ExecutionStatusReason = validationResult.ReadinessDetails;
            item.ApprovalAndCheckStatus = validationResult.ApprovalStatus;
        }

        #endregion

        #region Status modifiers

        /// <inheritdoc />
        public void Complete(FlowExecutionContext context, QueueItem queueItem)
        {
            var item = context.State.Queue.First(x => x.Sequential == queueItem.Sequential);
            item.ExecutionStatus = ActivityExecutionStatus.Completed;
        }

        /// <inheritdoc />
        public void Pending(FlowExecutionContext context, QueueItem queueItem)
        {
            var item = context.State.Queue.First(x => x.Sequential == queueItem.Sequential);
            item.ExecutionStatus = ActivityExecutionStatus.Pending;
        }

        /// <inheritdoc />
        public void Wait(FlowExecutionContext context, QueueItem queueItem, string? reason = null)
        {
            var item = context.State.Queue.First(x => x.Sequential == queueItem.Sequential);
            item.ExecutionStatus = ActivityExecutionStatus.Waiting;
            item.ExecutionStatusReason = reason;
        }

        /// <inheritdoc />
        public void Fail(FlowExecutionContext context, QueueItem queueItem)
        {
            var item = context.State.Queue.First(x => x.Sequential == queueItem.Sequential);
            item.ExecutionStatus = ActivityExecutionStatus.Failed;
        }

        /// <inheritdoc />
        public void Ready(FlowExecutionContext context, QueueItem queueItem)
        {
            var item = context.State.Queue.First(x => x.Sequential == queueItem.Sequential);
            item.ExecutionStatus = ActivityExecutionStatus.Ready;
            item.ExecutionStatusReason = null;
        }

        /// <inheritdoc />
        public void Reject(FlowExecutionContext context, QueueItem queueItem)
        {
            var item = context.State.Queue.First(x => x.Sequential == queueItem.Sequential);
            item.ApprovalAndCheckStatus = ApprovalAndCheckStatus.Rejected;
            item.ExecutionStatus = ActivityExecutionStatus.Failed;
        }

        /// <inheritdoc />
        public void Approve(FlowExecutionContext context, QueueItem queueItem)
        {
            var item = context.State.Queue.First(x => x.Sequential == queueItem.Sequential);
            item.ApprovalAndCheckStatus = ApprovalAndCheckStatus.Approved;
        }

        #endregion

        #region Private methods

        private static bool RequiredConnectionsExecuted(FlowExecutionContext context, QueueItem item)
        {
            var activity = context.Flow.Activities.First(x => x.Id == item.ActivityId);

            if (activity is IStart)
                return true;

            var portDescriptor = activity.Descriptor.Ports.First(x => x.Type == Core.Activities.Base.PortType.Input);
            var connections = context.Flow.Connections.Where(x => x.Target.ActivityId == activity.Id && x.Target.PortName == portDescriptor.Name);

            var allConnectionExecuted = true;

            foreach (var connection in connections)
            {
                if (!context.State.ExecutedConnections.Any(x => x.ConnectionId == connection.Id))
                {
                    allConnectionExecuted = false;
                    break;
                }
            }

            // If all connections have been executed, the activity status evolves to Waiting.
            if (allConnectionExecuted)
                return true;

            return false;
        }

        private static bool CanEnqueue(FlowExecutionContext context, IActivity activity)
        {
            // To avoid duplicate execution, if the same activity is already in the queue
            // and its status is not completed or failed, do not add it again.
            if (context.State.Queue.Any(x => x.ActivityId == activity.Id && (x.ExecutionStatus != ActivityExecutionStatus.Completed && x.ExecutionStatus != ActivityExecutionStatus.Failed)))
                return false;

            return true;
        }

        #endregion
    }
}