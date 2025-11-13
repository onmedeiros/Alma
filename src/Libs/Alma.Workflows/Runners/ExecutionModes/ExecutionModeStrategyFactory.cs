using Alma.Workflows.Core.InstanceExecutions.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Alma.Workflows.Runners.ExecutionModes
{
    /// <summary>
    /// Factory para criar estratégias de modo de execução.
    /// Implementa o Factory Pattern para isolar a criação de estratégias.
    /// </summary>
    public class ExecutionModeStrategyFactory : IExecutionModeStrategyFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ExecutionModeStrategyFactory> _logger;

        public ExecutionModeStrategyFactory(
            IServiceProvider serviceProvider,
            ILogger<ExecutionModeStrategyFactory> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public IExecutionModeStrategy GetStrategy(InstanceExecutionMode mode)
        {
            _logger.LogDebug("Creating execution mode strategy for {Mode}", mode);

            return mode switch
            {
                InstanceExecutionMode.Manual => 
                    _serviceProvider.GetRequiredService<ManualExecutionModeStrategy>(),
                
                InstanceExecutionMode.StepByStep => 
                    _serviceProvider.GetRequiredService<StepByStepExecutionModeStrategy>(),
                
                InstanceExecutionMode.Automatic => 
                    _serviceProvider.GetRequiredService<AutomaticExecutionModeStrategy>(),
                
                _ => throw new ArgumentException(
                    $"Unsupported execution mode: {mode}", nameof(mode))
            };
        }
    }
}
