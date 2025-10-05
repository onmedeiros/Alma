using Alma.Flows.Core.Activities.Enums;

namespace Alma.Flows.Core.Activities.Models
{
    public class ActivityStepData
    {
        public required string Id { get; set; }
        public required ActivityStepStatus Status { get; set; }
        public string? StatusDetails { get; set; }
    }
}