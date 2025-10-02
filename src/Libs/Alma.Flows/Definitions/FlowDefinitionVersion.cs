namespace Alma.Flows.Definitions
{
    public class FlowDefinitionVersion
    {
        public required string Id { get; set; }
        public required string FlowDefinitionId { get; set; }
        public string? Discriminator { get; set; }
        public required DateTime CreatedAt { get; set; }
        public required string Name { get; set; }
        public required FlowDefinition FlowDefinition { get; set; }
    }
}
