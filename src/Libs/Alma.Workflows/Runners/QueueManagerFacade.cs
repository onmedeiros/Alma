using Alma.Workflows.Core.Abstractions;
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

        public void LoadNavigations(FlowExecutionContext context)
            => _navigator.LoadNavigations(context);

        public bool HasNext(FlowExecutionContext context)
            => _navigator.HasNext(context);

        public IEnumerable<QueueItem> PeekNextReady(FlowExecutionContext context)
            => _navigator.PeekNextReady(context);

        public IEnumerable<QueueItem> PeekPending(FlowExecutionContext context)
            => _navigator.PeekPending(context);

        public IEnumerable<QueueItem> PeekWaiting(FlowExecutionContext context)
            => _navigator.PeekWaiting(context);

        public IEnumerable<QueueItem> PeekWaitingAndReady(FlowExecutionContext context)
            => _navigator.PeekWaitingAndReady(context);

        public IEnumerable<QueueItem> PeekCompleted(FlowExecutionContext context)
            => _navigator.PeekCompleted(context);

        public IEnumerable<QueueItem> PeekNext(FlowExecutionContext context, int count)
            => _navigator.PeekNext(context, count);

        public QueueItem PeekById(FlowExecutionContext context, string id)
            => _navigator.PeekById(context, id);

        #endregion

        #region Enqueueing Operations (Delegated to Enqueuer)

        public void EnqueueStart(FlowExecutionContext context)
            => _enqueuer.EnqueueStart(context);

        public void Enqueue(FlowExecutionContext context, IActivity activity)
            => _enqueuer.Enqueue(context, activity);

        public void Enqueue(FlowExecutionContext context, ExecutedConnection executedConnection)
            => _enqueuer.Enqueue(context, executedConnection);

        public int GetNextSequential(FlowExecutionContext context)
            => _enqueuer.GetNextSequential(context);

        #endregion

        #region State Management Operations (Delegated to StateManager)

        public void Complete(FlowExecutionContext context, QueueItem queueItem)
            => _stateManager.Complete(context, queueItem);

        public void Pending(FlowExecutionContext context, QueueItem queueItem)
            => _stateManager.Pending(context, queueItem);

        public void Wait(FlowExecutionContext context, QueueItem queueItem, string? reason = null)
            => _stateManager.Wait(context, queueItem, reason);

        public void Ready(FlowExecutionContext context, QueueItem queueItem)
            => _stateManager.Ready(context, queueItem);

        public void Fail(FlowExecutionContext context, QueueItem queueItem)
            => _stateManager.Fail(context, queueItem);

        public void Reject(FlowExecutionContext context, QueueItem queueItem)
            => _stateManager.Reject(context, queueItem);

        public void Approve(FlowExecutionContext context, QueueItem queueItem)
            => _stateManager.Approve(context, queueItem);

        #endregion

        #region Status Resolution Operations (Delegated to StatusResolver)

        public Task UpdateExecutionStatus(FlowExecutionContext context)
            => _statusResolver.UpdateExecutionStatusAsync(context);

        public Task UpdateExecutionStatus(FlowExecutionContext context, QueueItem queueItem)
            => _statusResolver.UpdateExecutionStatusAsync(context, queueItem);

        public void UpdateExecutionStatus(FlowExecutionContext context, QueueItem item, ActivityValidationResult validationResult)
            => _statusResolver.UpdateExecutionStatus(context, item, validationResult);

        #endregion
    }
}
