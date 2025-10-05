using Alma.Core.Entities;

namespace Alma.Flows.Monitoring.MonitoringObjects.Entities
{
    public class MonitoringObject : Entity
    {
        public string? OrganizationId { get; set; }
        public required string SchemaId { get; set; }
        public required DateTime Timestamp { get; set; }
        public required string PrimaryKey { get; set; }
        public required Dictionary<string, object?> Data { get; set; } = [];
    }
}