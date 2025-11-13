using Alma.Workflows.Core.Contexts;
using Alma.Workflows.States;

namespace Alma.Workflows.Runners.Queue
{
    /// <summary>
    /// Interface responsible for managing the execution state of queue items.
    /// Handles transitions between different execution statuses.
    /// </summary>
    public interface IQueueStateManager
    {
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
        /// <param name="reason">Optional reason for waiting.</param>
        void Wait(FlowExecutionContext context, QueueItem queueItem, string? reason = null);

        /// <summary>
        /// Marks a queue item as ready for execution.
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
        /// Marks a queue item as rejected (failed approval).
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
    }
}
