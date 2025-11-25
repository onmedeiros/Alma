using Alma.Workflows.Core.Abstractions;
using Alma.Workflows.Core.Activities.Base;
using Alma.Workflows.Core.Activities.Models;
using Alma.Workflows.Core.Contexts;
using Alma.Workflows.States;

namespace Alma.Workflows.Runners.Strategies
{
    /// <summary>
    /// Defines a strategy for executing specific types of activities.
    /// Implementations can provide custom execution logic for different activity types.
    /// </summary>
    public interface IActivityExecutionStrategy
    {
        /// <summary>
        /// Determines if this strategy can handle the specified activity.
        /// </summary>
        /// <param name="activity">The activity to check.</param>
        /// <returns>True if this strategy can handle the activity, otherwise false.</returns>
        bool CanHandle(IActivity activity);

        /// <summary>
        /// Executes the activity using the strategy's custom logic.
        /// </summary>
        /// <param name="activity">The activity to execute.</param>
        /// <param name="context">The execution context.</param>
        /// <param name="runner">The activity runner for executing the activity.</param>
        /// <returns>The result of the activity execution.</returns>
        Task<ActivityExecutionResult> ExecuteAsync(
            IActivity activity,
            WorkflowExecutionContext context,
            ActivityRunner runner);

        /// <summary>
        /// Handles post-execution logic after the activity has been executed.
        /// </summary>
        /// <param name="activity">The executed activity.</param>
        /// <param name="context">The execution context.</param>
        /// <param name="result">The execution result.</param>
        /// <param name="queueItem">The queue item for the activity.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task HandlePostExecutionAsync(
            IActivity activity,
            WorkflowExecutionContext context,
            ActivityExecutionResult result,
            QueueItem queueItem);
    }
}
