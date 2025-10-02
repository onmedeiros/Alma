namespace Alma.Flows.Core.CustomActivities.Entities
{
    public class CustomActivityScript
    {
        /// <summary>
        /// Identifier.
        /// </summary>
        public required string Id { get; set; }

        /// <summary>
        /// Custom activity template identifier.
        /// </summary>
        public required string CustomActivityTemplateId { get; set; }

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
        /// Script content.
        /// </summary>
        public required string Content { get; set; }
    }
}