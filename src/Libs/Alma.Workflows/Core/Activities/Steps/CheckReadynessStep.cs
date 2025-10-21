using Alma.Workflows.Core.Activities.Base;
using Alma.Workflows.Core.Activities.Enums;
using Alma.Workflows.Core.Contexts;
using Microsoft.Extensions.Logging;

namespace Alma.Workflows.Core.Activities.Steps
{
    public class CheckReadynessStep : Step
    {
        private readonly ILogger<CheckReadynessStep> _logger;

        public CheckReadynessStep(ILogger<CheckReadynessStep> logger)
        {
            _logger = logger;
        }

        public override async ValueTask<ActivityStepStatus> ExecuteAsync(ActivityExecutionContext context)
        {
            _logger.LogDebug("Executing CheckReadynessStep for activity {ActivityId}", Activity.Id);

            var isReadyResult = await Activity.IsReadyToExecuteAsync(context);

            if (!isReadyResult.IsReady)
            {
                _logger.LogDebug("Activity {ActivityId} is not ready to execute. Reason: {Reason}", Activity.Id, isReadyResult.Reason);
                return ActivityStepStatus.Waiting;
            }

            _logger.LogDebug("Activity {ActivityId} is ready to execute.", Activity.Id);
            return ActivityStepStatus.Completed;
        }
    }
}