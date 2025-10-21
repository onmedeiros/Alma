using Alma.Core.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace Alma.Workflows.Monitoring.MonitoringObjectSchemas.Entities
{
    [Table("Workflows.MonitoringObjectSchema")]
    public class MonitoringObjectSchema : Entity
    {
        public string? OrganizationId { get; set; }
        public required string Name { get; set; }
        public List<Field> Fields { get; set; } = [];
    }
}