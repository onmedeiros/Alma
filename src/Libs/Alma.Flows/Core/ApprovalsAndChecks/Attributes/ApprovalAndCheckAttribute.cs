namespace Alma.Flows.Core.ApprovalsAndChecks.Attributes
{
    public class ApprovalAndCheckAttribute : Attribute
    {
        public string? Namespace { get; set; }
        public string? Category { get; set; }
        public string? DisplayName { get; set; }
        public string? Description { get; set; }
        public bool CanBeApprovedManually { get; set; } = false;
    }
}
