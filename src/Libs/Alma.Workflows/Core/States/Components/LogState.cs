using Alma.Workflows.Enums;
using Alma.Workflows.Models.Activities;
using Alma.Workflows.States;

namespace Alma.Workflows.Core.States.Components
{
    /// <summary>
    /// Concrete implementation of log state management.
    /// </summary>
    public class LogState : ILogState
    {
        private readonly List<LogModel> _logs;

        public LogState()
        {
            _logs = new List<LogModel>();
        }

        public int Count => _logs.Count;

        public IReadOnlyCollection<LogModel> GetAll()
        {
            return _logs.AsReadOnly();
        }

        public void Log(string? message, LogSeverity severity = LogSeverity.Information)
        {
            _logs.Add(new LogModel
            {
                Message = message ?? string.Empty,
                Severity = severity
            });
        }

        public IEnumerable<LogModel> GetBySeverity(LogSeverity severity)
        {
            return _logs.Where(x => x.Severity == severity);
        }

        public LogModel? GetLatest()
        {
            return _logs.LastOrDefault();
        }

        public void Clear()
        {
            _logs.Clear();
        }

        public bool HasErrors()
        {
            return _logs.Any(x => x.Severity == LogSeverity.Error);
        }

        public bool HasWarnings()
        {
            return _logs.Any(x => x.Severity == LogSeverity.Warning);
        }
    }
}
