using Alma.Workflows.Core.Activities.Enums;

namespace Alma.Workflows.Core.Activities.Models
{
    public class ActivityStepData
    {
        public required string Id { get; set; }
        public required ActivityStepStatus Status { get; set; }
        public string? StatusDetails { get; set; }
    }
}