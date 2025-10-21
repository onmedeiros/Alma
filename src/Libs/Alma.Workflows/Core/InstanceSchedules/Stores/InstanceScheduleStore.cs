using Alma.Workflows.Core.InstanceSchedules.Entities;
using Alma.Core.Types;

namespace Alma.Workflows.Core.InstanceSchedules.Stores
{
    public interface IInstanceScheduleStore
    {
        ValueTask<InstanceSchedule> InsertAsync(InstanceSchedule instance, CancellationToken cancellationToken = default);
        ValueTask<InstanceSchedule> UpdateAsync(InstanceSchedule instance, CancellationToken cancellationToken = default);
        ValueTask<InstanceSchedule?> DeleteAsync(string id, string? discriminator = null, CancellationToken cancellationToken = default);
        ValueTask<InstanceSchedule?> FindByIdAsync(string id, string? discriminator = null, CancellationToken cancellationToken = default);
        ValueTask<PagedList<InstanceSchedule>> ListAsync(int page, int pageSize, InstanceScheduleFilters? filters = null, CancellationToken cancellationToken = default);
    }

    public class InstanceScheduleStore : IInstanceScheduleStore
    {
        public ValueTask<InstanceSchedule> InsertAsync(InstanceSchedule instance, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<InstanceSchedule?> DeleteAsync(string id, string? discriminator = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<InstanceSchedule?> FindByIdAsync(string id, string? discriminator = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<InstanceSchedule> UpdateAsync(InstanceSchedule instance, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<PagedList<InstanceSchedule>> ListAsync(int page, int pageSize, InstanceScheduleFilters? filters = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
