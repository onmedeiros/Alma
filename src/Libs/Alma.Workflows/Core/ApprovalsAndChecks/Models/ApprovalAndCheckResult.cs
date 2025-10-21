using Alma.Workflows.Core.ApprovalsAndChecks.Enums;

namespace Alma.Workflows.Core.ApprovalsAndChecks.Models
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
