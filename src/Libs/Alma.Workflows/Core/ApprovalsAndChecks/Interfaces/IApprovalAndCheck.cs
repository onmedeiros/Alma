using Alma.Workflows.Core.Activities.Abstractions;
using Alma.Workflows.Core.ApprovalsAndChecks.Models;
using Alma.Workflows.Core.Contexts;
using Alma.Workflows.Core.Description.Descriptors;

namespace Alma.Workflows.Core.ApprovalsAndChecks.Interfaces
{
    public interface IApprovalAndCheck : IParameterizable, IRenamable
    {
        string Id { get; set; }

        new ApprovalAndCheckDescriptor Descriptor { get; set; }

        IActivity? ParentActivity { get; set; }

        ValueTask<ApprovalAndCheckResult> Resolve(ActivityExecutionContext context);
    }
}
