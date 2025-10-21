using Alma.Workflows.Core.Activities.Base;
using Alma.Workflows.Core.ApprovalsAndChecks.Enums;
using Alma.Workflows.Enums;

namespace Alma.Workflows.Core.Activities.Models
{
    public class ActivityExecutionResult
    {
        public ApprovalAndCheckStatus ApprovalAndCheckStatus { get; set; }
        public ActivityExecutionStatus ExecutionStatus { get; set; }
        public string? ExecutionStatusDetails { get; set; }
        public IEnumerable<Port> ExecutedPorts { get; set; } = [];
    }
}