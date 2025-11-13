using Alma.Workflows.Core.Contexts;
using Microsoft.Extensions.Logging;

namespace Alma.Workflows.Runners.ExecutionModes
{
    /// <summary>
    /// Estratégia para modo de execução StepByStep.
    /// Para após cada atividade, independente de requerer interação ou não.
    /// Útil para debug e acompanhamento detalhado da execução.
    /// </summary>
    public class StepByStepExecutionModeStrategy : IExecutionModeStrategy
    {
        private readonly ILogger<StepByStepExecutionModeStrategy> _logger;

        public StepByStepExecutionModeStrategy(ILogger<StepByStepExecutionModeStrategy> logger)
        {
            _logger = logger;
        }

        public bool ShouldContinueAfterBatch(
            FlowExecutionContext context,
            IEnumerable<FlowExecution> pendingExecutions)
        {
            // No modo step-by-step, sempre para após cada lote
            _logger.LogDebug("StepByStep mode: Pausing after batch execution");
            return false;
        }

        public int GetBatchSize(
            FlowExecutionContext context,
            IEnumerable<FlowExecution> readyExecutions)
        {
            // Executa apenas uma atividade por vez
            return 1;
        }

        public bool RequiresUserInteraction(FlowExecution execution)
        {
            // No modo step-by-step, todas as atividades "requerem interação"
            // para pausar a execução
            return true;
        }
    }
}
