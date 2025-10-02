using Alma.Flows.Core.Common.Enums;
using Alma.Flows.Core.InstanceEndpoints.Entities;
using Alma.Flows.Core.InstanceEndpoints.Stores;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Alma.Core.Types;
using SimpleCore.Data.Mongo.Extensions;

namespace Alma.Modules.Flows.Stores
{
    public class MongoInstanceEndpointStore : IInstanceEndpointStore
    {
        private readonly ILogger<MongoInstanceEndpointStore> _logger;
        private readonly IMongoDatabase _context;
        private readonly IMongoCollection<InstanceEndpoint> _collection;

        public MongoInstanceEndpointStore(ILogger<MongoInstanceEndpointStore> logger, IMongoDatabase context)
        {
            _logger = logger;
            _context = context;
            _collection = _context.GetCollection<InstanceEndpoint>("flows.InstanceEndpoint");
        }

        public async ValueTask<InstanceEndpoint> InsertAsync(InstanceEndpoint endpoint, CancellationToken cancellationToken = default)
        {
            await _collection.InsertOneAsync(endpoint, cancellationToken: cancellationToken);
            return endpoint;
        }

        public async ValueTask<InstanceEndpoint> UpdateAsync(InstanceEndpoint endpoint, CancellationToken cancellationToken = default)
        {
            var lastUpdate = endpoint.UpdatedAt;

            endpoint.UpdatedAt = DateTime.Now;

            var filter = Builders<InstanceEndpoint>.Filter.And(
                Builders<InstanceEndpoint>.Filter.Eq(e => e.Id, endpoint.Id),
                Builders<InstanceEndpoint>.Filter.Eq(e => e.Discriminator, endpoint.Discriminator)
            );

            ReplaceOneResult result = null!;

            result = await _collection.ReplaceOneAsync(filter, endpoint, new ReplaceOptions { IsUpsert = false });

            if (!result.IsAcknowledged)
            {
                endpoint.UpdatedAt = lastUpdate;

                _logger.LogError("Error on updating document {Collection} - {Id}. Operation is not acknowledged.", _collection.CollectionNamespace.CollectionName, endpoint.Id);
                throw new Exception($"Error on updating document {_collection.CollectionNamespace.CollectionName} - {endpoint.Id}. Operation is not acknowledged.");
            }

            if (result.MatchedCount == 0)
            {
                var exists = await _collection.AsQueryable()
                    .FirstOrDefaultAsync(x => x.Id == endpoint.Id && x.Discriminator == endpoint.Discriminator);

                if (exists is null)
                {
                    endpoint.UpdatedAt = lastUpdate;

                    _logger.LogError("Error on updating document {Collection} - {Id}. Document not found.", _collection.CollectionNamespace.CollectionName, endpoint.Id);
                    throw new Exception($"Error on updating document {_collection.CollectionNamespace.CollectionName} - {endpoint.Id}. Document not found.");
                }

                endpoint.UpdatedAt = lastUpdate;

                _logger.LogError("Error on updating document {Collection} - {Id}. MatchedCount = 0.", _collection.CollectionNamespace.CollectionName, endpoint.Id);
                throw new Exception($"Error on updating document {_collection.CollectionNamespace.CollectionName} - {endpoint.Id}. MatchedCount = 0.");
            }

            return endpoint;
        }

        public ValueTask<InstanceEndpoint?> DeleteAsync(string id, string? discriminator = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async ValueTask<InstanceEndpoint?> FindByIdAsync(string id, string? discriminator = null, CancellationToken cancellationToken = default)
        {
            var query = _collection.AsQueryable();

            if (!string.IsNullOrWhiteSpace(discriminator))
                query = query.Where(x => x.Discriminator == discriminator);

            return await query.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async ValueTask<InstanceEndpoint?> FindByPathAsync(string apiId, string path, ApiMethod method, string? discriminator = null, CancellationToken cancellationToken = default)
        {
            var query = _collection.AsQueryable();

            if (!string.IsNullOrWhiteSpace(discriminator))
                query = query.Where(x => x.Discriminator == discriminator);

            return await query.FirstOrDefaultAsync(x => x.Path == path && x.Method == method && x.ApiId == apiId, cancellationToken);
        }

        public ValueTask<PagedList<InstanceEndpoint>> ListAsync(int page, int pageSize, InstanceEndpointFilters? filters = null, CancellationToken cancellationToken = default)
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

                if (!string.IsNullOrEmpty(filters.InstanceId))
                {
                    query = query.Where(x => x.InstanceId == filters.InstanceId);
                }
            }

            return new ValueTask<PagedList<InstanceEndpoint>>(query.ToPagedListAsync(page, pageSize));
        }
    }
}