namespace Alma.Workflows.Monitoring.MonitoringObjectSchemas.Models
{
    public class MonitoringObjectSchemaCreateModel
    {
        public string? OrganizationId { get; set; }
        public required string Name { get; set; }
    }
}