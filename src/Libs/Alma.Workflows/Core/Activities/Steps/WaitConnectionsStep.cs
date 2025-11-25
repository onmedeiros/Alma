using Alma.Workflows.Core.Activities.Base;
using Alma.Workflows.Core.Activities.Enums;
using Alma.Workflows.Core.Contexts;
using Microsoft.Extensions.Logging;

namespace Alma.Workflows.Core.Activities.Steps
{
    public class WaitConnectionsStep : Step
    {
        private readonly ILogger<WaitConnectionsStep> _logger;

        public WaitConnectionsStep(ILogger<WaitConnectionsStep> logger)
        {
            _logger = logger;
        }

        public override ValueTask<ActivityStepStatus> ExecuteAsync(ActivityExecutionContext context)
        {
            _logger.LogDebug("Executing WaitConnectionsStep for activity {ActivityId}", Activity.Id);

            var inputPorts = Activity.GetPorts()
                .Where(port => port.Type == PortType.Input)
                .ToList();

            // Check execution of all connected ports
            foreach (var port in inputPorts)
            {
                //foreach (var connectedPort in port.ConnectedPorts)
                //{
                //    if (!context.State.ExecutedConnections.Any(x => x.SourceId == connectedPort.Activity.Id && x.SourcePortName == connectedPort.Descriptor.Name))
                //    {
                //        _logger.LogDebug("Port {PortName} of activity {ActivityId} is not executed yet.", connectedPort.Descriptor.Name, connectedPort.Activity.Id);
                //        return new ValueTask<ActivityStepStatus>(ActivityStepStatus.Waiting);
                //    }
                //}

                var portsGroupedByActivity = port.ConnectedPorts.GroupBy(x => x.Activity.Id);

                foreach (var group in portsGroupedByActivity)
                {
                    var activityId = group.Key;

                    var isAnyPortExecuted = group.Any(connectedPort =>
                        context.State.Connections.AsCollection().Any(x =>
                            x.SourceId == activityId && x.SourcePortName == connectedPort.Descriptor.Name));

                    if (!isAnyPortExecuted)
                    {
                        _logger.LogDebug("No connected ports from activity {ActivityId} are executed yet.", activityId);
                        return new ValueTask<ActivityStepStatus>(ActivityStepStatus.Waiting);
                    }
                }
            }

            // If all connected ports have been executed, mark this step as completed
            _logger.LogDebug("All connected ports executed for activity {ActivityId}", Activity);
            return new ValueTask<ActivityStepStatus>(ActivityStepStatus.Completed);
        }
    }
}