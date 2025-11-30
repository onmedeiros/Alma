using Alma.Workflows.Core.Activities.Abstractions;
using Alma.Workflows.Core.Activities.Models;
using Alma.Workflows.Core.ApprovalsAndChecks.Models;
using Alma.Workflows.Core.Contexts;
using Alma.Workflows.Runners.Queue;
using Alma.Workflows.States;
using Microsoft.Extensions.Logging;

namespace Alma.Workflows.Runners
{
    /// <summary>
    /// Facade implementation of IQueueManager that coordinates specialized queue components.
    /// Delegates responsibilities to dedicated services following the Single Responsibility Principle.
    /// </summary>
    public class QueueManagerFacade : IQueueManager
    {
        private readonly ILogger<QueueManagerFacade> _logger;
        private readonly IQueueEnqueuer _enqueuer;
        private readonly IQueueNavigator _navigator;
        private readonly IQueueStateManager _stateManager;
        private readonly IQueueStatusResolver _statusResolver;

        public QueueManagerFacade(
            ILogger<QueueManagerFacade> logger,
            IQueueEnqueuer enqueuer,
            IQueueNavigator navigator,
            IQueueStateManager stateManager,
            IQueueStatusResolver statusResolver)
        {
            _logger = logger;
            _enqueuer = enqueuer;
            _navigator = navigator;
            _stateManager = stateManager;
            _statusResolver = statusResolver;
        }

        #region Navigation Operations (Delegated to Navigator)

        public void LoadNavigations(WorkflowExecutionContext context)
            => _navigator.LoadNavigations(context);

        public bool HasNext(WorkflowExecutionContext context)
            => _navigator.HasNext(context);

        public IEnumerable<QueueItem> PeekNextReady(WorkflowExecutionContext context)
            => _navigator.PeekNextReady(context);

        public IEnumerable<QueueItem> PeekPending(WorkflowExecutionContext context)
            => _navigator.PeekPending(context);

        public IEnumerable<QueueItem> PeekWaiting(WorkflowExecutionContext context)
            => _navigator.PeekWaiting(context);

        public IEnumerable<QueueItem> PeekWaitingAndReady(WorkflowExecutionContext context)
            => _navigator.PeekWaitingAndReady(context);

        public IEnumerable<QueueItem> PeekCompleted(WorkflowExecutionContext context)
            => _navigator.PeekCompleted(context);

        public IEnumerable<QueueItem> PeekNext(WorkflowExecutionContext context, int count)
            => _navigator.PeekNext(context, count);

        public QueueItem PeekById(WorkflowExecutionContext context, string id)
            => _navigator.PeekById(context, id);

        #endregion

        #region Enqueueing Operations (Delegated to Enqueuer)

        public void EnqueueStart(WorkflowExecutionContext context)
            => _enqueuer.EnqueueStart(context);

        public void Enqueue(WorkflowExecutionContext context, IActivity activity)
            => _enqueuer.Enqueue(context, activity);

        public void Enqueue(WorkflowExecutionContext context, ExecutedConnection executedConnection)
            => _enqueuer.Enqueue(context, executedConnection);

        public int GetNextSequential(WorkflowExecutionContext context)
            => _enqueuer.GetNextSequential(context);

        #endregion

        #region State Management Operations (Delegated to StateManager)

        public void Complete(WorkflowExecutionContext context, QueueItem queueItem)
            => _stateManager.Complete(context, queueItem);

        public void Pending(WorkflowExecutionContext context, QueueItem queueItem)
            => _stateManager.Pending(context, queueItem);

        public void Wait(WorkflowExecutionContext context, QueueItem queueItem, string? reason = null)
            => _stateManager.Wait(context, queueItem, reason);

        public void Ready(WorkflowExecutionContext context, QueueItem queueItem)
            => _stateManager.Ready(context, queueItem);

        public void Fail(WorkflowExecutionContext context, QueueItem queueItem)
            => _stateManager.Fail(context, queueItem);

        public void Reject(WorkflowExecutionContext context, QueueItem queueItem)
            => _stateManager.Reject(context, queueItem);

        public void Approve(WorkflowExecutionContext context, QueueItem queueItem)
            => _stateManager.Approve(context, queueItem);

        #endregion

        #region Status Resolution Operations (Delegated to StatusResolver)

        public Task UpdateExecutionStatus(WorkflowExecutionContext context)
            => _statusResolver.UpdateExecutionStatusAsync(context);

        public Task UpdateExecutionStatus(WorkflowExecutionContext context, QueueItem queueItem)
            => _statusResolver.UpdateExecutionStatusAsync(context, queueItem);

        public void UpdateExecutionStatus(WorkflowExecutionContext context, QueueItem item, ActivityValidationResult validationResult)
            => _statusResolver.UpdateExecutionStatus(context, item, validationResult);

        #endregion
    }
}
