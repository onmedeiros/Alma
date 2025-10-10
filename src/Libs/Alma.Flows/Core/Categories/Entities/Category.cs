using System.ComponentModel.DataAnnotations.Schema;

namespace Alma.Flows.Core.Categories.Entities
{
    [Table("flows.Category")]
    public class Category
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
        /// Represents the name the resource for localization. It can be null, indicating that no resource name is assigned.
        /// </summary>
        public string? ResourceName { get; set; }

        /// <summary>
        /// Category default name.
        /// </summary>
        public required string DefaultName { get; set; }

        /// <summary>
        /// Indicates whether the current instance is the system default. Defaults to false.
        /// </summary>
        public bool IsSystemDefault { get; set; } = false;
    }
}