namespace Alma.Workflows.Core.Categories.Models
{
    public class CategoryUpdateModel
    {
        public required string Id { get; set; }
        public string? Discriminator { get; set; }
        public string? DefaultName { get; set; }
        public string? ResourceName { get; set; }
    }
}