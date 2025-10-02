using Alma.Flows.Enums;

namespace Alma.Flows.Models.Activities
{
    public class LogModel
    {
        public DateTime DateTime { get; set; } = DateTime.Now;
        public string Message { get; set; } = string.Empty;
        public LogSeverity Severity { get; set; } = LogSeverity.Information;
    }
}
