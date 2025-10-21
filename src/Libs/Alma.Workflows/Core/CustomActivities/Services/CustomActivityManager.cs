using Alma.Workflows.Core.CustomActivities.Entities;
using Alma.Workflows.Core.CustomActivities.Models;
using Alma.Workflows.Core.CustomActivities.Stores;
using Microsoft.Extensions.Logging;
using Alma.Core.Types;

namespace Alma.Workflows.Core.CustomActivities.Services
{
    public interface ICustomActivityManager
    {
        ValueTask<CustomActivityTemplate> Create(string name, string? discriminator = null);

        ValueTask<CustomActivityTemplate> Update(CustomActivityEditModel model);

        ValueTask<CustomActivityTemplate?> AddParameter(CustomActivityParameterEditModel model);

        ValueTask<CustomActivityTemplate?> UpdateParameter(CustomActivityParameterEditModel model);

        ValueTask<CustomActivityTemplate?> RemoveParameter(string activityId, string? discriminator, string? id);

        ValueTask<CustomActivityTemplate?> AddPort(CustomActivityPortEditModel model);

        ValueTask<CustomActivityTemplate?> UpdatePort(CustomActivityPortEditModel model);

        ValueTask<CustomActivityTemplate?> RemovePort(string activityId, string? discriminator, string? id);

        ValueTask<CustomActivityScript> FindScriptAsync(string activityId, string? discriminator);

        ValueTask<CustomActivityScript> UpdateScript(string activityId, string? discriminator, string? script);

        ValueTask<CustomActivityTemplate?> FindById(string id, string? discriminator = null);

        ValueTask<PagedList<CustomActivityTemplate>> List(int page, int pageSize, CustomActivityFilters? filters = null);
    }

    public class CustomActivityManager : ICustomActivityManager
    {
        private readonly ILogger<CustomActivityManager> _logger;
        private readonly ICustomActivityTemplateStore _customActivityStore;

        public CustomActivityManager(ILogger<CustomActivityManager> logger, ICustomActivityTemplateStore customActivityStore)
        {
            _logger = logger;
            _customActivityStore = customActivityStore;
        }

        public async ValueTask<CustomActivityTemplate> Create(string name, string? discriminator = null)
        {
            _logger.LogDebug("Creating custom activity with name {Name}.", name);

            var now = DateTime.Now;

            var activity = new CustomActivityTemplate
            {
                Id = Guid.NewGuid().ToString(),
                Discriminator = discriminator,
                CreatedAt = now,
                UpdatedAt = now,
                Name = name
            };

            await _customActivityStore.InsertAsync(activity);

            _logger.LogDebug("Custom activity {Name} created with ID {Id}.", name, activity.Id);

            return activity;
        }

        public async ValueTask<CustomActivityTemplate> Update(CustomActivityEditModel model)
        {
            _logger.LogDebug("Updating custom activity with ID {Id}.", model.Id);

            var entity = await FindById(model.Id, model.Discriminator);

            if (entity == null)
            {
                _logger.LogWarning("Custom activity with ID {Id} not found.", model.Id);
                throw new InvalidOperationException($"Custom activity with ID {model.Id} not found.");
            }

            entity.UpdatedAt = DateTime.Now;

            if (!string.IsNullOrEmpty(model.Name) && entity.Name != model.Name)
                entity.Name = model.Name;

            if (!string.IsNullOrEmpty(model.Description) && entity.Description != model.Description)
                entity.Description = model.Description;

            if (!string.IsNullOrEmpty(model.CategoryId) && entity.CategoryId != model.CategoryId)
                entity.CategoryId = model.CategoryId;

            await _customActivityStore.UpdateAsync(entity);

            _logger.LogDebug("Custom activity {Name} updated with ID {Id}.", model.Name, entity.Id);

            return entity;
        }

        public async ValueTask<CustomActivityTemplate?> AddParameter(CustomActivityParameterEditModel model)
        {
            _logger.LogDebug("Adding parameter for custom activity with ID {CustomActivityId}.", model.CustomActivityId);

            var activity = await FindById(model.CustomActivityId, model.CustomActivityDiscriminator);

            if (activity == null)
            {
                _logger.LogWarning("Custom activity with ID {CustomActivityId} not found.", model.CustomActivityId);
                throw new InvalidOperationException($"Custom activity with ID {model.CustomActivityId} not found.");
            }

            // Validate parameter info
            if (string.IsNullOrEmpty(model.Name))
            {
                _logger.LogWarning("Parameter name is required.");
                throw new ArgumentException("Parameter name is required.", nameof(model.Name));
            }

            if (activity.Parameters.Any(p => p.Name == model.Name))
            {
                _logger.LogWarning("Parameter with name {Name} already exists.", model.Name);
                throw new InvalidOperationException($"Parameter with name {model.Name} already exists.");
            }

            if (string.IsNullOrEmpty(model.DisplayName))
                model.DisplayName = model.Name;

            var customActivityParameter = new CustomActivityParameterTemplate
            {
                Id = Guid.NewGuid().ToString(),
                Name = model.Name,
                DisplayName = model.DisplayName,
                Description = model.Description,
                Type = model.Type,
            };

            activity.Parameters.Add(customActivityParameter);
            await _customActivityStore.UpdateAsync(activity);

            _logger.LogDebug("Parameter {Name} added to custom activity with ID {CustomActivityId}.", model.Name, model.CustomActivityId);
            return activity;
        }

