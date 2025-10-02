using Alma.Flows.Core.Abstractions;
using Alma.Flows.Core.ApprovalsAndChecks.Models;
using Alma.Flows.Core.Contexts;
using Alma.Flows.Core.Description.Descriptors;

namespace Alma.Flows.Core.ApprovalsAndChecks.Interfaces
{
    public interface IApprovalAndCheck : IParameterizable, IRenamable
    {
        string Id { get; set; }

        new ApprovalAndCheckDescriptor Descriptor { get; set; }

        IActivity? ParentActivity { get; set; }

        ValueTask<ApprovalAndCheckResult> Resolve(ActivityExecutionContext context);
    }
}
