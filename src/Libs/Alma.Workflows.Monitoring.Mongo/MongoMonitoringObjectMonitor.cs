using Alma.Workflows.Monitoring.MonitoringObjects.Entities;
using Alma.Workflows.Monitoring.Monitors;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Alma.Workflows.Monitoring.Mongo
{
    public class MongoMonitoringObjectMonitor : IMonitoringObjectMonitor
    {
        private ILogger<MongoMonitoringObjectMonitor> _logger;
        private IMongoDatabase _database;
        private IMongoCollection<MonitoringObject> _collection;

        public MongoMonitoringObjectMonitor(ILogger<MongoMonitoringObjectMonitor> logger, IMongoDatabase database)
        {
            _logger = logger;
            _database = database;
            _collection = _database.GetCollection<MonitoringObject>(nameof(MonitoringObject));
        }

        public async ValueTask<long> Count(string? organizationId, string schemaId, List<Models.Filter> filters, DateTime since)
        {
            _logger.LogDebug("Counting monitoring objects for schema {SchemaId} since {Since}.", schemaId, since);

            var builder = Builders<MonitoringObject>.Filter;

            var filter = builder.Eq(mo => mo.SchemaId, schemaId)
                         & builder.Gte(mo => mo.Timestamp, since);

            if (!string.IsNullOrEmpty(organizationId))
                filter &= builder.Eq(mo => mo.OrganizationId, organizationId);

            // Apply additional filters
            foreach (var f in filters)
            {
                var fieldFilter = builder.Eq($"Data.{f.Field}", f.Value);
                filter &= fieldFilter;
            }

            return await _collection.CountDocumentsAsync(filter);
        }
    }
}