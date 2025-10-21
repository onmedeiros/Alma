using Alma.Workflows.Monitoring.Models;

namespace Alma.Workflows.Monitoring.Monitors
{
    public interface IMonitoringObjectMonitor
    {
        ValueTask<long> Count(string? organizationId, string schemaId, List<Filter> filters, DateTime since);
    }

    public class MonitoringObjectMonitor : IMonitoringObjectMonitor
    {
        public ValueTask<long> Count(string? organizationId, string schemaId, List<Filter> filters, DateTime since)
        {
            throw new NotImplementedException();
        }
    }
}