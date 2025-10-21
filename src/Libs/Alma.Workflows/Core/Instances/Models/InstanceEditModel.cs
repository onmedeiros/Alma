using Alma.Workflows.Core.InstanceExecutions.Enums;

namespace Alma.Workflows.Core.Instances.Models
{
    public class InstanceEditModel
    {
        public required string Id { get; set; }
        public string? Discriminator { get; set; }
        public string? Name { get; set; }
        public bool? IsActive { get; set; }
        public InstanceExecutionMode ExecutionMode { get; set; }
        public string? FlowDefinitionVersionId { get; set; }
    }
}