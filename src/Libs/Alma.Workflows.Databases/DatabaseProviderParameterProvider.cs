using Alma.Workflows.Core.Activities.Abstractions;
using Alma.Workflows.Core.Activities.Models;
using Alma.Workflows.Databases.Registry;
using Microsoft.Extensions.Logging;

namespace Alma.Workflows.Databases
{
    public class DatabaseProviderParameterProvider : IParameterProvider
    {
        private readonly ILogger<DatabaseProviderParameterProvider> _logger;
        private readonly IDatabaseProviderRegistry _databaseProviderRegistry;

        public DatabaseProviderParameterProvider(ILogger<DatabaseProviderParameterProvider> logger, IDatabaseProviderRegistry databaseProviderRegistry)
        {
            _logger = logger;
            _databaseProviderRegistry = databaseProviderRegistry;
        }

        public async Task<IEnumerable<ParameterOption>> LoadOptionsAsync(string? term = null, string? discriminator = null)
        {
            var providers = await _databaseProviderRegistry.GetAvailableDatabaseProviders();

            var options = providers
                .Select(provider => new ParameterOption
                {
                    DisplayName = provider.Descriptor.DisplayName,
                    Value = provider.Descriptor.SystemName
                })
                .Where(option => string.IsNullOrEmpty(term) || option.DisplayName.Contains(term, StringComparison.OrdinalIgnoreCase))
                .OrderBy(option => option.DisplayName)
                .AsEnumerable();

            return options;
        }
    }
}