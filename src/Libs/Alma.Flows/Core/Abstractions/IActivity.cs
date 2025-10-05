using Alma.Flows.Core.Activities.Abstractions;
using Alma.Flows.Core.ApprovalsAndChecks.Enums;
using Alma.Flows.Core.ApprovalsAndChecks.Interfaces;
using Alma.Flows.Core.ApprovalsAndChecks.Models;
using Alma.Flows.Core.Contexts;
using Alma.Flows.Core.Description.Descriptors;

namespace Alma.Flows.Core.Abstractions
{
    public interface IActivity : IParameterizable, IConnectable, IDataContaining
    {
        string Id { get; set; }
        string DisplayName { get; set; }
        ActivityDescriptor Descriptor { get; }
        ICollection<IApprovalAndCheck> ApprovalAndChecks { get; set; }
        IList<IStep> BeforeExecutionSteps { get; set; }
        IList<IStep> AfterExecutionSteps { get; set; }
        ApprovalAndCheckStatus ApprovalAndCheckStatus { get; }

        void Execute(ActivityExecutionContext context);

        ValueTask ExecuteAsync(ActivityExecutionContext context);

        IsReadyResult IsReadyToExecute(ActivityExecutionContext context);

        ValueTask<IsReadyResult> IsReadyToExecuteAsync(ActivityExecutionContext context);

        void SetDescriptor(ActivityDescriptor descriptor);
    }
}