        public async ValueTask<CustomActivityTemplate?> UpdateParameter(CustomActivityParameterEditModel model)
        {
            _logger.LogDebug("Updating parameter for custom activity with ID {CustomActivityId}.", model.CustomActivityId);

            var activity = await FindById(model.CustomActivityId, model.CustomActivityDiscriminator);

            if (activity == null)
            {
                _logger.LogWarning("Custom activity with ID {CustomActivityId} not found.", model.CustomActivityId);
                throw new InvalidOperationException($"Custom activity with ID {model.CustomActivityId} not found.");
            }

            var parameter = activity.Parameters.FirstOrDefault(p => p.Id == model.Id);

            if (parameter == null)
            {
                _logger.LogWarning("Parameter with ID {Id} not found in custom activity with ID {CustomActivityId}.", model.Id, model.CustomActivityId);
                throw new InvalidOperationException($"Parameter with ID {model.Id} not found in custom activity with ID {model.CustomActivityId}.");
            }

            if (!string.IsNullOrEmpty(model.Name))
            {
                if (activity.Parameters.Any(p => p.Name == model.Name && p.Id != model.Id))
                {
                    _logger.LogWarning("Parameter with name {Name} already exists.", model.Name);
                    throw new InvalidOperationException($"Parameter with name {model.Name} already exists.");
                }

                parameter.Name = model.Name;
            }

            if (!string.IsNullOrEmpty(model.DisplayName))
                parameter.DisplayName = model.DisplayName;

            if (!string.IsNullOrEmpty(model.Description))
                parameter.Description = model.Description;

            if (model.Type != parameter.Type)
                parameter.Type = model.Type;

            await _customActivityStore.UpdateAsync(activity);

            _logger.LogDebug("Parameter {Name} updated for custom activity with ID {CustomActivityId}.", model.Name, model.CustomActivityId);

            return activity;
        }

        public async ValueTask<CustomActivityTemplate?> RemoveParameter(string activityId, string? discriminator, string? id)
        {
            _logger.LogDebug("Removing parameter with ID {Id} from custom activity with ID {ActivityId}.", id, activityId);
            var activity = await FindById(activityId, discriminator);

            if (activity == null)
            {
                _logger.LogWarning("Custom activity with ID {ActivityId} not found.", activityId);
                throw new InvalidOperationException($"Custom activity with ID {activityId} not found.");
            }

            var parameter = activity.Parameters.FirstOrDefault(p => p.Id == id);

            if (parameter == null)
            {
                _logger.LogWarning("Parameter with ID {Id} not found in custom activity with ID {ActivityId}.", id, activityId);
                throw new InvalidOperationException($"Parameter with ID {id} not found in custom activity with ID {activityId}.");
            }

            activity.Parameters.Remove(parameter);

            await _customActivityStore.UpdateAsync(activity);

            _logger.LogDebug("Parameter with ID {Id} removed from custom activity with ID {ActivityId}.", id, activityId);

            return activity;
        }

        public async ValueTask<CustomActivityTemplate?> AddPort(CustomActivityPortEditModel model)
        {
            _logger.LogDebug("Adding port for custom activity with ID {CustomActivityId}.", model.CustomActivityId);

            var activity = await FindById(model.CustomActivityId, model.CustomActivityDiscriminator);

            if (activity == null)
            {
                _logger.LogWarning("Custom activity with ID {CustomActivityId} not found.", model.CustomActivityId);
                throw new InvalidOperationException($"Custom activity with ID {model.CustomActivityId} not found.");
            }

            // Validate port info
            if (string.IsNullOrEmpty(model.Name))
            {
                _logger.LogWarning("Port name is required.");
                throw new ArgumentException("Port name is required.", nameof(model.Name));
            }

            if (activity.Ports.Any(p => p.Name == model.Name))
            {
                _logger.LogWarning("Port with name {Name} already exists.", model.Name);
                throw new InvalidOperationException($"Port with name {model.Name} already exists.");
            }

            if (string.IsNullOrEmpty(model.DisplayName))
                model.DisplayName = model.Name;

            var customActivityPort = new CustomActivityPort
            {
                Id = Guid.NewGuid().ToString(),
                Name = model.Name,
                DisplayName = model.DisplayName,
                Type = model.Type,
            };

            activity.Ports.Add(customActivityPort);
            await _customActivityStore.UpdateAsync(activity);

            _logger.LogDebug("Port {Name} added to custom activity with ID {CustomActivityId}.", model.Name, model.CustomActivityId);
            return activity;
        }

