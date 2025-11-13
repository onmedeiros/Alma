using Alma.Workflows.Core.Activities.Enums;
using Alma.Workflows.Core.Contexts;
using Alma.Workflows.Enums;
using Alma.Workflows.Options;
using Alma.Workflows.Runners.Connections;
using Alma.Workflows.Runners.Coordination;
using Alma.Workflows.Runners.ExecutionModes;
using Alma.Workflows.States;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Alma.Workflows.Runners
{
    /// <summary>
    /// Orchestrates the execution of workflows by coordinating specialized components.
    /// Refactored to follow Single Responsibility Principle - delegates execution
    /// concerns to specialized managers and coordinators.
    /// </summary>
    public class FlowRunner
    {
        private readonly ILogger<FlowRunner> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IActivityRunnerFactory _activityRunnerFactory;
        private readonly IQueueManager _queueManager;
        private readonly IDataSetter _dataSetter;
        private readonly IExecutionCoordinator _executionCoordinator;
        private readonly IConnectionManager _connectionManager;

        public FlowExecutionContext Context { get; private set; }
        public ICollection<FlowExecution> PendingExecutions { get; set; } = [];

        public FlowRunner(
            IServiceProvider serviceProvider, 
            Flow flow, 
            ExecutionState state, 
            ExecutionOptions options)
        {
            _serviceProvider = serviceProvider;

            _logger = serviceProvider.GetRequiredService<ILogger<FlowRunner>>();
            _activityRunnerFactory = serviceProvider.GetRequiredService<IActivityRunnerFactory>();
            _queueManager = serviceProvider.GetRequiredService<IQueueManager>();
            _dataSetter = serviceProvider.GetRequiredService<IDataSetter>();
            _executionCoordinator = serviceProvider.GetRequiredService<IExecutionCoordinator>();
            _connectionManager = serviceProvider.GetRequiredService<IConnectionManager>();

            Context = new FlowExecutionContext(flow, _serviceProvider, state, options);

            LoadStateData();

            _queueManager.LoadNavigations(Context);
            _queueManager.EnqueueStart(Context);
            
            // Inicializa o cache de conexões para performance
            if (_connectionManager is ConnectionManager concreteManager)
            {
                concreteManager.InitializeConnectionCache(flow);
            }
        }

        public void LoadStateData()
        {
            foreach (var activity in Context.Flow.Activities)
            {
                _dataSetter.LoadData(Context.State, activity);
            }
        }

        /// <summary>
        /// Executes the next batch of activities.
        /// In automatic mode, continues executing until completion or pause.
        /// In step-by-step/manual modes, executes only one step per call.
        /// </summary>
        public async Task<bool> ExecuteNextAsync()
        {
            if (Context.Options.Delay > 0)
                await Task.Delay(Context.Options.Delay);

            var continueExecuting = true;

            while (continueExecuting)
            {
                // Prepara as execuções pendentes
                if (!await PreparePendingExecutionsAsync())
                    return false;

                if (!PendingExecutions.Any(x => x.QueueItem.CanExecute))
                    return false;

                // Executa o próximo lote de atividades
                await _executionCoordinator.ExecuteNextBatchAsync(Context, PendingExecutions);

                // Verifica se deve continuar executando no mesmo ExecuteNextAsync
                // Modo Automático: continua até o fim ou até uma pausa
                // Modo Step-by-Step/Manual: executa apenas um lote e retorna
                var strategy = _serviceProvider.GetRequiredService<IExecutionModeStrategyFactory>()
                    .GetStrategy(Context.Options.ExecutionMode);
                
                continueExecuting = strategy.ShouldContinueAfterBatch(Context, PendingExecutions);
            }

            // Atualiza as execuções pendentes uma última vez
            await PreparePendingExecutionsAsync();

            return PendingExecutions.Any(x => x.QueueItem.CanExecute);
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

        /// <summary>
        /// Checks and updates the status of activity execution steps.
        /// </summary>
        public async Task CheckActivityStepStatus(FlowExecution execution)
        {
            if (execution.QueueItem.ExecutionStatus == ActivityExecutionStatus.Waiting || 
                execution.QueueItem.ExecutionStatus == ActivityExecutionStatus.Pending)
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

        public IEnumerable<QueueItem> GetCompletedQueueItems(bool includeAutomaticExecutions = false)
        {
            var queueItems = _queueManager.PeekCompleted(Context);

            if (!includeAutomaticExecutions)
                queueItems = queueItems.Where(x => x.Activity.Descriptor.RequireInteraction);

            return queueItems;
        }
    }
}