using Alma.Workflows.Core.Abstractions;
using Alma.Workflows.Core.States.Abstractions;
using Alma.Workflows.Core.States.Data;
using Alma.Workflows.Options;
using Alma.Workflows.Runners.Scopes;
using Alma.Workflows.States;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Alma.Workflows.Runners
{
    /// <summary>
    /// Factory interface for creating instances of <see cref="ActivityRunner"/>.
    /// </summary>
    public interface IActivityRunnerFactory
    {
        /// <summary>
        /// Creates a new instance of <see cref="ActivityRunner"/>.
        /// </summary>
        /// <param name="activity">The activity to be executed.</param>
        /// <param name="state">The current execution state. If null, a new state will be created.</param>
        /// <param name="options">The execution options. If null, default options will be used.</param>
        /// <returns>A new instance of <see cref="ActivityRunner"/>.</returns>
        ActivityRunner Create(IActivity activity, StateData? state = null, ExecutionOptions? options = null);
    }

    /// <summary>
    /// Factory class for creating instances of <see cref="ActivityRunner"/>.
    /// </summary>
    public class ActivityRunnerFactory : IActivityRunnerFactory
    {
        private readonly ILogger<ActivityRunnerFactory> _logger;
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityRunnerFactory"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="serviceProvider">The service provider for dependency injection.</param>
        public ActivityRunnerFactory(ILogger<ActivityRunnerFactory> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc />
        public ActivityRunner Create(IActivity activity, StateData? state = null, ExecutionOptions? options = null)
        {
            var executionScope = _serviceProvider.GetRequiredService<IExecutionScope>();

            if (!executionScope.HasCurrentScope)
            {
                executionScope.Initialize();

                var executionState = executionScope.Current.ServiceProvider.GetRequiredService<IExecutionState>();
                executionState.Initialize(state ?? new StateData());
            }

            options ??= new ExecutionOptions();

            return new ActivityRunner(_serviceProvider, activity, options);
        }
    }
}