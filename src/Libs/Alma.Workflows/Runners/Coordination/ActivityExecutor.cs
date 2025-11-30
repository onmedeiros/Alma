using Alma.Workflows.Core.Activities.Enums;
using Alma.Workflows.Core.Contexts;
using Alma.Workflows.Enums;
using Alma.Workflows.Runners.Connections;
using Alma.Workflows.Runners.Strategies;
using Microsoft.Extensions.Logging;

namespace Alma.Workflows.Runners.Coordination
{
    /// <summary>
    /// Executa atividades individuais delegando para strategies específicas
    /// e gerenciando o pós-processamento.
    /// </summary>
    public class ActivityExecutor : IActivityExecutor
    {
        private readonly ILogger<ActivityExecutor> _logger;
        private readonly IActivityExecutionStrategyResolver _strategyResolver;
        private readonly IConnectionManager _connectionManager;

        public ActivityExecutor(
            ILogger<ActivityExecutor> logger,
            IActivityExecutionStrategyResolver _strategyResolver,
            IConnectionManager connectionManager)
        {
            _logger = logger;
            this._strategyResolver = _strategyResolver;
            _connectionManager = connectionManager;
        }

        public async Task ExecuteAsync(ExecutionBatchItem execution, WorkflowExecutionContext context)
        {
            if (!execution.QueueItem.CanExecute)
            {
                _logger.LogDebug(
                    "Activity {ActivityId} cannot be executed (status: {Status})",
                    execution.QueueItem.ActivityId,
                    execution.QueueItem.ExecutionStatus);
                return;
            }

            var activity = execution.QueueItem.Activity;

            // Resolve a estratégia apropriada para este tipo de atividade
            var strategy = _strategyResolver.Resolve(activity);

            _logger.LogDebug(
                "Executing activity {ActivityId} ({ActivityType}) using strategy {StrategyType}",
                activity.Id, activity.GetType().Name, strategy.GetType().Name);

            // Executa a atividade usando a estratégia resolvida
            var executionResult = await strategy.ExecuteAsync(activity, context, execution.Runner);

            // Trata a lógica pós-execução usando a estratégia
            await strategy.HandlePostExecutionAsync(
                activity, context, executionResult, execution.QueueItem);

            // Enfileira conexões se a atividade completou com sucesso
            if (executionResult.ExecutionStatus == ActivityExecutionStatus.Completed && 
                executionResult.ExecutedPorts.Any())
            {
                _connectionManager.EnqueueConnectedActivities(context, executionResult.ExecutedPorts);
            }
        }
    }
}
