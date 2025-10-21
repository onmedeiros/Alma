using Alma.Workflows.Core.Activities.Base;

namespace Alma.Workflows.Core.CustomActivities.Models
{
    public class CustomActivityPortEditModel
    {
        public required string CustomActivityId { get; set; }
        public string? CustomActivityDiscriminator { get; set; }
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? DisplayName { get; set; }
        public PortType Type { get; set; }
    }
}