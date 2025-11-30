using Alma.Workflows.Core.Contexts;

namespace Alma.Workflows.Runners.Coordination
{
    /// <summary>
    /// Interface responsável por executar uma única atividade.
    /// Separada do coordenador para manter responsabilidades únicas.
    /// </summary>
    public interface IActivityExecutor
    {
        /// <summary>
        /// Executa uma atividade e gerencia seu ciclo de vida completo.
        /// </summary>
        /// <param name="execution">Execução da atividade a ser processada</param>
        /// <param name="context">Contexto de execução do flow</param>
        Task ExecuteAsync(ExecutionBatchItem execution, WorkflowExecutionContext context);
    }
}
