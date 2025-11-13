using Alma.Workflows.Core.Contexts;
using Alma.Workflows.Core.InstanceExecutions.Enums;
using Microsoft.Extensions.Logging;

namespace Alma.Workflows.Runners.ExecutionModes
{
    /// <summary>
    /// Estratégia para modo de execução Manual.
    /// Para após cada atividade que requer interação do usuário.
    /// Atividades automáticas são executadas normalmente.
    /// </summary>
    public class ManualExecutionModeStrategy : IExecutionModeStrategy
    {
        private readonly ILogger<ManualExecutionModeStrategy> _logger;

        public ManualExecutionModeStrategy(ILogger<ManualExecutionModeStrategy> logger)
        {
            _logger = logger;
        }

        public bool ShouldContinueAfterBatch(
            FlowExecutionContext context,
            IEnumerable<FlowExecution> pendingExecutions)
        {
            // Continua se não há atividades com interação pendentes
            var hasInteractiveReady = pendingExecutions.Any(x => 
                x.QueueItem.CanExecute && x.RequireInteraction);

            if (hasInteractiveReady)
            {
                _logger.LogDebug("Manual mode: Pausing execution due to interactive activity");
                return false;
            }

            // Continua se há atividades automáticas pendentes
            return pendingExecutions.Any(x => 
                x.QueueItem.CanExecute && !x.RequireInteraction);
        }

        public int GetBatchSize(
            FlowExecutionContext context,
            IEnumerable<FlowExecution> readyExecutions)
        {
            // No modo manual, executa uma atividade com interação por vez
            return 1;
        }

        public bool RequiresUserInteraction(FlowExecution execution)
        {
            return execution.RequireInteraction;
        }
    }
}
