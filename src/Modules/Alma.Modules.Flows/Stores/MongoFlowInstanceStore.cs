using Alma.Modules.Flows.Caching;
using Alma.Flows.Core.Instances.Entities;
using Alma.Flows.Core.Instances.Stores;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Alma.Core.Caching.Extensions;
using Alma.Core.Extensions;
using Alma.Core.Types;
using SimpleCore.Data.Mongo.Extensions;

namespace Alma.Modules.Flows.Stores
{
    public class MongoFlowInstanceStore : IFlowInstanceStore
    {
        private readonly ILogger<MongoFlowInstanceStore> _logger;
        private readonly IMongoDatabase _context;
        private readonly IMongoCollection<FlowInstance> _collection;
        private readonly IDistributedCache _cache;

        public MongoFlowInstanceStore(ILogger<MongoFlowInstanceStore> logger, IMongoDatabase context, IDistributedCache cache)
        {
            _logger = logger;
            _context = context;
            _collection = _context.GetCollection<FlowInstance>("flows.FlowInstance");
            _cache = cache;
        }

        public async ValueTask<FlowInstance> InsertAsync(FlowInstance instance, CancellationToken cancellationToken = default)
        {
            await _collection.InsertOneAsync(instance, cancellationToken: cancellationToken);
            return instance;
        }

        public async ValueTask<FlowInstance> UpdateAsync(FlowInstance instance, CancellationToken cancellationToken = default)
        {
            var lastUpdate = instance.UpdatedAt;

            instance.UpdatedAt = DateTime.Now;

            var filter = Builders<FlowInstance>.Filter.And(
                Builders<FlowInstance>.Filter.Eq(e => e.Id, instance.Id),
                Builders<FlowInstance>.Filter.Eq(e => e.Discriminator, instance.Discriminator)
            );

            ReplaceOneResult result = null!;

            result = await _collection.ReplaceOneAsync(filter, instance, new ReplaceOptions { IsUpsert = false });

            if (!result.IsAcknowledged)
            {
                instance.UpdatedAt = lastUpdate;

                _logger.LogError("Error on updating document {Collection} - {Id}. Operation is not acknowledged.", _collection.CollectionNamespace.CollectionName, instance.Id);
                throw new Exception($"Error on updating document {_collection.CollectionNamespace.CollectionName} - {instance.Id}. Operation is not acknowledged.");
            }

            if (result.MatchedCount == 0)
            {
                var exists = await _collection.AsQueryable()
                    .FirstOrDefaultAsync(x => x.Id == instance.Id && x.Discriminator == instance.Discriminator);

                if (exists is null)
                {
                    instance.UpdatedAt = lastUpdate;

                    _logger.LogError("Error on updating document {Collection} - {Id}. Document not found.", _collection.CollectionNamespace.CollectionName, instance.Id);
                    throw new Exception($"Error on updating document {_collection.CollectionNamespace.CollectionName} - {instance.Id}. Document not found.");
                }

                instance.UpdatedAt = lastUpdate;

                _logger.LogError("Error on updating document {Collection} - {Id}. MatchedCount = 0.", _collection.CollectionNamespace.CollectionName, instance.Id);
                throw new Exception($"Error on updating document {_collection.CollectionNamespace.CollectionName} - {instance.Id}. MatchedCount = 0.");
            }

            // Clear cache
            await _cache.RemoveAsync(string.Format(InstanceCaching.INSTANCE_NAME_BY_ID_ORGANIZATION, instance.Id, instance.Discriminator));

            return instance;
        }

        public ValueTask<FlowInstance?> DeleteAsync(string id, string? discriminator = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async ValueTask<FlowInstance?> FindByIdAsync(string id, string? discriminator = null, CancellationToken cancellationToken = default)
        {
            var query = _collection.AsQueryable();

            if (!string.IsNullOrWhiteSpace(discriminator))
                query = query.Where(x => x.Discriminator == discriminator);

            return await query.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public ValueTask<PagedList<FlowInstance>> ListAsync(int page, int pageSize, FlowInstanceFilters? filters = null, CancellationToken cancellationToken = default)
        {
            var query = _collection.AsQueryable();

            if (filters is not null)
            {
                if (!string.IsNullOrWhiteSpace(filters.Name))
                {
                    query = query.Where(x => x.Name.Contains(filters.Name));
                }

                if (!string.IsNullOrWhiteSpace(filters.Discriminator))
                {
                    query = query.Where(x => x.Discriminator == filters.Discriminator);
                }
            }

            return new ValueTask<PagedList<FlowInstance>>(query.ToPagedListAsync(page, pageSize));
        }

        public async ValueTask<string> GetName(string id, string? discriminator = null, CancellationToken cancellationToken = default)
        {
            var cacheKey = string.Format(InstanceCaching.INSTANCE_NAME_BY_ID_ORGANIZATION, id, discriminator);

            string? name = await _cache.GetOrSetAsync(cacheKey, InstanceCaching.Options, () =>
            {
                var query = _collection.AsQueryable();

                if (!string.IsNullOrWhiteSpace(discriminator))
                    query = query.Where(x => x.Discriminator == discriminator);

                return query.Where(x => x.Id == id).Select(x => x.Name).FirstOrDefaultAsync(cancellationToken);
            });

            return name.IsNullOrEmpty("Sem nome");
        }
    }
}