using Alma.Workflows.Core.Activities.Base;
using Alma.Workflows.Core.Activities.Enums;
using Alma.Workflows.Core.ApprovalsAndChecks.Models;
using Alma.Workflows.Core.Contexts;
using Alma.Workflows.Enums;
using Alma.Workflows.Options;
using Alma.Workflows.States;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Alma.Workflows.Runners
{
    public class FlowRunnerV2
    {
        private readonly ILogger<FlowRunnerV2> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IActivityRunnerFactory _activityRunnerFactory;
        private readonly IQueueManager _queueManager;
        private readonly IDataSetter _dataSetter;
        private readonly IParameterSetter _parameterSetter;

        public FlowExecutionContext Context { get; private set; }
        public ICollection<FlowExecution> PendingExecutions { get; set; } = [];

        public FlowRunnerV2(IServiceProvider serviceProvider, Flow flow, ExecutionState state, ExecutionOptions options)
        {
            _serviceProvider = serviceProvider;

            _logger = serviceProvider.GetRequiredService<ILogger<FlowRunnerV2>>();
            _activityRunnerFactory = serviceProvider.GetRequiredService<IActivityRunnerFactory>();
            _queueManager = serviceProvider.GetRequiredService<IQueueManager>();
            _dataSetter = serviceProvider.GetRequiredService<IDataSetter>();
            _parameterSetter = serviceProvider.GetRequiredService<IParameterSetter>();

            Context = new FlowExecutionContext(flow, _serviceProvider, state, options);

            LoadStateData();

            _queueManager.LoadNavigations(Context);
            _queueManager.EnqueueStart(Context);
        }

        public void LoadStateData()
        {
            foreach (var activity in Context.Flow.Activities)
            {
                _dataSetter.LoadData(Context.State, activity);
            }
        }

        public async Task<bool> ExecuteNextAsync()
        {
            if (PendingExecutions.Count == 0 && !await PreparePendingExecutionsAsync())
                return false;

            var continueExecuting = true;

            while (continueExecuting)
            {
                // Primeiro, executa todas as atividades prontas para execução que não requerem interação.
                var toExecute = PendingExecutions
                    .Where(x => x.QueueItem.CanExecute && !x.RequireInteraction)
                    .Take(Context.Options.MaxDegreeOfParallelism);

                if (toExecute.Any())
                {
                    await Task.WhenAll(toExecute.Select(ExecuteAsync));
                    await PreparePendingExecutionsAsync();

                    // Enquanto estiver executando atividades que não requerem interação, verifica se há mais atividades
                    // sem interação para continuar executando automaticamente. Se não houverem mais e o modo de execução
                    // for manual, para a execução automática.
                    if (!PendingExecutions.Any(x => !x.RequireInteraction && x.QueueItem.CanExecute)
                        && Context.Options.ExecutionMode == Core.InstanceExecutions.Enums.InstanceExecutionMode.Manual)
                    {
                        continueExecuting = false;
                    }

                    continue;
                }

                // Em seguida, executa as próximas atividades que requerem interação.
                var take = Context.Options.ExecutionMode == Core.InstanceExecutions.Enums.InstanceExecutionMode.Manual
                    ? 1
                    : Context.Options.MaxDegreeOfParallelism;

                toExecute = PendingExecutions
                    .Where(x => x.QueueItem.CanExecute)
                    .OrderByDescending(x => x.Selected)
                    .Take(take);

                await Task.WhenAll(toExecute.Select(ExecuteAsync));
                await PreparePendingExecutionsAsync();

                // Após executar as atividades que requerem interação, verifica se há atividades
                // sem interação para rodar.
                if (PendingExecutions.Any(x => !x.RequireInteraction && x.QueueItem.CanExecute))
                    continue;

                // Se não houver mais atividades para executar, verifica se o modo de execução é manual.
                // O modo manual permite que as atividades sejam executadas uma a uma.
                if (Context.Options.ExecutionMode == Core.InstanceExecutions.Enums.InstanceExecutionMode.Manual)
                {
                    continueExecuting = false;
                    continue;
                }

                continueExecuting = PendingExecutions.Any(x => x.QueueItem.CanExecute);
            }

            return PendingExecutions.Any(x => x.QueueItem.CanExecute);
        }

        public async Task ExecuteAsync(FlowExecution execution)
        {
            if (!execution.QueueItem.CanExecute)
                return;

            var executionResult = await execution.Runner.ExecuteAsync();

            if (executionResult.ExecutionStatus == ActivityExecutionStatus.Completed)
            {
                _queueManager.Complete(Context, execution.QueueItem);

                var executedPorts = executionResult.ExecutedPorts;

                if (executedPorts.Any())
                    EnqueueExecutedPortConnections(executedPorts);
            }
            else if (executionResult.ExecutionStatus == ActivityExecutionStatus.Waiting)
            {
                _queueManager.Wait(Context, execution.QueueItem);
            }
            else
            {
                _queueManager.Fail(Context, execution.QueueItem);
            }
        }

        public async Task<bool> PreparePendingExecutionsAsync(string? selectedQueueItemId = null)
        {
            var pendingExecutions = new List<FlowExecution>();

            foreach (var queueItem in _queueManager.PeekNext(Context, int.MaxValue))
            {
                var execution = PendingExecutions.FirstOrDefault(x => x.QueueItem.Id == queueItem.Id);

                if (execution is null)
                {
                    var activityRunner = _activityRunnerFactory.Create(queueItem.Activity, Context.State, Context.Options);
                    execution = new FlowExecution(queueItem, activityRunner);
                }

                await CheckActivityStepStatus(execution);

                pendingExecutions.Add(execution);

                if (selectedQueueItemId is not null && queueItem.Id == selectedQueueItemId)
                    execution.Selected = true;
            }

            PendingExecutions = pendingExecutions;

            return PendingExecutions.Any();
        }

        public async Task CheckActivityStepStatus(FlowExecution execution)
        {
            if (execution.QueueItem.ExecutionStatus == ActivityExecutionStatus.Waiting || execution.QueueItem.ExecutionStatus == ActivityExecutionStatus.Pending)
            {
                var activityStepStatus = await execution.Runner.RunBeforeExecutionSteps();

                if (activityStepStatus == ActivityStepStatus.Waiting)
                    _queueManager.Wait(Context, execution.QueueItem);
                else if (activityStepStatus == ActivityStepStatus.Failed)
                    _queueManager.Fail(Context, execution.QueueItem);
                else if (activityStepStatus == ActivityStepStatus.Completed)
                    _queueManager.Ready(Context, execution.QueueItem);
                else
                    _queueManager.Pending(Context, execution.QueueItem);
            }
        }

        public void EnqueueExecutedPortConnections(IEnumerable<Port> executedPorts)
        {
            foreach (var port in executedPorts)
            {
                foreach (var connection in Context.Flow.Connections.Where(x => x.Source.ActivityId == port.Activity.Id && x.Source.PortName == port.Descriptor.Name))
                {
                    var executedConnection = new ExecutedConnection(connection, new ValueObject(port.Data));

                    Context.State.ExecutedConnections.Add(executedConnection);

                    var target = Context.Flow.Activities.First(x => x.Id == connection.Target.ActivityId);

                    _queueManager.Enqueue(Context, target);
                }
            }
        }

        public IEnumerable<QueueItem> GetCompletedQueueItems(bool includeAutomaticExecutions = false)
        {
            var queueItems = _queueManager.PeekCompleted(Context);

            if (!includeAutomaticExecutions)
                queueItems = queueItems.Where(x => x.Activity.Descriptor.RequireInteraction);

            return queueItems;
        }
    }
}