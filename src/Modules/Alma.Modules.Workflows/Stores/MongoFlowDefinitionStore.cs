using Alma.Workflows.Definitions;
using Alma.Workflows.Stores;
using Alma.Workflows.Stores.Filters;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Alma.Core.Types;
using SimpleCore.Data.Mongo.Extensions;
using Alma.Core.Data;

namespace Alma.Modules.Workflows.Stores
{
    public class MongoFlowDefinitionStore : IFlowDefinitionStore
    {
        private readonly ILogger<MongoFlowDefinitionStore> _logger;
        private readonly IMongoDatabase _context;
        private readonly IMongoCollection<FlowDefinition> _collection;

        public MongoFlowDefinitionStore(ILogger<MongoFlowDefinitionStore> logger, IMongoDatabase context)
        {
            _logger = logger;
            _context = context;
            _collection = _context.GetCollection<FlowDefinition>("Workflows.FlowDefinition");
        }

        public async ValueTask<FlowDefinition> InsertAsync(FlowDefinition definition, CancellationToken cancellationToken = default)
        {
            await _collection.InsertOneAsync(definition, cancellationToken: cancellationToken);
            return definition;
        }

        public ValueTask<FlowDefinition?> DeleteAsync(string id, string? discriminator = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async ValueTask<FlowDefinition?> FindByIdAsync(string id, string? discriminator = null, CancellationToken cancellationToken = default)
        {
            var query = _collection.AsQueryable();

            if (!string.IsNullOrWhiteSpace(discriminator))
                query = query.Where(x => x.Discriminator == discriminator);

            return await query.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async ValueTask<FlowDefinition> UpdateAsync(FlowDefinition definition, CancellationToken cancellationToken = default)
        {
            var lastUpdate = definition.LastUpdate;

            definition.LastUpdate = DateTime.Now;

            var filter = Builders<FlowDefinition>.Filter.And(
                Builders<FlowDefinition>.Filter.Eq(e => e.Id, definition.Id),
                Builders<FlowDefinition>.Filter.Eq(e => e.Discriminator, definition.Discriminator)
            );

            ReplaceOneResult result = null!;

            result = await _collection.ReplaceOneAsync(filter, definition, new ReplaceOptions { IsUpsert = false });

            if (!result.IsAcknowledged)
            {
                definition.LastUpdate = lastUpdate;

                _logger.LogError("Error on updating document {Collection} - {Id}. Operation is not acknowledged.", _collection.CollectionNamespace.CollectionName, definition.Id);
                throw new Exception($"Error on updating document {_collection.CollectionNamespace.CollectionName} - {definition.Id}. Operation is not acknowledged.");
            }

            if (result.MatchedCount == 0)
            {
                var exists = await _collection.AsQueryable()
                    .FirstOrDefaultAsync(x => x.Id == definition.Id && x.Discriminator == definition.Discriminator);

                if (exists is null)
                {
                    definition.LastUpdate = lastUpdate;

                    _logger.LogError("Error on updating document {Collection} - {Id}. Document not found.", _collection.CollectionNamespace.CollectionName, definition.Id);
                    throw new Exception($"Error on updating document {_collection.CollectionNamespace.CollectionName} - {definition.Id}. Document not found.");
                }

                definition.LastUpdate = lastUpdate;

                _logger.LogError("Error on updating document {Collection} - {Id}. MatchedCount = 0.", _collection.CollectionNamespace.CollectionName, definition.Id);
                throw new Exception($"Error on updating document {_collection.CollectionNamespace.CollectionName} - {definition.Id}. MatchedCount = 0.");
            }

            return definition;
        }

        public ValueTask<PagedList<FlowDefinition>> ListAsync(int page, int pageSize, FlowDefinitionFilters? filters = null, CancellationToken cancellationToken = default)
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
            }

            query = query.OrderByDescending(x => x.Name);

            return new ValueTask<PagedList<FlowDefinition>>(query.ToPagedListAsync(page, pageSize));
        }

        public ValueTask<PagedList<FlowDefinition>> ListAsync(int page, int pageSize, Filters<FlowDefinition>? filters = null, CancellationToken cancellationToken = default)
        {
            var query = _collection.AsQueryable();

            // Apply filters if provided
            if (filters is not null)
            {
                query = filters.Apply(query);
            }

            var totalCount = query.Count();
            var items = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var list = new PagedList<FlowDefinition>
            {
                PageIndex = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };

            list.AddRange(items);

            return new ValueTask<PagedList<FlowDefinition>>(list);
        }

        public ValueTask<string> GetName(string id, string? discriminator = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}