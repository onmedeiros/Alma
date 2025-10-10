using Alma.Core.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace Alma.Flows.Monitoring.MonitoringObjects.Entities
{
    [Table("flows.MonitoringObject")]
    public class MonitoringObject : Entity
    {
        public string? OrganizationId { get; set; }
        public required string SchemaId { get; set; }
        public required DateTime Timestamp { get; set; }
        public required string PrimaryKey { get; set; }
        public required Dictionary<string, object?> Data { get; set; } = [];
    }
}