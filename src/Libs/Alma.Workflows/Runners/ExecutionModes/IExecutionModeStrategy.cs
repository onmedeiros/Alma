using Alma.Workflows.Core.Contexts;

namespace Alma.Workflows.Runners.ExecutionModes
{
    /// <summary>
    /// Define uma estratégia para controlar o modo de execução do workflow.
    /// Implementa o Strategy Pattern para diferentes comportamentos de execução.
    /// </summary>
    public interface IExecutionModeStrategy
    {
        /// <summary>
        /// Determina se a execução deve continuar após processar um lote de atividades.
        /// </summary>
        /// <param name="context">Contexto de execução do flow</param>
        /// <param name="pendingExecutions">Execuções pendentes</param>
        /// <returns>True se deve continuar executando, false para pausar</returns>
        bool ShouldContinueAfterBatch(
            FlowExecutionContext context,
            IEnumerable<FlowExecution> pendingExecutions);

        /// <summary>
        /// Determina quantas atividades executar no próximo lote.
        /// </summary>
        /// <param name="context">Contexto de execução do flow</param>
        /// <param name="readyExecutions">Execuções prontas para executar</param>
        /// <returns>Número de atividades a executar</returns>
        int GetBatchSize(
            FlowExecutionContext context,
            IEnumerable<FlowExecution> readyExecutions);

        /// <summary>
        /// Determina se uma execução requer interação do usuário neste modo.
        /// </summary>
        /// <param name="execution">Execução a verificar</param>
        /// <returns>True se requer interação</returns>
        bool RequiresUserInteraction(FlowExecution execution);
    }
}
