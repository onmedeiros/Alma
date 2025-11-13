using Alma.Workflows.Activities.Flow;
using Alma.Workflows.Core.Activities.Base;
using Alma.Workflows.Core.Activities.Enums;
using Alma.Workflows.Core.ApprovalsAndChecks.Models;
using Alma.Workflows.Core.Contexts;
using Alma.Workflows.Core.InstanceExecutions.Enums;
using Alma.Workflows.Enums;
using Alma.Workflows.Options;
using Alma.Workflows.Runners.Strategies;
using Alma.Workflows.States;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Alma.Workflows.Runners
{
    /// <summary>
    /// Responsible for running Workflows and managing their execution state.
    /// Uses Strategy Pattern for activity-specific execution logic.
    /// </summary>
    public class FlowRunner
    {
        private readonly ILogger<FlowRunner> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IActivityRunnerFactory _activityRunnerFactory;
        private readonly IQueueManager _queueManager;
        private readonly IDataSetter _dataSetter;
        private readonly IParameterSetter _parameterSetter;
        private readonly IActivityExecutionStrategyResolver _strategyResolver;

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
            _parameterSetter = serviceProvider.GetRequiredService<IParameterSetter>();
            _strategyResolver = serviceProvider.GetRequiredService<IActivityExecutionStrategyResolver>();

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
            if (Context.Options.Delay > 0)
                await Task.Delay(Context.Options.Delay);

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
                        && Context.Options.ExecutionMode == InstanceExecutionMode.Manual)
                    {
                        continueExecuting = false;
                    }

                    continue;
                }

                // Em seguida, executa as próximas atividades que requerem interação.
                var take = Context.Options.ExecutionMode == InstanceExecutionMode.Manual || Context.Options.ExecutionMode == InstanceExecutionMode.StepByStep
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
                if (Context.Options.ExecutionMode == InstanceExecutionMode.Manual || Context.Options.ExecutionMode == InstanceExecutionMode.StepByStep)
                {
                    continueExecuting = false;
                    continue;
                }

                continueExecuting = PendingExecutions.Any(x => x.QueueItem.CanExecute);
            }

            return PendingExecutions.Any(x => x.QueueItem.CanExecute);
        }

        /// <summary>
        /// Executes a single flow execution using the Strategy Pattern.
        /// The appropriate strategy is resolved based on the activity type.
        /// </summary>
        public async Task ExecuteAsync(FlowExecution execution)
        {
            if (!execution.QueueItem.CanExecute)
                return;

            var activity = execution.QueueItem.Activity;

            // Resolve the appropriate execution strategy for this activity type
            var strategy = _strategyResolver.Resolve(activity);

            _logger.LogDebug("Executing activity {ActivityId} ({ActivityType}) using strategy {StrategyType}",
                activity.Id, activity.GetType().Name, strategy.GetType().Name);

            // Execute the activity using the resolved strategy
            var executionResult = await strategy.ExecuteAsync(activity, Context, execution.Runner);

            // Handle post-execution logic using the strategy
            await strategy.HandlePostExecutionAsync(activity, Context, executionResult, execution.QueueItem);

            // Enqueue connections if activity completed successfully
            if (executionResult.ExecutionStatus == ActivityExecutionStatus.Completed && executionResult.ExecutedPorts.Any())
            {
                EnqueueExecutedPortConnections(executionResult.ExecutedPorts);
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

        /// <summary>
        /// Enqueues connections from executed ports to their target activities.
        /// Handles special cases like loop body completion.
        /// </summary>
        public void EnqueueExecutedPortConnections(IEnumerable<Port> executedPorts)
        {
            foreach (var port in executedPorts)
            {
                foreach (var connection in Context.Flow.Connections.Where(x => x.Source.ActivityId == port.Activity.Id && x.Source.PortName == port.Descriptor.Name))
                {
                    var executedConnection = new ExecutedConnection(connection, new ValueObject(port.Data));

                    Context.State.ExecutedConnections.Add(executedConnection);

                    var target = Context.Flow.Activities.First(x => x.Id == connection.Target.ActivityId);

                    // Special handling for loop body complete connections
                    if (IsLoopBodyCompleteConnection(connection))
                    {
                        HandleLoopBodyCompleteConnection(connection);
                    }
                    else
                    {
                        _queueManager.Enqueue(Context, target);
                    }
                }
            }
        }

        /// <summary>
        /// Checks if a connection is from any activity back to a loop's BodyComplete port.
        /// </summary>
        private bool IsLoopBodyCompleteConnection(Connection connection)
        {
            // Check if the target is a LoopActivity
            var targetActivity = Context.Flow.Activities.FirstOrDefault(x => x.Id == connection.Target.ActivityId);
            if (targetActivity is not LoopActivity)
                return false;

            // Check if the target port is BodyComplete
            return connection.Target.PortName == nameof(LoopActivity.BodyComplete);
        }

        /// <summary>
        /// Handles the special case when a loop body completes and reconnects to the loop.
        /// Updates the loop phase and prepares it for the next iteration.
        /// </summary>
        private void HandleLoopBodyCompleteConnection(Connection connection)
        {
            _logger.LogInformation("Loop body complete connection detected from {SourceActivityId} to loop {TargetActivityId}",
                connection.Source.ActivityId, connection.Target.ActivityId);

            // Find the loop activity in the queue
            var loopQueueItem = Context.State.Queue.FirstOrDefault(q => q.ActivityId == connection.Target.ActivityId);
            if (loopQueueItem == null)
            {
                _logger.LogWarning("Loop queue item not found for activity {ActivityId}", connection.Target.ActivityId);
                return;
            }

            var loopActivity = loopQueueItem.Activity as LoopActivity;
            if (loopActivity == null)
            {
                _logger.LogWarning("Activity {ActivityId} is not a LoopActivity", connection.Target.ActivityId);
                return;
            }

            // Set phase to BodyCompleted so next execution will increment and check condition
            loopActivity.LoopPhase = new Data<string> { Value = LoopConstants.PhaseBodyCompleted };
            _dataSetter.UpdateData(Context.State, loopActivity);

            _logger.LogInformation("Loop {ActivityId} phase set to {Phase}", connection.Target.ActivityId, LoopConstants.PhaseBodyCompleted);

            // Mark the loop as ready for re-execution
            _queueManager.Ready(Context, loopQueueItem);
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