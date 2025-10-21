using System.ComponentModel.DataAnnotations.Schema;

namespace Alma.Workflows.Core.CustomActivities.Entities
{
    [Table("Workflows.CustomActivityTemplate")]
    public class CustomActivityTemplate
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
        /// Activity name.
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Activity description.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Activity category.
        /// </summary>
        public string? CategoryId { get; set; }

        /// <summary>
        /// Activity parameters.
        /// </summary>
        public ICollection<CustomActivityParameterTemplate> Parameters { get; set; } = [];

        /// <summary>
        /// Activity ports.
        /// </summary>
        public ICollection<CustomActivityPort> Ports { get; set; } = [];
    }
}