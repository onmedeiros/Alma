using Alma.Workflows.Core.Common.Enums;

namespace Alma.Workflows.Core.CustomActivities.Entities
{
    public class CustomActivityParameterTemplate
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public required string DisplayName { get; set; }
        public string? Description { get; set; }
        public ParameterType Type { get; set; }
    }
}