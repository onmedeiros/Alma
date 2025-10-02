using Alma.Flows.Core.CustomActivities.Entities;
using Alma.Flows.Core.CustomActivities.Stores;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Alma.Core.Types;
using SimpleCore.Data.Mongo.Extensions;

namespace Alma.Modules.Flows.Stores
{
    public class MongoCustomActivityTemplateStore : ICustomActivityTemplateStore
    {
        private readonly ILogger<MongoCustomActivityTemplateStore> _logger;
        private readonly IMongoCollection<CustomActivityTemplate> _collection;
        private readonly IMongoCollection<CustomActivityScript> _scriptCollection;

        public MongoCustomActivityTemplateStore(ILogger<MongoCustomActivityTemplateStore> logger, IMongoDatabase context)
        {
            _logger = logger;
            _collection = context.GetCollection<CustomActivityTemplate>("flows.CustomActivityTemplate");
            _scriptCollection = context.GetCollection<CustomActivityScript>("flows.CustomActivityScript");
        }

        public async ValueTask<CustomActivityTemplate?> DeleteAsync(string id, string? discriminator = null, CancellationToken cancellationToken = default)
        {
            var filter = Builders<CustomActivityTemplate>.Filter.Eq(x => x.Id, id);

            if (!string.IsNullOrWhiteSpace(discriminator))
            {
                filter &= Builders<CustomActivityTemplate>.Filter.Eq(x => x.Discriminator, discriminator);
            }

            return await _collection.FindOneAndDeleteAsync(filter, cancellationToken: cancellationToken);
        }

        public async ValueTask<CustomActivityTemplate?> FindByIdAsync(string id, string? discriminator = null, CancellationToken cancellationToken = default)
        {
            var query = _collection.AsQueryable();

            if (!string.IsNullOrWhiteSpace(discriminator))
            {
                query = query.Where(x => x.Discriminator == discriminator);
            }

            return await query.FirstOrDefaultAsync(x => x.Id == id, cancellationToken: cancellationToken);
        }

        public async ValueTask<CustomActivityTemplate> InsertAsync(CustomActivityTemplate activity, CancellationToken cancellationToken = default)
        {
            await _collection.InsertOneAsync(activity, cancellationToken: cancellationToken);
            return activity;
        }

        public async ValueTask<PagedList<CustomActivityTemplate>> ListAsync(int page, int pageSize, CustomActivityFilters? filters = null, CancellationToken cancellationToken = default)
        {
            var query = _collection.AsQueryable();

            if (filters is not null)
            {
                if (!string.IsNullOrWhiteSpace(filters.Discriminator))
                {
                    query = query.Where(x => x.Discriminator == filters.Discriminator);
                }
                if (!string.IsNullOrWhiteSpace(filters.Name))
                {
                    query = query.Where(x => x.Name.Contains(filters.Name));
                }
            }

            query = query.OrderByDescending(x => x.Name);

            return await query.ToPagedListAsync(page, pageSize);
        }

        public async ValueTask<CustomActivityTemplate> UpdateAsync(CustomActivityTemplate activity, CancellationToken cancellationToken = default)
        {
            var lastUpdate = activity.UpdatedAt;

            activity.UpdatedAt = DateTime.Now;

            var filter = Builders<CustomActivityTemplate>.Filter.And(
                Builders<CustomActivityTemplate>.Filter.Eq(e => e.Id, activity.Id),
                Builders<CustomActivityTemplate>.Filter.Eq(e => e.Discriminator, activity.Discriminator)
            );

            ReplaceOneResult result = null!;

            result = await _collection.ReplaceOneAsync(filter, activity, new ReplaceOptions { IsUpsert = false });

            if (!result.IsAcknowledged)
            {
                activity.UpdatedAt = lastUpdate;
                _logger.LogError("Error on updating document {Collection} - {Id}. Operation is not acknowledged.", _collection.CollectionNamespace.CollectionName, activity.Id);
                throw new Exception($"Error on updating document {_collection.CollectionNamespace.CollectionName} - {activity.Id}. Operation is not acknowledged.");
            }
            if (result.MatchedCount == 0)
            {
                var exists = await _collection.AsQueryable()
                    .FirstOrDefaultAsync(x => x.Id == activity.Id && x.Discriminator == activity.Discriminator);

                if (exists is null)
                {
                    activity.UpdatedAt = lastUpdate;
                    throw new Exception($"Document {_collection.CollectionNamespace.CollectionName} - {activity.Id} does not exist.");
                }
            }

            return activity;
        }

        public async ValueTask<CustomActivityScript?> FindScriptAsync(string customActivityId, string? discriminator = null, CancellationToken cancellationToken = default)
        {
            var query = _scriptCollection.AsQueryable()
                .Where(x => x.CustomActivityTemplateId == customActivityId);

            if (!string.IsNullOrWhiteSpace(discriminator))
            {
                query = query.Where(x => x.Discriminator == discriminator);
            }

            return await query.FirstOrDefaultAsync(cancellationToken: cancellationToken);
        }

        public async ValueTask<CustomActivityScript> InsertScriptAsync(CustomActivityScript script, CancellationToken cancellationToken = default)
        {
            script.CreatedAt = DateTime.Now;
            script.UpdatedAt = script.CreatedAt;

            await _scriptCollection.InsertOneAsync(script, cancellationToken: cancellationToken);

            return script;
        }

        public ValueTask<CustomActivityScript> UpdateScriptAsync(CustomActivityScript script, CancellationToken cancellationToken = default)
        {
            var lastUpdate = script.UpdatedAt;

            script.UpdatedAt = DateTime.Now;

            var filter = Builders<CustomActivityScript>.Filter.And(
                Builders<CustomActivityScript>.Filter.Eq(e => e.Id, script.Id),
                Builders<CustomActivityScript>.Filter.Eq(e => e.Discriminator, script.Discriminator)
            );

            ReplaceOneResult result = null!;

            result = _scriptCollection.ReplaceOne(filter, script, new ReplaceOptions { IsUpsert = false });

            if (!result.IsAcknowledged)
            {
                script.UpdatedAt = lastUpdate;
                _logger.LogError("Error on updating document {Collection} - {Id}. Operation is not acknowledged.", _scriptCollection.CollectionNamespace.CollectionName, script.Id);
                throw new Exception($"Error on updating document {_scriptCollection.CollectionNamespace.CollectionName} - {script.Id}. Operation is not acknowledged.");
            }

            if (result.MatchedCount == 0)
            {
                var exists = _scriptCollection.AsQueryable()
                    .FirstOrDefault(x => x.Id == script.Id && x.Discriminator == script.Discriminator);
                if (exists is null)
                {
                    script.UpdatedAt = lastUpdate;
                    throw new Exception($"Document {_scriptCollection.CollectionNamespace.CollectionName} - {script.Id} does not exist.");
                }
            }

            return ValueTask.FromResult(script);
        }
    }
}