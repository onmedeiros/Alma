using Microsoft.Extensions.Logging;

namespace Alma.Workflows.Core.ExecutionPanels
{
    public interface IExecutionPanel
    {
        ValueTask LoadAsync(Workflow)
    }

    public class ExecutionPanel : IExecutionPanel
    {
        private readonly ILogger<ExecutionPanel> _logger;

        public ExecutionPanel(ILogger<ExecutionPanel> logger)
        {
            _logger = logger;
        }
    }
}