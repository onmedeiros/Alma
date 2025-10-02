using Alma.Flows.Core.Activities.Base;
using Alma.Flows.Core.Activities.Enums;
using Alma.Flows.Core.Contexts;
using Alma.Flows.Runners;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Alma.Flows.Core.Activities.Steps
{
    public class SetParametersStep : Step
    {
        private readonly ILogger<SetParametersStep> _logger;

        public SetParametersStep(ILogger<SetParametersStep> logger)
        {
            _logger = logger;
        }

        public override ValueTask<ActivityStepStatus> ExecuteAsync(ActivityExecutionContext context)
        {
            _logger.LogDebug("Executing SetParametersStep for activity {ActivityId}", Activity.Id);

            var parameterSetter = context.ServiceProvider.GetRequiredService<IParameterSetter>();

            return new ValueTask<ActivityStepStatus>(ActivityStepStatus.Completed);
        }
    }
}