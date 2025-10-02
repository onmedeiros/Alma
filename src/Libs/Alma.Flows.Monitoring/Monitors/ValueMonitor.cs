namespace Alma.Flows.Monitoring.Monitors
{
    public interface IValueMonitor
    {
        ValueTask<decimal> Average(string? organizationId, string schemaId, string field, DateTime since);
    }

    public class ValueMonitor : IValueMonitor
    {
        public ValueTask<decimal> Average(string? organizationId, string schemaId, string field, DateTime since)
        {
            throw new NotImplementedException();
        }
    }
}