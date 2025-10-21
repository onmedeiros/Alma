using System.ComponentModel.DataAnnotations.Schema;

namespace Alma.Workflows.Core.InstanceSchedules.Entities
{
    [Table("Workflows.InstanceSchedule")]
    public class InstanceSchedule
    {
        public required string Id { get; set; }
        public string? Discriminator { get; set; }
        public required string InstanceId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? LastRunAt { get; set; }
        public string? Name { get; set; }
        public string? CronExpression { get; set; }
        public bool IsActive { get; set; }
    }
}