using Alma.Workflows.Monitoring.MonitoringObjectSchemas.Entities;

namespace Alma.Workflows.Monitoring.MonitoringObjectSchemas.Models
{
    public class FieldEditModel
    {
        public required string Name { get; set; }
        public required FieldType Type { get; set; }
        public int Order { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsRequired { get; set; }
        public bool IsTimestamp { get; set; }
    }
}