using Alma.Core.Data;
using Alma.Core.Entities;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace Alma.Core.Mongo.Exceptions
{
    public class MongoUpdate<T> : IUpdate<T>
        where T : Entity
    {
        private readonly MongoContext _context;
        private readonly MongoRepository<T> _repository;
        private readonly FilterDefinitionBuilder<T> _filterBuilder = Builders<T>.Filter;
        private readonly UpdateDefinitionBuilder<T> _updateBuilder = Builders<T>.Update;
        private FilterDefinition<T>? _filter;
        private UpdateDefinition<T>? _update;

        public MongoUpdate(MongoContext context, MongoRepository<T> repository)
        {
            _context = context;
            _repository = repository;
        }

        public IUpdate<T> Set<TValue>(Expression<Func<T, TValue>> expression, TValue value)
        {
            if (_update == null)
            {
                _update = _updateBuilder.Set(expression, value);
            }
            else
            {
                _update = _update.Set(expression, value);
            }

            return this;
        }

        public IUpdate<T> Where(Expression<Func<T, bool>> expression)
        {
            if (_filter == null)
            {
                _filter = _filterBuilder.Where(expression);
            }
            else
            {
                var definition = _filterBuilder.Where(expression);
                _filter &= definition;
            }

            return this;
        }

        public bool HasChanges()
        {
            return _update is not null;
        }

        /// <summary>
        /// Executes the update operation.
        /// </summary>
        /// <returns>Returns the entity updated.</returns>
        /// <exception cref="MongoRepositoryException"></exception>
        public async Task<T?> ExecuteAsync(bool ignoreMatchedCount = false)
        {
            if (_filter is null)
                throw new MongoRepositoryException("Filter is not defined. Use Where method to set a filter before executing the update.", typeof(T).Name, "Update");
            if (_update is null)
                throw new MongoRepositoryException("Update definition is not defined. Use Set method to define an update before executing the update.", typeof(T).Name, "Update");

            Set(x => x.ModifiedAt, DateTime.UtcNow);

            UpdateResult result = null!;

            if (_context.Session is not null)
                result = await _repository.Collection.UpdateOneAsync(_context.Session, _filter, _update);
            else
                result = await _repository.Collection.UpdateOneAsync(_filter, _update);

            if (!result.IsAcknowledged)
                throw new MongoRepositoryException($"Error on updating document {typeof(T).Name}. Operation is not acknowledged.", typeof(T).Name, "Update");

            if (result.MatchedCount == 0 && !ignoreMatchedCount)
                throw new MongoRepositoryException($"No documents matched the filter for update in {typeof(T).Name}.", typeof(T).Name, "Update");

            return await _repository.Collection
                .Find(_filter)
                .FirstOrDefaultAsync();
        }
    }
}