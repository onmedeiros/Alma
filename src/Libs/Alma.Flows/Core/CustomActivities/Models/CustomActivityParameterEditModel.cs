using Alma.Flows.Core.Common.Enums;

namespace Alma.Flows.Core.CustomActivities.Models
{
    public class CustomActivityParameterEditModel
    {
        public required string CustomActivityId { get; set; }
        public string? CustomActivityDiscriminator { get; set; }
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? DisplayName { get; set; }
        public string? Description { get; set; }
        public ParameterType Type { get; set; }
    }
}