using Alma.Workflows.Activities.Flow;
using Alma.Workflows.Core.Abstractions;
using Alma.Workflows.Core.Activities.Base;
using Alma.Workflows.Core.ApprovalsAndChecks.Models;
using Alma.Workflows.Core.Contexts;
using Alma.Workflows.Enums;
using Alma.Workflows.Models.Activities;
using Alma.Workflows.States;
using Microsoft.Extensions.Logging;

namespace Alma.Workflows.Runners.Connections
{
    /// <summary>
    /// Gerencia navegação e enfileiramento de conexões no workflow.
    /// Implementa cache para otimizar consultas de conexões.
    /// </summary>
    public class ConnectionManager : IConnectionManager
    {
        private readonly ILogger<ConnectionManager> _logger;
        private readonly IQueueManager _queueManager;

        // Cache de conexões para performance (proteção contra acesso concorrente)
        private readonly Dictionary<string, List<Connection>> _connectionCache = new();

        private readonly object _cacheLock = new();

        public ConnectionManager(
            ILogger<ConnectionManager> logger,
            IQueueManager queueManager)
        {
            _logger = logger;
            _queueManager = queueManager;
        }

        public IEnumerable<Connection> GetOutgoingConnections(IActivity activity, Port port)
        {
            var cacheKey = $"{activity.Id}:{port.Descriptor.Name}";

            lock (_cacheLock)
            {
                if (!_connectionCache.ContainsKey(cacheKey))
                {
                    _logger.LogDebug(
                        "Caching connections for activity {ActivityId}, port {PortName}",
                        activity.Id, port.Descriptor.Name);

                    _connectionCache[cacheKey] = new List<Connection>();
                }

                return _connectionCache[cacheKey];
            }
        }

        public void EnqueueConnectedActivities(
            WorkflowExecutionContext context,
            IEnumerable<Port> executedPorts)
        {
            foreach (var port in executedPorts)
            {
                var connections = context.Flow.Connections
                    .Where(x =>
                        x.Source.ActivityId == port.Activity.Id &&
                        x.Source.PortName == port.Descriptor.Name);

                foreach (var connection in connections)
                {
                    ProcessConnection(context, connection, port);
                }
            }
        }

        public bool IsLoopBackConnection(Connection connection)
        {
            // Verifica se é uma conexão que volta para a porta BodyComplete de um loop
            return connection.Target.PortName == nameof(LoopActivity.BodyComplete);
        }

        private void ProcessConnection(
            WorkflowExecutionContext context,
            Connection connection,
            Port sourcePort)
        {
            var executedConnection = new ExecutedConnection(
                connection,
                new ValueObject(sourcePort.Data));

            context.State.Connections.Add(executedConnection);

            var targetActivity = context.Flow.Activities
                .First(x => x.Id == connection.Target.ActivityId);

            // Tratamento especial para conexões de loop
            if (IsLoopBackConnection(connection))
            {
                HandleLoopBodyCompleteConnection(context, connection, targetActivity);
            }
            else
            {
                _queueManager.Enqueue(context, targetActivity);
            }
        }

        private void HandleLoopBodyCompleteConnection(
            WorkflowExecutionContext context,
            Connection connection,
            IActivity loopActivity)
        {
            _logger.LogInformation(
                "Loop body complete connection detected from {SourceActivityId} to loop {TargetActivityId}",
                connection.Source.ActivityId, connection.Target.ActivityId);

            // Encontra o item do loop na fila
            var loopQueueItem = context.State.Queue.AsCollection()
                .FirstOrDefault(q => q.ActivityId == connection.Target.ActivityId);

            if (loopQueueItem == null)
            {
                _logger.LogWarning(
                    "Loop queue item not found for activity {ActivityId}",
                    connection.Target.ActivityId);
                return;
            }

            var loop = loopQueueItem.Activity as LoopActivity;
            if (loop == null)
            {
                _logger.LogWarning(
                    "Activity {ActivityId} is not a LoopActivity",
                    connection.Target.ActivityId);
                return;
            }

            // Define a fase do loop para BodyCompleted para que a próxima execução incremente
            context.State.Memory.Set(loop.Id, "phase", LoopConstants.PhaseBodyCompleted);

            _logger.LogInformation(
                "Loop {ActivityId} phase set to {Phase}",
                connection.Target.ActivityId, LoopConstants.PhaseBodyCompleted);

            // Marca o loop como pronto para re-execução
            _queueManager.Ready(context, loopQueueItem);
        }

        /// <summary>
        /// Inicializa o cache de conexões para um flow.
        /// Deve ser chamado após carregar o flow.
        /// </summary>
        public void InitializeConnectionCache(Flow flow)
        {
            lock (_cacheLock)
            {
                _connectionCache.Clear();

                foreach (var connection in flow.Connections)
                {
                    var cacheKey = $"{connection.Source.ActivityId}:{connection.Source.PortName}";

                    if (!_connectionCache.ContainsKey(cacheKey))
                    {
                        _connectionCache[cacheKey] = new List<Connection>();
                    }

                    _connectionCache[cacheKey].Add(connection);
                }

                _logger.LogDebug(
                    "Connection cache initialized with {Count} unique source ports",
                    _connectionCache.Count);
            }
        }
    }
}