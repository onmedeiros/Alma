using Alma.Workflows.Core.InstanceSchedules.Entities;
using Alma.Workflows.Core.InstanceSchedules.Models;
using Alma.Workflows.Core.InstanceSchedules.Stores;
using Microsoft.Extensions.Logging;
using Alma.Core.Types;

namespace Alma.Workflows.Core.InstanceSchedules.Services
{
    public interface IInstanceScheduleManager
    {
        ValueTask<InstanceSchedule> Create(string name, string instanceId, string? discriminator = null);
        ValueTask<InstanceSchedule> Update(InstanceScheduleEditModel model);

        ValueTask<InstanceSchedule?> FindById(string id, string? discriminator = null);

        ValueTask<PagedList<InstanceSchedule>> ListAsync(int page, int pageSize, InstanceScheduleFilters? filters = null);

        ValueTask<PagedList<InstanceSchedule>> ListAllByInstanceId(string instanceId, string? discriminator = null);
    }

    public class InstanceScheduleManager : IInstanceScheduleManager
    {
        private readonly ILogger<InstanceScheduleManager> _logger;
        private readonly IInstanceScheduleStore _instanceScheduleStore;
        private readonly IInstanceScheduleJobManager _instanceScheduleJobManager;

        public InstanceScheduleManager(ILogger<InstanceScheduleManager> logger, IInstanceScheduleStore instanceScheduleStore, IInstanceScheduleJobManager instanceScheduleJobManager)
        {
            _logger = logger;
            _instanceScheduleStore = instanceScheduleStore;
            _instanceScheduleJobManager = instanceScheduleJobManager;
        }

        public async ValueTask<InstanceSchedule> Create(string name, string instanceId, string? discriminator = null)
        {
            _logger.LogDebug("Creating instance schedule with name {Name}.", name);

            var now = DateTime.Now;

            var schedule = new InstanceSchedule
            {
                Id = Guid.NewGuid().ToString(),
                Discriminator = discriminator,
                InstanceId = instanceId,
                CreatedAt = now,
                UpdatedAt = now,
                Name = name,
                IsActive = false,
            };

            await _instanceScheduleStore.InsertAsync(schedule);

            _logger.LogDebug("Instance schedule with name {Name} created with id {Id}.", name, schedule.Id);

            return schedule;
        }

        public async ValueTask<InstanceSchedule> Update(InstanceScheduleEditModel model)
        {
            _logger.LogDebug("Updating instance schedule with id {Id}.", model.Id);

            var entity = await _instanceScheduleStore.FindByIdAsync(model.Id, model.Discriminator);

            if (entity is null)
            {
                _logger.LogError("Instance schedule with id {Id} not found.", model.Id);
                throw new Exception($"Instance schedule with id {model.Id} not found.");
            }

            if (!string.IsNullOrEmpty(model.Name) && model.Name != entity.Name)
                entity.Name = model.Name;

            if (model.IsActive.HasValue && model.IsActive != entity.IsActive)
                entity.IsActive = model.IsActive.Value;

            if (!string.IsNullOrEmpty(model.CronExpression) && model.CronExpression != entity.CronExpression)
                entity.CronExpression = model.CronExpression;

            await _instanceScheduleStore.UpdateAsync(entity);

            _logger.LogDebug("Instance schedule with id {Id} updated. Updating Jobs.", model.Id);

            if (entity.IsActive)
            {
                await _instanceScheduleJobManager.AddOrUpdateRecurring(entity);
            }
            else
            {
                await _instanceScheduleJobManager.RemoveIfExistsRecurring(entity);
            }

            return entity;
        }

        public ValueTask<InstanceSchedule?> FindById(string id, string? discriminator = null)
        {
            return _instanceScheduleStore.FindByIdAsync(id, discriminator);
        }

        public ValueTask<PagedList<InstanceSchedule>> ListAsync(int page, int pageSize, InstanceScheduleFilters? filters = null)
        {
            return _instanceScheduleStore.ListAsync(page, pageSize, filters);
        }

        public ValueTask<PagedList<InstanceSchedule>> ListAllByInstanceId(string instanceId, string? discriminator = null)
        {
            return _instanceScheduleStore.ListAsync(1, int.MaxValue, new InstanceScheduleFilters { InstanceId = instanceId, Discriminator = discriminator });
        }
    }
}
