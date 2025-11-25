using Alma.Workflows.Core.States.Abstractions;
using Alma.Workflows.Core.States.Data;
using Alma.Workflows.Options;
using Alma.Workflows.Runners.Scopes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Alma.Workflows.Runners
{
    /// <summary>
    /// Factory interface for creating instances of <see cref="WorkflowRunner"/>.
    /// </summary>
    public interface IWorkflowRunnerFactory
    {
        /// <summary>
        /// Creates a new instance of <see cref="WorkflowRunner"/>.
        /// </summary>
        /// <param name="flow">The flow to be executed.</param>
        /// <param name="state">The current execution state. If null, a new state will be created.</param>
        /// <param name="options">The execution options. If null, default options will be used.</param>
        /// <returns>A new instance of <see cref="WorkflowRunner"/>.</returns>
        WorkflowRunner Create(Flow flow, StateData? state = null, ExecutionOptions? options = null);
    }

    /// <summary>
    /// Factory class for creating instances of <see cref="WorkflowRunner"/>.
    /// </summary>
    public class WorkflowRunnerFactory : IWorkflowRunnerFactory
    {
        private readonly ILogger<WorkflowRunnerFactory> _logger;
        private readonly IExecutionScope _executionScope;

        public WorkflowRunnerFactory(ILogger<WorkflowRunnerFactory> logger, IExecutionScope executionScope)
        {
            _logger = logger;
            _executionScope = executionScope;
        }

        /// <inheritdoc />
        public WorkflowRunner Create(Flow flow, StateData? state = null, ExecutionOptions? options = null)
        {
            // Create isolated scope for workflow execution.
            _executionScope.Initialize();

            var executionState = _executionScope.Current.ServiceProvider.GetRequiredService<IExecutionState>();
            executionState.Initialize(state ?? new StateData());

            options ??= new ExecutionOptions();

            return new WorkflowRunner(_executionScope, flow, options);
        }
    }
}