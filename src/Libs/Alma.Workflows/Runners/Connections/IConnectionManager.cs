using Alma.Workflows.Core.Activities.Abstractions;
using Alma.Workflows.Core.Activities.Base;
using Alma.Workflows.Core.Contexts;

namespace Alma.Workflows.Runners.Connections
{
    /// <summary>
    /// Interface para gerenciar navegação e enfileiramento de conexões no workflow.
    /// Separa a responsabilidade de gerenciar conexões da lógica de execução.
    /// </summary>
    public interface IConnectionManager
    {
        /// <summary>
        /// Obtém todas as conexões de saída de uma atividade através de uma porta específica.
        /// </summary>
        /// <param name="activity">Atividade de origem</param>
        /// <param name="port">Porta de saída</param>
        /// <returns>Coleção de conexões de saída</returns>
        IEnumerable<Connection> GetOutgoingConnections(IActivity activity, Port port);

        /// <summary>
        /// Enfileira as atividades conectadas às portas executadas.
        /// </summary>
        /// <param name="context">Contexto de execução do flow</param>
        /// <param name="executedPorts">Portas que foram executadas</param>
        void EnqueueConnectedActivities(
            WorkflowExecutionContext context,
            IEnumerable<Port> executedPorts);

        /// <summary>
        /// Verifica se uma conexão é uma conexão de volta (loopback), 
        /// como no caso de loops.
        /// </summary>
        /// <param name="connection">Conexão a verificar</param>
        /// <returns>True se é uma conexão de loopback</returns>
        bool IsLoopBackConnection(Connection connection);
    }
}
