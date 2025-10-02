using MongoDB.Driver;
using System.Linq.Expressions;

namespace SimpleCore.Data.Mongo.Runners
{
    public class UpdateRunner<T>
    {
        private readonly IMongoRepository<T> _repository;
        private readonly T? _entity;
        private readonly FilterDefinitionBuilder<T> _filterBuilder = Builders<T>.Filter;
        private readonly UpdateDefinitionBuilder<T> _updateBuilder = Builders<T>.Update;
        private FilterDefinition<T>? _filter;
        private UpdateDefinition<T>? _update;

        public T? Entity => _entity;

        public UpdateRunner(IMongoRepository<T> repository)
        {
            _repository = repository;
        }

        public UpdateRunner(IMongoRepository<T> repository, T entity) : this(repository)
        {
            _entity = entity;
        }

        public UpdateRunner<T> Where(Expression<Func<T, bool>> expression)
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

        public UpdateRunner<T> Set<TValue>(Expression<Func<T, TValue>> expression, TValue value)
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

        public UpdateRunner<T> Push<TValue>(Expression<Func<T, IEnumerable<TValue>>> expression, TValue value)
        {
            // Converte o valor em um array para combinar corretamente
            var pushUpdate = _updateBuilder.PushEach(expression, [value]);

            if (_update == null)
            {
                // Caso seja o primeiro estágio de atualização
                _update = pushUpdate;
            }
            else
            {
                // Combina explicitamente o novo estágio com os existentes
                _update = _updateBuilder.Combine(_update, pushUpdate);
            }

            // Renderiza a atualização para debug
            // Console.WriteLine(_update?.Render(new RenderArgs<T>(BsonSerializer.SerializerRegistry.GetSerializer<T>(), BsonSerializer.SerializerRegistry)));

            return this;
        }

        public UpdateRunner<T> PushEach<TValue>(Expression<Func<T, IEnumerable<TValue>>> expression, IEnumerable<TValue> values)
        {
            // Converte o valor em um array para combinar corretamente
            var pushUpdate = _updateBuilder.PushEach(expression, values);

            if (_update == null)
            {
                // Caso seja o primeiro estágio de atualização
                _update = pushUpdate;
            }
            else
            {
                // Combina explicitamente o novo estágio com os existentes
                _update = _updateBuilder.Combine(_update, pushUpdate);
            }

            // Renderiza a atualização para debug
            // Console.WriteLine(_update?.Render(new RenderArgs<T>(BsonSerializer.SerializerRegistry.GetSerializer<T>(), BsonSerializer.SerializerRegistry)));

            return this;
        }

        public Task ExecuteAsync()
        {
            if (_filter == null)
            {
                throw new Exception("Filter is required to update documents.");
            }

            if (_update == null)
            {
                throw new Exception("Update definition is required to update documents.");
            }

            // Tratamento de concorrencia
            //if (_entity != null && _entity is Entity entity)
            //{
            //    _filter &= _filterBuilder.Eq("ModifiedAt", entity.ModifiedAt);
            //}

            return _repository.UpdateAsync(_filter, _update);
        }
    }
}
