using Alma.Workflows.Core.Abstractions;
using Alma.Workflows.Core.ApprovalsAndChecks.Models;
using Alma.Workflows.Core.Contexts;
using Alma.Workflows.States;

namespace Alma.Workflows.Runners.Queue
{
    /// <summary>
    /// Interface responsible for enqueueing activities to the execution queue.
    /// Follows the Single Responsibility Principle by handling only queue additions.
    /// </summary>
    public interface IQueueEnqueuer
    {
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
        /// Checks if an activity can be enqueued (not a duplicate).
        /// </summary>
        /// <param name="context">The flow execution context.</param>
        /// <param name="activity">The activity to check.</param>
        /// <returns>True if the activity can be enqueued, otherwise false.</returns>
        bool CanEnqueue(FlowExecutionContext context, IActivity activity);
    }
}
