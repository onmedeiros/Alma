using Alma.Workflows.Core.InstanceExecutions.Enums;

namespace Alma.Workflows.Runners.ExecutionModes
{
    /// <summary>
    /// Factory para criar estratégias de modo de execução baseado no tipo.
    /// </summary>
    public interface IExecutionModeStrategyFactory
    {
        /// <summary>
        /// Obtém a estratégia apropriada para o modo de execução especificado.
        /// </summary>
        /// <param name="mode">Modo de execução</param>
        /// <returns>Estratégia correspondente ao modo</returns>
        IExecutionModeStrategy GetStrategy(InstanceExecutionMode mode);
    }
}
