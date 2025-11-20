using Alma.Workflows.Databases.Types;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.Json;
using MongoDB.Driver.Linq;

namespace Alma.Workflows.Databases.Providers
{
    public class MongoDBDatabaseProvider : IDatabaseProvider
    {
        private IMongoClient? _client;
        private IMongoDatabase? _database;
        private string? _connectionString;
        private string? _databaseName;

        public DatabaseProviderDescriptor Descriptor { get; init; } = new DatabaseProviderDescriptor
        {
            SystemName = "MongoDB",
            DisplayName = "MongoDB"
        };

        public async ValueTask<ConnectionResult> ConnectAsync(string connectionString, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                return new ConnectionResult { Succeeded = false, Message = "Connection string vazia." };

            try
            {
                _connectionString = connectionString.Trim();
                var mongoUrl = new MongoUrl(_connectionString);
                _databaseName = mongoUrl.DatabaseName;
                _client = new MongoClient(mongoUrl);

                if (!string.IsNullOrWhiteSpace(_databaseName))
                {
                    _database = _client.GetDatabase(_databaseName);
                    using var cursor = await _database.ListCollectionsAsync(cancellationToken: cancellationToken);
                    await cursor.FirstOrDefaultAsync(cancellationToken);
                    return new ConnectionResult { Succeeded = true, Message = $"Conectado (db='{_databaseName}')." };
                }
                else
                {
                    var adminDb = _client.GetDatabase("admin");
                    await adminDb.RunCommandAsync<BsonDocument>(new BsonDocument("ping", 1), cancellationToken: cancellationToken);
                    return new ConnectionResult { Succeeded = true, Message = "Conectado (db não especificado)." };
                }
            }
            catch (Exception ex)
            {
                return new ConnectionResult { Succeeded = false, Message = "Falha ao conectar.", Details = ex.Message };
            }
        }

        public async ValueTask<CommandResult<T>> RunCommandAsync<T>(string command, CancellationToken cancellationToken = default)
        {
            if (_client is null)
                return new CommandResult<T> { Succeeded = false, Message = "Não conectado." };
            if (_database is null)
                return new CommandResult<T> { Succeeded = false, Message = "Database não inicializada." };
            if (string.IsNullOrWhiteSpace(command))
                return new CommandResult<T> { Succeeded = false, Message = "Query vazia." };

            var jsonResult = await RunCommandJsonAsync(command, cancellationToken);

            return new CommandResult<T>
            {
                Succeeded = jsonResult.Succeeded,
                Message = jsonResult.Message,
                Details = jsonResult.Details,
                Data = jsonResult.Succeeded
                    ? JsonSerializer.Deserialize<T>(jsonResult.Data!)
                    : default
            };
        }

        public async ValueTask<CommandResult<string>> RunCommandJsonAsync(string command, CancellationToken cancellationToken = default)
        {
            if (_client is null)
                return new CommandResult<string> { Succeeded = false, Message = "Não conectado." };
            if (_database is null)
                return new CommandResult<string> { Succeeded = false, Message = "Database não inicializada." };
            if (string.IsNullOrWhiteSpace(command))
                return new CommandResult<string> { Succeeded = false, Message = "Query vazia." };

            CommandRequest runCommandRequest;
            try
            {
                runCommandRequest = ParseCommand(command);
            }
            catch (Exception ex)
            {
                return new CommandResult<string> { Succeeded = false, Message = "Query inválida.", Details = ex.Message };
            }

            if (string.IsNullOrWhiteSpace(runCommandRequest.Collection))
                return new CommandResult<string> { Succeeded = false, Message = "Collection inválida." };

            try
            {
                var collection = _database.GetCollection<BsonDocument>(runCommandRequest.Collection);
                var filter = string.IsNullOrWhiteSpace(runCommandRequest.Filter) ? new BsonDocument() : BsonDocument.Parse(runCommandRequest.Filter);

                var docCount = await collection.AsQueryable().CountAsync();

                var docs = await collection
                    .Find(filter)
                    .ToListAsync(cancellationToken);

                if (docs.Count == 0)
                    return new CommandResult<string> { Succeeded = true, Data = "[]", Message = "Nenhum resultado." };

                // Build JSON array
                var jsonArray = "[" + string.Join(',', docs.Select(d => d.ToJson())) + "]";
                return new CommandResult<string> { Succeeded = true, Data = jsonArray, Message = "OK" };
            }
            catch (Exception ex)
            {
                return new CommandResult<string> { Succeeded = false, Message = "Falha na consulta.", Details = ex.Message };
            }
        }

        private static CommandRequest ParseCommand(string query)
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return JsonSerializer.Deserialize<CommandRequest>(query, options)
                ?? throw new ArgumentException("Invalid command.");
        }
    }
}