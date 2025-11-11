namespace Alma.Workflows.Stores.Filters
{
    public class FlowDefinitionFilters
    {
        public string? Name { get; set; }
        public string? Discriminator { get; set; }
        public Dictionary<string, int> OrderBy { get; set; } = [];
    }
}