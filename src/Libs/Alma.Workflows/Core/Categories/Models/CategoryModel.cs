namespace Alma.Workflows.Core.Categories.Models
{
    public class CategoryModel
    {
        public required string Id { get; set; }
        public required string ResourceName { get; set; }
        public required string DefaultName { get; set; }
        public bool IsSystemDefault { get; set; } = true;
    }
}