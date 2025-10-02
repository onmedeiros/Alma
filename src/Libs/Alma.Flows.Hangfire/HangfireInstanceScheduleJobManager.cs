using Alma.Flows.Core.InstanceExecutions.Services;
using Alma.Flows.Core.Instances.Entities;
using Alma.Flows.Core.InstanceSchedules.Entities;
using Alma.Flows.Core.InstanceSchedules.Services;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace Alma.Flows.Hangfire
{
    internal class HangfireInstanceScheduleJobManager : IInstanceScheduleJobManager
    {
        private readonly ILogger<HangfireInstanceScheduleJobManager> _logger;
        private readonly IRecurringJobManager _recurringJobManager;
        private readonly IBackgroundJobClient _backgroundJobClient;

        public HangfireInstanceScheduleJobManager(ILogger<HangfireInstanceScheduleJobManager> logger, IRecurringJobManager recurringJobManager, IBackgroundJobClient backgroundJobClient)
        {
            _logger = logger;
            _recurringJobManager = recurringJobManager;
            _backgroundJobClient = backgroundJobClient;
        }

        public ValueTask AddOrUpdateRecurring(InstanceSchedule schedule)
        {
            _logger.LogDebug("Adding or updating instance schedule job with id {Id}.", schedule.Id);

            _recurringJobManager.AddOrUpdate<IInstanceScheduleRunner>($"schedule-{schedule.Id}", "Alma-flows-recurring", runner => runner.Run(schedule.Id, schedule.Discriminator), schedule.CronExpression);

            return ValueTask.CompletedTask;
        }

        public ValueTask RemoveIfExistsRecurring(InstanceSchedule schedule)
        {
            _logger.LogDebug("Removing instance schedule job with id {Id}.", schedule.Id);

            _recurringJobManager.RemoveIfExists($"schedule-{schedule.Id}");
            return ValueTask.CompletedTask;
        }

        public ValueTask Schedule(FlowInstance instance, TimeSpan delay = default)
        {
            _logger.LogDebug("Scheduling instance {InstanceId} to run in {Delay}.", instance.Id, delay);

            _backgroundJobClient.Schedule<IInstanceExecutionRunner>(x => x.ExecuteAsync(instance.Id, instance.Discriminator, null), delay);
            return ValueTask.CompletedTask;
        }
    }
}