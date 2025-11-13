using Alma.Workflows.States;

namespace Alma.Workflows.Core.States.Components
{
    /// <summary>
    /// Manages execution history state during flow execution.
    /// </summary>
    public interface IHistoryState
    {
        /// <summary>
        /// Gets all execution history entries.
        /// </summary>
        IReadOnlyCollection<ExecutionHistory> GetAll();

        /// <summary>
        /// Adds an execution history entry.
        /// </summary>
        void Add(ExecutionHistory entry);

        /// <summary>
        /// Gets history entries for a specific activity.
        /// </summary>
        IEnumerable<ExecutionHistory> GetByActivityId(string activityId);

        /// <summary>
        /// Gets the latest history entry.
        /// </summary>
        ExecutionHistory? GetLatest();

        /// <summary>
        /// Clears all history entries.
        /// </summary>
        void Clear();

        /// <summary>
        /// Gets the count of history entries.
        /// </summary>
        int Count { get; }
    }
}
