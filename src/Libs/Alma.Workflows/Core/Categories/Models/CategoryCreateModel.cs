namespace Alma.Workflows.Core.Categories.Models
{
    public class CategoryCreateModel
    {
        public string? Discriminator { get; set; }
        public string? ResourceName { get; set; }
        public required string DefaultName { get; set; }
    }
}