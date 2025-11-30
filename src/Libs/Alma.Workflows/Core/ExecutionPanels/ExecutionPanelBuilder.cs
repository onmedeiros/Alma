using Alma.Workflows.Core.InstanceExecutions.Services;
using Alma.Workflows.Core.Instances.Entities;
using Alma.Workflows.Options;
using Microsoft.Extensions.Logging;

namespace Alma.Workflows.Core.ExecutionPanels
{
    public interface IExecutionPanelBuilder
    {
        ValueTask Begin(Instance instance, ExecutionOptions? options = null);

        ExecutionPanel Build();
    }

    public class ExecutionPanelBuilder : IExecutionPanelBuilder
    {
        private readonly ILogger<ExecutionPanelBuilder> _logger;
        private readonly ILogger<ExecutionPanel> _executionPanelLogger;
        private readonly IInstanceRunner _instanceRunner;

        private bool _beginExecuted = false;
        private ExecutionPanel? _executionPanel = null;

        public ExecutionPanelBuilder(ILogger<ExecutionPanelBuilder> logger, ILogger<ExecutionPanel> executionPanelLogger, IInstanceRunner instanceRunner)
        {
            _logger = logger;
            _executionPanelLogger = executionPanelLogger;
            _instanceRunner = instanceRunner;
        }

        public ValueTask Begin(Instance instance, ExecutionOptions? options = null)
        {
            if (_beginExecuted)
                throw new InvalidOperationException("Begin method has already been executed. It can be called only once.");

            _beginExecuted = true;

            _executionPanel = new ExecutionPanel(_executionPanelLogger, _instanceRunner, instance, options ?? new ExecutionOptions());

            return ValueTask.CompletedTask;
        }

        public ExecutionPanel Build()
        {
            if (!_beginExecuted)
                throw new InvalidOperationException("Begin method must be called before Build.");

            return _executionPanel!;
        }
    }
}