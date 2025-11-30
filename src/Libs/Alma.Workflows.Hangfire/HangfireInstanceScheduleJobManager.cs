using Alma.Workflows.Core.InstanceExecutions.Services;
using Alma.Workflows.Core.Instances.Entities;
using Alma.Workflows.Core.InstanceSchedules.Entities;
using Alma.Workflows.Core.InstanceSchedules.Services;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace Alma.Workflows.Hangfire
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

            _recurringJobManager.AddOrUpdate<IInstanceScheduleRunner>($"schedule-{schedule.Id}", "Alma-Workflows-recurring", runner => runner.Run(schedule.Id, schedule.Discriminator), schedule.CronExpression);

            return ValueTask.CompletedTask;
        }

        public ValueTask RemoveIfExistsRecurring(InstanceSchedule schedule)
        {
            _logger.LogDebug("Removing instance schedule job with id {Id}.", schedule.Id);

            _recurringJobManager.RemoveIfExists($"schedule-{schedule.Id}");
            return ValueTask.CompletedTask;
        }

        public ValueTask Schedule(Instance instance, TimeSpan delay = default)
        {
            _logger.LogDebug("Scheduling instance {InstanceId} to run in {Delay}.", instance.Id, delay);

            _backgroundJobClient.Schedule<IInstanceRunner>(x => x.ExecuteAsync(instance.Id, instance.Discriminator, null), delay);
            return ValueTask.CompletedTask;
        }
    }
}