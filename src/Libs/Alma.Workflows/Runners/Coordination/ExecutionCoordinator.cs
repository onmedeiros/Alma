using Alma.Workflows.Core.Contexts;
using Alma.Workflows.Runners.ExecutionModes;
using Microsoft.Extensions.Logging;

namespace Alma.Workflows.Runners.Coordination
{
    /// <summary>
    /// Coordena a execução de atividades respeitando paralelização,
    /// priorização e modos de execução.
    /// Implementa a responsabilidade única de coordenar quando e quantas
    /// atividades executar simultaneamente.
    /// </summary>
    public class ExecutionCoordinator : IExecutionCoordinator
    {
        private readonly ILogger<ExecutionCoordinator> _logger;
        private readonly IExecutionModeStrategyFactory _modeStrategyFactory;
        private readonly IActivityExecutor _activityExecutor;

        public ExecutionCoordinator(
            ILogger<ExecutionCoordinator> logger,
            IExecutionModeStrategyFactory modeStrategyFactory,
            IActivityExecutor activityExecutor)
        {
            _logger = logger;
            _modeStrategyFactory = modeStrategyFactory;
            _activityExecutor = activityExecutor;
        }

        public async Task<bool> ExecuteNextBatchAsync(
            WorkflowExecutionContext context,
            ICollection<FlowExecution> pendingExecutions)
        {
            if (!HasPendingExecutions(pendingExecutions))
                return false;

            var strategy = _modeStrategyFactory.GetStrategy(context.Options.ExecutionMode);

            // Primeiro: executar atividades sem interação (sempre em paralelo)
            var nonInteractiveExecutions = pendingExecutions
                .Where(x => x.QueueItem.CanExecute && !x.RequireInteraction)
                .Take(context.Options.MaxDegreeOfParallelism)
                .ToList();

            if (nonInteractiveExecutions.Any())
            {
                _logger.LogDebug(
                    "Executing {Count} non-interactive activities in parallel",
                    nonInteractiveExecutions.Count);

                await Task.WhenAll(
                    nonInteractiveExecutions.Select(e => _activityExecutor.ExecuteAsync(e, context)));

                return HasPendingExecutions(pendingExecutions);
            }

            // Segundo: executar atividades com interação conforme estratégia
            var batchSize = strategy.GetBatchSize(context, pendingExecutions.Where(x => x.QueueItem.CanExecute));
            var interactiveExecutions = pendingExecutions
                .Where(x => x.QueueItem.CanExecute)
                .OrderByDescending(x => x.Selected)
                .Take(batchSize)
                .ToList();

            if (interactiveExecutions.Any())
            {
                _logger.LogDebug(
                    "Executing {Count} interactive activities",
                    interactiveExecutions.Count);

                await Task.WhenAll(
                    interactiveExecutions.Select(e => _activityExecutor.ExecuteAsync(e, context)));
            }

            return HasPendingExecutions(pendingExecutions);
        }

        public bool HasPendingExecutions(ICollection<FlowExecution> pendingExecutions)
        {
            return pendingExecutions.Any(x => x.QueueItem.CanExecute);
        }
    }
}
