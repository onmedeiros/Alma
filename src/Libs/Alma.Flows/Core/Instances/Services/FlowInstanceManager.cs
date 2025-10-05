using Alma.Flows.Core.Instances.Entities;
using Alma.Flows.Core.Instances.Models;
using Alma.Flows.Core.Instances.Stores;
using Microsoft.Extensions.Logging;
using Alma.Core.Types;

namespace Alma.Flows.Core.Instances.Services
{
    public interface IFlowInstanceManager
    {
        ValueTask<FlowInstance> Create(string name, string? id = null, string? discriminator = null);

        ValueTask<FlowInstance> Update(InstanceEditModel model);

        ValueTask<FlowInstance?> FindById(string id, string? discriminator = null);

        ValueTask<FlowInstance?> Delete(string id);

        ValueTask<PagedList<FlowInstance>> List(int page, int pageSize, FlowInstanceFilters? filters = null);

        ValueTask<string> GetName(string id, string? discriminator = null);
    }

    public class FlowInstanceManager : IFlowInstanceManager
    {
        private readonly ILogger<FlowInstanceManager> _logger;
        private readonly IFlowInstanceStore _flowInstanceStore;

        public FlowInstanceManager(ILogger<FlowInstanceManager> logger, IFlowInstanceStore flowInstanceStore)
        {
            _logger = logger;
            _flowInstanceStore = flowInstanceStore;
        }

        public ValueTask<FlowInstance> Create(string name, string? id = null, string? discriminator = null)
        {
            _logger.LogDebug("Creating flow instance with name {Name}.", name);

            id ??= Guid.NewGuid().ToString();

            var now = DateTime.Now;

            var instance = new FlowInstance
            {
                Id = id,
                Discriminator = discriminator,
                CreatedAt = now,
                UpdatedAt = now,
                Name = name,
                IsActive = false
            };

            return _flowInstanceStore.InsertAsync(instance);
        }

        public async ValueTask<FlowInstance> Update(InstanceEditModel model)
        {
            _logger.LogDebug("Updating flow instance with id {Id}.", model.Id);

            var entity = await FindById(model.Id, model.Discriminator);

            if (entity is null)
            {
                _logger.LogError("Flow instance with id {Id} not found.", model.Id);
                throw new Exception($"Flow instance with id {model.Id} not found.");
            }

            entity.UpdatedAt = DateTime.Now;

            if (!string.IsNullOrEmpty(model.Name) && model.Name != entity.Name)
                entity.Name = model.Name;

            if (model.IsActive.HasValue && model.IsActive != entity.IsActive)
                entity.IsActive = model.IsActive.Value;

            if (model.ExecutionMode != entity.ExecutionMode)
                entity.ExecutionMode = model.ExecutionMode;

            if (!string.IsNullOrEmpty(model.FlowDefinitionVersionId) && model.FlowDefinitionVersionId != entity.FlowDefinitionVersionId)
                entity.FlowDefinitionVersionId = model.FlowDefinitionVersionId;

            return await _flowInstanceStore.UpdateAsync(entity);
        }

        public ValueTask<FlowInstance?> FindById(string id, string? discriminator = null)
        {
            _logger.LogDebug("Finding flow instance with id {Id}.", id);
            return _flowInstanceStore.FindByIdAsync(id, discriminator);
        }

        public ValueTask<FlowInstance?> Delete(string id)
        {
            _logger.LogDebug("Deleting flow instance with id {Id}.", id);
            return _flowInstanceStore.DeleteAsync(id);
        }

        public ValueTask<PagedList<FlowInstance>> List(int page, int pageSize, FlowInstanceFilters? filters = null)
        {
            _logger.LogDebug("Listing flow instances.");
            return _flowInstanceStore.ListAsync(page, pageSize, filters);
        }

        public ValueTask<string> GetName(string id, string? discriminator = null)
        {
            return _flowInstanceStore.GetName(id, discriminator);
        }
    }
}