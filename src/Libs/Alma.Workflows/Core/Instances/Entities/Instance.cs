using Alma.Workflows.Core.InstanceExecutions.Enums;
using Org.BouncyCastle.Asn1;
using System.ComponentModel.DataAnnotations.Schema;

namespace Alma.Workflows.Core.Instances.Entities
{
    [Table("Workflows.Instance")]
    public class Instance
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
        /// ID of workflow definition that this instance is based on.
        /// </summary>
        public string? WorkflowDefinitionId { get; set; }

        /// <summary>
        /// ID of workflow definition version that will be run on this instance.
        /// If null, run the latest workflow definition.
        /// </summary>
        public string? WorkflowDefinitionVersionId { get; set; }
    }
}