        public async ValueTask<CustomActivityTemplate?> UpdatePort(CustomActivityPortEditModel model)
        {
            _logger.LogDebug("Updating port for custom activity with ID {CustomActivityId}.", model.CustomActivityId);

            var activity = await FindById(model.CustomActivityId, model.CustomActivityDiscriminator);

            if (activity == null)
            {
                _logger.LogWarning("Custom activity with ID {CustomActivityId} not found.", model.CustomActivityId);
                throw new InvalidOperationException($"Custom activity with ID {model.CustomActivityId} not found.");
            }

            var parameter = activity.Ports.FirstOrDefault(p => p.Id == model.Id);

            if (parameter == null)
            {
                _logger.LogWarning("Port with ID {Id} not found in custom activity with ID {CustomActivityId}.", model.Id, model.CustomActivityId);
                throw new InvalidOperationException($"Port with ID {model.Id} not found in custom activity with ID {model.CustomActivityId}.");
            }

            if (!string.IsNullOrEmpty(model.Name))
            {
                if (activity.Ports.Any(p => p.Name == model.Name && p.Id != model.Id))
                {
                    _logger.LogWarning("Port with name {Name} already exists.", model.Name);
                    throw new InvalidOperationException($"Port with name {model.Name} already exists.");
                }

                parameter.Name = model.Name;
            }

            if (!string.IsNullOrEmpty(model.DisplayName))
                parameter.DisplayName = model.DisplayName;

            if (model.Type != parameter.Type)
                parameter.Type = model.Type;

            await _customActivityStore.UpdateAsync(activity);

            _logger.LogDebug("Port {Name} updated for custom activity with ID {CustomActivityId}.", model.Name, model.CustomActivityId);

            return activity;
        }

        public async ValueTask<CustomActivityTemplate?> RemovePort(string activityId, string? discriminator, string? id)
        {
            _logger.LogDebug("Removing port with ID {Id} from custom activity with ID {ActivityId}.", id, activityId);
            var activity = await FindById(activityId, discriminator);

            if (activity == null)
            {
                _logger.LogWarning("Custom activity with ID {ActivityId} not found.", activityId);
                throw new InvalidOperationException($"Custom activity with ID {activityId} not found.");
            }

            var port = activity.Ports.FirstOrDefault(p => p.Id == id);

            if (port == null)
            {
                _logger.LogWarning("Port with ID {Id} not found in custom activity with ID {ActivityId}.", id, activityId);
                throw new InvalidOperationException($"Port with ID {id} not found in custom activity with ID {activityId}.");
            }

            activity.Ports.Remove(port);

            await _customActivityStore.UpdateAsync(activity);

            _logger.LogDebug("Port with ID {Id} removed from custom activity with ID {ActivityId}.", id, activityId);

            return activity;
        }

        public async ValueTask<CustomActivityScript> FindScriptAsync(string activityId, string? discriminator)
        {
            _logger.LogDebug("Finding script for custom activity with ID {ActivityId}.", activityId);

            var entity = await _customActivityStore.FindScriptAsync(activityId, discriminator);

            if (entity is null)
            {
                entity = new CustomActivityScript
                {
                    Id = Guid.NewGuid().ToString(),
                    CustomActivityTemplateId = activityId,
                    Discriminator = discriminator,
                    Content = string.Empty
                };

                await _customActivityStore.InsertScriptAsync(entity);
            }

            return entity;
        }

        public async ValueTask<CustomActivityScript> UpdateScript(string activityId, string? discriminator, string? script)
        {
            var entity = await FindScriptAsync(activityId, discriminator);

            _logger.LogDebug("Updating script for custom activity with ID {ActivityId}.", activityId);

            entity.Content = script ?? string.Empty;
            return await _customActivityStore.UpdateScriptAsync(entity);
        }

        public ValueTask<CustomActivityTemplate?> FindById(string id, string? discriminator = null)
        {
            _logger.LogDebug("Finding custom activity with ID {Id}.", id);
            return _customActivityStore.FindByIdAsync(id, discriminator);
        }

        public ValueTask<PagedList<CustomActivityTemplate>> List(int page, int pageSize, CustomActivityFilters? filters = null)
        {
            _logger.LogDebug("Listing custom activities with page {Page} and page size {PageSize}.", page, pageSize);
            return _customActivityStore.ListAsync(page, pageSize, filters);
        }
    }
}