using Alma.Workflows.States;

namespace Alma.Workflows.Core.States.Components
{
    /// <summary>
    /// Concrete implementation of approval state management.
    /// </summary>
    public class ApprovalState : IApprovalState
    {
        private readonly List<ApprovalAndCheckState> _approvalStates;

        public ApprovalState()
        {
            _approvalStates = new List<ApprovalAndCheckState>();
        }

        public int Count => _approvalStates.Count;

        public IReadOnlyCollection<ApprovalAndCheckState> GetAll()
        {
            return _approvalStates.AsReadOnly();
        }

        public void Add(ApprovalAndCheckState state)
        {
            _approvalStates.Add(state);
        }

        public bool Remove(ApprovalAndCheckState state)
        {
            return _approvalStates.Remove(state);
        }

        public IEnumerable<ApprovalAndCheckState> GetByActivityId(string activityId)
        {
            return _approvalStates.Where(x => x.ParentActivityId == activityId);
        }

        public bool HasPendingApprovals()
        {
            return _approvalStates.Any(x => x.Status == Core.ApprovalsAndChecks.Enums.ApprovalAndCheckStatus.Pending);
        }

        public void Clear()
        {
            _approvalStates.Clear();
        }
    }
}
