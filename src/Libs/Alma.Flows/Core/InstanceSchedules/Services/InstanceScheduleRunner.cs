using Microsoft.Extensions.Logging;

namespace Alma.Flows.Core.InstanceSchedules.Services
{
    public interface IInstanceScheduleRunner
    {
        Task Run(string scheduleId, string? discriminator = null);
    }

    public class InstanceScheduleRunner : IInstanceScheduleRunner
    {
        private readonly ILogger<InstanceScheduleRunner> _logger;
        private readonly IInstanceScheduleManager _instanceScheduleManager;
        private readonly IFlowRunManager _flowRunner;

        public InstanceScheduleRunner(ILogger<InstanceScheduleRunner> logger, IInstanceScheduleManager instanceScheduleManager, IFlowRunManager flowRunner)
        {
            _logger = logger;
            _instanceScheduleManager = instanceScheduleManager;
            _flowRunner = flowRunner;
        }

        public async Task Run(string scheduleId, string? discriminator = null)
        {
            var schedule = await _instanceScheduleManager.FindById(scheduleId, discriminator);

            try
            {
                if (schedule is null)
                    throw new Exception("Instance schedule not found.");

                if (!schedule.IsActive)
                    throw new Exception("Instance schedule is not active.");

                await _flowRunner.RunAsync(schedule.InstanceId, schedule.Discriminator);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to run instance schedule.");
                if (schedule is null)
                {
                    return;
                }
                else
                {
                    _logger.LogWarning("Deactivating instance schedule due to running error.");

                    await _instanceScheduleManager.Update(new Models.InstanceScheduleEditModel
                    {
                        Id = schedule.Id,
                        IsActive = false
                    });
                }
            }
        }
    }
}
