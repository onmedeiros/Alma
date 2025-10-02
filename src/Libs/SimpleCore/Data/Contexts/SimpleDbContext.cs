using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Alma.Core.Attributes;
using Alma.Core.Entities;
using Alma.Core.Data.Extensions;

namespace SimpleCore.Data.Contexts
{
    public class SimpleDbContext : DbContext
    {
        protected readonly ILogger<SimpleDbContext> Logger;
        private readonly IMemoryCache _memoryCache;

        // Transactions
        private string? _transactionName;
        private IDbContextTransaction? _transaction;

        public SimpleDbContext(ILogger<SimpleDbContext> logger, IMemoryCache memoryCache, DbContextOptions options)
            : base(options)
        {
            Logger = logger;
            _memoryCache = memoryCache;
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Configure Entity mapping as TPC
            builder.Entity<Entity>()
                .Property(x => x.Id)
                .HasMaxLength(36);

            builder.Entity<Entity>()
                .UseTpcMappingStrategy()
                .HasKey(x => x.Id);

            base.OnModelCreating(builder);
        }

        #region Transactions
        public async Task BeginTransaction(string name)
        {
            if (_transaction != null)
            {
                Logger.LogError("A Transaction ({TransactionName}) is already open for this context.", _transactionName);
                throw new InvalidOperationException($"A Transaction ({_transactionName}) is already open for this context.");
            }

            Logger.LogInformation("Starting transaction with name \"{name}\".", name);

            _transactionName = name;
            _transaction = await Database.BeginTransactionAsync();
        }

        public async Task Commit()
        {
            if (_transaction == null)
            {
                Logger.LogError("Has no Transaction to commit.");
                throw new InvalidOperationException("Has no Transaction to commit.");
            }

            Logger.LogInformation("Committing transaction with name \"{name}\".", _transactionName);

            try
            {
                await SaveChangesAsync();
                await _transaction.CommitAsync();
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
                _transactionName = null;
            }
        }

        public async Task Rollback()
        {
            if (_transaction == null)
            {
                Logger.LogError("Has no Transaction to commit.");
                throw new InvalidOperationException("Has no Transaction to commit.");
            }

            Logger.LogInformation("Rolling back transaction with name \"{name}\".", _transactionName);

            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
            _transactionName = null;
        }
        #endregion

        #region Operations

        public override int SaveChanges()
        {
            BeforeSaveChanges();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            BeforeSaveChanges();
            return base.SaveChangesAsync(cancellationToken);
        }

        #endregion

        #region Cache
        public async Task<TEntity> FromCache<TEntity>(string id)
            where TEntity : Entity
        {
            var cacheableAttribute = (CacheableAttribute?)Attribute.GetCustomAttribute(typeof(TEntity), typeof(CacheableAttribute));
            var t = typeof(TEntity);
            if (cacheableAttribute == null)
            {
                var name = typeof(TEntity).Name;
                throw new Exception($"Entity {name} doesn't have the attribute CacheableAttribute.");
            }

            var cacheKey = $"{cacheableAttribute.Key}-{id}";

            Logger.LogInformation("Retrieving object from cache with key {key}.", cacheKey);

            if (!_memoryCache.TryGetValue<TEntity>(cacheKey, out var entity))
            {
                Logger.LogInformation("Object from cache with key {key} not found. Caching object.", cacheKey);

                var slidingExpiration = cacheableAttribute.SlidingExpiration > 0 ? cacheableAttribute.SlidingExpiration : 90;
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromSeconds(10));

                entity = await this.AsQueryable<TEntity>().FirstOrDefaultAsync(x => x.Id == id);

                _memoryCache.Set(cacheKey, entity, cacheEntryOptions);
            }

            if (entity == null)
                throw new Exception("Entity not found.");

            return entity;
        }

        public void ClearCacheFor<TEntity>(TEntity entity)
            where TEntity : Entity
        {
            var cacheableAttribute = (CacheableAttribute?)Attribute.GetCustomAttribute(entity.GetType(), typeof(CacheableAttribute));

            if (cacheableAttribute == null)
            {
                return;
            }

            var cacheKey = $"{cacheableAttribute.Key}-{entity.Id}";

            Logger.LogInformation("Clearing object from cache with key {key}.", cacheKey);
            _memoryCache.Remove(cacheKey);
        }
        #endregion

        #region Private
        private void BeforeSaveChanges()
        {
            foreach (var entry in ChangeTracker.Entries<Entity>())
            {
                if (entry.State == EntityState.Modified)
                {
                    entry.Entity.ModifiedAt = DateTime.UtcNow;
                    ClearCacheFor(entry.Entity);
                }
            }
        }
        #endregion
    }
}
