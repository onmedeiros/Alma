using Alma.Core.Data;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Alma.Core.Mongo
{
    public class IndexConfigurator : IHostedService
    {
        private readonly ILogger<IndexConfigurator> _logger;
        private ContextOptions _options;
        private IMongoDatabase _database;

        public IndexConfigurator(ILogger<IndexConfigurator> logger, IOptions<ContextOptions> options, IMongoDatabase database)
        {
            _logger = logger;
            _options = options.Value;
            _database = database;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (!_options.Indexes.Any())
                return;

            _logger.LogInformation("Starting index configuration...");

            foreach (var index in _options.Indexes)
            {
                try
                {
                    await EnsureIndex(index);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error ensuring index for entity {Entity}: {Message}", index.EntityType.Name, ex.Message);
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private async ValueTask EnsureIndex(EntityIndex index)
        {
            // Get collection name from entity type
            var collectionName = CollectionNameResolver.GetCollectionName(index.EntityType);

            // Get collection
            var collection = _database.GetCollection<BsonDocument>(collectionName);

            // Get existing indexes
            var existingIndexes = await collection.Indexes.ListAsync();
            var indexNames = await existingIndexes
                .ToListAsync()
                .ContinueWith(x => x.Result.Select(i => i["name"].AsString).ToList());

            // Define index name
            var indexName = index.Name ?? $"idx_{string.Join("_", index.Properties)}_{index.Type}";

            if (indexNames.Contains(indexName))
            {
                _logger.LogInformation("Index {IndexName} already exists on collection {CollectionName}. Skipping...", indexName, collectionName);
                return;
            }

            if (index.Properties is null || index.Properties.Length == 0)
            {
                _logger.LogWarning("No properties defined for index {IndexName} on collection {CollectionName}. Skipping...", indexName, collectionName);
                return;
            }

            // Build index keys (compound if multiple properties)
            var builder = Builders<BsonDocument>.IndexKeys;
            var keyDefs = new List<IndexKeysDefinition<BsonDocument>>();
            foreach (var prop in index.Properties)
            {
                if (string.IsNullOrWhiteSpace(prop))
                    continue;

                var current = index.Type switch
                {
                    EntityIndexType.Ascending => builder.Ascending(prop),
                    EntityIndexType.Descending => builder.Descending(prop),
                    _ => builder.Ascending(prop)
                };
                keyDefs.Add(current);
            }

            if (keyDefs.Count == 0)
            {
                _logger.LogWarning("Could not build index keys for {IndexName} on collection {CollectionName}. Skipping...", indexName, collectionName);
                return;
            }

            var keys = keyDefs.Count == 1 ? keyDefs[0] : builder.Combine(keyDefs);

            var model = new CreateIndexModel<BsonDocument>(keys, new CreateIndexOptions
            {
                Name = indexName
            });

            await collection.Indexes.CreateOneAsync(model);
            _logger.LogInformation("Created index {IndexName} on collection {CollectionName}", indexName, collectionName);
        }
    }
}