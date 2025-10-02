using Alma.Flows.Definitions;
using Alma.Flows.Stores;
using Alma.Flows.Stores.Filters;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Alma.Core.Types;
using SimpleCore.Data.Mongo.Extensions;

namespace Alma.Modules.Flows.Stores
{
    public class MongoFlowDefinitionVersionStore : IFlowDefinitionVersionStore
    {
        private readonly ILogger<MongoFlowDefinitionVersionStore> _logger;
        private readonly IMongoDatabase _context;
        private readonly IMongoCollection<FlowDefinitionVersion> _collection;

        public MongoFlowDefinitionVersionStore(ILogger<MongoFlowDefinitionVersionStore> logger, IMongoDatabase context)
        {
            _logger = logger;
            _context = context;
            _collection = _context.GetCollection<FlowDefinitionVersion>("flows.FlowDefinitionVersion");
        }

        public async ValueTask<FlowDefinitionVersion> InsertAsync(FlowDefinitionVersion version, CancellationToken cancellationToken = default)
        {
            await _collection.InsertOneAsync(version, cancellationToken: cancellationToken);
            return version;
        }

        public ValueTask<FlowDefinitionVersion?> DeleteAsync(string id, string? discriminator = null, CancellationToken cancellationToken = default)
        {
            var filter = Builders<FlowDefinitionVersion>.Filter.Eq(x => x.Id, id);

            if (!string.IsNullOrWhiteSpace(discriminator))
            {
                filter &= Builders<FlowDefinitionVersion>.Filter.Eq(x => x.Discriminator, discriminator);
            }

            return new ValueTask<FlowDefinitionVersion?>(_collection.FindOneAndDelete(filter));
        }

        public async ValueTask<FlowDefinitionVersion?> FindByIdAsync(string id, string? discriminator = null, CancellationToken cancellationToken = default)
        {
            var query = _collection.AsQueryable().Where(x => x.Id == id);

            if (!string.IsNullOrWhiteSpace(discriminator))
            {
                query = query.Where(x => x.Discriminator == discriminator);
            }

            return await query.FirstOrDefaultAsync(cancellationToken);
        }

        public ValueTask<PagedList<FlowDefinitionVersion>> ListAsync(int page, int pageSize, FlowDefinitionVersionFilters? filters = null, CancellationToken cancellationToken = default)
        {
            var query = _collection.AsQueryable();

            if (filters is not null)
            {
                if (!string.IsNullOrWhiteSpace(filters.Name))
                {
                    query = query.Where(x => x.Name.ToLower().Contains(filters.Name));
                }

                if (!string.IsNullOrWhiteSpace(filters.Discriminator))
                {
                    query = query.Where(x => x.Discriminator == filters.Discriminator);
                }

                if (!string.IsNullOrEmpty(filters.FlowDefinitionId))
                {
                    query = query.Where(x => x.FlowDefinitionId == filters.FlowDefinitionId);
                }
            }

            query = query.OrderByDescending(x => x.CreatedAt);

            return new ValueTask<PagedList<FlowDefinitionVersion>>(query.ToPagedListAsync(page, pageSize));
        }

        public ValueTask<string> GetName(string id, string? discriminator = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}