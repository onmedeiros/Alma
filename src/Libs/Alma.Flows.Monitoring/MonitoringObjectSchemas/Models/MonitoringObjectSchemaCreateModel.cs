namespace Alma.Flows.Monitoring.MonitoringObjectSchemas.Models
{
    public class MonitoringObjectSchemaCreateModel
    {
        public string? OrganizationId { get; set; }
        public required string Name { get; set; }
    }
}