using Alma.Workflows.Options;
using Alma.Workflows.States;
using Microsoft.Extensions.Logging;

namespace Alma.Workflows.Runners
{
    /// <summary>
    /// Factory interface for creating instances of <see cref="FlowRunner"/>.
    /// </summary>
    public interface IFlowRunnerFactory
    {
        /// <summary>
        /// Creates a new instance of <see cref="FlowRunner"/>.
        /// </summary>
        /// <param name="flow">The flow to be executed.</param>
        /// <param name="state">The current execution state. If null, a new state will be created.</param>
        /// <param name="options">The execution options. If null, default options will be used.</param>
        /// <returns>A new instance of <see cref="FlowRunner"/>.</returns>
        FlowRunner Create(Flow flow, ExecutionState? state = null, ExecutionOptions? options = null);
    }

    /// <summary>
    /// Factory class for creating instances of <see cref="FlowRunner"/>.
    /// </summary>
    public class FlowRunnerFactory : IFlowRunnerFactory
    {
        private readonly ILogger<FlowRunnerFactory> _logger;
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="FlowRunnerFactory"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="serviceProvider">The service provider for dependency injection.</param>
        public FlowRunnerFactory(ILogger<FlowRunnerFactory> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc />
        public FlowRunner Create(Flow flow, ExecutionState? state = null, ExecutionOptions? options = null)
        {
            options ??= new ExecutionOptions();
            state ??= new ExecutionState();

            return new FlowRunner(_serviceProvider, flow, state, options);
        }
    }
}