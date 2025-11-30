using Alma.Workflows.Core.InstanceExecutions.Services;
using Alma.Workflows.Core.Instances.Entities;
using Alma.Workflows.Options;
using Microsoft.Extensions.Logging;

namespace Alma.Workflows.Core.ExecutionPanels
{
    public class ExecutionPanel
    {
        private readonly ILogger<ExecutionPanel> _logger;
        private readonly IInstanceRunner _runner;
        public Instance Instance { get; init; }
        public ExecutionOptions Options { get; init; }

        public ExecutionPanel(ILogger<ExecutionPanel> logger, IInstanceRunner runner, Instance instance, ExecutionOptions options)
        {
            _logger = logger;
            _runner = runner;
            Instance = instance;
            Options = options;
        }
    }
}