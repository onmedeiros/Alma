using Alma.Workflows.Core.InstanceExecutions.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Alma.Workflows.Core.Instances.Entities
{
    [Table("Workflows.Instance")]
    public class FlowInstance
    {
        /// <summary>
        /// Identifier.
        /// </summary>
        public required string Id { get; set; }

        /// <summary>
        /// Discriminator.
        /// </summary>
        public string? Discriminator { get; set; }

        /// <summary>
        /// Flow creation date.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Last date update.
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Identifier name.
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Flow description.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Instance status.
        /// </summary>
        public required bool IsActive { get; set; }

        /// <summary>
        /// Execution mode.
        /// </summary>
        public InstanceExecutionMode ExecutionMode { get; set; }

        /// <summary>
        /// ID of flow definition version that will be run on this instance.
        /// </summary>
        public string? FlowDefinitionVersionId { get; set; }
    }
}