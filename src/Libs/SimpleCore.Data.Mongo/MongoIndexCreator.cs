using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Alma.Core.Data.Mongo.Attributes;
using System.Reflection;

namespace SimpleCore.Data.Mongo
{
    public class MongoIndexCreator : BackgroundService
    {
        private readonly ILogger<MongoIndexCreator> _logger;
        private readonly MongoDbContext _context;
        private readonly SimpleMongoOptions _options;

        public MongoIndexCreator(ILogger<MongoIndexCreator> logger, MongoDbContext context, IOptions<SimpleMongoOptions> options)
        {
            _logger = logger;
            _context = context;
            _options = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            foreach (var assembly in _options.IndexAssemblies)
            {
                var types = assembly.GetTypes().Where(x => x.GetCustomAttributes<CollectionAttribute>().Any());

                foreach (var type in types)
                {
                    _logger.LogInformation($"Ensuring indexes for type {type.Name}.");
                    await EnsureIndexForType(type);
                }
            }
        }

        protected async ValueTask EnsureIndexForType(Type type)
        {
            // Busca o nome da collection.
            var collectionAttribute = type.GetCustomAttribute<CollectionAttribute>()!;
            var collectionName = collectionAttribute.Name;

            // Verifica se alguma propriedade requer índice.
            // Se não houver, não faz nada.
            var properties = type.GetProperties().Where(x => x.GetCustomAttributes<IndexAttribute>().Any());

            if (!properties.Any())
                return;

            // Busca a collection
            var collection = _context.GetCollection<BsonDocument>(collectionName);

            // Busca os índices existentes
            var indexes = await collection.Indexes.ListAsync();
            var indexNames = indexes.ToList().Select(x => x["name"].AsString);

            // Cria o Index para cada propriedade encontrada.
            foreach (var property in properties)
            {
                var indexAttribute = property.GetCustomAttribute<IndexAttribute>()!;
                var indexName = $"Index_{property.Name}_{indexAttribute.Type}";

                if (indexNames.Contains(indexName))
                    continue;

                CreateIndexModel<BsonDocument> indexModel = null!;
                var indexOptions = new CreateIndexOptions
                {
                    Name = indexName,
                };

                if (indexAttribute.Type == IndexType.Ascending)
                    indexModel = new CreateIndexModel<BsonDocument>(Builders<BsonDocument>.IndexKeys.Ascending(property.Name), indexOptions);
                else if (indexAttribute.Type == IndexType.Descending)
                    indexModel = new CreateIndexModel<BsonDocument>(Builders<BsonDocument>.IndexKeys.Descending(property.Name), indexOptions);
                else
                    throw new InvalidOperationException($"Impossible to create index for property {property.Name} in {collectionName}. Index type not supported.");

                await collection.Indexes.CreateOneAsync(indexModel);
                _logger.LogInformation($"Index {indexName} created for collection {collectionName}.");
            }
        }
    }
}
