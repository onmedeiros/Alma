using Alma.Flows.Core.Instances.Entities;
using Alma.Flows.Core.InstanceSchedules.Entities;
using Microsoft.Extensions.Logging;

namespace Alma.Flows.Core.InstanceSchedules.Services
{
    public interface IInstanceScheduleJobManager
    {
        ValueTask AddOrUpdateRecurring(InstanceSchedule schedule);

        ValueTask RemoveIfExistsRecurring(InstanceSchedule schedule);

        ValueTask Schedule(FlowInstance instance, TimeSpan delay);
    }

    public class InstanceScheduleJobManager : IInstanceScheduleJobManager
    {
        private readonly ILogger<InstanceScheduleJobManager> _logger;

        public InstanceScheduleJobManager(ILogger<InstanceScheduleJobManager> logger)
        {
            _logger = logger;
        }

        public ValueTask AddOrUpdateRecurring(InstanceSchedule schedule)
        {
            throw new NotImplementedException();
        }

        public ValueTask RemoveIfExistsRecurring(InstanceSchedule schedule)
        {
            throw new NotImplementedException();
        }

        public ValueTask Schedule(FlowInstance instance, TimeSpan delay)
        {
            throw new NotImplementedException();
        }
    }
}