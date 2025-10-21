namespace Alma.Workflows.Alerts.Stores.Filters
{
    public class AlertFilters
    {
        public string? OrganizationId { get; set; }
        public string? Title { get; set; }
        public Alma.Workflows.Alerts.Common.AlertSeverity? Severity { get; set; }
    }
}
