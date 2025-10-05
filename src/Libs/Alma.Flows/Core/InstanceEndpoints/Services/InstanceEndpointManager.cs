using Alma.Flows.Core.Common.Enums;
using Alma.Flows.Core.InstanceEndpoints.Entities;
using Alma.Flows.Core.InstanceEndpoints.Models;
using Alma.Flows.Core.InstanceEndpoints.Stores;
using Microsoft.Extensions.Logging;
using Alma.Core.Types;

namespace Alma.Flows.Core.InstanceEndpoints.Services
{
    public interface IInstanceEndpointManager
    {
        ValueTask<InstanceEndpoint> CreateAsync(InstanceEndpointCreateModel model);

        ValueTask<InstanceEndpoint> UpdateAsync(InstanceEndpointEditModel model);

        ValueTask<InstanceEndpoint?> FindByIdAsync(string id, string? discriminator = null);

        ValueTask<InstanceEndpoint?> FindByPath(string apiId, string path, ApiMethod method, string? discriminator = null, CancellationToken cancellationToken = default);

        ValueTask<PagedList<InstanceEndpoint>> ListAsync(int page, int pageSize, InstanceEndpointFilters? filters = null);

        ValueTask<PagedList<InstanceEndpoint>> ListAllByInstanceIdAsync(string instanceId, string? discriminator = null);
    }

    public class InstanceEndpointManager : IInstanceEndpointManager
    {
        private readonly ILogger<InstanceEndpointManager> _logger;
        private readonly IInstanceEndpointStore _instanceApiStore;

        public InstanceEndpointManager(ILogger<InstanceEndpointManager> logger, IInstanceEndpointStore instanceApiStore)
        {
            _logger = logger;
            _instanceApiStore = instanceApiStore;
        }

        public async ValueTask<InstanceEndpoint> CreateAsync(InstanceEndpointCreateModel model)
        {
            _logger.LogDebug("Creating instance endpoint with name {Name}.", model.Name);

            var now = DateTime.Now;

            var api = new InstanceEndpoint
            {
                Id = Guid.NewGuid().ToString(),
                Discriminator = model.Discriminator,
                InstanceId = model.InstanceId,
                CreatedAt = now,
                UpdatedAt = now,
                Name = model.Name,
                ApiId = model.ApiId,
                Path = model.Path.Trim().TrimStart('/').TrimEnd('/'),
                Method = model.Method,
                IsActive = model.IsActive
            };

            await _instanceApiStore.InsertAsync(api);

            _logger.LogDebug("Instance endpoint with name {Name} created with id {Id}.", model.Name, api.Id);

            return api;
        }

        public async ValueTask<InstanceEndpoint> UpdateAsync(InstanceEndpointEditModel model)
        {
            _logger.LogDebug("Updating instance endpoint with id {Id}.", model.Id);

            var entity = await _instanceApiStore.FindByIdAsync(model.Id, model.Discriminator);

            if (entity == null)
            {
                _logger.LogError("Instance API with id {Id} not found.", model.Id);
                throw new InvalidOperationException($"Instance endpoint with id {model.Id} not found.");
            }

            if (!string.IsNullOrEmpty(model.Name))
                entity.Name = model.Name;

            if (!string.IsNullOrEmpty(model.ApiId))
                entity.ApiId = model.ApiId;

            if (!string.IsNullOrEmpty(model.Path))
            {
                var path = model.Path.Trim().TrimEnd('/').TrimStart('/');
                entity.Path = path;
            }

            entity.Method = model.Method;

            if (model.IsActive.HasValue)
                entity.IsActive = model.IsActive.Value;

            await _instanceApiStore.UpdateAsync(entity);

            _logger.LogDebug("Instance endpoint with id {Id} updated.", model.Id);

            return entity;
        }

        public async ValueTask<InstanceEndpoint> DeleteAsync(string id, string? discriminator = null)
        {
            _logger.LogDebug("Deleting instance endpoint with id {Id}.", id);

            var entity = await _instanceApiStore.FindByIdAsync(id, discriminator);

            if (entity == null)
            {
                _logger.LogError("Instance endpoint with id {Id} not found.", id);
                throw new InvalidOperationException($"Instance endpoint with id {id} not found.");
            }

            await _instanceApiStore.DeleteAsync(id, discriminator);

            _logger.LogDebug("Instance endpoint with id {Id} deleted.", id);

            return entity;
        }

        public ValueTask<InstanceEndpoint?> FindByIdAsync(string id, string? discriminator = null)
        {
            return _instanceApiStore.FindByIdAsync(id, discriminator);
        }

        public ValueTask<PagedList<InstanceEndpoint>> ListAsync(int page, int pageSize, InstanceEndpointFilters? filters = null)
        {
            return _instanceApiStore.ListAsync(page, pageSize, filters);
        }

        public ValueTask<PagedList<InstanceEndpoint>> ListAllByInstanceIdAsync(string instanceId, string? discriminator = null)
        {
            var filters = new InstanceEndpointFilters { InstanceId = instanceId, Discriminator = discriminator };
            return _instanceApiStore.ListAsync(1, int.MaxValue, filters);
        }

        public ValueTask<InstanceEndpoint?> FindByPath(string apiId, string path, ApiMethod method, string? discriminator = null, CancellationToken cancellationToken = default)
        {
            path = path.Trim().TrimEnd('/').TrimStart('/');
            return _instanceApiStore.FindByPathAsync(apiId, path, method, discriminator, cancellationToken);
        }
    }
}