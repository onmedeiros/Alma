using Alma.Flows.Core.InstanceExecutions.Entities;
using Alma.Flows.Core.InstanceExecutions.Stores;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Alma.Core.Types;
using SimpleCore.Data.Mongo.Extensions;

namespace Alma.Modules.Flows.Stores
{
    internal class MongoInstanceExecutionStore : IInstanceExecutionStore
    {
        private readonly ILogger<MongoInstanceExecutionStore> _logger;
        private readonly IMongoDatabase _context;
        private readonly IMongoCollection<InstanceExecution> _collection;

        public MongoInstanceExecutionStore(ILogger<MongoInstanceExecutionStore> logger, IMongoDatabase context)
        {
            _logger = logger;
            _context = context;
            _collection = _context.GetCollection<InstanceExecution>("flows.InstanceExecution");
        }

        public ValueTask<InstanceExecution?> DeleteAsync(string id, string? discriminator = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async ValueTask<InstanceExecution?> FindByIdAsync(string id, string? discriminator = null, CancellationToken cancellationToken = default)
        {
            var query = _collection.AsQueryable();

            if (!string.IsNullOrWhiteSpace(discriminator))
            {
                query = query.Where(x => x.Discriminator == discriminator);
            }

            return await query.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async ValueTask<InstanceExecution> InsertAsync(InstanceExecution execution, CancellationToken cancellationToken = default)
        {
            await _collection.InsertOneAsync(execution, cancellationToken: cancellationToken);
            return execution;
        }

        public ValueTask<PagedList<InstanceExecution>> ListAsync(int page, int pageSize, InstanceExecutionFilters? filters = null, CancellationToken cancellationToken = default)
        {
            var query = _collection.AsQueryable();

            if (filters is not null)
            {
                if (!string.IsNullOrWhiteSpace(filters.Discriminator))
                {
                    query = query.Where(x => x.Discriminator == filters.Discriminator);
                }
            }

            query = query.OrderByDescending(x => x.CreatedAt);

            return new ValueTask<PagedList<InstanceExecution>>(query.ToPagedListAsync(page, pageSize));
        }

        public async ValueTask<InstanceExecution> UpdateAsync(InstanceExecution execution, CancellationToken cancellationToken = default)
        {
            var lastUpdate = execution.UpdatedAt;

            execution.UpdatedAt = DateTime.Now;
            var filter = Builders<InstanceExecution>.Filter.And(
                Builders<InstanceExecution>.Filter.Eq(e => e.Id, execution.Id),
                Builders<InstanceExecution>.Filter.Eq(e => e.Discriminator, execution.Discriminator)
            );

            ReplaceOneResult result = null!;

            result = await _collection.ReplaceOneAsync(filter, execution, new ReplaceOptions { IsUpsert = false });

            if (!result.IsAcknowledged)
            {
                execution.UpdatedAt = lastUpdate;
                _logger.LogError("Error on updating document {Collection} - {Id}. Operation is not acknowledged.", _collection.CollectionNamespace.CollectionName, execution.Id);
                throw new Exception($"Error on updating document {_collection.CollectionNamespace.CollectionName} - {execution.Id}. Operation is not acknowledged.");
            }

            if (result.MatchedCount == 0)
            {
                var exists = await _collection.AsQueryable()
                    .FirstOrDefaultAsync(x => x.Id == execution.Id && x.Discriminator == execution.Discriminator);

                if (exists is null)
                {
                    _logger.LogError("Error on updating document {Collection} - {Id}. Document not found.", _collection.CollectionNamespace.CollectionName, execution.Id);
                    throw new Exception($"Error on updating document {_collection.CollectionNamespace.CollectionName} - {execution.Id}. Document not found.");
                }

                execution.UpdatedAt = lastUpdate;

                _logger.LogError("Error on updating document {Collection} - {Id}. Document not found.", _collection.CollectionNamespace.CollectionName, execution.Id);
                throw new Exception($"Error on updating document {_collection.CollectionNamespace.CollectionName} - {execution.Id}. Document not found.");
            }

            return execution;
        }
    }
}