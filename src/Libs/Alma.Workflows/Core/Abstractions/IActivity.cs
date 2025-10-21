using Alma.Workflows.Core.Activities.Abstractions;
using Alma.Workflows.Core.ApprovalsAndChecks.Enums;
using Alma.Workflows.Core.ApprovalsAndChecks.Interfaces;
using Alma.Workflows.Core.ApprovalsAndChecks.Models;
using Alma.Workflows.Core.Contexts;
using Alma.Workflows.Core.Description.Descriptors;

namespace Alma.Workflows.Core.Abstractions
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