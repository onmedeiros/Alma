using Alma.Workflows.Core.InstanceExecutions.Enums;
using Alma.Workflows.Options;
using Alma.Workflows.States;
using System.ComponentModel.DataAnnotations.Schema;

namespace Alma.Workflows.Core.InstanceExecutions.Entities
{
    [Table("Workflows.InstanceExecution")]
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