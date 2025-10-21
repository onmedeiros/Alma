using Alma.Workflows.Core.Activities.Base;
using Alma.Workflows.Core.Activities.Enums;
using Alma.Workflows.Core.Contexts;
using Alma.Workflows.Runners;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Alma.Workflows.Core.Activities.Steps
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