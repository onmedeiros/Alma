using Alma.Flows.Core.ApprovalsAndChecks.Enums;

namespace Alma.Flows.Core.ApprovalsAndChecks.Models
{
    public class ApprovalAndCheckResult
    {
        public ApprovalAndCheckStatus Status { get; set; } = ApprovalAndCheckStatus.Pending;
        public string? Message { get; set; }

        public static ApprovalAndCheckResult Pending => new ApprovalAndCheckResult
        {
            Status = ApprovalAndCheckStatus.Pending
        };
    }
}
