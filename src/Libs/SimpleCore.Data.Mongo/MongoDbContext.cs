using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace SimpleCore.Data.Mongo
{
    public class MongoDbContext
    {
        private readonly ILogger<MongoDbContext> _logger;
        private readonly IMongoDatabase _database;

        public MongoDbContext(ILogger<MongoDbContext> logger, IMongoClient client, string connectionString, string databaseName)
        {
            _logger = logger;
            _database = client.GetDatabase(databaseName);
        }

        public IMongoCollection<T> GetCollection<T>(string name)
        {
            return _database.GetCollection<T>(name);
        }

        public Task<IClientSessionHandle> StartSessionAsync()
        {
            return _database.Client.StartSessionAsync();
        }
    }
}
