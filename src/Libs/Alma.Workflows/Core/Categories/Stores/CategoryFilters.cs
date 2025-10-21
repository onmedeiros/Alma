namespace Alma.Workflows.Core.Categories.Stores
{
    public class CategoryFilters
    {
        /// <summary>
        /// Category name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Category discriminator.
        /// </summary>
        public string? Discriminator { get; set; }
    }
}