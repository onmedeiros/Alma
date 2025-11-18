using Alma.Workflows.Databases.Providers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Alma.Workflows.Databases.Registry
{
    public interface IDatabaseProviderRegistry
    {
        Task<IDatabaseProvider> GetProvider(string systemName);

        Task<ICollection<IDatabaseProvider>> GetAvailableDatabaseProviders();
    }

    public class DatabaseProviderRegistry : IDatabaseProviderRegistry
    {
        private readonly ILogger<DatabaseProviderRegistry> _logger;
        private readonly IServiceProvider _serviceProvider;

        public DatabaseProviderRegistry(ILogger<DatabaseProviderRegistry> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public Task<IDatabaseProvider> GetProvider(string systemName)
        {
            var provider = _serviceProvider.GetServices<IDatabaseProvider>()
                .FirstOrDefault(p => p.Descriptor.SystemName.Equals(systemName, StringComparison.OrdinalIgnoreCase));

            if (provider is null)
            {
                _logger.LogError("No database provider found for system name: {SystemName}", systemName);
                throw new InvalidOperationException($"No database provider found for system name: {systemName}");
            }

            return Task.FromResult(provider);
        }

        public Task<ICollection<IDatabaseProvider>> GetAvailableDatabaseProviders()
        {
            var providers = _serviceProvider.GetServices<IDatabaseProvider>().ToList();
            return Task.FromResult<ICollection<IDatabaseProvider>>(providers);
        }
    }
}