using Alma.Flows.Definitions;
using Alma.Flows.Stores;
using Alma.Flows.Stores.Filters;
using Microsoft.Extensions.Logging;
using Alma.Core.Types;

namespace Alma.Flows
{
    public interface IFlowManager
    {
        ValueTask<FlowDefinition> CreateDefinition(string name, string? identifier = null, string? discriminator = null);
        ValueTask<FlowDefinition> UpdateDefinition(FlowDefinition definition);
        ValueTask<FlowDefinition?> FindDefinitionById(string id, string? discriminator = null);
        ValueTask<FlowDefinition?> DeleteDefinition(string id);
        ValueTask<string> GetName(string id, string? discriminator = null);

        ValueTask<PagedList<FlowDefinition>> ListDefinitions(int page, int pageSize, FlowDefinitionFilters? filters = null);
        ValueTask<FlowDefinitionVersion> PublishDefinitionVersion(FlowDefinition definition, string name, string? identifier = null, string? discriminator = null);
        ValueTask<FlowDefinitionVersion?> FindDefinitionVersionById(string id, string? discriminator = null);
        ValueTask<PagedList<FlowDefinitionVersion>> ListDefinitionVersions(int page, int pageSize, FlowDefinitionVersionFilters? filters = null);
        ValueTask<string> GetDefinitionVersionName(string id, string? discriminator = null);
    }

    public class FlowManager : IFlowManager
    {
        private readonly ILogger<FlowManager> _logger;
        private readonly IFlowDefinitionStore _flowDefinitionStore;
        private readonly IFlowDefinitionVersionStore _flowDefinitionVersionStore;

        public FlowManager(ILogger<FlowManager> logger, IFlowDefinitionStore flowDefinitionStore, IFlowDefinitionVersionStore flowDefinitionVersionStore)
        {
            _logger = logger;
            _flowDefinitionStore = flowDefinitionStore;
            _flowDefinitionVersionStore = flowDefinitionVersionStore;
        }

        public ValueTask<FlowDefinition> CreateDefinition(string name, string? identifier = null, string? discriminator = null)
        {
            _logger.LogDebug("Creating flow definition with name {Name}.", name);

            identifier ??= Guid.NewGuid().ToString();

            var definition = new FlowDefinition
            {
                Id = identifier,
                Discriminator = discriminator,
                Namespace = "Alma.Flows",
                Name = name,
                FullName = $"Alma.Flows.Flow",
                Type = "Flow",
                TypeName = "Alma.Flows.Flow",
            };

            return _flowDefinitionStore.InsertAsync(definition);
        }

        public ValueTask<FlowDefinition> UpdateDefinition(FlowDefinition definition)
        {
            _logger.LogDebug("Updating flow definition with id {Id}.", definition.Id);

            return _flowDefinitionStore.UpdateAsync(definition);
        }

        public ValueTask<FlowDefinition?> FindDefinitionById(string id, string? discriminator = null)
        {
            _logger.LogDebug("Finding flow definition with id {Id}.", id);

            return _flowDefinitionStore.FindByIdAsync(id, discriminator);
        }

        public ValueTask<FlowDefinition?> DeleteDefinition(string id)
        {
            _logger.LogDebug("Deleting flow definition with id {Id}.", id);
            return _flowDefinitionStore.DeleteAsync(id);
        }

        public ValueTask<PagedList<FlowDefinition>> ListDefinitions(int page, int pageSize, FlowDefinitionFilters? filters = null)
        {
            _logger.LogDebug("Listing flow definitions.");
            return _flowDefinitionStore.ListAsync(page, pageSize, filters);
        }
        public ValueTask<string> GetName(string id, string? discriminator = null)
        {
            return _flowDefinitionStore.GetName(id, discriminator);
        }


        public ValueTask<FlowDefinitionVersion> PublishDefinitionVersion(FlowDefinition definition, string name, string? identifier = null, string? discriminator = null)
        {
            _logger.LogDebug("Publishing flow definition version with name {Name}.", name);

            var now = DateTime.Now;
            identifier ??= Guid.NewGuid().ToString();

            var version = new FlowDefinitionVersion
            {
                Id = Guid.NewGuid().ToString(),
                FlowDefinitionId = definition.Id,
                Discriminator = definition.Discriminator,
                Name = name,
                CreatedAt = now,
                FlowDefinition = definition
            };

            return _flowDefinitionVersionStore.InsertAsync(version);
        }

        public ValueTask<FlowDefinitionVersion?> FindDefinitionVersionById(string id, string? discriminator = null)
        {
            _logger.LogDebug("Finding flow definition version with id {Id}.", id);
            return _flowDefinitionVersionStore.FindByIdAsync(id, discriminator);
        }

        public ValueTask<PagedList<FlowDefinitionVersion>> ListDefinitionVersions(int page, int pageSize, FlowDefinitionVersionFilters? filters = null)
        {
            _logger.LogDebug("Listing flow definition versions.");

            return _flowDefinitionVersionStore.ListAsync(page, pageSize, filters);
        }
        public ValueTask<string> GetDefinitionVersionName(string id, string? discriminator = null)
        {
            return _flowDefinitionVersionStore.GetName(id, discriminator);
        }
    }
}