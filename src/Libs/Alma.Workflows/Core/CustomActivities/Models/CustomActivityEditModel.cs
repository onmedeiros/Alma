namespace Alma.Workflows.Core.CustomActivities.Models
{
    public class CustomActivityEditModel
    {
        public required string Id { get; set; }
        public string? Discriminator { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? CategoryId { get; set; }
        public ICollection<CustomActivityParameterEditModel> Parameters { get; set; } = [];
        public ICollection<CustomActivityPortEditModel> Ports { get; set; } = [];
    }
}