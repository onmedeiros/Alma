namespace Alma.Workflows.Definitions
{
    public class ConnectionDefinition
    {
        public required string Id { get; set; }
        public required string SourceActivityId { get; set; }
        public required string SourceActivityPort { get; set; }
        public required string TargetActivityId { get; set; }
        public required string TargetActivityPort { get; set; }
    }
}
