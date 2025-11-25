using Alma.Workflows.Core.States.Abstractions;
using Alma.Workflows.States;

namespace Alma.Workflows.Core.States.Components
{
    public class ApprovalState : StateComponent, IApprovalState
    {
        public IReadOnlyCollection<ApprovalStateData> AsCollection()
        {
            return GetState();
        }

        public ApprovalStateData? Get(string id)
        {
            var state = GetState();
            return state.FirstOrDefault(a => a.Id == id);
        }

        public void Add(ApprovalStateData approvalStateData)
        {
            GetState()
                .Add(approvalStateData);
        }

        private List<ApprovalStateData> GetState()
        {
            EnsureInitialized();

            if (StateData!.TryGetValue("Approvals", out var stateObj) && stateObj is List<ApprovalStateData> state)
            {
                return state;
            }

            var newState = new List<ApprovalStateData>();

            StateData["Approvals"] = newState;

            return newState;
        }
    }
}