using Alma.Core.Entities;
using Alma.Workflows.Alerts.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Alma.Workflows.Alerts.Entities
{
    [Table("Workflows.Alert")]
    public class Alert : Entity
    {
        public string? OrganizationId { get; set; }
        public required AlertSeverity Severity { get; set; }
        public required string Title { get; set; }
        public string? Details { get; set; }
    }
}