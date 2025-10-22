using Alma.Core.Mongo;
using Alma.Workflows.Monitoring.MonitoringObjects.Entities;
using Alma.Workflows.Monitoring.Monitors;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
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
            _collection = _database.GetCollection<MonitoringObject>(CollectionNameResolver.GetCollectionName(typeof(MonitoringObject)));
        }

        public async ValueTask<long> Count(string? organizationId, string schemaId, List<Models.Filter> filters, DateTime since)
        {
            _logger.LogDebug("Counting monitoring objects for schema {SchemaId} since {Since}.", schemaId, since.ToUniversalTime());

            var builder = Builders<MonitoringObject>.Filter;

            var filter = builder.Eq(x => x.SchemaId, schemaId);
            filter &= builder.Gte(x => x.Timestamp, since);

            if (!string.IsNullOrEmpty(organizationId))
                filter &= builder.Eq(mo => mo.OrganizationId, organizationId);

            // Apply additional filters
            foreach (var f in filters)
            {
                switch (f.Operator)
                {
                    case Models.FilterOperator.Equals:
                        filter &= builder.Eq($"Data.{f.Field}", f.Value);
                        break;

                    case Models.FilterOperator.NotEquals:
                        filter &= builder.Ne($"Data.{f.Field}", f.Value);
                        continue;
                    case Models.FilterOperator.GreaterThan:
                        filter &= builder.Gt($"Data.{f.Field}", f.Value);
                        continue;
                    case Models.FilterOperator.LessThan:
                        filter &= builder.Lt($"Data.{f.Field}", f.Value);
                        continue;
                    case Models.FilterOperator.GreaterThanOrEqual:
                        filter &= builder.Gte($"Data.{f.Field}", f.Value);
                        continue;
                    case Models.FilterOperator.LessThanOrEqual:
                        filter &= builder.Lte($"Data.{f.Field}", f.Value);
                        continue;
                    case Models.FilterOperator.Contains:
                        filter &= builder.Regex($"Data.{f.Field}", new MongoDB.Bson.BsonRegularExpression(f.Value ?? string.Empty, "i"));
                        continue;
                    case Models.FilterOperator.NotContains:
                        filter &= builder.Not(builder.Regex($"Data.{f.Field}", new MongoDB.Bson.BsonRegularExpression(f.Value ?? string.Empty, "i")));
                        continue;
                    default:
                        _logger.LogWarning("Unsupported filter operator: {Operator}", f.Operator);
                        continue;
                }
            }

#if DEBUG
            var rendered = filter.Render(new RenderArgs<MonitoringObject>
            {
                SerializerRegistry = MongoDB.Bson.Serialization.BsonSerializer.SerializerRegistry,
                DocumentSerializer = MongoDB.Bson.Serialization.BsonSerializer.LookupSerializer<MonitoringObject>()
            });
            _logger.LogDebug("Final filter applied: \n{Filter}", rendered.ToJson(new MongoDB.Bson.IO.JsonWriterSettings { Indent = true }));
#endif

            return await _collection.CountDocumentsAsync(filter);
        }
    }
}