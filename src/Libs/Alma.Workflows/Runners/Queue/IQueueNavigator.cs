using Alma.Workflows.Core.Contexts;
using Alma.Workflows.States;

namespace Alma.Workflows.Runners.Queue
{
    /// <summary>
    /// Interface responsible for navigating and querying the execution queue.
    /// Provides read-only access to queue items without modifying them.
    /// </summary>
    public interface IQueueNavigator
    {
        /// <summary>
        /// Loads property navigations of the queue items (Activity, ExecutedConnections, etc.).
        /// </summary>
        /// <param name="context">The flow execution context.</param>
        void LoadNavigations(WorkflowExecutionContext context);

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

        /// <summary>
        /// Peeks activities that are either waiting or ready.
        /// </summary>
        /// <param name="context">The flow execution context.</param>
        /// <returns>A collection of waiting or ready queue items.</returns>
        IEnumerable<QueueItem> PeekWaitingAndReady(WorkflowExecutionContext context);

        /// <summary>
        /// Peeks the completed activities in the queue.
        /// </summary>
        /// <param name="context">The flow execution context.</param>
        /// <returns>A collection of completed queue items.</returns>
        IEnumerable<QueueItem> PeekCompleted(WorkflowExecutionContext context);

        /// <summary>
        /// Peeks the next activities (pending, waiting, or ready) up to the specified count.
        /// </summary>
        /// <param name="context">The flow execution context.</param>
        /// <param name="count">Maximum number of items to return.</param>
        /// <returns>A collection of queue items.</returns>
        IEnumerable<QueueItem> PeekNext(WorkflowExecutionContext context, int count);

        /// <summary>
        /// Finds a queue item by its ID.
        /// </summary>
        /// <param name="context">The flow execution context.</param>
        /// <param name="id">The queue item ID.</param>
        /// <returns>The queue item with the specified ID.</returns>
        QueueItem PeekById(WorkflowExecutionContext context, string id);
    }
}
