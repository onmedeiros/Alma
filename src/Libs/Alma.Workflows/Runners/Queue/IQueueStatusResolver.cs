using Alma.Workflows.Core.Activities.Models;
using Alma.Workflows.Core.Contexts;
using Alma.Workflows.States;

namespace Alma.Workflows.Runners.Queue
{
    /// <summary>
    /// Interface responsible for resolving and updating the execution status of queue items.
    /// Determines if activities are ready to execute and handles approval/check validations.
    /// </summary>
    public interface IQueueStatusResolver
    {
        /// <summary>
        /// Updates the execution status of all items in the queue.
        /// </summary>
        /// <param name="context">The flow execution context.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UpdateExecutionStatusAsync(WorkflowExecutionContext context);

        /// <summary>
        /// Updates the execution status of a specific queue item.
        /// </summary>
        /// <param name="context">The flow execution context.</param>
        /// <param name="queueItem">The queue item to update.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UpdateExecutionStatusAsync(WorkflowExecutionContext context, QueueItem queueItem);

        /// <summary>
        /// Updates the execution status based on a validation result.
        /// </summary>
        /// <param name="context">The flow execution context.</param>
        /// <param name="item">The queue item to update.</param>
        /// <param name="validationResult">The validation result.</param>
        void UpdateExecutionStatus(WorkflowExecutionContext context, QueueItem item, ActivityValidationResult validationResult);

        /// <summary>
        /// Resolves if an activity is ready to execute.
        /// </summary>
        /// <param name="context">The flow execution context.</param>
        /// <param name="item">The queue item to check.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task ResolveReadyStatusAsync(WorkflowExecutionContext context, QueueItem item);

        /// <summary>
        /// Resolves approval and check status for an activity.
        /// </summary>
        /// <param name="context">The flow execution context.</param>
        /// <param name="item">The queue item to check.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task ResolveApprovalAndChecksAsync(WorkflowExecutionContext context, QueueItem item);
    }
}
