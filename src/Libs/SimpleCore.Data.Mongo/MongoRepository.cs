using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Alma.Core.Entities;
using Alma.Core.Data.Mongo.Attributes;
using Alma.Core.Data.Mongo.Exceptions;
using Alma.Core.Data.Mongo.Runners;
using System.Linq.Expressions;
using System.Reflection;

namespace SimpleCore.Data.Mongo
{
    public interface IMongoRepository<T>
    {
        Task InsertAsync(T entity);
        Task InsertAsync(IClientSessionHandle session, T entity);
        Task InsertAsync(IEnumerable<T> entities);

        Task UpdateAsync(T entity, bool isUpsert = false);
        Task UpdateAsync(IClientSessionHandle? session, T entity, bool isUpsert = false);
        Task<UpdateResult> UpdateAsync(FilterDefinition<T> filter, UpdateDefinition<T> update);

        Task DeleteAsync(string id, bool soft = true);
        Task SoftDeleteAsync(string id);
        IQueryable<T> AsQueryable();
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
        Task<T?> GetByIdAsync(string id);
        Task<T?> GetOneAsync(Expression<Func<T, bool>> predicate);

        #region Transactions
        Task<IClientSessionHandle> StartSession();
        #endregion

        #region Runners

        UpdateRunner<T> BeginUpdate();
        UpdateRunner<T> BeginUpdate(string id);
        UpdateRunner<T> BeginUpdate(T entity);

        #endregion

    }

    public class MongoRepository<T> : IMongoRepository<T>
        where T : Entity
    {
        private readonly ILogger<MongoRepository<T>> _logger;
        private readonly MongoDbContext _context;
        private readonly IMongoCollection<T> _collection;
        private readonly IMemoryCache _cache;

        public MongoRepository(ILogger<MongoRepository<T>> logger, MongoDbContext context, IMemoryCache cache)
        {
            _logger = logger;
            _context = context;
            _cache = cache;

            // Define o nome da collection
            var type = typeof(T);
            var typeName = type.Name;

            var collection = _cache.GetOrCreate($"MongoRepository_{typeName}_Collection", entry =>
            {
                entry.AbsoluteExpiration = DateTimeOffset.Now.AddDays(30);

                var collectionName = type.GetCustomAttribute<CollectionAttribute>()?.Name;

                if (string.IsNullOrEmpty(collectionName))
                    collectionName = typeName;

                return collectionName;
            });

            // Obtém a collection
            _collection = _context.GetCollection<T>(collection!);
        }


        #region Insert

        public Task InsertAsync(T entity)
        {
            return _collection.InsertOneAsync(entity);
        }

        public Task InsertAsync(IClientSessionHandle session, T entity)
        {
            return _collection.InsertOneAsync(session, entity);
        }

        public Task InsertAsync(IEnumerable<T> entities)
        {
            return _collection.InsertManyAsync(entities);
        }

        #endregion

        #region Update

        // Atualizar um documento
        public Task UpdateAsync(T entity, bool isUpsert = false)
        {
            return UpdateAsync(null, entity, isUpsert);
        }

        public async Task UpdateAsync(IClientSessionHandle? session, T entity, bool isUpsert = false)
        {
            var lastVersion = entity.ModifiedAt;

            entity.ModifiedAt = DateTime.Now;

            var filter = Builders<T>.Filter.And(
                Builders<T>.Filter.Eq(e => e.Id, entity.Id),
                Builders<T>.Filter.Eq(e => e.ModifiedAt, lastVersion)
            );

            ReplaceOneResult result = null!;

            if (session is null)
                result = await _collection.ReplaceOneAsync(filter, entity, new ReplaceOptions { IsUpsert = isUpsert });
            else
                result = await _collection.ReplaceOneAsync(session, filter, entity, new ReplaceOptions { IsUpsert = isUpsert });

            if (!result.IsAcknowledged)
            {
                _logger.LogError("Error on updating document {Collection} - {Id}. Operation is not acknowledged.", _collection.CollectionNamespace.CollectionName, entity.Id);
                throw new Exception("Error on updating document. Operation is not acknowledged.");
            }

            if (result.MatchedCount == 0)
            {
                var exists = _collection.AsQueryable()
                    .FirstOrDefaultAsync(x => x.Id == entity.Id);

                if (exists != null)
                {
                    var entityName = typeof(T).Name;

                    _logger.LogError("Concurrency error on trying to update Entity {EntityName}, with ID {EntityIdentifier}.", entityName, entity.Id);
                    throw new ConcurrencyException($"Concurrency error on trying to update Entity {entityName}, with ID {entity.Id}.")
                    {
                        EntityName = entityName,
                        EntityIdentifier = entity.Id
                    };
                }

                _logger.LogWarning("Warning on updating document {Collection} - {Id}. No document found with specified filters.", _collection.CollectionNamespace.CollectionName, entity.Id);
            }
        }

        // Atualizar apenas campos em um documento existente
        public Task<UpdateResult> UpdateAsync(FilterDefinition<T> filter, UpdateDefinition<T> update)
        {
            return _collection.UpdateManyAsync(filter, update);
        }

        #endregion

        #region Operations
        // Excluir um documento
        public async Task DeleteAsync(string id, bool soft = true)
        {
            if (soft)
            {
                await SoftDeleteAsync(id);
                return;
            }

            var filter = Builders<T>.Filter.Eq(e => e.Id, id);
            var result = _collection.DeleteOneAsync(filter);

            if (!result.IsCompletedSuccessfully)
                throw new Exception("Erro ao excluir o documento.");
        }

        // Deletar virtualmente um documento
        public async Task SoftDeleteAsync(string id)
        {
            var filter = Builders<T>.Filter.Eq(e => e.Id, id);
            var update = Builders<T>.Update
                .Set(e => e.Deleted, true)
                .Set(e => e.ModifiedAt, DateTime.Now);

            var result = await _collection.UpdateOneAsync(filter, update);

            if (!result.IsAcknowledged)
                throw new Exception("Erro ao excluir o documento.");
        }
        #endregion

        #region Queries

        public virtual IQueryable<T> AsQueryable()
        {
            return _collection.AsQueryable()
                .Where(x => !x.Deleted);
        }

        public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            return await _collection.Find(predicate).AnyAsync();
        }

        public virtual async Task<T?> GetByIdAsync(string id)
        {
            return await _collection.Find(e => e.Id == id).FirstOrDefaultAsync();
        }

        public virtual async Task<T?> GetOneAsync(Expression<Func<T, bool>> predicate)
        {
            return await _collection.Find(predicate).FirstOrDefaultAsync();
        }

        #endregion

        #region Transações

        public Task<IClientSessionHandle> StartSession()
        {
            return _context.StartSessionAsync();
        }

        #endregion

        #region Runners

        public UpdateRunner<T> BeginUpdate()
        {
            return new UpdateRunner<T>(this)
                .Set(x => x.ModifiedAt, DateTime.Now);
        }

        public UpdateRunner<T> BeginUpdate(string id)
        {
            return new UpdateRunner<T>(this)
                .Where(x => x.Id == id)
                .Set(x => x.ModifiedAt, DateTime.Now);
        }

        public UpdateRunner<T> BeginUpdate(T entity)
        {
            return new UpdateRunner<T>(this, entity)
                .Where(x => x.Id == entity.Id)
                .Set(x => x.ModifiedAt, DateTime.Now);
        }

        #endregion
    }
}
