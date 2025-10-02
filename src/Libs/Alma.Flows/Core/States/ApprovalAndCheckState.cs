using Alma.Flows.Core.ApprovalsAndChecks.Enums;

namespace Alma.Flows.States
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
