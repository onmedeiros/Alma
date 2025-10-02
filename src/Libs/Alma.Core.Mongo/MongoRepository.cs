using Alma.Core.Data;
using Alma.Core.Entities;
using Alma.Core.Mongo.Exceptions;
using Alma.Core.Types;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Linq.Expressions;

namespace Alma.Core.Mongo
{
    public class MongoRepository<T> : IRepository<T>
        where T : Entity
    {
        private readonly ILogger<MongoRepository<T>> _logger;
        private readonly MongoContext _context;
        private readonly IMongoDatabase _database;
        public IMongoCollection<T> Collection;

        public MongoRepository(ILogger<MongoRepository<T>> logger, MongoContext context, IMongoDatabase database)
        {
            _logger = logger;
            _context = context;
            _database = database;
            Collection = _database.GetCollection<T>(typeof(T).Name);
        }

        #region Operations

        public Task InsertAsync(T entity)
        {
            if (_context.Session is not null)
                return Collection.InsertOneAsync(_context.Session, entity);

            return Collection.InsertOneAsync(entity);
        }

        public async Task UpdateAsync(T entity)
        {
            var lastVersion = entity.ModifiedAt;

            entity.ModifiedAt = DateTime.Now;

            var filter = Builders<T>.Filter.And(
                Builders<T>.Filter.Eq(e => e.Id, entity.Id),
                Builders<T>.Filter.Eq(e => e.ModifiedAt, lastVersion)
            );

            ReplaceOneResult result = null!;

            if (_context.Session is not null)
                result = await Collection.ReplaceOneAsync(_context.Session, filter, entity, new ReplaceOptions { IsUpsert = false });
            else
                result = await Collection.ReplaceOneAsync(filter, entity, new ReplaceOptions { IsUpsert = false });

            if (!result.IsAcknowledged)
            {
                _logger.LogError("Error on updating document {EntityName} - {Id}. Operation is not acknowledged.", typeof(T).Name, entity.Id);
                throw new MongoRepositoryException($"Error on updating document {typeof(T).Name} - {entity.Id}. Operation is not acknowledged.", typeof(T).Name, "Update");
            }

            if (result.MatchedCount == 0)
            {
                var exists = Collection.AsQueryable()
                    .FirstOrDefaultAsync(x => x.Id == entity.Id);

                if (exists != null)
                {
                    var entityName = typeof(T).Name;

                    _logger.LogError("Document {EntityName} with ID {Id} was modified by another operation. Update failed.", entityName, entity.Id);
                    throw new MongoRepositoryException($"Document {entityName} with ID {entity.Id} was modified by another operation. Update failed.", entityName, "Update");
                }
            }
        }

        public IUpdate<T> BeginUpdate()
        {
            return new MongoUpdate<T>(_context, this);
        }

        public async Task<T?> DeleteAsync(string id, bool hard = false)
        {
            var entity = await AsQueryable().FirstOrDefaultAsync(x => x.Id == id);

            if (entity is null)
            {
                _logger.LogWarning("Document with ID {Id} not found for deletion.", id);
                return null;
            }

            if (!hard)
            {
                entity.Deleted = true;
                await UpdateAsync(entity);
                return entity;
            }

            DeleteResult? result = null;

            if (_context.Session is not null)
                result = await Collection.DeleteOneAsync(_context.Session, x => x.Id == id);
            else
                result = await Collection.DeleteOneAsync(x => x.Id == id);

            if (result is null || !result.IsAcknowledged || result.DeletedCount == 0)
            {
                _logger.LogError("Error on deleting document {EntityName} - {Id}. Operation is not acknowledged.", typeof(T).Name, id);
                throw new MongoRepositoryException($"Error on deleting document {typeof(T).Name} - {id}. Operation is not acknowledged.", typeof(T).Name, "Delete");
            }

            return entity;
        }

        #endregion

        #region Queries

        public IQueryable<T> AsQueryable()
        {
            return Collection.AsQueryable()
                .Where(x => !x.Deleted);
        }

        public Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            return AsQueryable()
                .AnyAsync(predicate);
        }

        public Task<List<T>> GetAllAsync()
        {
            return AsQueryable()
                .ToListAsync();
        }

        public Task<T> GetByIdAsync(string id)
        {
            return AsQueryable()
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<T?> GetOneAsync(Expression<Func<T, bool>> predicate)
        {
            var entity = await AsQueryable().FirstOrDefaultAsync(predicate);
            return entity;
        }

        public async Task<PagedList<T>> GetPagedAsync(int page, int pageSize, IQueryable<T>? query = null)
        {
            query ??= AsQueryable();

            var count = await query.CountAsync();

            page = page == 0 ? 1 : page;
            pageSize = pageSize == 0 || pageSize == int.MaxValue ? count : pageSize;

            var result = new PagedList<T>()
            {
                TotalCount = count,
                PageIndex = page,
                PageSize = pageSize
            };

            if (count == 0) return result;

            if (pageSize != count)
                query = query.Skip((page - 1) * pageSize).Take(pageSize);

            result.AddRange(await query.ToListAsync());

            return result;
        }

        public Task<List<T>> ToListAsync(IQueryable<T>? query = null)
        {
            query ??= AsQueryable();
            return query.ToListAsync();
        }

        #endregion
    }
}