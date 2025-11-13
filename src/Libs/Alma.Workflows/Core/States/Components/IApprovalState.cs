using Alma.Workflows.States;

namespace Alma.Workflows.Core.States.Components
{
    /// <summary>
    /// Manages approval and check state during flow execution.
    /// </summary>
    public interface IApprovalState
    {
        /// <summary>
        /// Gets all approval and check states.
        /// </summary>
        IReadOnlyCollection<ApprovalAndCheckState> GetAll();

        /// <summary>
        /// Adds an approval and check state.
        /// </summary>
        void Add(ApprovalAndCheckState state);

        /// <summary>
        /// Removes an approval and check state.
        /// </summary>
        bool Remove(ApprovalAndCheckState state);

        /// <summary>
        /// Gets approval states by activity ID.
        /// </summary>
        IEnumerable<ApprovalAndCheckState> GetByActivityId(string activityId);

        /// <summary>
        /// Checks if there are any pending approvals.
        /// </summary>
        bool HasPendingApprovals();

        /// <summary>
        /// Clears all approval states.
        /// </summary>
        void Clear();

        /// <summary>
        /// Gets the count of approval states.
        /// </summary>
        int Count { get; }
    }
}
