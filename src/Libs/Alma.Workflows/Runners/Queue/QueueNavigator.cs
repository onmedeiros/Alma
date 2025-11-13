using Alma.Workflows.Core.Contexts;
using Alma.Workflows.Enums;
using Alma.Workflows.States;
using Microsoft.Extensions.Logging;

namespace Alma.Workflows.Runners.Queue
{
    /// <summary>
    /// Implementation of queue navigation and query operations.
    /// Provides read-only access to queue items.
    /// </summary>
    public class QueueNavigator : IQueueNavigator
    {
        private readonly ILogger<QueueNavigator> _logger;
        private readonly IQueueEnqueuer _enqueuer;

        public QueueNavigator(
            ILogger<QueueNavigator> logger,
            IQueueEnqueuer enqueuer)
        {
            _logger = logger;
            _enqueuer = enqueuer;
        }

        public void LoadNavigations(FlowExecutionContext context)
        {
            _logger.LogDebug("Loading navigations for {Count} queue items", context.State.Queue.Count);

            foreach (var item in context.State.Queue)
            {
                // Load activity reference
                item.Activity = context.Flow.Activities.First(x => x.Id == item.ActivityId);

                // Load executed connections
                if (item.ExecutedConnectionIds == null)
                    continue;

                foreach (var executedConnectionId in item.ExecutedConnectionIds)
                {
                    var executedConnection = context.State.ExecutedConnections.First(x => x.Id == executedConnectionId);
                    item.ExecutedConnections.Add(executedConnection);
                }
            }
        }

        public bool HasNext(FlowExecutionContext context)
        {
            return context.State.Queue.Any(x => x.CanExecute);
        }

        public IEnumerable<QueueItem> PeekNextReady(FlowExecutionContext context)
        {
            if (context.State.Queue.Count == 0)
            {
                _logger.LogDebug("Queue is empty, enqueueing start activity");
                _enqueuer.EnqueueStart(context);
            }

            var count = context.Options.MaxDegreeOfParallelism;
            count = count > 0 ? count : 1;

            var items = context.State.Queue
                .Where(x => x.ExecutionStatus == ActivityExecutionStatus.Ready)
                .OrderBy(x => x.Sequential)
                .Take(count)
                .ToList();

            if (!items.Any())
            {
                _logger.LogWarning("Queue doesn't have any ready items");
                throw new InvalidOperationException("Queue doesn't have pending items.");
            }

            _logger.LogDebug("Peeked {Count} ready items from queue", items.Count);
            return items;
        }

        public IEnumerable<QueueItem> PeekPending(FlowExecutionContext context)
        {
            return context.State.Queue.Where(x => x.ExecutionStatus == ActivityExecutionStatus.Pending);
        }

        public IEnumerable<QueueItem> PeekWaiting(FlowExecutionContext context)
        {
            return context.State.Queue.Where(x => x.ExecutionStatus == ActivityExecutionStatus.Waiting);
        }

        public IEnumerable<QueueItem> PeekWaitingAndReady(FlowExecutionContext context)
        {
            return context.State.Queue.Where(x =>
                x.ExecutionStatus == ActivityExecutionStatus.Waiting ||
                x.ExecutionStatus == ActivityExecutionStatus.Ready);
        }

        public IEnumerable<QueueItem> PeekCompleted(FlowExecutionContext context)
        {
            return context.State.Queue.Where(x => x.ExecutionStatus == ActivityExecutionStatus.Completed);
        }

        public IEnumerable<QueueItem> PeekNext(FlowExecutionContext context, int count)
        {
            return context.State.Queue
                .OrderBy(x => x.Sequential)
                .Where(x =>
                    x.ExecutionStatus == ActivityExecutionStatus.Pending ||
                    x.ExecutionStatus == ActivityExecutionStatus.Waiting ||
                    x.ExecutionStatus == ActivityExecutionStatus.WaitingApprovalAndChecks ||
                    x.ExecutionStatus == ActivityExecutionStatus.Ready)
                .Take(count);
        }

        public QueueItem PeekById(FlowExecutionContext context, string id)
        {
            var item = context.State.Queue.FirstOrDefault(x => x.Id == id);

            if (item == null)
            {
                _logger.LogError("Queue item with ID {Id} not found", id);
                throw new InvalidOperationException($"Queue item with ID {id} not found.");
            }

            return item;
        }
    }
}
