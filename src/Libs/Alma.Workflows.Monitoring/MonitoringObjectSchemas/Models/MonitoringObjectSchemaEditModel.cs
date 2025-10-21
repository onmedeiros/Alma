using Alma.Workflows.Monitoring.MonitoringObjectSchemas.Entities;

namespace Alma.Workflows.Monitoring.MonitoringObjectSchemas.Models
{
    public class MonitoringObjectSchemaEditModel
    {
        public required string Id { get; set; }
        public string? OrganizationId { get; set; }
        public string? Name { get; set; }
        public List<Field> Fields { get; set; } = [];
    }
}