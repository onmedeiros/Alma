using Alma.Flows.Core.InstanceExecutions.Enums;
using Alma.Flows.Options;
using Alma.Flows.States;

namespace Alma.Flows.Core.InstanceExecutions.Entities
{
    public class InstanceExecution
    {
        public required string Id { get; set; }

        public string? Discriminator { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public required string InstanceId { get; set; }
        public string? DefinitionVersionId { get; set; }

        public ExecutionOptions Options { get; set; } = new();

        public InstanceExecutionStatus Status { get; set; }

        public ExecutionState? State { get; set; }
    }
}