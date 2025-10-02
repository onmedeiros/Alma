namespace Alma.Flows.Alerts.Stores.Filters
{
    public class AlertFilters
    {
        public string? OrganizationId { get; set; }
        public string? Title { get; set; }
        public Alma.Flows.Alerts.Common.AlertSeverity? Severity { get; set; }
    }
}
