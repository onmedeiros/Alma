using Alma.Core.Data;
using Alma.Core.Entities;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Alma.Core.Mongo
{
    public class MongoContext : IContext
    {
        private readonly ILogger<MongoContext> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IMongoClient _client;
        public IClientSessionHandle? Session { get; private set; } = null;

        public MongoContext(ILogger<MongoContext> logger, IServiceProvider serviceProvider, IMongoClient client)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _client = client;
        }

        #region Configuration

        public ValueTask SetupAsync()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Repositories

        public IRepository<T> Repository<T>() where T : Entity
        {
            var repositoryType = typeof(IRepository<>).MakeGenericType(typeof(T));
            var repository = _serviceProvider.GetService(repositoryType);

            return repository is null
                ? throw new InvalidOperationException($"Repository for type {typeof(T).Name} not found.")
                : (IRepository<T>)repository;
        }

        #endregion

        #region Transactions

        public async ValueTask BeginTransactionAsync()
        {
            if (Session is not null)
            {
                _logger.LogError("A transaction is already in progress.");
                throw new InvalidOperationException("A transaction is already in progress.");
            }

            Session = await _client.StartSessionAsync();
            Session.StartTransaction();
        }

        public async ValueTask CommitAsync()
        {
            if (Session is null)
            {
                _logger.LogError("No transaction in progress to commit.");
                throw new InvalidOperationException("No transaction in progress to commit.");
            }

            await Session.CommitTransactionAsync();
            Session.Dispose();
            Session = null;
        }

        public async ValueTask RollbackAsync()
        {
            if (Session is null)
            {
                _logger.LogError("No transaction in progress to rollback.");
                throw new InvalidOperationException("No transaction in progress to rollback.");
            }

            await Session.AbortTransactionAsync();
            Session.Dispose();
            Session = null;
        }

        #endregion
    }
}