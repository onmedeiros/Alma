using Alma.Workflows.Core.Activities.Abstractions;
using Microsoft.Extensions.Logging;

namespace Alma.Workflows.Runners.Strategies
{
    /// <summary>
    /// Interface for resolving the appropriate execution strategy for an activity.
    /// </summary>
    public interface IActivityExecutionStrategyResolver
    {
        /// <summary>
        /// Resolves the appropriate execution strategy for the given activity.
        /// </summary>
        /// <param name="activity">The activity to resolve a strategy for.</param>
        /// <returns>The appropriate execution strategy.</returns>
        IActivityExecutionStrategy Resolve(IActivity activity);
    }

    /// <summary>
    /// Resolves the appropriate execution strategy for activities.
    /// Checks registered strategies in order and returns the first one that can handle the activity.
    /// </summary>
    public class ActivityExecutionStrategyResolver : IActivityExecutionStrategyResolver
    {
        private readonly ILogger<ActivityExecutionStrategyResolver> _logger;
        private readonly IEnumerable<IActivityExecutionStrategy> _strategies;
        private readonly IActivityExecutionStrategy _defaultStrategy;

        public ActivityExecutionStrategyResolver(
            ILogger<ActivityExecutionStrategyResolver> logger,
            IEnumerable<IActivityExecutionStrategy> strategies,
            StandardActivityExecutionStrategy defaultStrategy)
        {
            _logger = logger;
            _strategies = strategies ?? throw new ArgumentNullException(nameof(strategies));
            _defaultStrategy = defaultStrategy ?? throw new ArgumentNullException(nameof(defaultStrategy));
        }

        public IActivityExecutionStrategy Resolve(IActivity activity)
        {
            if (activity == null)
                throw new ArgumentNullException(nameof(activity));

            // Try to find a specialized strategy that can handle this activity
            // Strategies are checked in registration order
            foreach (var strategy in _strategies)
            {
                // Skip the default strategy in this loop
                if (strategy == _defaultStrategy)
                    continue;

                if (strategy.CanHandle(activity))
                {
                    _logger.LogDebug(
                        "Resolved strategy {StrategyType} for activity {ActivityId} ({ActivityType})",
                        strategy.GetType().Name,
                        activity.Id,
                        activity.GetType().Name);

                    return strategy;
                }
            }

            // If no specialized strategy found, use the default
            _logger.LogDebug(
                "Using default strategy for activity {ActivityId} ({ActivityType})",
                activity.Id,
                activity.GetType().Name);

            return _defaultStrategy;
        }
    }
}
