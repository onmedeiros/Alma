using Alma.Workflows.Core.Activities.Base;
using Alma.Workflows.Core.Activities.Enums;
using Alma.Workflows.Core.Contexts;
using Microsoft.Extensions.Logging;

namespace Alma.Workflows.Activities.Flow
{
    /// <summary>
    /// Step customizado para o LoopActivity que ignora a porta BodyComplete
    /// ao verificar se as conexões de entrada foram executadas.
    /// A porta BodyComplete é tratada de forma especial pelo FlowRunner.
    /// </summary>
    public class LoopWaitConnectionsStep : Core.Activities.Abstractions.IStep
    {
        private readonly ILogger<LoopWaitConnectionsStep> _logger;
        private string _id = string.Empty;
        private Core.Abstractions.IActivity? _activity;

        public LoopWaitConnectionsStep(ILogger<LoopWaitConnectionsStep> logger)
        {
            _logger = logger;
        }

        public string Id => _id;

        public Core.Abstractions.IActivity Activity => _activity ?? throw new InvalidOperationException("Activity not set");

        public void SetId(string id)
        {
            _id = id;
        }

        public void SetActivity(Core.Abstractions.IActivity activity)
        {
            _activity = activity;
        }

        public ValueTask<ActivityStepStatus> GetStatus(ActivityExecutionContext context)
        {
            return ExecuteAsync(context);
        }

        public ValueTask<ActivityStepStatus> ExecuteAsync(ActivityExecutionContext context)
        {
            _logger.LogDebug("Executing LoopWaitConnectionsStep for activity {ActivityId}", Activity.Id);

            var inputPorts = Activity.GetPorts()
                .Where(port => port.Type == PortType.Input)
                .ToList();

            // Para o LoopActivity, ignoramos a porta BodyComplete
            // pois ela é tratada de forma especial pelo FlowRunner
            var loopActivity = Activity as LoopActivity;
            if (loopActivity != null)
            {
                inputPorts = inputPorts
                    .Where(port => port.Descriptor.Name != nameof(LoopActivity.BodyComplete))
                    .ToList();
            }

            // Verifica execução de todas as portas conectadas (exceto BodyComplete)
            foreach (var port in inputPorts)
            {
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

            // Se todas as portas conectadas (exceto BodyComplete) foram executadas, marca este step como completado
            _logger.LogDebug("All required connected ports executed for loop activity {ActivityId}", Activity.Id);
            return new ValueTask<ActivityStepStatus>(ActivityStepStatus.Completed);
        }
    }
}