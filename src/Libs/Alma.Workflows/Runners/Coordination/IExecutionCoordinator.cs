using Alma.Workflows.Core.Contexts;

namespace Alma.Workflows.Runners.Coordination
{
    /// <summary>
    /// Interface para coordenação da execução de atividades em um workflow.
    /// Gerencia paralelização, priorização e dependências entre atividades.
    /// </summary>
    public interface IExecutionCoordinator
    {
        /// <summary>
        /// Executa o próximo lote de atividades respeitando o grau de paralelismo
        /// e as dependências entre atividades.
        /// </summary>
        /// <param name="context">Contexto de execução do flow</param>
        /// <param name="pendingExecutions">Execuções pendentes a serem coordenadas</param>
        /// <returns>True se ainda há execuções pendentes, false se concluído</returns>
        Task<bool> ExecuteNextBatchAsync(
            WorkflowExecutionContext context,
            ICollection<ExecutionBatchItem> pendingExecutions);

        /// <summary>
        /// Verifica se há execuções pendentes que podem ser executadas.
        /// </summary>
        /// <param name="pendingExecutions">Lista de execuções pendentes</param>
        /// <returns>True se há execuções pendentes</returns>
        bool HasPendingExecutions(ICollection<ExecutionBatchItem> pendingExecutions);
    }
}
