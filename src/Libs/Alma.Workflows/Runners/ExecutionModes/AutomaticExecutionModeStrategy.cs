using Alma.Workflows.Core.Contexts;
using Microsoft.Extensions.Logging;

namespace Alma.Workflows.Runners.ExecutionModes
{
    /// <summary>
    /// Estratégia para modo de execução Automático.
    /// Executa todas as atividades automaticamente até completar ou encontrar
    /// uma atividade que realmente requer interação do usuário.
    /// </summary>
    public class AutomaticExecutionModeStrategy : IExecutionModeStrategy
    {
        private readonly ILogger<AutomaticExecutionModeStrategy> _logger;

        public AutomaticExecutionModeStrategy(ILogger<AutomaticExecutionModeStrategy> logger)
        {
            _logger = logger;
        }

        public bool ShouldContinueAfterBatch(
            WorkflowExecutionContext context,
            IEnumerable<ExecutionBatchItem> pendingExecutions)
        {
            // Continua executando enquanto houver atividades prontas
            var hasReady = pendingExecutions.Any(x => x.QueueItem.CanExecute);
            
            if (!hasReady)
            {
                _logger.LogDebug("Automatic mode: No more activities ready to execute");
            }

            return hasReady;
        }

        public int GetBatchSize(
            WorkflowExecutionContext context,
            IEnumerable<ExecutionBatchItem> readyExecutions)
        {
            // Respeita o grau máximo de paralelismo configurado
            return context.Options.MaxDegreeOfParallelism;
        }

        public bool RequiresUserInteraction(ExecutionBatchItem execution)
        {
            // Apenas atividades que realmente requerem interação
            return execution.Activity.Descriptor.RequireInteraction;
        }
    }
}
