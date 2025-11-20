using Alma.Workflows.Options;
using Alma.Workflows.States;
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
        WorkflowRunner Create(Flow flow, ExecutionState? state = null, ExecutionOptions? options = null);
    }

    /// <summary>
    /// Factory class for creating instances of <see cref="WorkflowRunner"/>.
    /// </summary>
    public class WorkflowRunnerFactory : IWorkflowRunnerFactory
    {
        private readonly ILogger<WorkflowRunnerFactory> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public WorkflowRunnerFactory(ILogger<WorkflowRunnerFactory> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        /// <inheritdoc />
        public WorkflowRunner Create(Flow flow, ExecutionState? state = null, ExecutionOptions? options = null)
        {
            // Create isolated scope for workflow execution.
            var scope = _scopeFactory.CreateScope();

            options ??= new ExecutionOptions();
            state ??= new ExecutionState();

            return new WorkflowRunner(scope.ServiceProvider, flow, state, options);
        }
    }
}