namespace Alma.Flows.Monitoring.Models
{
    public class Filter
    {
        public required string Id { get; set; }
        public required string Field { get; set; }
        public FilterOperator Operator { get; set; }
        public string? Value { get; set; }
    }
}