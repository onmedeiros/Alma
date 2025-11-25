using Alma.Workflows.Core.Abstractions;
using Alma.Workflows.Core.Activities.Models;
using Alma.Workflows.Core.ApprovalsAndChecks.Enums;
using Alma.Workflows.Core.ApprovalsAndChecks.Models;
using Alma.Workflows.Core.Contexts;
using Alma.Workflows.Enums;
using Alma.Workflows.States;
using Microsoft.Extensions.Logging;

namespace Alma.Workflows.Runners
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
        void LoadNavigations(WorkflowExecutionContext context);

        #endregion

        /// <summary>
        /// Enqueues the start activity of the flow.
        /// </summary>
        /// <param name="context">The flow execution context.</param>
        void EnqueueStart(WorkflowExecutionContext context);

        /// <summary>
        /// Enqueues an activity to the flow execution queue.
        /// </summary>
        /// <param name="context">The flow execution context.</param>
        /// <param name="activity">The activity to be enqueued.</param>
        void Enqueue(WorkflowExecutionContext context, IActivity activity);

        /// <summary>
        /// Enqueues an activity to the flow execution queue.
        /// </summary>
        /// <param name="context">The flow execution context.</param>
        /// <param name="executedConnection">The executed connection containing the activity to be enqueued.</param>
        void Enqueue(WorkflowExecutionContext context, ExecutedConnection executedConnection);

        /// <summary>
        /// Gets the next sequential number for the queue.
        /// </summary>
        /// <param name="context">The flow execution context.</param>
        /// <returns>The next sequential number.</returns>
        int GetNextSequential(WorkflowExecutionContext context);

        /// <summary>
        /// Checks if there are any activities ready to be executed in the queue.
        /// </summary>
        /// <param name="context">The flow execution context.</param>
        /// <returns>True if there are activities ready to be executed, otherwise false.</returns>
        bool HasNext(WorkflowExecutionContext context);

        /// <summary>
        /// Peeks the next activities ready to be executed in the queue.
        /// </summary>
        /// <param name="context">The flow execution context.</param>
        /// <returns>A collection of queue items ready to be executed.</returns>
        IEnumerable<QueueItem> PeekNextReady(WorkflowExecutionContext context);

        /// <summary>
        /// Peeks the pending activities in the queue.
        /// </summary>
        /// <param name="context">The flow execution context.</param>
        /// <returns>A collection of pending queue items.</returns>
        IEnumerable<QueueItem> PeekPending(WorkflowExecutionContext context);

        /// <summary>
        /// Peeks the waiting activities in the queue.
        /// </summary>
        /// <param name="context">The flow execution context.</param>
        /// <returns>A collection of waiting queue items.</returns>
        IEnumerable<QueueItem> PeekWaiting(WorkflowExecutionContext context);

        IEnumerable<QueueItem> PeekWaitingAndReady(WorkflowExecutionContext context);

        IEnumerable<QueueItem> PeekCompleted(WorkflowExecutionContext context);

        IEnumerable<QueueItem> PeekNext(WorkflowExecutionContext context, int count);

        QueueItem PeekById(WorkflowExecutionContext context, string id);

        /// <summary>
        /// Marks a queue item as completed.
        /// </summary>
        /// <param name="context">The flow execution context.</param>
        /// <param name="queueItem">The queue item to be marked as completed.</param>
        void Complete(WorkflowExecutionContext context, QueueItem queueItem);

        /// <summary>
        /// Marks a queue item as pending.
        /// </summary>
        /// <param name="context">The flow execution context.</param>
        /// <param name="queueItem">The queue item to be marked as pending.</param>
        void Pending(WorkflowExecutionContext context, QueueItem queueItem);

        /// <summary>
        /// Marks a queue item as waiting.
        /// </summary>
        /// <param name="context">The flow execution context.</param>
        /// <param name="queueItem">The queue item to be marked as waiting.</param>
        void Wait(WorkflowExecutionContext context, QueueItem queueItem, string? reason = null);

        /// <summary>
        /// Marks a queue item as ready.
        /// </summary>
        /// <param name="context">The flow execution context.</param>
        /// <param name="queueItem">The queue item to be marked as ready.</param>
        void Ready(WorkflowExecutionContext context, QueueItem queueItem);

        /// <summary>
        /// Marks a queue item as failed.
        /// </summary>
        /// <param name="context">The flow execution context.</param>
        /// <param name="queueItem">The queue item to be marked as failed.</param>
        void Fail(WorkflowExecutionContext context, QueueItem queueItem);

        /// <summary>
        /// Marks a queue item as rejected.
        /// </summary>
        /// <param name="context">The flow execution context.</param>
        /// <param name="queueItem">The queue item to be marked as rejected.</param>
        void Reject(WorkflowExecutionContext context, QueueItem queueItem);

        /// <summary>
        /// Marks a queue item as approved.
        /// </summary>
        /// <param name="context">The flow execution context.</param>
        /// <param name="queueItem">The queue item to be marked as approved.</param>
        void Approve(WorkflowExecutionContext context, QueueItem queueItem);

        /// <summary>
        /// Updates the execution status of the queue.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Task UpdateExecutionStatus(WorkflowExecutionContext context);

        /// <summary>
        /// Updates the execution status of a specific queue item.
        /// </summary>
        Task UpdateExecutionStatus(WorkflowExecutionContext context, QueueItem queueItem);

        void UpdateExecutionStatus(WorkflowExecutionContext context, QueueItem item, ActivityValidationResult validationResult);
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

        public void LoadNavigations(WorkflowExecutionContext context)
        {
            foreach (var item in context.State.Queue.AsCollection())
            {
                item.Activity = context.Flow.Activities.First(x => x.Id == item.ActivityId);

                if (item.ExecutedConnectionIds is null)
                    continue;

                foreach (var executedConnectionId in item.ExecutedConnectionIds)
                {
                    var executedConnection = context.State.Connections
                        .AsCollection()
                        .First(x => x.Id == executedConnectionId);

                    item.ExecutedConnections.Add(executedConnection);
                }
            }
        }

        #endregion

        #region Enqueueing

        /// <inheritdoc />
        public void EnqueueStart(WorkflowExecutionContext context)
        {
            var startActivity = context.Flow.GetStart();

            // To avoid duplicate execution, if the start activity is already in the queue, do not add it again.
            if (context.State.Queue.AsCollection().Any(x => x.ActivityId == startActivity.Id))
                return;

            var item = new QueueItem(startActivity, 0);
            context.State.Queue.Add(item);

            Ready(context, item); // The start activity is always ready for execution.
            Approve(context, item); // The start activity does not require approval.
        }

        /// <inheritdoc />
        public void Enqueue(WorkflowExecutionContext context, IActivity activity)
        {
            if (!CanEnqueue(context, activity))
                return;

            var item = new QueueItem(activity, GetNextSequential(context));

            context.State.Queue.Add(item);
        }

        public void Enqueue(WorkflowExecutionContext context, ExecutedConnection executedConnection)
        {
            if (!CanEnqueue(context, executedConnection.Target))
                return;

            var item = new QueueItem(executedConnection, GetNextSequential(context));

            context.State.Queue.Add(item);
        }

        /// <inheritdoc />
        public int GetNextSequential(WorkflowExecutionContext context)
        {
            return context.State.Queue.AsCollection().Count + 1;
        }

        #endregion

        #region Peeking

        /// <inheritdoc />
        public bool HasNext(WorkflowExecutionContext context)
        {
            return context.State.Queue
                .AsCollection()
                .Any(x => x.CanExecute);
        }

        /// <inheritdoc />
        public IEnumerable<QueueItem> PeekNextReady(WorkflowExecutionContext context)
        {
            if (context.State.Queue.AsCollection().Count == 0)
                EnqueueStart(context);

            var count = context.Options.MaxDegreeOfParallelism;
            count = count > 0 ? count : 1;

            var items = context.State.Queue
                .AsCollection()
                .Where(x => x.ExecutionStatus == ActivityExecutionStatus.Ready)
                .OrderBy(x => x.Sequential)
                .Take(count);

            if (!items.Any())
                throw new InvalidOperationException("Queue doesn't have pending items.");

            return items;
        }

        /// <inheritdoc />
        public IEnumerable<QueueItem> PeekPending(WorkflowExecutionContext context)
        {
            return context.State.Queue
                .AsCollection()
                .Where(x => x.ExecutionStatus == ActivityExecutionStatus.Pending);
        }

        /// <inheritdoc />
        public IEnumerable<QueueItem> PeekWaiting(WorkflowExecutionContext context)
        {
            return context.State.Queue
                .AsCollection()
                .Where(x => x.ExecutionStatus == ActivityExecutionStatus.Waiting);
        }

        public IEnumerable<QueueItem> PeekWaitingAndReady(WorkflowExecutionContext context)
        {
            return context.State.Queue
                .AsCollection()
                .Where(x => x.ExecutionStatus == ActivityExecutionStatus.Waiting || x.ExecutionStatus == ActivityExecutionStatus.Ready);
        }

        public IEnumerable<QueueItem> PeekCompleted(WorkflowExecutionContext context)
        {
            return context.State.Queue
                .AsCollection()
                .Where(x => x.ExecutionStatus == ActivityExecutionStatus.Completed);
        }

        public IEnumerable<QueueItem> PeekNext(WorkflowExecutionContext context, int count)
        {
            return context.State.Queue.AsCollection()
                .OrderBy(x => x.Sequential)
                .Where(x =>
                    x.ExecutionStatus == ActivityExecutionStatus.Pending
                    || x.ExecutionStatus == ActivityExecutionStatus.Waiting
                    || x.ExecutionStatus == ActivityExecutionStatus.WaitingApprovalAndChecks
                    || x.ExecutionStatus == ActivityExecutionStatus.Ready)
                .Take(count);
        }

        public QueueItem PeekById(WorkflowExecutionContext context, string id)
        {
            return context.State.Queue
                .AsCollection()
                .First(x => x.Id == id);
        }

        #endregion

        #region Queue execution status managing

        public async Task UpdateExecutionStatus(WorkflowExecutionContext context)
        {
            UpdatePendingItems(context);
            await UpdateWaitingAndReadyStatus(context);
        }

        public async Task UpdateExecutionStatus(WorkflowExecutionContext context, QueueItem queueItem)
        {
            await ResolveReadyStatus(context, queueItem);
            await ResolveApprovalAndChecks(context, queueItem);
        }

        public void UpdatePendingItems(WorkflowExecutionContext context)
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

        public async Task UpdateWaitingAndReadyStatus(WorkflowExecutionContext context)
        {
            // Check if there are any waiting activities that can be executed.
            foreach (var item in PeekWaitingAndReady(context))
            {
                await ResolveReadyStatus(context, item);
                await ResolveApprovalAndChecks(context, item);
            }
        }

        public async Task ResolveReadyStatus(WorkflowExecutionContext context, QueueItem item)
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

        public async Task ResolveApprovalAndChecks(WorkflowExecutionContext context, QueueItem item)
        {
            var activity = context.Flow.Activities.First(x => x.Id == item.ActivityId);
            var approvalAndCheckResults = new List<ApprovalAndCheckResult>();

            foreach (var approvalAndCheck in activity.ApprovalAndChecks)
            {
                var resolver = _approvalAndCheckResolverFactory.Create(approvalAndCheck, context.Options);
                approvalAndCheckResults.Add(await resolver.Resolve());
            }

            if (approvalAndCheckResults.Any(x => x.Status == ApprovalAndCheckStatus.Rejected))
                Reject(context, item);
            else if (approvalAndCheckResults.All(x => x.Status == ApprovalAndCheckStatus.Approved))
                Approve(context, item);
            else
                Wait(context, item);
        }

        public void UpdateExecutionStatus(WorkflowExecutionContext context, QueueItem item, ActivityValidationResult validationResult)
        {
            item.ExecutionStatus = validationResult.ReadinessStatus;
            item.ExecutionStatusReason = validationResult.ReadinessDetails;
            item.ApprovalAndCheckStatus = validationResult.ApprovalStatus;
        }

        #endregion

        #region Status modifiers

        /// <inheritdoc />
        public void Complete(WorkflowExecutionContext context, QueueItem queueItem)
        {
            var item = context.State.Queue
                .AsCollection()
                .First(x => x.Sequential == queueItem.Sequential);

            item.ExecutionStatus = ActivityExecutionStatus.Completed;
        }

        /// <inheritdoc />
        public void Pending(WorkflowExecutionContext context, QueueItem queueItem)
        {
            var item = context.State.Queue
                .AsCollection()
                .First(x => x.Sequential == queueItem.Sequential);

            item.ExecutionStatus = ActivityExecutionStatus.Pending;
        }

        /// <inheritdoc />
        public void Wait(WorkflowExecutionContext context, QueueItem queueItem, string? reason = null)
        {
            var item = context.State.Queue
                .AsCollection()
                .First(x => x.Sequential == queueItem.Sequential);

            item.ExecutionStatus = ActivityExecutionStatus.Waiting;
            item.ExecutionStatusReason = reason;
        }

        /// <inheritdoc />
        public void Fail(WorkflowExecutionContext context, QueueItem queueItem)
        {
            var item = context.State.Queue
                .AsCollection()
                .First(x => x.Sequential == queueItem.Sequential);

            item.ExecutionStatus = ActivityExecutionStatus.Failed;
        }

        /// <inheritdoc />
        public void Ready(WorkflowExecutionContext context, QueueItem queueItem)
        {
            var item = context.State.Queue
                .AsCollection()
                .First(x => x.Sequential == queueItem.Sequential);

            item.ExecutionStatus = ActivityExecutionStatus.Ready;
            item.ExecutionStatusReason = null;
        }

        /// <inheritdoc />
        public void Reject(WorkflowExecutionContext context, QueueItem queueItem)
        {
            var item = context.State.Queue
                .AsCollection()
                .First(x => x.Sequential == queueItem.Sequential);

            item.ApprovalAndCheckStatus = ApprovalAndCheckStatus.Rejected;
            item.ExecutionStatus = ActivityExecutionStatus.Failed;
        }

        /// <inheritdoc />
        public void Approve(WorkflowExecutionContext context, QueueItem queueItem)
        {
            var item = context.State.Queue
                .AsCollection()
                .First(x => x.Sequential == queueItem.Sequential);

            item.ApprovalAndCheckStatus = ApprovalAndCheckStatus.Approved;
        }

        #endregion

        #region Private methods

        private static bool RequiredConnectionsExecuted(WorkflowExecutionContext context, QueueItem item)
        {
            var activity = context.Flow.Activities.First(x => x.Id == item.ActivityId);

            if (activity is IStart)
                return true;

            var portDescriptor = activity.Descriptor.Ports.First(x => x.Type == Core.Activities.Base.PortType.Input);
            var connections = context.Flow.Connections.Where(x => x.Target.ActivityId == activity.Id && x.Target.PortName == portDescriptor.Name);

            var allConnectionExecuted = true;

            foreach (var connection in connections)
            {
                if (!context.State.Connections.AsCollection().Any(x => x.ConnectionId == connection.Id))
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

        private static bool CanEnqueue(WorkflowExecutionContext context, IActivity activity)
        {
            // To avoid duplicate execution, if the same activity is already in the queue
            // and its status is not completed or failed, do not add it again.
            if (context.State.Queue.AsCollection().Any(x => x.ActivityId == activity.Id && (x.ExecutionStatus != ActivityExecutionStatus.Completed && x.ExecutionStatus != ActivityExecutionStatus.Failed)))
                return false;

            return true;
        }

        #endregion
    }
}