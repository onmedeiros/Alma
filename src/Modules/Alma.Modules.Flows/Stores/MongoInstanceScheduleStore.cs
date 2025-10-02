using Alma.Flows.Core.InstanceSchedules.Entities;
using Alma.Flows.Core.InstanceSchedules.Stores;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Alma.Core.Types;
using SimpleCore.Data.Mongo.Extensions;

namespace Alma.Modules.Flows.Stores
{
    public class MongoInstanceScheduleStore : IInstanceScheduleStore
    {
        private readonly ILogger<MongoInstanceScheduleStore> _logger;
        private readonly IMongoDatabase _context;
        private readonly IMongoCollection<InstanceSchedule> _collection;

        public MongoInstanceScheduleStore(ILogger<MongoInstanceScheduleStore> logger, IMongoDatabase context)
        {
            _logger = logger;
            _context = context;
            _collection = _context.GetCollection<InstanceSchedule>("flows.InstanceSchedule");
        }

        public async ValueTask<InstanceSchedule> InsertAsync(InstanceSchedule schedule, CancellationToken cancellationToken = default)
        {
            await _collection.InsertOneAsync(schedule, cancellationToken: cancellationToken);
            return schedule;
        }

        public async ValueTask<InstanceSchedule> UpdateAsync(InstanceSchedule schedule, CancellationToken cancellationToken = default)
        {
            var lastUpdate = schedule.UpdatedAt;

            schedule.UpdatedAt = DateTime.Now;

            var filter = Builders<InstanceSchedule>.Filter.And(
                Builders<InstanceSchedule>.Filter.Eq(e => e.Id, schedule.Id),
                Builders<InstanceSchedule>.Filter.Eq(e => e.Discriminator, schedule.Discriminator)
            );

            ReplaceOneResult result = null!;

            result = await _collection.ReplaceOneAsync(filter, schedule, new ReplaceOptions { IsUpsert = false });

            if (!result.IsAcknowledged)
            {
                schedule.UpdatedAt = lastUpdate;

                _logger.LogError("Error on updating document {Collection} - {Id}. Operation is not acknowledged.", _collection.CollectionNamespace.CollectionName, schedule.Id);
                throw new Exception($"Error on updating document {_collection.CollectionNamespace.CollectionName} - {schedule.Id}. Operation is not acknowledged.");
            }

            if (result.MatchedCount == 0)
            {
                var exists = await _collection.AsQueryable()
                    .FirstOrDefaultAsync(x => x.Id == schedule.Id && x.Discriminator == schedule.Discriminator);

                if (exists is null)
                {
                    schedule.UpdatedAt = lastUpdate;

                    _logger.LogError("Error on updating document {Collection} - {Id}. Document not found.", _collection.CollectionNamespace.CollectionName, schedule.Id);
                    throw new Exception($"Error on updating document {_collection.CollectionNamespace.CollectionName} - {schedule.Id}. Document not found.");
                }

                schedule.UpdatedAt = lastUpdate;

                _logger.LogError("Error on updating document {Collection} - {Id}. MatchedCount = 0.", _collection.CollectionNamespace.CollectionName, schedule.Id);
                throw new Exception($"Error on updating document {_collection.CollectionNamespace.CollectionName} - {schedule.Id}. MatchedCount = 0.");
            }

            return schedule;
        }

        public ValueTask<InstanceSchedule?> DeleteAsync(string id, string? discriminator = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async ValueTask<InstanceSchedule?> FindByIdAsync(string id, string? discriminator = null, CancellationToken cancellationToken = default)
        {
            var query = _collection.AsQueryable();

            if (!string.IsNullOrWhiteSpace(discriminator))
                query = query.Where(x => x.Discriminator == discriminator);

            return await query.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public ValueTask<PagedList<InstanceSchedule>> ListAsync(int page, int pageSize, InstanceScheduleFilters? filters = null, CancellationToken cancellationToken = default)
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

            return new ValueTask<PagedList<InstanceSchedule>>(query.ToPagedListAsync(page, pageSize));
        }
    }
}