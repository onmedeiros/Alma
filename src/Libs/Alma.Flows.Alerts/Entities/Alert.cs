using Alma.Core.Entities;
using Alma.Flows.Alerts.Common;

namespace Alma.Flows.Alerts.Entities
{
    public class Alert : Entity
    {
        public string? OrganizationId { get; set; }
        public required AlertSeverity Severity { get; set; }
        public required string Title { get; set; }
        public string? Details { get; set; }
    }
}