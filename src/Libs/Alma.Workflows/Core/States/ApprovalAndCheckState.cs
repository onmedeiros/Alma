using Alma.Workflows.Core.ApprovalsAndChecks.Enums;

namespace Alma.Workflows.States
{
    public class ApprovalAndCheckState
    {
        public required string Id { get; set; }
        public required string FullName { get; set; }
        public string? ParentActivityId { get; set; }
        public ApprovalAndCheckStatus Status { get; set; }
        public string? Message { get; set; }
    }
}
