namespace Alma.Workflows.Core.Instances.Stores
{
    public class FlowInstanceFilters
    {
        public string? Name { get; set; }
        public string? Discriminator { get; set; }
        public bool? IsActive { get; set; }
    }
}