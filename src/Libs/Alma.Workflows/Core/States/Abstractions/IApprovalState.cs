using Alma.Workflows.States;

namespace Alma.Workflows.Core.States.Abstractions
{
    public interface IApprovalState : IStateComponent
    {
        void Add(ApprovalStateData approvalStateData);

        ApprovalStateData? Get(string id);

        IReadOnlyCollection<ApprovalStateData> AsCollection();
    }
}