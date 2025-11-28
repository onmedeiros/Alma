using Alma.Workflows.Core.States.Abstractions;
using Alma.Workflows.Enums;
using Alma.Workflows.Models.Activities;

namespace Alma.Workflows.Core.States.Components
{
    public class LogState : StateComponent, ILogState
    {
        public void Add(string message)
        {
            Add(message, LogSeverity.Information);
        }

        public void Add(string message, LogSeverity level)
        {
            Add(message, level, DateTime.UtcNow);
        }

        public void Add(string message, LogSeverity severity, DateTime timestamp)
        {
            var log = new LogModel
            {
                Message = message,
                Severity = severity,
                DateTime = timestamp
            };
            GetState().Add(log);
        }

        public IReadOnlyCollection<LogModel> AsCollection()
        {
            return GetState();
        }

        private List<LogModel> GetState()
        {
            EnsureInitialized();
            return StateData!.Logs;
        }
    }
}