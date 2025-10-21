using Alma.Workflows.Core.Abstractions;
using Alma.Workflows.Core.Activities.Base;
using Alma.Workflows.Core.Activities.Models;
using Alma.Workflows.Core.ApprovalsAndChecks.Models;
using Alma.Workflows.Core.Contexts;
using Alma.Workflows.Enums;
using Alma.Workflows.Options;
using Alma.Workflows.States;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Alma.Workflows.Runners
{
    /// <summary>
    /// Responsible for running Workflows and managing their execution state.
    /// </summary>
    public class FlowRunner
    {
        private readonly ILogger<FlowRunner> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IActivityRunnerFactory _activityRunnerFactory;
        private readonly IQueueManager _queueManager;
        private readonly IDataSetter _dataSetter;

        public FlowExecutionContext Context { get; private set; }
        public ICollection<FlowExecution> NextExecutions { get; } = [];

        /// <summary>
        /// Initializes a new instance of the <see cref="FlowRunner"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider for dependency injection.</param>
        /// <param name="flow">The flow to be executed.</param>
        /// <param name="state">The current execution state.</param>
        /// <param name="options">The execution options.</param>
        public FlowRunner(IServiceProvider serviceProvider, Flow flow, ExecutionState state, ExecutionOptions options)
        {
            _serviceProvider = serviceProvider;

            _logger = serviceProvider.GetRequiredService<ILogger<FlowRunner>>();
            _activityRunnerFactory = serviceProvider.GetRequiredService<IActivityRunnerFactory>();
            _queueManager = serviceProvider.GetRequiredService<IQueueManager>();
            _dataSetter = serviceProvider.GetRequiredService<IDataSetter>();

            Context = new FlowExecutionContext(flow, _serviceProvider, state, options);

            _queueManager.LoadNavigations(Context);
            _queueManager.EnqueueStart(Context);

            LoadStateData();
        }

        #region Load state

        public void LoadStateData()
        {
            foreach (var activity in Context.Flow.Activities)
            {
                _dataSetter.LoadData(Context.State, activity);
            }
        }

        #endregion

        #region Execution controllers

        public async Task<bool> SelectNext(string? queueItemId = null)
        {
            NextExecutions.Clear();

            await _queueManager.UpdateExecutionStatus(Context);

            if (!string.IsNullOrEmpty(queueItemId))
            {
                var queueItem = _queueManager.PeekById(Context, queueItemId);

                if (queueItem is not null)
                {
                    var activityRunner = _activityRunnerFactory.Create(queueItem.Activity, Context.State, Context.Options);
                    NextExecutions.Add(new FlowExecution(queueItem, activityRunner));
                    return true;
                }
            }
            else
            {
                var queueItems = _queueManager.PeekNext(Context, Context.Options.MaxDegreeOfParallelism);

                if (!queueItems.Any())
                    return false;

                foreach (var queueItem in queueItems)
                {
                    var activityRunner = _activityRunnerFactory.Create(queueItem.Activity, Context.State, Context.Options);
                    NextExecutions.Add(new FlowExecution(queueItem, activityRunner));
                }

                return true;
            }

            return false;
        }

        public async Task<bool> ExecuteNext()
        {
            if (NextExecutions.Count == 0 && !await SelectNext())
                return false;

            await Task.WhenAll(NextExecutions.Select(Execute));

            if (Context.Options.Delay > 0)
                await Task.Delay(Context.Options.Delay);

            await SelectNext();

            return NextExecutions.Any(x => x.QueueItem.CanExecute);
        }

        public async Task Execute(FlowExecution execution)
        {
            if (!execution.QueueItem.CanExecute)
                return;

            var executionResult = await execution.Runner.ExecuteAsync();

            EnqueueExecutedPortConnections(executionResult.ExecutedPorts);

            if (executionResult.ExecutionStatus == ActivityExecutionStatus.Completed)
            {
                _queueManager.Complete(Context, execution.QueueItem);
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

        /// <summary>
        /// Executes the next activity in the flow asynchronously.
        /// </summary>
        /// <returns>True if there are more activities to be executed, otherwise false.</returns>
        public async Task<bool> ExecuteNextAsync()
        {
            // Peek returns only the activities that are ready for execution
            var queueItems = _queueManager.PeekNextReady(Context);
            await Task.WhenAll(queueItems.Select(ExecuteQueueItem));

            if (Context.Options.Delay > 0)
                await Task.Delay(Context.Options.Delay);

            return await HasNext();
        }

        /// <summary>
        /// Executes a specific queue item asynchronously.
        /// </summary>
        /// <param name="item">The queue item to be executed.</param>
        public async Task ExecuteQueueItem(QueueItem item)
        {
            var activity = Context.Flow.Activities.First(a => a.Id == item.ActivityId);

            var executionResult = await ExecuteActivity(activity);

            if (executionResult.ExecutionStatus == ActivityExecutionStatus.Completed)
            {
                _queueManager.Complete(Context, item);
            }
            else if (executionResult.ExecutionStatus == ActivityExecutionStatus.Waiting)
            {
                _queueManager.Wait(Context, item);
            }
            else
            {
                _queueManager.Fail(Context, item);
            }
        }

        /// <summary>
        /// Executes a specific activity asynchronously.
        /// </summary>
        /// <param name="activity">The activity to be executed.</param>
        /// <returns>The result of the activity execution.</returns>
        public async Task<ActivityExecutionResult> ExecuteActivity(IActivity activity)
        {
            var runner = _activityRunnerFactory.Create(activity, Context.State, Context.Options);

            var executionResult = await runner.ExecuteAsync();

            EnqueueExecutedPortConnections(executionResult.ExecutedPorts);

            return executionResult;
        }

        public async Task Resume()
        {
            await _queueManager.UpdateExecutionStatus(Context);
        }

        #endregion

        #region Queue

        /// <summary>
        /// Checks if there are any activities ready to be executed in the flow.
        /// </summary>
        /// <returns>True if there are activities ready to be executed, otherwise false.</returns>
        public async Task<bool> HasNext()
        {
            // Update the status of pending and waiting activities to check if they are just ready for execution.
            await _queueManager.UpdateExecutionStatus(Context);

            return _queueManager.HasNext(Context);
        }

        public IEnumerable<QueueItem> GetNext(int count)
        {
            return _queueManager.PeekNext(Context, count);
        }

        public IEnumerable<QueueItem> GetCompletedQueueItems()
        {
            return _queueManager.PeekCompleted(Context);
        }

        public async Task UpdateQueueItemExecutionStatus()
        {
            await _queueManager.UpdateExecutionStatus(Context);
        }

        public Task UpdateQueueItemExecutionStatus(QueueItem item)
        {
            return _queueManager.UpdateExecutionStatus(Context, item);
        }

        /// <summary>
        /// Enqueues the connections of the executed ports.
        /// </summary>
        /// <param name="executedPorts">The executed ports.</param>
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

        #endregion
    }
}