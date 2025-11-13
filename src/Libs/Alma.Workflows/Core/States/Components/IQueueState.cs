using Alma.Workflows.Enums;
using Alma.Workflows.Models;
using Alma.Workflows.States;

namespace Alma.Workflows.Core.States.Components
{
    /// <summary>
    /// Manages the execution queue state.
    /// </summary>
    public interface IQueueState
    {
        /// <summary>
        /// Gets the current execution queue.
        /// </summary>
        ExecutionQueue Queue { get; }

        /// <summary>
        /// Gets the overall execution status based on queue items.
        /// </summary>
        ExecutionStatus GetExecutionStatus();

        /// <summary>
        /// Gets the execution status of a specific activity.
        /// </summary>
        ActivityExecutionStatus GetActivityExecutionStatus(string activityId);

        /// <summary>
        /// Gets all executed connections.
        /// </summary>
        ICollection<ExecutedConnection> GetExecutedConnections();

        /// <summary>
        /// Adds an executed connection.
        /// </summary>
        void AddExecutedConnection(ExecutedConnection connection);

        /// <summary>
        /// Clears all executed connections.
        /// </summary>
        void ClearExecutedConnections();
    }
}
