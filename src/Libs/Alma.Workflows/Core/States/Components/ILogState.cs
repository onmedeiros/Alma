using Alma.Workflows.Enums;
using Alma.Workflows.Models.Activities;
using Alma.Workflows.States;

namespace Alma.Workflows.Core.States.Components
{
    /// <summary>
    /// Manages log state during flow execution.
    /// </summary>
    public interface ILogState
    {
        /// <summary>
        /// Gets all log entries.
        /// </summary>
        IReadOnlyCollection<LogModel> GetAll();

        /// <summary>
        /// Adds a log entry.
        /// </summary>
        void Log(string? message, LogSeverity severity = LogSeverity.Information);

        /// <summary>
        /// Gets logs filtered by severity.
        /// </summary>
        IEnumerable<LogModel> GetBySeverity(LogSeverity severity);

        /// <summary>
        /// Gets the latest log entry.
        /// </summary>
        LogModel? GetLatest();

        /// <summary>
        /// Clears all log entries.
        /// </summary>
        void Clear();

        /// <summary>
        /// Gets the count of log entries.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Checks if there are any error logs.
        /// </summary>
        bool HasErrors();

        /// <summary>
        /// Checks if there are any warning logs.
        /// </summary>
        bool HasWarnings();
    }
}
