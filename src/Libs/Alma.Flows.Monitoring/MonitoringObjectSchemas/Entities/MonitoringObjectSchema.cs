using Alma.Core.Entities;

namespace Alma.Flows.Monitoring.MonitoringObjectSchemas.Entities
{
    public class MonitoringObjectSchema : Entity
    {
        public string? OrganizationId { get; set; }
        public required string Name { get; set; }
        public List<Field> Fields { get; set; } = [];
    }
}