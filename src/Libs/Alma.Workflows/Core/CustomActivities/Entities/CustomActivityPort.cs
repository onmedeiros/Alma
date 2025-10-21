using Alma.Workflows.Core.Activities.Base;

namespace Alma.Workflows.Core.CustomActivities.Entities
{
    public class CustomActivityPort
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public required string DisplayName { get; set; }
        public required PortType Type { get; set; }
    }
}