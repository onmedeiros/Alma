using Microsoft.Extensions.Logging;

namespace Alma.Workflows.Core.States.Observers
{
    /// <summary>
    /// Observer that logs state changes.
    /// </summary>
    public class LoggingStateObserver : IStateObserver
    {
        private readonly ILogger<LoggingStateObserver> _logger;

        public LoggingStateObserver(ILogger<LoggingStateObserver> logger)
        {
            _logger = logger;
        }

        public void OnStateChanged(StateChangeEvent changeEvent)
        {
            _logger.LogInformation(
                "State change: {ChangeType} {Target} from {OldValue} to {NewValue}",
                changeEvent.ChangeType,
                changeEvent.Target ?? "N/A",
                changeEvent.OldValue ?? "null",
                changeEvent.NewValue ?? "null"
            );
        }
    }
}
