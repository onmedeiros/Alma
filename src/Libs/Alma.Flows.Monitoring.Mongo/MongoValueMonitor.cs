using Alma.Flows.Monitoring.MonitoringObjects.Entities;
using Alma.Flows.Monitoring.Monitors;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Globalization;

namespace Alma.Flows.Monitoring.Mongo
{
    public class MongoValueMonitor : IValueMonitor
    {
        private ILogger<MongoValueMonitor> _logger;
        private IMongoDatabase _database;
        private IMongoCollection<MonitoringObject> _collection;

        public MongoValueMonitor(ILogger<MongoValueMonitor> logger, IMongoDatabase database)
        {
            _logger = logger;
            _database = database;
            _collection = _database.GetCollection<MonitoringObject>(nameof(MonitoringObject));
        }

        public async ValueTask<decimal> Average(string? organizationId, string schemaId, string field, DateTime since)
        {
            _logger.LogDebug("Calculating average for schema {SchemaId} and field {Field} since {Since}.", schemaId, field, since);

            // Build the aggregation pipeline to compute average on the server
            var match = new BsonDocument
            {
                { "SchemaId", schemaId },
                { "OrganizationId", organizationId == null ? BsonNull.Value : (BsonValue)organizationId },
                { "Timestamp", new BsonDocument("$gte", since) },
                { $"Data.{field}", new BsonDocument("$exists", true) }
            };

            var project = new BsonDocument("$project", new BsonDocument
            {
                { "value", new BsonDocument("$convert", new BsonDocument
                    {
                        { "input", new BsonString($"$Data.{field}") },
                        { "to", "decimal" },
                        { "onError", BsonNull.Value },
                        { "onNull", BsonNull.Value }
                    })
                }
            });

            var nonNull = new BsonDocument("$match", new BsonDocument
            {
                { "value", new BsonDocument("$ne", BsonNull.Value) }
            });

            var group = new BsonDocument("$group", new BsonDocument
            {
                { "_id", BsonNull.Value },
                { "avg", new BsonDocument("$avg", "$value") }
            });

            var pipeline = new[]
            {
                new BsonDocument("$match", match),
                project,
                nonNull,
                group
            };

            var result = await _collection.Aggregate<BsonDocument>(pipeline).FirstOrDefaultAsync();

            if (result == null || !result.TryGetValue("avg", out var avgValue) || avgValue.IsBsonNull)
            {
                _logger.LogDebug("No numeric values found for schema {SchemaId} and field {Field} since {Since}. Returning 0.", schemaId, field, since);
                return 0m;
            }

            if (!TryToDecimal(avgValue, out var average))
            {
                _logger.LogDebug("Average value could not be converted for schema {SchemaId} and field {Field} since {Since}. Returning 0.", schemaId, field, since);
                return 0m;
            }

            _logger.LogDebug("Calculated average for schema {SchemaId} and field {Field} since {Since}: {Average}.", schemaId, field, since, average);
            return average;
        }

        private static bool TryToDecimal(BsonValue value, out decimal result)
        {
            switch (value.BsonType)
            {
                case BsonType.Decimal128:
                    result = Decimal128.ToDecimal(value.AsDecimal128);
                    return true;

                case BsonType.Double:
                    result = Convert.ToDecimal(value.AsDouble, CultureInfo.InvariantCulture);
                    return true;

                case BsonType.Int64:
                    result = value.AsInt64;
                    return true;

                case BsonType.Int32:
                    result = value.AsInt32;
                    return true;

                case BsonType.String:
                    if (decimal.TryParse(value.AsString, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed))
                    {
                        result = parsed;
                        return true;
                    }
                    break;
            }

            result = 0m;
            return false;
        }
    }
}