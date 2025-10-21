using Alma.Workflows.Core.Categories.Entities;
using Alma.Workflows.Core.Categories.Stores;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Alma.Core.Types;
using SimpleCore.Data.Mongo.Extensions;

namespace Alma.Modules.Workflows.Stores
{
    internal class MongoCategoryStore : ICategoryStore
    {
        private readonly ILogger<MongoCategoryStore> _logger;
        private readonly IMongoCollection<Category> _collection;

        public MongoCategoryStore(ILogger<MongoCategoryStore> logger, IMongoDatabase context)
        {
            _logger = logger;
            _collection = context.GetCollection<Category>("Workflows.Category");
        }

        public async ValueTask<Category?> DeleteAsync(string id, string? discriminator = null, CancellationToken cancellationToken = default)
        {
            var filter = Builders<Category>.Filter.Eq(x => x.Id, id);

            if (!string.IsNullOrWhiteSpace(discriminator))
            {
                filter &= Builders<Category>.Filter.Eq(x => x.Discriminator, discriminator);
            }

            return await _collection.FindOneAndDeleteAsync(filter, cancellationToken: cancellationToken);
        }

        public async ValueTask<Category?> FindByIdAsync(string id, string? discriminator = null, CancellationToken cancellationToken = default)
        {
            var query = _collection.AsQueryable();

            if (!string.IsNullOrWhiteSpace(discriminator))
            {
                query = query.Where(x => x.Discriminator == discriminator);
            }

            return await query.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async ValueTask<Category> InsertAsync(Category category, CancellationToken cancellationToken = default)
        {
            await _collection.InsertOneAsync(category, cancellationToken: cancellationToken);
            return category;
        }

        public async ValueTask<PagedList<Category>> ListAsync(int page, int pageSize, CategoryFilters? filters = null, CancellationToken cancellationToken = default)
        {
            var query = _collection.AsQueryable();

            if (filters is not null)
            {
                if (!string.IsNullOrWhiteSpace(filters.Discriminator))
                {
                    query = query.Where(x => x.Discriminator == filters.Discriminator);
                }
            }

            query = query.OrderByDescending(x => x.DefaultName);

            return await query.ToPagedListAsync(page, pageSize);
        }

        public async ValueTask<Category> UpdateAsync(Category category, CancellationToken cancellationToken = default)
        {
            var lastUpdate = category.UpdatedAt;

            category.UpdatedAt = DateTime.Now;

            var filter = Builders<Category>.Filter.And(
                Builders<Category>.Filter.Eq(e => e.Id, category.Id),
                Builders<Category>.Filter.Eq(e => e.Discriminator, category.Discriminator)
            );

            ReplaceOneResult result = null!;

            result = await _collection.ReplaceOneAsync(filter, category, new ReplaceOptions { IsUpsert = false });

            if (!result.IsAcknowledged)
            {
                category.UpdatedAt = lastUpdate;
                _logger.LogError("Error on updating document {Collection} - {Id}. Operation is not acknowledged.", _collection.CollectionNamespace.CollectionName, category.Id);
                throw new Exception($"Error on updating document {_collection.CollectionNamespace.CollectionName} - {category.Id}. Operation is not acknowledged.");
            }
            if (result.MatchedCount == 0)
            {
                var exists = await _collection.AsQueryable()
                    .FirstOrDefaultAsync(x => x.Id == category.Id && x.Discriminator == category.Discriminator);

                if (exists is null)
                {
                    category.UpdatedAt = lastUpdate;
                    throw new Exception($"Document {_collection.CollectionNamespace.CollectionName} - {category.Id} does not exist.");
                }
            }

            return category;
        }
    }
}