using Alma.Flows.Core.Activities.Base;
using Alma.Flows.Core.ApprovalsAndChecks.Enums;
using Alma.Flows.Enums;

namespace Alma.Flows.Core.Activities.Models
{
    public class ActivityExecutionResult
    {
        public ApprovalAndCheckStatus ApprovalAndCheckStatus { get; set; }
        public ActivityExecutionStatus ExecutionStatus { get; set; }
        public string? ExecutionStatusDetails { get; set; }
        public IEnumerable<Port> ExecutedPorts { get; set; } = [];
    }
}