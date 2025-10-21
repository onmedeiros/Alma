using Alma.Workflows.Enums;

namespace Alma.Workflows.Models.Activities
{
    public class LogModel
    {
        public DateTime DateTime { get; set; } = DateTime.Now;
        public string Message { get; set; } = string.Empty;
        public LogSeverity Severity { get; set; } = LogSeverity.Information;
    }
}
